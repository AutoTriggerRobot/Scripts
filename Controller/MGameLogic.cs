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
    IMGameModel proce;
    IMGameView view;

    public MGameLogic(IMGameView game)
    {
        proce = new ProcessorModel(this);
        view = game;
    }

    //登入
    void IController.OnLogin(string name, string password)
    {
        
    }
    //确认设置
    void IController.OnOption(OptionData.DataBase data)
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
    void IController.SetStatusFlag(int userID, UserAction act, params List<int>[] arg)
    {

        switch(act)
        {
            case UserAction.ready:
                break;
            //出牌 arg[0]出牌id arg[1]插入id 如果为-1则忽略
            case UserAction.put_card:
                view.OnUserAction(userID, -1, act, arg[0][0], arg[0][1]);
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

            case UserAction.ready_flag:
                break;
            //发牌标志
            case UserAction.send_card_flag:
                //发牌 arg[0]当前用户手牌  arg[1][0]当前用户如果是庄家则这是摸牌
                view.OnSendCard(arg[0],arg[1][0]);
                break;
            case UserAction.get_card_flag:
                break;
            case UserAction.put_card_flag:
                break;
            case UserAction.chi_card_flag:
                break;
            case UserAction.peng_card_flag:
                break;
            case UserAction.jia_gang_flag:
                break;
            case UserAction.gang_flag:
                break;
            case UserAction.an_gang_flag:
                break;
            case UserAction.chi_hu_flag:
                break;
            case UserAction.hu_flag:
                break;
            case UserAction.ting_flag:
                break;
            
        }
        proce.SetStatusFlag(userID, act, arg);
    }
    //状态完成标志
    void IController.EndStatusFlag(int userID, UserAction act, Logic logic, params List<int>[] arg)
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
        proce.EndStatusFlag(userID, act, arg);
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