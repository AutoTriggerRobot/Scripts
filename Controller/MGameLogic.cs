/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 界面反馈
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
    //登入
    void IController.OnLogin()
    {

    }
    //确认设置
    void IController.OnOption()
    {

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
    void IController.SetStatusFlag(int userID, UserAction act, params int[] arg)
    {

        switch(act)
        {
            case UserAction.ready:
                break;
            //arg[0]
            case UserAction.put_card:
                break;
            //arg[0] arg[1] arg[2] or arg[3]
            case UserAction.chi:
            case UserAction.peng:
            case UserAction.ming_gang:
                break;
            //arg[0]
            case UserAction.jia_gang:
                break;
            //arg[0] arg[1] arg[2] arg[3]
            case UserAction.an_gang:
                break;
            //arg[0]
            case UserAction.chi_hu:
            case UserAction.hu:
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
    //状态完成标志
    void IController.EndStatusFlag(int userID, UserAction act, Logic logic, params int[] arg)
    {
        switch(logic)
        {
            //掷色子结束
            case Logic.turn_dice:

                break;
            //发牌结束
            case Logic.add_hand_card_end:
                break;
            //摸牌结束
            case Logic.get_handlecard:
                break;
            default:
                switch(act)
                {
                    //arg[0]
                    case UserAction.put_card:
                        break;
                    case UserAction.insert_card:
                        break;
                    //arg[0] arg[1] arg[2] or arg[3]
                    case UserAction.chi:
                    case UserAction.peng:
                    case UserAction.ming_gang:
                        break;
                    //arg[0]
                    case UserAction.jia_gang:
                        break;
                    //arg[0] arg[1] arg[2] arg[3]
                    case UserAction.an_gang:
                        break;
                    //arg[0]
                    case UserAction.chi_hu:
                    case UserAction.hu:
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
                break;
        }
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