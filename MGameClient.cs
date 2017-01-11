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
    int turnUserID = 0;                             //轮到哪个玩家  0表示庄家  从庄家逆时针轮流
    UserCard[] userPriority = new UserCard[4];      //玩家存储次序等于发牌顺序
    List<int> handCard = new List<int>();           //当前用户手牌
    int handleCard = -1 ;                           //当前用户手中的牌 -1为空
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
                userPriority[currentUserID].operateFlag = false;
                if(selectHandCardID >= 0)
                    if(userPriority[currentUserID].handCard[selectHandCardID].animator.GetCurrentAnimatorStateInfo(0).fullPathHash == GlobalData.ANIMA_OnTriggerEnter)
                        StartCoroutine(LogicCprocessor(currentUserID, userPriority[currentUserID].handCard.GetMahjongCard(selectHandCardID, 0)));
                    else if(selectHandlecardID >= 0)
                        if(userPriority[currentUserID].handleCard[0].animator.GetCurrentAnimatorStateInfo(0).fullPathHash == GlobalData.ANIMA_OnTriggerEnter)
                            StartCoroutine(LogicCprocessor(currentUserID, userPriority[currentUserID].handleCard.GetMahjongCard()));
            }
        }
    }

    //临时测试
    bool vReady = true;
    bool vGuo;
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
            control.SetStatusFlag(ID, UserAction.ready);
        }
        if(vGuo && GUILayout.Button("过"))
        {

        }
        if(vHu && GUILayout.Button("胡"))
        {

        }
        if(vPeng && GUILayout.Button("碰"))
        {

        }
        if(vGang && GUILayout.Button("杠"))
        {

        }
        if(vChi && GUILayout.Button("吃"))
        {

        }
        if(vJiaGang && GUILayout.Button("加杠"))
        {

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
                control.EndStatusFlag(userID, UserAction.turn_dice, previousState);
                break;
            //发牌
            case Logic.add_hand_card:
                previousState = nextState;
                nextState = Logic.add_hand_card_end;
                //2秒后色子消失
                StartCoroutine(gameAct.DisapperDice(2f));
                //确定取牌次序
                int prio = (gameData.dice_num[0] + gameData.dice_num[1]) % gameData.player_priority;
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

                //庄家摸牌
                if(currentUserID != 0 && userPriority[userID].handleCard.IsNotFull)
                    yield return gameAct.AddHandleCard(userPriority[userID], gameAct.group_H.GetMahjongCard(index));
                else if(userPriority[userID].handleCard.IsNotFull)
                {
                    MahjongPrefab cardtemp = gameAct.group_H.GetMahjongCard(index);
                    //替换模型
                    cardtemp.mesh.mesh = ResoucreMtr.Instance.GetMesh(handleCard);
                    yield return gameAct.AddHandleCard(userPriority[userID],cardtemp);
                }

                yield return new WaitForSeconds(.3f);
                yield return LogicCprocessor(userID);
                break;
            //发牌结束
            case Logic.add_hand_card_end:
                previousState = nextState;
                nextState = Logic.empty;
                //可以界面操作
                globalOperateFlag = true;
                //允许玩家操作
                userPriority[userID].operateFlag = true;

                control.EndStatusFlag(userID, UserAction.send_card, previousState);
                break;
            //出牌
            case Logic.out_card:
                previousState = nextState;
                nextState = Logic.empty;
                //出牌
                yield return gameAct.AddOutCard(userPriority[userID], mahjong);
                break;
            //摸牌
            case Logic.get_handlecard:
                previousState = nextState;
                nextState = Logic.empty;
                //获取手牌
                if(userPriority[userID].handleCard.IsNotFull)
                    yield return gameAct.AddHandleCard(userPriority[userID], gameAct.group_H.GetMahjongCard(index));

                userPriority[userID].operateFlag = true;

                control.EndStatusFlag(userID,UserAction.empty, previousState);
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

    //某个玩家出某个牌 将手中的牌插入某个位置 -1表示空操作
    IEnumerator UserPutCard(int userID,int cardID,int sortID)
    {
        //摸牌
        nextState = Logic.get_handlecard;
        yield return LogicCprocessor(userID);
        //出牌
        nextState = Logic.out_card;
        //如果ID大于手牌数量 则出刚摸上来的牌
        MahjongPrefab mahjong;
        if(cardID >= userPriority[userID].handCard.Count)
            mahjong = userPriority[userID].handleCard.GetMahjongCard();
        else
            mahjong = userPriority[userID].handCard.GetMahjongCard(cardID,0);  //0表示从左手边开始算起 第cardID个牌
        yield return LogicCprocessor(userID,mahjong);
        //将手牌插入
        if(!userPriority[userID].handleCard.IsEmpty)
        {
            yield return gameAct.InsertToHandCard(userPriority[userID], sortID, userPriority[userID].handleCard.GetMahjongCard());
        }
        control.EndStatusFlag(userID,UserAction.put_card);
    }

    //吃 碰 明杠 吃胡  arg[0] 第一个手牌id  arg[1] 第二个 arg[2] 第三 arg[3] 四
    IEnumerator UserChiCard(int userID, int supID, UserAction action, params int[] arg)
    {

        //吃掉出牌玩家刚出的牌
        MahjongPrefab card1 = userPriority[supID].outCardPoint.GetMahjongCard();
        //获取吃牌的第一个手牌
        MahjongPrefab card2 ;
        //第二个时手牌已减1 对应索引减1 第三个要减2
        MahjongPrefab card3 ;

        switch(action)
        {
            case UserAction.chi:
            case UserAction.peng:
                card2 = userPriority[userID].handCard.GetMahjongCard(arg[0], 0);
                card3 = userPriority[userID].handCard.GetMahjongCard(arg[1] - 1, 0);
                yield return gameAct.AddPengChi(userPriority[userID], CardActType.Chi, card1, card2, card3);
                break;
            case UserAction.ming_gang:
                card2 = userPriority[userID].handCard.GetMahjongCard(arg[0], 0);
                card3 = userPriority[userID].handCard.GetMahjongCard(arg[1] - 1, 0);
                MahjongPrefab card4 = userPriority[userID].handCard.GetMahjongCard(arg[2] - 2, 0);
                yield return gameAct.AddGang(userPriority[userID], CardActType.MingGang, card1, card2, card3, card4);
                break;
             //吃胡将吃牌加入Handlecard
            case UserAction.chi_hu:
                yield return gameAct.AddHandleCard(userPriority[userID], card1);
                nextState = Logic.hu;
                yield return LogicCprocessor();
                break;
        }
        control.EndStatusFlag(userID, action);
    }

    //暗杠  同上                                                                
    IEnumerator UserAnGangCard(int userID,int supID,params int[] arg)
    {
        MahjongPrefab card1 = userPriority[userID].handCard.GetMahjongCard(arg[0], 0);
        MahjongPrefab card2 = userPriority[userID].handCard.GetMahjongCard(arg[1] - 1, 0);
        MahjongPrefab card3 = userPriority[userID].handCard.GetMahjongCard(arg[2] - 2, 0);
        //第四张有可能是刚摸上来的
        MahjongPrefab card4;
        if(arg[3] - 3 >= userPriority[userID].handCard.Count)
            card4 = userPriority[userID].handleCard.GetMahjongCard();
        else
            card4 = userPriority[userID].handCard.GetMahjongCard(arg[3] - 3, 0);

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
            StartCoroutine(LogicCprocessor(turnUserID));
        } else
        {
            nextState = Logic.out_card;
            userPriority[turnUserID].operateFlag = false;
            StartCoroutine(LogicCprocessor(turnUserID, userPriority[turnUserID].handleCard.GetMahjongCard()));
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
    void IMGameView.OnSendCard(List<int> cards,int handleCard)
    {
        //获取手牌
        this.handCard = cards;
        this.handleCard = handleCard;
        nextState = Logic.add_hand_card;
        StartCoroutine(LogicCprocessor());
    }
    //玩家操作
    void IMGameView.OnUserAction(int userID, int supID, UserAction action, params int[] arg)
    {
        switch(action)
        {
            case UserAction.put_card:
                StartCoroutine(UserPutCard(userID, arg[0],arg[1]));
                break;
            case UserAction.insert_card:
                break;
            case UserAction.chi:
            case UserAction.peng:
            case UserAction.ming_gang:
            case UserAction.chi_hu:
                StartCoroutine(UserChiCard(userID, supID, action, arg));
                break;
            case UserAction.an_gang:
                StartCoroutine(UserAnGangCard(userID, supID, arg));
                break;
            case UserAction.hu:
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