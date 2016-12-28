/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 数据处理
* 类 名： IMGameModel.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMGameModel
{
    //洗牌
    byte[] RandCardData();
    //更新牌库
    bool RemoveCard();

    //有效判断
    bool IsValidCard();
    //麻将数目
    byte GetCardCount();
    //排列麻将
    byte GetWeaveCard();

    //动作等级
    byte GetUserActionRank();
    //胡牌等级
    byte GetHuActionRank();

    //吃牌判断
    byte EstimateEatCard();
    //碰牌判断
    byte EstimatePengCard();
    //杠牌判断
    byte EstimateGangCard();

    //杠分析
    byte AnalyseGangCard();
    //吃胡分析
    byte AnalyseChiHuCard();
    //听牌分析
    byte AnalyseTingCard();

    /*转换*/
    //转牌面意思
    byte SwitchToCardData();
    //转index
    byte SwitchToIndex();

    /*胡牌分析*/
    //平胡
    bool IsPingHu();
    //大七对
    bool IsDaQiDui();
    //小七对
    bool IsXiaoQiDui();
    //烂胡
    bool IsLangHu();
}
