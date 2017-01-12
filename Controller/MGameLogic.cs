/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 界面反馈使用协程传递
* 类 名： MGameLogic.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGameLogic: IController
{
    MonoBehaviour main;
    IMGameModel proce;
    IMGameView view;

    int supID = -1;

    IEnumerator SetStatus(int userID, UserAction act, params List<int>[] arg)
    {
        view.OnUserAction(userID, act, arg);
        yield return 0;
    }

    IEnumerator EndStatus(int userID,UserAction act, params List<int>[] arg)
    {
        proce.EndStatusFlag(userID, act, arg);
        yield return 0;
    }

    public MGameLogic(MGameClient game)
    {
        proce = new ProcessorModel(this);
        view = game;
        main = game;
    }

    //登入
    void IController.OnLogin(string name, string password)
    {
        
    }
    //确认设置
    void IController.OnOption(OptionData.DataBase data)
    {
        view.OnStart(data);
    }
    //基础积分
    void IController.SetCellScore()
    {

    }

    //庄家
    void IController.SetBankerUser()
    {

    }

    //状态设置标志
    void IController.SetStatusFlag(int userID, UserAction act, params List<int>[] arg)
    {
        main.StartCoroutine(SetStatus(userID, act, arg));
    }
    //状态完成标志
    void IController.EndStatusFlag(int userID, UserAction act, params List<int>[] arg)
    {
        main.StartCoroutine(EndStatus(userID, act, arg));   
    }

    //设置玩家
    void IController.SetDiscUser()
    {

    }
    //设置当前玩家
    void IController.SetCurrentUser()
    {

    }
    //设置特殊牌
    void IController.SetSpecialCard()
    {

    }
}