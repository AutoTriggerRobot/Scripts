/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 客户端逻辑
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

public class MGameClient : MonoBehaviour, IMGameView
{
    MGameClientAction gameAct;
    OptionData.DataBase gameData = new OptionData.DataBase();

    logic currentState;
    logic nextState;

    void Awake()
    {
        gameAct = new MGameClientAction(this);
    }

    IEnumerator LogiCprocessor()
    {
        switch(nextState)
        {
            case logic.turn_dice:
                currentState = nextState;
                nextState = logic.add_hand_card;
                //显示色子
                yield return gameAct.DisplayDice();
                yield return new WaitForSeconds(1f);
                //掷色子
                yield return gameAct.TurnDice(gameData.dice_num[0], gameData.dice_num[1]);
                StartCoroutine(gameAct.DisapperDice(2f));
                //进入下一个状态
                yield return LogiCprocessor();
                break;
            case logic.add_hand_card:
                currentState = nextState;
                nextState = logic.add_hand_card_end;
                //确定取牌次序
                gameAct.CardDirection(gameData.get_card_priority);
                //开始发牌

                break;
        }
        yield return 0;
    }

    enum logic
    {
       add_group,           //洗牌
       turn_dice,           //掷色子
       add_hand_card,       //发牌
       add_hand_card_end,   //发牌结束
    }

    #region IMGameView

    /*动作*/

    //游戏开始
    bool IMGameView.OnSubGameStart()
    {
        //当前状态
        currentState = logic.add_group;
        //下一个状态
        nextState = logic.turn_dice;
        //重置
        gameAct.Reset();
        //随便连起链表便于洗牌
        gameAct.CardDirection(0);
        //洗牌
        StartCoroutine(gameAct.AddGroup(() =>
        {
            StartCoroutine(LogiCprocessor());
        }));

        return true;
    }

    //用户出牌
    bool IMGameView.OnSubOutCard()
    {
        throw new NotImplementedException();
    }
    //发牌消息
    bool IMGameView.OnSubSendCard()
    {
        throw new NotImplementedException();
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
    //补牌消息
    bool IMGameView.OnSubReplaceCard()
    {
        throw new NotImplementedException();
    }

    /*辅助*/

    //播放牌声音
    void IMGameView.PlayCardSound()
    {
        throw new NotImplementedException();
    }
    //播放动作声音
    void IMGameView.PlayActionSound()
    {
        throw new NotImplementedException();
    }
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
    //操作提示
    byte IMGameView.GetSelectCardInfo()
    {
        throw new NotImplementedException();
    }
    //卡牌移动
    bool IMGameView.BeginMoveCard()
    {
        throw new NotImplementedException();
    }
    //停止移动
    bool IMGameView.StopMoveCard()
    {
        throw new NotImplementedException();
    }
    //出牌移动
    bool IMGameView.BeginMoveOutCard()
    {
        throw new NotImplementedException();
    }
    //发牌移动
    bool IMGameView.BeginMoveSendCard()
    {
        throw new NotImplementedException();
    }
    //补张动画
    bool IMGameView.BeginMoveReplaceCard()
    {
        throw new NotImplementedException();
    }
    //开局发牌动画
    bool IMGameView.BeginMoveStartCard()
    {
        throw new NotImplementedException();
    }
    //出牌动画完成
    bool IMGameView.OnMoveOutCardFinish()
    {
        throw new NotImplementedException();
    }
    //发牌动画完成
    bool IMGameView.OnMoveSendCardFinish()
    {
        throw new NotImplementedException();
    }
    //补花动画完成
    bool IMGameView.OnMoveReplaceCardFinish()
    {
        throw new NotImplementedException();
    }
    //开局动画完成
    bool IMGameView.OnMoveStartCardFinish()
    {
        throw new NotImplementedException();
    }

    /*其他玩家*/

    //时间消息
    void IMGameView.OnTimer()
    {
        throw new NotImplementedException();
    }
    //开始消息
    int IMGameView.OnStart()
    {
        throw new NotImplementedException();
    }
    //出牌操作
    int IMGameView.OnOutCard()
    {
        throw new NotImplementedException();
    }
    //卡牌动作
    int IMGameView.OnCardOperate()
    {
        throw new NotImplementedException();
    }
    //托管控制
    int IMGameView.OnTrusteeControl()
    {
        throw new NotImplementedException();
    }
    //买底操作
    int IMGameView.OnChip()
    {
        throw new NotImplementedException();
    }
    //第二次摇色子消息
    int IMGameView.OnSiceTwo()
    {
        throw new NotImplementedException();
    }
    //摇色子结束
    int IMGameView.OnSiceFinish()
    {
        throw new NotImplementedException();
    }
    //玩家操作
    int IMGameView.OnUserAction()
    {
        throw new NotImplementedException();
    }
    //动画完成消息
    int IMGameView.OnMoveCardFinish()
    {
        throw new NotImplementedException();
    }

    #endregion
}
