/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 数据处理实体
* 类 名： ProcessorModel.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessorModel: IMGameModel
{
    //	麻将定义
    //	0x01,0x02,0x03,0x04,0x05,0x06,0x07,0x08,0x09,						//万子
    //	0x11,0x12,0x13,0x14,0x15,0x16,0x17,0x18,0x19,						//条子
    //	0x21,0x22,0x23,0x24,0x25,0x26,0x27,0x28,0x29,						//同子
    //	0x31,0x32,0x33,0x34,					            				//风牌 东 南 西 北
    //	0x35,0x36,0x37,                         							//箭牌 中 发 白

    List<byte> cardWare;                                                    //桌面还剩麻将

    List<byte>[] HandCard = new List<byte>[4];                              //手牌  0庄 1右 2对 3左
    List<byte[]>[] PengCard = new List<byte[]>[4];                          //碰牌 吃牌 明杠等 （下面的类似
    Queue<byte>[] AnGang = new Queue<byte>[4];                              //庄家暗杠 （下面的类似
    Queue<byte>[] OutCard = new Queue<byte>[4];                             //庄家出牌区域  (下面类似

    byte handleCard;                                                        //摸起来的牌
    byte outCard;                                                           //刚出的牌
    bool isSorted = false;                                                  //是否已经排序
    int userID;                                                             //当前玩家id

    //初始化
    public void Reset()
    {
        cardWare = new List<byte>(GlobalData.CardWare);
        for(int i = 0; i < 4; ++i)
        {
            HandCard[i] = new List<byte>();
            PengCard[i] = new List<byte[]>();
            AnGang[i] = new Queue<byte>();
            OutCard[i] = new Queue<byte>();
        }
        isSorted = false;
        userID = 0;
    }

    //洗牌
    public List<byte> RandCardData()
    {
        Reset();
        int i = GlobalData.MAHJONG_WARE_MAX;
        byte temp;
        int indexA;
        int indexB;

        while(i > 0)
        {
            indexA = Mathf.FloorToInt(UnityEngine.Random.value * i);
            indexB = --i;
            if(indexA == indexB)
                continue;
            temp = cardWare[indexA];
            cardWare[indexA] = cardWare[indexB];
            cardWare[indexB] = temp;
        }
        return cardWare;
    }

    //发牌
    public List<byte>[] SendCard()
    {
        //发三轮
        for(int i = 0; i < 3; ++i)
            //每轮四个玩家
            for(int j = 0; j < 4; ++j)
                //每个玩家每次四张
                for(int k = 0; k < 4; ++k)
                {
                    HandCard[j].Add(RemoveCard());
                }

        //补发一轮
        for(int i = 0; i < 4; ++i)
        {
            HandCard[i].Add(RemoveCard());
        }

        return HandCard;
    }

    //有效判断
    public bool IsValidCard()
    {
        throw new NotImplementedException();
    }

    //更新牌库并取出牌  默认取最后一个
    byte RemoveCard(int index = -1)
    {
        if(index < 0)
            index = cardWare.Count - 1;
        byte card = cardWare[index];
        cardWare.RemoveAt(index);
        return card;
    }

    //麻将数目
    public int GetCardCount()
    {
        return cardWare.Count;
    }

    //排列麻将
    public List<byte> GetWeaveCard(int userID)
    {
        if(isSorted)
            return HandCard[userID];
        else
        {
            for(int i = 0; i < 4; ++i)
            {
                SelectSort(ref HandCard[i]);
            }
            isSorted = true;
            return HandCard[userID];
        }
    }

    //摸牌
    public byte GetUserCard(int index = -1)
    {
        handleCard = RemoveCard(index);
        return handleCard;
    }

    //插入手牌
    public int InsertCard(int userID)
    {
        return GetInsert(ref HandCard[userID], handleCard);
    }

    /// <summary>
    /// 出牌 某用户出了手中第几张牌    
    /// </summary>
    /// <param name="userID"></param>
    /// <param name="index">用户出了手中第几张牌 如果是handlecard index = -1</param>
    /// <returns>返回插入index  如果不用插入则放回 -1</returns>
    public int UserOutCard(int userID,int index)
    {
        if(index < 0)
        {
            //出的是handlecard
            outCard = handleCard;
            //出牌分析
            AnalyseCard(outCard);
            return -1;
        } else
        {
            outCard = HandCard[userID][index];
            HandCard[userID].RemoveAt(index);
            AnalyseCard(outCard);
            //将handleCard插入手牌中 返回插入位置
            return GetInsert(ref HandCard[userID], handleCard);
        }
    }

    //出入牌分析
    public void AnalyseCard(byte card)
    {
        //胡>杠>碰>吃 按动作等级回馈
    }

    //动作等级
    public byte GetUserActionRank()
    {
        throw new NotImplementedException();
    }
    //胡牌等级
    public byte GetHuActionRank()
    {
        throw new NotImplementedException();
    }

    //吃牌判断
    public byte EstimateEatCard()
    {
        throw new NotImplementedException();
    }
    //碰牌判断
    public byte EstimatePengCard()
    {
        throw new NotImplementedException();
    }
    //杠牌判断
    public byte EstimateGangCard()
    {
        throw new NotImplementedException();
    }

    //杠分析
    public byte AnalyseGangCard()
    {
        throw new NotImplementedException();
    }
    //吃胡分析
    public byte AnalyseChiHuCard()
    {
        throw new NotImplementedException();
    }
    //听牌分析
    public byte AnalyseTingCard()
    {
        throw new NotImplementedException();
    }

    /*转换*/
    //转牌面意思
    public byte SwitchToCardData()
    {
        throw new NotImplementedException();
    }
    //转index
    public byte SwitchToIndex()
    {
        throw new NotImplementedException();
    }

    /*胡牌分析*/
    //平胡
    public bool IsPingHu()
    {
        throw new NotImplementedException();
    }
    //碰碰胡
    public bool IsPengPengHu()
    {
        throw new NotImplementedException();
    }
    //七对
    public bool IsDaQiDui()
    {
        throw new NotImplementedException();
    }
    //清一色
    public bool IsQingYiSe()
    {
        throw new NotImplementedException();
    }
    //烂胡
    public bool IsLangHu()
    {
        throw new NotImplementedException();
    }
    //七星烂胡
    public bool IsQiXingLangHu()
    {
        throw new NotImplementedException();
    }
    //字一色
    public bool IsZiYiSe()
    {
        throw new NotImplementedException();
    }

    //排序 
    public void SelectSort(ref List<byte> list)
    {
        byte temp;
        byte MinValue;
        int MinValueIndex;
        for(int i = 0; i < list.Count; i++)
        {
            MinValue = list[i];
            MinValueIndex = i;
            for(int j = i; j < list.Count; j++)
            {
                if(MinValue > list[j])
                {
                    MinValue = list[j];
                    MinValueIndex = j;
                }
            }
            temp = list[i];
            list[i] = list[MinValueIndex];
            list[MinValueIndex] = temp;
        }
    }

    //插入
    public int GetInsert(ref List<byte> list, byte item)
    {
        int index = list.FindIndex(pre => pre > item);
        if(index >= 0)
            list.Insert(index, item);
        else
        {
            index = list.Count;
            list.Add(item);
        }
        return index;
    }
}
