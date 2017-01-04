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

    //开始
    bool OnStart();
    //时间消息
    void OnTimer();
    //出牌操作
    void OnOutCard();
    //托管控制
    int OnTrusteeControl();
    //第二次摇色子消息
    void OnSiceTwo();
    //发牌
    void OnSendCard();
    //玩家操作
    void OnUserAction(int userID,int supID, UserAction action,params int[] arg);

    //玩家准备
    void OnReady(int userID);

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

    /*辅助*/

    //校验出牌
    bool VerdiotOutCard();
    //是否可出
    bool CheckOutCard();
    //设置手牌控制
    void SetHandCardControl();
    //操作提示
    byte GetSelectCardInfo();
}

public enum UserAction
{
    PutCard,        //出牌
    Chi,            //吃      
    Peng,           //碰
    JiaGang,        //加杠
    MingGang,       //明杠
    AnGang,         //暗杠
    ChiHu,          //吃胡
    Hu,             //胡
    Ting,           //听
    TingCancel,     //取消听
    TuoGuang,       //托管
    AICancel,       //取消托管
}