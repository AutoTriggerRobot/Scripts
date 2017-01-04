/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 存储游戏数据常量
* 类 名： GlobalData.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData
{

    //牌桌颜色
    public const int SET_MAHJONG_0 = 0;


    //牌颜色
    public const int SET_MJ_CARD_0 = 0;


    //胡牌类型
    public const int MAHJONG_None = -1;
    public const int MAHJONG_PingHu = 0;

    //各个组的容量
    public const int HANDLECARD_Count = 1;    //手中的牌
    public const int HANDCARD_Count = 14;     //手牌
    public const int MING_GANG_Count = 10;     //明杠
    public const int AN_GANG_Count = 5;       //暗杠
    public const int OUT_CARD_Count = 136;     //出牌记录
    public const int SPACIAL_CARD_Count = 4;  //功能牌
    public const int GROUP_Count = 34;        //牌库

    //各个预设占用的宽度
    public const float MAHJONG_Width = 0.0565f;           //普通表示当个麻将的宽度 
    public const float MAHJONG_Thickness = 0.0347f;       //麻将厚度
    public const float MAHJONG_High = 0.0757f;            //麻将高度
    public const float AN_GANG_Width = .2f;               //暗杠
    public const float MING_GANG_Width = .25f;            //明杠
    public const float PENG_Width = .21f;                 //碰
    public const float CHI_Width = .18f;                  //吃


    //麻将动画名
    public static int ANIMA_GetCard = Animator.StringToHash("Base Layer.GetCard");
    public static int ANIMA_OutCard = Animator.StringToHash("Base Layer.OutCard");
    public static int ANIMA_TurnOverCard = Animator.StringToHash("Base Layer.TurnOverCard");
    public static int ANIMA_InsertCard = Animator.StringToHash("Base Layer.InsertCard");
    public static int ANIMA_CardIdle = Animator.StringToHash("Base Layer.CardIdle");
    public static int ANIMA_CloseCard = Animator.StringToHash("Base Layer.CloseCard");
    public static int ANIMA_ChiPengCard = Animator.StringToHash("Base Layer.ChiPengCard");
    public static int ANIMA_OnTriggerEnter = Animator.StringToHash("Base Layer.OnTriggerEnter");
    public static int ANIMA_OnTriggerExit = Animator.StringToHash("Base Layer.OnTriggerExit");
}
