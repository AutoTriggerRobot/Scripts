/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 游戏界面控制逻辑
* 类 名： MGameClient.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;

public class MGameClient : MonoBehaviour, IMGameView
{
    public int Frame = 60;                         //帧数
    public Mesh[] Meshs;
    public int ID = 2333;                           //唯一ID

    new Camera camera;                              //主摄像机
    LayerMask mask;                                 //事件层级
    bool gameStartFlag = false;                     //游戏开始标志
    bool globalOperateFlag = false;                 //可控制标志
    int index;                                      //取牌起始位置
    int insertID = 0;                               //手牌插入位置
    int selectHandCardID = -1;                      //当前选择的手牌ID
    int selectHandlecardID = -1;                    //当前选择的HandleCardID
    int currentUserID = 0;                          //当前用户ID  0表示庄家  从庄家逆时针算起 最大为3
    int supID = 0;                             //轮到哪个玩家  0表示庄家  从庄家逆时针轮流
    UserCard[] userPriority = new UserCard[4];      //玩家存储次序等于发牌顺序
    List<int> handCard = new List<int>();           //当前用户手牌
    int handleCard = -1 ;                           //当前用户手中的牌 -1为空

    List<PlayerAct> gameTig = new List<PlayerAct>();//操作提示

    //暂存数据
    List<List<int>> gangBuffer;
    List<List<int>> chiBuffer;
    List<int> pengBuffer;
    List<int> addGangBuffer;
    List<List<int>> cardData;

    List<int> exebuff;

    //界面动作
    MGameClientAction gameAct;
    //游戏参数
    OptionData.DataBase gameData = new OptionData.DataBase();
    //控制
    IController control;
    //临时测试
    IMGameView game;
 

    Logic previousState = Logic.empty;
    Logic nextState = Logic.empty;

    

    UserAction userState = UserAction.empty;

    void Awake()
    {
        Application.targetFrameRate = Frame;
        game = this;
        control = new MGameLogic(this);
    }

    void Start()
    {
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        gameAct = new MGameClientAction(this);
        gameData = new OptionData.DataBase(0);
        mask = 1 << 1;
        Meshs = Resources.LoadAll<Mesh>("Model");
    }

    void Update()
    {
        //可全局操作
        if(globalOperateFlag)
        {
            TriggerHandCard(userPriority[currentUserID].handCard, ref selectHandCardID);
            TriggerHandCard(userPriority[currentUserID].handleCard, ref selectHandlecardID);
        }
        //用户可以出牌
        if(userPriority[currentUserID].operateFlag)
        {
            //单击鼠标右键出牌
            if(Input.GetMouseButtonUp(0) && (selectHandCardID >= 0 || selectHandlecardID >= 0))
            {
                nextState = Logic.out_card;
                if(selectHandCardID >= 0)
                {
                    if(userPriority[currentUserID].handCard[selectHandCardID].animator.GetCurrentAnimatorStateInfo(0).fullPathHash == GlobalData.ANIMA_OnTriggerEnter)
                    {
                        control.EndStatusFlag(currentUserID, UserAction.put_card_flag, new List<int>() { selectHandCardID });
                        //StartCoroutine(LogicCprocessor(currentUserID, userPriority[currentUserID].handCard.GetMahjongCard(selectHandCardID, 0)));
                    }


                } else if(selectHandlecardID >= 0)
                {
                    if(userPriority[currentUserID].handleCard[0].animator.GetCurrentAnimatorStateInfo(0).fullPathHash == GlobalData.ANIMA_OnTriggerEnter)
                    {
                        control.EndStatusFlag(currentUserID, UserAction.put_card_flag, new List<int>() { -1 });
                        //StartCoroutine(LogicCprocessor(currentUserID, userPriority[currentUserID].handleCard.GetMahjongCard()));
                    }
                }
            }
        }
    }

    //临时测试
    bool vReady = true;
    bool vPass;
    bool vHu;
    bool vChi;
    bool vPeng;
    bool vGang;
    bool vJiaGang;

    void OnGUI()
    {

        if(vReady && GUILayout.Button("准备"))
        {
            vReady = false;
            control.EndStatusFlag(ID, UserAction.ready);
        }
        if(vPass && GUILayout.Button("过"))
        {
            vPass = false;
            vHu = false;
            vPeng = false;
            vGang = false;
            vChi = false;
            vJiaGang = false;
            control.EndStatusFlag(currentUserID, UserAction.pass_flag);
        }
        if(vHu && GUILayout.Button("胡"))
        {
            vHu = false;
            vPass = false;
            control.EndStatusFlag(currentUserID, UserAction.hu_flag);
        }
        if(vPeng && GUILayout.Button("碰"))
        {
            vPeng = false;
            vPass = false;
            exebuff = pengBuffer;
            control.EndStatusFlag(currentUserID, UserAction.peng_card_flag, exebuff);
        }
        if(vGang && GUILayout.Button("杠"))
        {
            vGang = false;
            vPass = false;
            exebuff = gangBuffer[0];
            control.EndStatusFlag(currentUserID, UserAction.gang_flag, exebuff);
        }
        if(vChi && GUILayout.Button("吃"))
        {
            vChi = false;
            vPass = false;
            exebuff = chiBuffer[0];
            control.EndStatusFlag(currentUserID, UserAction.chi_card_flag, exebuff);
        }
        if(vJiaGang && GUILayout.Button("加杠"))
        {
            vJiaGang = false;
            vPass = false;
            exebuff = gangBuffer[0];
            control.EndStatusFlag(currentUserID, UserAction.add_gang,exebuff);
        }

        /*
        if(GUILayout.Button("开始"))
        {
            game.OnStart();
        }
        if(globalOperateFlag)
        {

            if(GUILayout.Button("获取手牌"))
            {
                AutoOutCard();
            }
        }
        if(GUILayout.Button("摇色子"))
        {
            game.OnSiceTwo();
        }
        if(GUILayout.Button("发牌"))
        {
            game.OnSendCard();
        }
        if(GUILayout.Button("胡"))
        {
            game.OnUserAction(0, 0, UserAction.hu);
        }
        */
    }

    //混乱手牌（假象）
    List<int> RandCard(List<int> cards)
    {
        List<int> tempCard = new List<int>(cards);
        int i = cards.Count;
        int temp;
        int indexA;
        int indexB;

        while(i > 0)
        {
            indexA = Mathf.FloorToInt(UnityEngine.Random.value * i);
            indexB = --i;
            if(indexA == indexB)
                continue;
            temp = tempCard[indexA];
            tempCard[indexA] = tempCard[indexB];
            tempCard[indexB] = temp;
        }
        return tempCard;
    }

    //手动逻辑
    IEnumerator LogicCprocessor(int userID = 0,MahjongPrefab mahjong = new MahjongPrefab())
    {
        switch(nextState)
        {
            //掷色子
            case Logic.turn_dice:
                previousState = nextState;
                nextState = Logic.empty;
                //显示色子
                yield return gameAct.DisplayDice();
                //掷色子
                yield return gameAct.TurnDice(gameData.dice_num[0], gameData.dice_num[1]);
                //设置发牌起始位置
                index = 2 * Mathf.Min(gameData.dice_num[0], gameData.dice_num[1]);
                control.EndStatusFlag(userID, UserAction.turn_dice);
                break;
            //发牌
            case Logic.add_hand_card:
                previousState = nextState;
                nextState = Logic.add_hand_card_end;
                //2秒后色子消失
                StartCoroutine(gameAct.DisapperDice(2f));
                //确定取牌次序
                int prio;
                //如果当前玩家不是庄家
                if(currentUserID != 0)
                    prio = (gameData.dice_num[0] + gameData.dice_num[1]) % gameData.player_priority;
                else
                    //如果是庄家则除4
                    prio = (gameData.dice_num[0] + gameData.dice_num[1]) % 4;
                gameAct.CardDirection(prio);
                
                //临时牌 保存乱序后的手牌
                List<int> cardTemp = RandCard(handCard); 
                //每个玩家抽取三轮
                for(int i = 0; i < 3; ++i)
                {
                    //每轮玩家抽取四张
                    for(int j = 0; j < 4; ++j)
                    {
                        for(int k = 0; k < 4; ++k)
                        {
                            if(userPriority[j].handCard.IsNotFull)
                            {
                                yield return gameAct.AddHandCard(userPriority[j], gameAct.group_H.GetMahjongCard(index));
                            } else
                                Debug.LogError("<MGameClient::LogicCprocessor>:玩家手牌已满." + userPriority[j].ToString());
                        }
                        //翻牌
                        if(j == currentUserID)
                            yield return gameAct.DisplayCard(userPriority[j], cardTemp);
                        else
                            yield return gameAct.DisplayCard(userPriority[j]);
                        //每次抽完牌稍微停滞一下
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                //再各多发一张
                for(int i = 0; i < 4; ++i)
                {
                    if(userPriority[i].handCard.IsNotFull)
                        //使用函数内部延迟
                        yield return gameAct.AddHandCard(userPriority[i], gameAct.group_H.GetMahjongCard(index));
                    else
                        Debug.LogError("<MGameClient::LogicCprocessor>:玩家手牌已满." + userPriority[i].ToString());
                    //翻牌 如果是当前用户 则替换成需要的牌
                    if(i == currentUserID)
                        yield return gameAct.DisplayCard(userPriority[i], cardTemp);
                    else
                        yield return gameAct.DisplayCard(userPriority[i]);
                    //每次抽完牌稍微停滞一下
                    yield return new WaitForSeconds(0.1f);
                }
                //整理手牌                                                  
                yield return new WaitForSeconds(0.5f);
                yield return gameAct.SortCard(userPriority[currentUserID], handCard);

                control.EndStatusFlag(userID, UserAction.send_card);
                break;
            //摸牌
            case Logic.get_handlecard:
                previousState = nextState;
                nextState = Logic.empty;
                //获取手牌
                if(currentUserID != userID && userPriority[userID].handleCard.IsNotFull)
                    yield return gameAct.AddHandleCard(userPriority[userID], gameAct.group_H.GetMahjongCard(index));
                else if(userPriority[userID].handleCard.IsNotFull)
                {
                    MahjongPrefab cardtemp = gameAct.group_H.GetMahjongCard(index);
                    //替换模型
                    cardtemp.mesh.mesh = ResoucreMtr.Instance.GetMesh(handleCard);
                    yield return gameAct.AddHandleCard(userPriority[userID], cardtemp);
                }

                //可以界面操作
                globalOperateFlag = true;
                //允许玩家操作
                userPriority[userID].operateFlag = true;

                //如果是当前用户 则判断是否有操作提示
                if(currentUserID == userID)
                    GameTipAnalys();

                control.EndStatusFlag(userID, UserAction.get_card);
                break;
            //出牌
            case Logic.out_card:
                previousState = nextState;
                nextState = Logic.empty;
                //出牌
                yield return gameAct.AddOutCard(userPriority[userID], mahjong);
                break;
            //胡
            case Logic.hu:
                previousState = nextState;
                nextState = Logic.empty;
                //全局控制关闭
                globalOperateFlag = false;
                //玩家控制关闭并摊开玩家手牌
                for(int i = 0;i< 4; ++i)
                {
                    userPriority[i].operateFlag = false;
                    yield return gameAct.SortCard(userPriority[i], cardData[i]);
                    //如果手牌不为空
                    if(!userPriority[i].handleCard.IsEmpty)
                        userPriority[i].handleCard[0].mesh.mesh = ResoucreMtr.Instance.GetMesh(cardData[4][0]);
                    yield return gameAct.TurnOverCard(userPriority[i]);
                }
                break;

        }
        yield return 0;
    }

    //托管逻辑
    IEnumerator TrusteeCprocessor(int userID, int index)
    {

        yield return 0;
    }

    //确认发牌顺序 逆时针发牌顺序  0从主角开始发牌  1从右家开始发牌  2从对家开始发牌  3从左家开始发牌
    void SetPriority(int priority)
    {
        //设置当前用户ID
        currentUserID = priority;
        userPriority[priority % 4] = gameAct.hostUser;
        userPriority[(1 + priority) % 4] = gameAct.rightUser;
        userPriority[(2 + priority) % 4] = gameAct.frontUser;
        userPriority[(3 + priority) % 4] = gameAct.leftUser;
    }

    //某个玩家出某个牌 将手中的牌插入某个位置 -1表示空操作  cardType为要出的牌型
    IEnumerator UserPutCard(int userID,int cardID,int sortID,int cardType = -1)
    {
        //出牌
        nextState = Logic.out_card;
        //如果ID小于0 则出刚摸上来的牌
        MahjongPrefab mahjong;
        if(cardID < 0)
            mahjong = userPriority[userID].handleCard.GetMahjongCard();
        else
            mahjong = userPriority[userID].handCard.GetMahjongCard(cardID,0);  //0表示从左手边开始算起 第cardID个牌
        //如果有传入牌型则替换牌型
        if(cardType > 0)
            mahjong.mesh.mesh = ResoucreMtr.Instance.GetMesh(cardType);
        yield return LogicCprocessor(userID,mahjong);
        //将手牌插入
        if(!userPriority[userID].handleCard.IsEmpty && sortID >= 0)
        {
            yield return gameAct.InsertToHandCard(userPriority[userID], sortID, userPriority[userID].handleCard.GetMahjongCard());
        }
        control.EndStatusFlag(userID,UserAction.put_card);
    }

    //吃胡
    IEnumerator UserChiHuCard(int userID,int supID)
    {
        //吃掉出牌玩家刚出的牌
        MahjongPrefab card1 = userPriority[supID].outCardPoint.GetMahjongCard();
        yield return gameAct.AddHandleCard(userPriority[userID], card1);
        nextState = Logic.hu;
        yield return LogicCprocessor();
    }

    /// <summary>
    /// 加杠
    /// </summary>
    /// <param name="userID">id</param>
    /// <param name="cardId">哪个位置的牌</param>
    /// <param name="outCardId">碰牌区域第几个</param>
    /// <param name="cardType">是什么牌型</param>
    /// <returns></returns>
    IEnumerator UserAddCangCard(int userID,int cardId,int outCardId,int cardType)
    {
        //加杠
        yield return 0;
    }

    //吃 碰 明杠  arg[0] 第一个手牌id  arg[0][1] 第二个 arg[0][2] 第三 arg[0][3] 四   arg[1] 牌型
    IEnumerator UserChiCard(int userID, int supID, UserAction action, params List<int>[] arg)
    {

        //获取吃牌的第一个手牌
        MahjongPrefab card1 ;
        //第二个时手牌已减1 对应索引减1 第三个要减2
        MahjongPrefab card2 ;

        //吃掉出牌玩家刚出的牌
        MahjongPrefab card = userPriority[supID].outCardPoint.GetMahjongCard();

        switch(action)
        {
            case UserAction.chi:
            case UserAction.peng:
                card1 = userPriority[userID].handCard.GetMahjongCard(arg[0][0], 0);
                card2 = userPriority[userID].handCard.GetMahjongCard(arg[0][1] - 1, 0);
                //牌型验证
                if(arg.Length > 1 && arg[1].Count > 2)
                {
                    card1.mesh.mesh = ResoucreMtr.Instance.GetMesh(arg[1][0]);
                    card2.mesh.mesh = ResoucreMtr.Instance.GetMesh(arg[1][1]);
                    card.mesh.mesh = ResoucreMtr.Instance.GetMesh(arg[1][2]);
                }

                yield return gameAct.AddPengChi(userPriority[userID], CardActType.Chi, card, card1, card2);
                break;
            case UserAction.ming_gang:
                card1 = userPriority[userID].handCard.GetMahjongCard(arg[0][0], 0);
                card2 = userPriority[userID].handCard.GetMahjongCard(arg[0][1] - 1, 0);
                MahjongPrefab card3 = userPriority[userID].handCard.GetMahjongCard(arg[0][2] - 2, 0);
                //牌型验证
                if(arg.Length > 1 && arg[1].Count > 3)
                {
                    card1.mesh.mesh = ResoucreMtr.Instance.GetMesh(arg[1][0]);
                    card2.mesh.mesh = ResoucreMtr.Instance.GetMesh(arg[1][1]);
                    card3.mesh.mesh = ResoucreMtr.Instance.GetMesh(arg[1][2]);
                    card.mesh.mesh = ResoucreMtr.Instance.GetMesh(arg[1][3]);
                }

                yield return gameAct.AddGang(userPriority[userID], CardActType.MingGang, card, card1, card2, card3);
                break;
             //吃胡将吃牌加入Handlecard
            case UserAction.chi_hu:
                yield return gameAct.AddHandleCard(userPriority[userID], card);
                nextState = Logic.hu;
                yield return LogicCprocessor();
                break;
        }
        control.EndStatusFlag(userID, action);
    }

    //暗杠  同上                                                                
    IEnumerator UserAnGangCard(int userID,int supID,params List<int>[] arg)
    {
        MahjongPrefab card1 = userPriority[userID].handCard.GetMahjongCard(arg[0][0], 0);
        MahjongPrefab card2 = userPriority[userID].handCard.GetMahjongCard(arg[0][1] - 1, 0);
        MahjongPrefab card3 = userPriority[userID].handCard.GetMahjongCard(arg[0][2] - 2, 0);
        //第四张有可能是刚摸上来的
        MahjongPrefab card4;
        if(arg[0][3] - 3 < 0 )
            card4 = userPriority[userID].handleCard.GetMahjongCard();
        else
            card4 = userPriority[userID].handCard.GetMahjongCard(arg[0][3] - 3, 0);

        yield return gameAct.AddGang(userPriority[userID], CardActType.AnGang, card1, card2, card3, card4);
        control.EndStatusFlag(userID, UserAction.an_gang);
    }

    //自动出牌                                                                      ----临时
    void AutoOutCard()
    {
        if(previousState == Logic.out_card)
        {
            nextState = Logic.get_handlecard;
            //谁获取手牌
            StartCoroutine(LogicCprocessor(supID));
        } else
        {
            nextState = Logic.out_card;
            userPriority[supID].operateFlag = false;
            StartCoroutine(LogicCprocessor(supID, userPriority[supID].handleCard.GetMahjongCard()));
        }
    }

    //出牌动画
    void TriggerHandCard(Container container,ref int currentSelectID)
    {
        bool b;
        RaycastHit hit;
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
            
            //模式监听第二层  即 TransparentFX 
            b = Physics.Raycast(ray, out hit, mask);
        if(b)
        {
            //如果检查到碰撞并且与当前选择id不一致 则改变当前id
            if(currentSelectID != container.FindTransform(hit.transform.parent))
            {
                //id大于零则表示鼠标刚从上个物体经过并该物体还存在
                if(currentSelectID >= 0 && container.Count > currentSelectID)
                {
                    container[currentSelectID].animator.Play(GlobalData.ANIMA_OnTriggerExit);
                    //Debug.Log(currentSelectID + "out");
                }
                //重置新id
                currentSelectID = container.FindTransform(hit.transform.parent);
                //大于零表示进入新物体
                if(currentSelectID >= 0)
                {
                    container[currentSelectID].animator.Play(GlobalData.ANIMA_OnTriggerEnter);
                    //Debug.Log(currentSelectID + "in");
                }
            }
        } else
        {
            //id大于零则表示鼠标刚从上个物体经过
            if(currentSelectID >= 0)
            {
                //如果该物体还在容器里则播放动画
                if(container.Count > currentSelectID)
                    container[currentSelectID].animator.Play(GlobalData.ANIMA_OnTriggerExit);
                //Debug.Log(currentSelectID + "out");
                currentSelectID = -1;
            }
        }
    }

    //操作提示
    void GameTipAnalys()
    {
        for(int i = 0; i < gameTig.Count; ++i)
        {
            switch(gameTig[i])
            {
                case PlayerAct.zi_mo:
                    vHu = true;
                    break;
                case PlayerAct.gang:
                    vGang = true;
                    break;
                case PlayerAct.add_gang:
                    vJiaGang = true;
                    break;
            }
        }
    }

    #region IMGameView

    /*动作*/


    //开始
    bool IMGameView.OnStart(OptionData.DataBase data)
    {
        //游戏开始
        gameStartFlag = true;
        //禁止界面操作
        globalOperateFlag = false;
        //当前状态
        previousState = Logic.add_group;
        //下一个状态
        nextState = Logic.turn_dice;
        //配置
        gameData = data;
        //发牌顺序
        SetPriority(gameData.player_priority);
        //重置
        gameAct.Reset();
        //随便连起链表便于洗牌
        gameAct.CardDirection(0);
        //洗牌 第一个出牌的总是庄家 庄家为0
        StartCoroutine(gameAct.AddGroup(() =>
        {
            //洗完牌摇色子
            StartCoroutine(LogicCprocessor(currentUserID));
        }));

        return true;
    }

    //时间消息 时间到自动出牌                                                       ----临时
    void IMGameView.OnTimer()
    {
        AutoOutCard();
    }

    //出牌操作                                                                     
    void IMGameView.OnOutCard(int userID, int outCardInd, int sortInd)
    {
        AutoOutCard();
    }

    //托管控制
    int IMGameView.OnTrusteeControl()
    {
        throw new NotImplementedException();
    }

    //第二次摇色子消息
    void IMGameView.OnSiceTwo()
    {
        nextState = Logic.turn_dice;
        //摇色子
        StartCoroutine(LogicCprocessor());
    }

    //发牌
    void IMGameView.OnSendCard(List<int> cards)
    {
        //获取手牌
        this.handCard = cards;
        nextState = Logic.add_hand_card;
        StartCoroutine(LogicCprocessor());
    }
    //玩家操作
    void IMGameView.OnUserAction(int userID, UserAction action, params List<int>[] arg)
    {
        switch(action)
        {
            //发牌标志
            case UserAction.send_card_flag:
                //获取手牌
                this.handCard = arg[0];
                nextState = Logic.add_hand_card;
                StartCoroutine(LogicCprocessor());
                break;
            //摸牌
            case UserAction.get_card:
                gameTig.Clear();
                //摸牌
                this.handleCard = arg[0][0];
                //如果长度大于1则有后序操作
                if(arg[1].Count > 1)
                    for(int i = 1; i < arg[1].Count; ++i)
                    {
                        gameTig.Add((PlayerAct)arg[1][i]);
                    }
                gangBuffer = new List<List<int>>(arg);
                //获取能杠的牌
                gangBuffer.RemoveRange(0, 2);
                nextState = Logic.get_handlecard;
                StartCoroutine(LogicCprocessor(userID));
                break;
            case UserAction.put_card:
                userPriority[userID].operateFlag = false;
                if(userID == currentUserID)
                    StartCoroutine(UserPutCard(userID, arg[0][0], arg[0][1]));
                else
                    StartCoroutine(UserPutCard(userID, arg[0][0], arg[0][1], arg[0][2]));
                break;
            case UserAction.insert_card:
                break;
            //arg[0][0]被吃玩家id arg[1].... 吃牌index 
            //arg[1][0] arg[1][1] arg[[1]2] or arg[1][3]  
            case UserAction.chi:
            case UserAction.peng:
                userPriority[userID].operateFlag = true;
                StartCoroutine(UserChiCard(userID, arg[0][0], action, arg[1], arg[2]));
                break;
            case UserAction.ming_gang:
                StartCoroutine(UserChiCard(userID, arg[0][0], action, arg[1], arg[2]));
                break;
            case UserAction.an_gang:
                StartCoroutine(UserAnGangCard(userID, arg[0][0], arg[1], arg[2]));
                break;
            case UserAction.add_gang:
                StartCoroutine(UserAddCangCard(userID, arg[0][0], arg[0][1], arg[0][2]));
                break;
            case UserAction.chi_hu:
                cardData = new List<List<int>>(arg);
                cardData.RemoveAt(0);
                StartCoroutine(UserChiHuCard(userID, arg[0][0]));
                break;
            case UserAction.hu:
                //要获取所有玩家的牌型
                cardData = new List<List<int>>(arg);
                nextState = Logic.hu;
                StartCoroutine(LogicCprocessor());
                break;
            case UserAction.ting:
                break;
            case UserAction.ting_cancel:
                break;
            case UserAction.trustee:
                break;
            case UserAction.AI_cancel:
                break;
            case UserAction.chi_card_flag:
                this.supID = arg[0][0];
                vChi = true;
                vPass = true;
                chiBuffer = new List<List<int>>(arg);
                chiBuffer.RemoveAt(0);
                break;
            case UserAction.peng_card_flag:
                this.supID = arg[0][0];
                vPeng = true;
                vPass = true;
                pengBuffer = arg[1];
                break;
            case UserAction.gang_flag:
                this.supID = arg[0][0];
                vGang = true;
                vPass = true;
                gangBuffer = new List<List<int>>(arg);
                gangBuffer.RemoveAt(0);
                break;
            case UserAction.chi_hu_flag:
                this.supID = arg[0][0];
                vHu = true;
                break;
            case UserAction.ting_flag:
                break;
            //可从新开始
            case UserAction.end:
                vReady = true;
                break;
        }
    }

    //玩家准备                                                                          ---未完成
    void IMGameView.OnReady(int userID)
    {
        
    }

    //操作提示                                                                         ---杠碰吃提示 待完成
    bool IMGameView.OnOperateNotify()
    {
        throw new NotImplementedException();
    }
    //操作结果                                                                        ---回馈选择信息
    bool IMGameView.OnOperateResult()
    {
        throw new NotImplementedException();
    }
    //游戏结束
    bool IMGameView.OnGameEnd()
    {
        throw new NotImplementedException();
    }
    //用户托管
    bool IMGameView.OnTrustee()
    {
        throw new NotImplementedException();
    }
    //用户听牌
    bool IMGameView.OnListenCard()
    {
        throw new NotImplementedException();
    }

    /*辅助*/

    //校验出牌
    bool IMGameView.VerdiotOutCard()
    {
        throw new NotImplementedException();
    }
    //是否可出
    bool IMGameView.CheckOutCard()
    {
        throw new NotImplementedException();
    }
    //设置手牌控制
    void IMGameView.SetHandCardControl()
    {
        throw new NotImplementedException();
    }
    //操作提示                                                                          ---出牌提示&信息
    byte IMGameView.GetSelectCardInfo()
    {
        throw new NotImplementedException();
    }
    #endregion
}