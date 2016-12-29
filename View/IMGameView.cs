/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 视图动作
* 类 名： IMGameView.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMGameView
{
    /*动作*/

    //游戏开始
    bool OnSubGameStart();
    //用户出牌
    bool OnSubOutCard();
    //发牌消息
    bool OnSubSendCard();
    //操作提示
    bool OnSubOperateNotify();
    //操作结果
    bool OnSubOperateResult();
    //游戏结束
    bool OnSubGameEnd();
    //用户托管
    bool OnSubTrustee();
    //用户听牌
    bool OnSubListenCard();
    //补牌消息
    bool OnSubReplaceCard();

    /*辅助*/

    //播放牌声音
    void PlayCardSound();
    //播放动作声音
    void PlayActionSound();
    //校验出牌
    bool VerdiotOutCard();
    //是否可出
    bool CheckOutCard();
    //设置手牌控制
    void SetHandCardControl();
    //操作提示
    byte GetSelectCardInfo();
    //卡牌移动
    bool BeginMoveCard();
    //停止移动
    bool StopMoveCard();
    //出牌移动
    bool BeginMoveOutCard();
    //发牌移动
    bool BeginMoveSendCard();
    //补张动画
    bool BeginMoveReplaceCard();
    //开局发牌动画
    bool BeginMoveStartCard();
    //出牌动画完成
    bool OnMoveOutCardFinish();
    //发牌动画完成
    bool OnMoveSendCardFinish();
    //补花动画完成
    bool OnMoveReplaceCardFinish();
    //开局动画完成
    bool OnMoveStartCardFinish();

    /*其他玩家*/

    //时间消息
    void OnTimer();
    //开始消息
    int OnStart();
    //出牌操作
    int OnOutCard();
    //卡牌动作
    int OnCardOperate();
    //托管控制
    int OnTrusteeControl();
    //买底操作
    int OnChip();
    //第二次摇色子消息
    int OnSiceTwo();
    //摇色子结束
    int OnSiceFinish();
    //玩家操作
    int OnUserAction();
    //动画完成消息
    int OnMoveCardFinish();
}
