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
    public const int HANDCARD_Len = 14;     //手牌
    public const int MING_GANG_Len = 5;     //明杠
    public const int AN_GANG_Len = 5;       //暗杠
    public const int OUT_CARD_Len = 34;     //出牌记录
    public const int SPACIAL_CARD_Len = 4;  //功能牌
    public const int GROUP_Len = 34;        //牌库

    //各个预设占用的宽度
    public const float MAHJONG_Width = 0.0565f;     //普通表示当个麻将的宽度 
    public const float MAHJONG_Thickness = 0.0347f; //麻将厚度
    public const float MAHJONG_High = 0.0757f;      //麻将高度
    public const float AN_GANG = .2f;               //暗杠
    public const float MING_GANG = .25f;            //明杠
    public const float PENG_CHI = .21f;             //碰吃
}
