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
    bool OnStart(OptionData.DataBase data);
    //时间消息
    void OnTimer();
    //出牌操作
    void OnOutCard(int userID, int outCardInd,int sortInd);
    //托管控制
    int OnTrusteeControl();
    //第二次摇色子消息
    void OnSiceTwo();
    //发牌
    void OnSendCard(List<int> cards);
    //玩家操作
    void OnUserAction(int userID, UserAction action, params List<int>[] arg);

    //玩家准备
    void OnReady(int userID);

    //操作提示
    bool OnOperateNotify();
    //操作结果
    bool OnOperateResult();
    //游戏结束
    bool OnGameEnd();
    //用户托管
    bool OnTrustee();
    //用户听牌
    bool OnListenCard();

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

//游戏状态
public enum Logic
{
    empty,               //空
    add_group,           //洗牌
    turn_dice,           //掷色子
    add_hand_card,       //发牌
    add_hand_card_end,   //发牌结束
    game_start,          //游戏开始
    get_handlecard,      //摸牌
    out_card,            //出牌
    chi_hu,
    hu,                  //胡
}

//用户状态
public enum UserAction
{
    empty,          //空
    ready,          //准备
    turn_dice,      //掷色子
    send_card,      //发牌
    get_card,       //摸牌
    put_card,       //出牌
    insert_card,    //摸的牌插入手牌
    chi,            //吃      
    peng,           //碰
    add_gang,       //补杠
    ming_gang,      //明杠
    an_gang,        //暗杠
    chi_hu,         //吃胡
    hu,             //胡
    ting,           //听
    ting_cancel,    //取消听
    trustee,        //托管
    AI_cancel,      //取消托管

    ready_flag,     //状态标志
    pass_flag,      //过
    send_card_flag, //发牌标志
    put_card_flag,
    chi_card_flag,
    peng_card_flag,
    add_gang_flag,
    gang_flag,
    an_gang_flag,
    chi_hu_flag,
    hu_flag,
    ting_flag,

    end,            //游戏结束
}