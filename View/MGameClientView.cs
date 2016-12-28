/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 加装和绑定视图中的obj
* 类 名： MGameClienView.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MGameClientView
{
    //麻将位置管理根
    public GameObject MahjongGroup;
    //主角区域
    public MahjongGrooves Host;
    //对家区域
    public MahjongGrooves Front;
    //右家区域
    public MahjongGrooves Right;
    //左家区域
    public MahjongGrooves Left;

    #region 交互UI

    #endregion

    public MGameClientView()
    {
        MahjongGroup = GameObject.FindWithTag("MahjongGrooves");
        Host = new MahjongGrooves(MahjongGroup.transform.Find("Host"));
        Front = new MahjongGrooves(MahjongGroup.transform.Find("Front"));
        Right = new MahjongGrooves(MahjongGroup.transform.Find("Right"));
        Left = new MahjongGrooves(MahjongGroup.transform.Find("Left"));
    }
}

public struct MahjongGrooves
{
    public Transform handleCard;    //刚摸到的牌的位置
    public Transform handCard;      //手牌
    public Transform mingGangPoint; //明杠位置
    public Transform anGangPoint;   //暗杠位置
    public Transform outCardPoint;  //出牌位置
    public Transform spacialCard;   //特殊牌摆放位置
    public Transform group;         //牌库

    public MahjongGrooves(Transform tran)
    {
        handCard = tran.Find("HandCard");
        handleCard = tran.Find("HandleCard");
        mingGangPoint = tran.Find("MingGangPoint");
        anGangPoint = tran.Find("AnGangPoint");
        outCardPoint = tran.Find("OutCardPoint");
        spacialCard = tran.Find("SpacialCard");
        group = tran.Find("Group");
    }
}