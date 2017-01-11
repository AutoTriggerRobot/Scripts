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
    public readonly static int[] CardWare =
    {
        01,02,03,04,05,06,07,08,09,
        01,02,03,04,05,06,07,08,09,
        01,02,03,04,05,06,07,08,09,
        01,02,03,04,05,06,07,08,09,
        11,12,13,14,15,16,17,18,19,
        11,12,13,14,15,16,17,18,19,
        11,12,13,14,15,16,17,18,19,
        11,12,13,14,15,16,17,18,19,
        21,22,23,24,25,26,27,28,29,
        21,22,23,24,25,26,27,28,29,
        21,22,23,24,25,26,27,28,29,
        21,22,23,24,25,26,27,28,29,
        31,32,33,34,
        31,32,33,34,
        31,32,33,34,
        31,32,33,34,
        35,36,37,
        35,36,37,
        35,36,37,
        35,36,37,
    };
    //牌型
    public readonly static byte[] CardType =
    {
        01,02,03,04,05,06,07,08,09,                       //万子
        11,12,13,14,15,16,17,18,19,                       //条子
        21,22,23,24,25,26,27,28,29,                       //同子
        31,32,33,34,35,36,37,                             //东 南 西 北 中 发 白
        //41,42,43,44,45,46,47,48,                          //梅 兰 竹 菊 春 夏 秋 冬
    };

    //牌型对应的模型名称
    public readonly static string[] MashType=
    {
        "Mahjong_1W","Mahjong_2W","Mahjong_3W","Mahjong_4W","Mahjong_5W","Mahjong_6W","Mahjong_7W","Mahjong_8W","Mahjong_9W",
        "Mahjong_1S","Mahjong_2S","Mahjong_3S","Mahjong_4S","Mahjong_5S","Mahjong_6S","Mahjong_7S","Mahjong_8S","Mahjong_9S",
        "Mahjong_1T","Mahjong_2T","Mahjong_3T","Mahjong_4T","Mahjong_5T","Mahjong_6T","Mahjong_7T","Mahjong_8T","Mahjong_9T",
        "Mahjong_D","Mahjong_N","Mahjong_X","Mahjong_B","Mahjong_HZ","Mahjong_FC","Mahjong_BB",
        //"Mahjong_MH","Mahjong_LH","Mahjong_ZH","Mahjong_JH","Mahjong_CH","Mahjong_XH","Mahjong_QH","Mahjong_DH",
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
