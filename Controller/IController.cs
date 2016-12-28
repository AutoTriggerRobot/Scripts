/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 事件处理判断
* 类 名： IController.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IController
{
    //登入
    byte OnLogin();
    //确认设置
    byte OnOption();
    //基础积分
    void SetCellScore();
    //庄家
    void SetBankerUser();
    //状态标志
    void SetStatusFlag();
    //出牌信息
    void SetOutCardInfo();
    //动作信息
    void SetUserAction();
    //设置动作特效
    bool SetBombEffect();
    //设置玩家
    void SetDiscUser();
    //设置当前玩家
    void SetCurrentUser();
    //设置托管
    void SetTrustee();
    //听牌标志
    void SetUserListenStatus();
    //是否动画中
    bool IsMoveCard();
    //允许动画
    void EnableAnimate();
    //是否允许动画
    bool IsEnableAnimate();
    //麻将动画
    void OnMoveVardItem();
    //设置特殊牌
    void SetSpecialCard();

}
