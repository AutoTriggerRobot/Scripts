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
    //登入 登录计时  超时登录失败
    void OnLogin(string name, string password);
    //确认设置
    void OnOption(OptionData.DataBase data);
    //基础积分
    void SetCellScore();
    //庄家
    void SetBankerUser();
    //状态标志
    void SetStatusFlag(int userID, UserAction act, params List<int>[] arg);
    void EndStatusFlag(int userID, UserAction act, Logic logic = Logic.empty, params List<int>[] arg);

    //设置玩家
    void SetDiscUser();
    //设置当前玩家
    void SetCurrentUser();
    //设置特殊牌
    void SetSpecialCard();

}