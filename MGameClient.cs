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
    UserCard[] UserPriority = new UserCard[4];      //玩家存储次序等于发牌顺序

    MGameClientAction gameAct;
    OptionData.DataBase gameData = new OptionData.DataBase();
    IMGameView game;

    logic currentState;
    logic nextState;

    void Awake()
    {
        Application.targetFrameRate = Frame;
        game = this;
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
            TriggerHandCard(UserPriority[currentUserID].handCard, ref selectHandCardID);
            TriggerHandCard(UserPriority[currentUserID].handleCard, ref selectHandlecardID);
        }
        //用户可以出牌
        if(UserPriority[currentUserID].operateFlag)
        {
            //单击鼠标右键出牌
            if(Input.GetMouseButtonUp(0) && (selectHandCardID >= 0 || selectHandlecardID >= 0))
            {
                nextState = logic.out_card;
                UserPriority[currentUserID].operateFlag = false;
                if(selectHandCardID >= 0)
                    if(UserPriority[currentUserID].handCard[selectHandCardID].animator.GetCurrentAnimatorStateInfo(0).fullPathHash == GlobalData.ANIMA_OnTriggerEnter)
                        StartCoroutine(LogicCprocessor(currentUserID, UserPriority[currentUserID].handCard.GetMahjongCard(selectHandCardID, 0)));
                    else if(selectHandlecardID >= 0)
                        if(UserPriority[currentUserID].handleCard[0].animator.GetCurrentAnimatorStateInfo(0).fullPathHash == GlobalData.ANIMA_OnTriggerEnter)
                            StartCoroutine(LogicCprocessor(currentUserID, UserPriority[currentUserID].handleCard.GetMahjongCard()));
            }
        }
    }

    void OnGUI()
    {
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
            game.OnUserAction(0, 0, UserAction.Hu);
        }
    }

    //手动逻辑
    IEnumerator LogicCprocessor(int userID = 0,MahjongPrefab mahjong = new MahjongPrefab())
    {
        switch(nextState)
        {
            case logic.turn_dice:
                currentState = nextState;
                nextState = logic.empty;
                //显示色子
                yield return gameAct.DisplayDice();
                //掷色子
                yield return gameAct.TurnDice(gameData.dice_num[0], gameData.dice_num[1]);
                //确认发牌起始位置
                index = 2 * Mathf.Min(gameData.dice_num[0], gameData.dice_num[1]);

                break;
            case logic.add_hand_card:
                currentState = nextState;
                nextState = logic.add_hand_card_end;
                //2秒后色子消失
                StartCoroutine(gameAct.DisapperDice(2f));
                //确定取牌次序
                gameAct.CardDirection(gameData.get_card_priority);
                //发牌顺序
                SetPriority(gameData.player_priority);
                //每个玩家抽取三轮
                for(int i = 0; i < 3; ++i)
                {
                    //每轮玩家抽取四张
                    for(int j = 0; j < 4; ++j)
                    {
                        for(int k = 0; k < 4; ++k)
                        {
                            if(UserPriority[j].handCard.IsNotFull)
                            {
                                yield return gameAct.AddHandCard(UserPriority[j], gameAct.group_H.GetMahjongCard(index));
                            } else
                                Debug.LogError("<MGameClient::LogicCprocessor>:玩家手牌已满." + UserPriority[j].ToString());
                        }
                        //翻牌
                        yield return gameAct.DisplayCard(UserPriority[j]);
                        //每次抽完牌稍微停滞一下
                        yield return new WaitForSeconds(0.1f);
                    }
                }
                //再各多发一张
                for(int i = 0; i < 4; ++i)
                {
                    if(UserPriority[i].handCard.IsNotFull)
                        //使用函数内部延迟
                        yield return gameAct.AddHandCard(UserPriority[i], gameAct.group_H.GetMahjongCard(index));
                    else
                        Debug.LogError("<MGameClient::LogicCprocessor>:玩家手牌已满." + UserPriority[i].ToString());
                    //翻牌
                    yield return gameAct.DisplayCard(UserPriority[i]);
                    //每次抽完牌稍微停滞一下
                    yield return new WaitForSeconds(0.1f);
                }
                //整理手牌                                                  ----未完成

                //庄家获取手牌
                if(UserPriority[userID].handleCard.IsNotFull)
                    yield return gameAct.AddHandleCard(UserPriority[userID], gameAct.group_H.GetMahjongCard(index));

                yield return new WaitForSeconds(.3f);
                yield return LogicCprocessor(userID);
                break;
            case logic.add_hand_card_end:
                currentState = nextState;
                nextState = logic.empty;
                //可以界面操作
                globalOperateFlag = true;
                //允许庄家操作
                UserPriority[userID].operateFlag = true;
                break;
            case logic.out_card:
                currentState = nextState;
                nextState = logic.empty;
                //出牌
                yield return gameAct.AddOutCard(UserPriority[userID], mahjong);
                yield return new WaitForSeconds(.3f);
                //判定是否有handlecard 如果有 则加入手牌
                if(!UserPriority[userID].handleCard.IsEmpty)
                {
                    insertID = GetHandleCardInsertID(UserPriority[userID]);
                    yield return gameAct.InsertToHandCard(UserPriority[userID], insertID, UserPriority[userID].handleCard.GetMahjongCard());
                }

                //轮下个玩家出牌                                      ---临时 此状态结尾插入吃碰判定
                ++turnUserID;
                turnUserID %= 4;
                yield return new WaitForSeconds(.5f);                                        
                //直接获取手牌 非当前玩家自动出牌                                     
                    nextState = logic.get_handlecard;
                    //谁获取手牌
                    yield return LogicCprocessor(turnUserID);
                break;
            case logic.get_handlecard:
                currentState = nextState;
                nextState = logic.empty;
                //获取手牌
                if(UserPriority[userID].handleCard.IsNotFull)
                    yield return gameAct.AddHandleCard(UserPriority[userID], gameAct.group_H.GetMahjongCard(index));

                UserPriority[userID].operateFlag = true;

                if(turnUserID != currentUserID)
                {
                    nextState = logic.out_card;
                    UserPriority[turnUserID].operateFlag = false;
                    yield return LogicCprocessor(turnUserID, UserPriority[turnUserID].handleCard.GetMahjongCard());

                }
                break;
            case logic.hu:
                currentState = nextState;
                nextState = logic.empty;
                //全局控制关闭
                globalOperateFlag = false;
                //玩家控制关闭并摊开玩家手牌
                for(int i = 0;i< 4; ++i)
                {
                    UserPriority[i].operateFlag = false;
                    yield return gameAct.TurnOverCard(UserPriority[i]);
                }
                break;

        }
        yield return 0;
    }

    //托管逻辑
    IEnumerator AutoLogicCprocessor(int userID, int index)
    {

        yield return 0;
    }

    //确认发牌顺序 逆时针发牌顺序  0从主角开始发牌  1从右家开始发牌  2从对家开始发牌  3从左家开始发牌
    void SetPriority(int priority)
    {
        //设置当前用户ID
        currentUserID = priority;
        UserPriority[priority % 4] = gameAct.hostUser;
        UserPriority[(1 + priority) % 4] = gameAct.rightUser;
        UserPriority[(2 + priority) % 4] = gameAct.frontUser;
        UserPriority[(3 + priority) % 4] = gameAct.leftUser;
    }

    //某个玩家出某个牌
    IEnumerator UserPutCard(int userID,int cardID)
    {
        //摸牌
        nextState = logic.get_handlecard;
        yield return LogicCprocessor(userID);
        //出牌
        nextState = logic.out_card;
        //如果ID大于手牌数量 则出刚摸上来的牌
        MahjongPrefab mahjong;
        if(cardID >= UserPriority[userID].handCard.Count)
            mahjong = UserPriority[userID].handleCard.GetMahjongCard();
        else
            mahjong = UserPriority[userID].handCard.GetMahjongCard(cardID,0);  //0表示从左手边开始算起 第cardID个牌
        yield return LogicCprocessor(userID,mahjong);
    }

    //                                                                            ---未完成回调
    //吃 碰 明杠 吃胡  arg[0] 第一个手牌id  arg[1] 第二个 arg[2] 第三 arg[3] 四
    IEnumerator UserChiCard(int userID, int supID, UserAction action, params int[] arg)
    {

        //吃掉出牌玩家刚出的牌
        MahjongPrefab card1 = UserPriority[supID].outCardPoint.GetMahjongCard();
        //获取吃牌的第一个手牌
        MahjongPrefab card2 ;
        //第二个时手牌已减1 对应索引减1 第三个要减2
        MahjongPrefab card3 ;

        switch(action)
        {
            case UserAction.Chi:
            case UserAction.Peng:
                card2 = UserPriority[userID].handCard.GetMahjongCard(arg[0], 0);
                card3 = UserPriority[userID].handCard.GetMahjongCard(arg[1] - 1, 0);
                yield return gameAct.AddPengChi(UserPriority[userID], CardActType.Chi, card1, card2, card3);
                break;
            case UserAction.MingGang:
                card2 = UserPriority[userID].handCard.GetMahjongCard(arg[0], 0);
                card3 = UserPriority[userID].handCard.GetMahjongCard(arg[1] - 1, 0);
                MahjongPrefab card4 = UserPriority[userID].handCard.GetMahjongCard(arg[2] - 2, 0);
                yield return gameAct.AddGang(UserPriority[userID], CardActType.MingGang, card1, card2, card3, card4);
                break;
             //吃胡将吃牌加入Handlecard
            case UserAction.ChiHu:
                yield return gameAct.AddHandleCard(UserPriority[userID], card1);
                break;
        }
    }

    //暗杠  同上                                                                ---未添加回调
    IEnumerator UserAnGangCard(int userID,int supID,params int[] arg)
    {
        MahjongPrefab card1 = UserPriority[userID].handCard.GetMahjongCard(arg[0], 0);
        MahjongPrefab card2 = UserPriority[userID].handCard.GetMahjongCard(arg[1] - 1, 0);
        MahjongPrefab card3 = UserPriority[userID].handCard.GetMahjongCard(arg[2] - 2, 0);
        //第四张有可能是刚摸上来的
        MahjongPrefab card4;
        if(arg[3] - 3 >= UserPriority[userID].handCard.Count)
            card4 = UserPriority[userID].handleCard.GetMahjongCard();
        else
            card4 = UserPriority[userID].handCard.GetMahjongCard(arg[3] - 3, 0);

        yield return gameAct.AddGang(UserPriority[userID], CardActType.AnGang, card1, card2, card3, card4);
    }

    //发牌前替换手牌

    //自动出牌                                                              -------临时
    void AutoOutCard()
    {
        if(currentState == logic.out_card)
        {
            nextState = logic.get_handlecard;
            //谁获取手牌
            StartCoroutine(LogicCprocessor(turnUserID));
        } else
        {
            nextState = logic.out_card;
            UserPriority[turnUserID].operateFlag = false;
            StartCoroutine(LogicCprocessor(turnUserID, UserPriority[turnUserID].handleCard.GetMahjongCard()));
        }
    }

    //获取手牌插入位置                                                      -------待完成
    int GetHandleCardInsertID(UserCard user)
    {
        return UnityEngine.Random.Range(0,user.handCard.Count);
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

    enum logic
    {
       empty,               //空
       add_group,           //洗牌
       turn_dice,           //掷色子
       add_hand_card,       //发牌
       add_hand_card_end,   //发牌结束
       game_start,          //游戏开始
       get_handlecard,      //摸牌
       out_card,            //出牌
       hu,                  //胡
    }

    #region IMGameView

    /*动作*/


    //开始
    bool IMGameView.OnStart()
    {
        //游戏开始
        gameStartFlag = true;
        //禁止界面操作
        globalOperateFlag = false;
        //当前状态
        currentState = logic.add_group;
        //下一个状态
        nextState = logic.turn_dice;
        //重置
        gameAct.Reset();
        //随便连起链表便于洗牌
        gameAct.CardDirection(0);
        //洗牌 第一个出牌的总是庄家 庄家为0
        StartCoroutine(gameAct.AddGroup(() =>
        {
            //洗完牌摇色子
            StartCoroutine(LogicCprocessor(0));
        }));

        return true;
    }

    //时间消息 时间到自动出牌
    void IMGameView.OnTimer()
    {
        AutoOutCard();
    }

    //出牌操作
    void IMGameView.OnOutCard()
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
        nextState = logic.turn_dice;
        //摇色子
        StartCoroutine(LogicCprocessor());
    }

    //发牌
    void IMGameView.OnSendCard()
    {
        nextState = logic.add_hand_card;
        StartCoroutine(LogicCprocessor());
    }
    //玩家操作
    void IMGameView.OnUserAction(int userID, int supID, UserAction action, params int[] arg)
    {
        switch(action)
        {
            case UserAction.PutCard:
                StartCoroutine(UserPutCard(userID, arg[0]));
                break;
            case UserAction.Chi:
            case UserAction.Peng:
            case UserAction.MingGang:
            case UserAction.ChiHu:
                StartCoroutine(UserChiCard(userID, supID, action, arg));
                break;
            case UserAction.AnGang:
                StartCoroutine(UserAnGangCard(userID, supID, arg));
                break;
            case UserAction.Hu:
                nextState = logic.hu;
                StartCoroutine(LogicCprocessor());
                break;
            case UserAction.Ting:
                break;
            case UserAction.TingCancel:
                break;
            case UserAction.TuoGuang:
                break;
            case UserAction.AICancel:
                break;
        }
    }

    //玩家准备                                                                   ---未完成
    void IMGameView.OnReady(int userID)
    {
        
    }

    //操作提示
    bool IMGameView.OnSubOperateNotify()
    {
        throw new NotImplementedException();
    }
    //操作结果
    bool IMGameView.OnSubOperateResult()
    {
        throw new NotImplementedException();
    }
    //游戏结束
    bool IMGameView.OnSubGameEnd()
    {
        throw new NotImplementedException();
    }
    //用户托管
    bool IMGameView.OnSubTrustee()
    {
        throw new NotImplementedException();
    }
    //用户听牌
    bool IMGameView.OnSubListenCard()
    {
        throw new NotImplementedException();
    }

    /*辅助*/

    //校验出牌 网络不好的时候
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
    //操作提示
    byte IMGameView.GetSelectCardInfo()
    {
        throw new NotImplementedException();
    }
    #endregion
}
