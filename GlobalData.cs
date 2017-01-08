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
    //服务器连接提示
    public const string NET_MSG_Fail = "服务器连接失败...";
    public const string NET_MSG_Error = "连接错误.";
    public const string NET_MSG_Try = "正在尝试重新连接...";


    //牌桌颜色
    public const int SET_MAHJONG_0 = 0;


    //牌颜色
    public const int SET_MJ_CARD_0 = 0;


    //胡牌类型
    public const int MAHJONG_None = -1;
    public const int MAHJONG_PingHu = 0;

    //总牌数
    public const int MAHJONG_WARE_MAX = 108;

    //各个组的容量
    public const int HANDLECARD_Count = 1;    //手中的牌
    public const int HANDCARD_Count = 14;     //手牌
    public const int MING_GANG_Count = 10;     //明杠
    public const int AN_GANG_Count = 5;       //暗杠
    public const int OUT_CARD_Count = 136;     //出牌记录
    public const int SPACIAL_CARD_Count = 4;  //功能牌
    public const int GROUP_Count = 34;        //每列牌墩容量

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

    //	麻将定义
    //	0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,						//万子
    //	0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,						//条子
    //	0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,						//同子
    //	0x31,0x32,0x33,0x34,					            				//风牌 东 南 西 北
    //	0x35,0x36,0x37,                         							//箭牌 中 发 白
    public readonly static byte[] CardWare =
    {
        0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
        0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
        0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
        0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,
        0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,
        0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,
        0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,
        0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,
        0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,
        0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,
        0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,
        0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,
        0x31,0x32,0x33,0x34,
        0x31,0x32,0x33,0x34,
        0x31,0x32,0x33,0x34,
        0x31,0x32,0x33,0x34,
        0x35,0x36,0x37,
        0x35,0x36,0x37,
        0x35,0x36,0x37,
        0x35,0x36,0x37,
    };

}

//  麻将动作类型
public enum MahjongAct
{
    Null,
    Hu,
    ChiHu,
    MingGang,
    AnGang,
    JiaGang,
    Peng,
    Chi,
    Ting,

    //胡牌类型
    PingHu,
    PengPengHu,
    QiDui,
    QingYiSe,
    LangHu,
    QiXingLangHu,
    ZiYiSe,
}
