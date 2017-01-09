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

    List<byte> cardWare;                                                    //等待抽取的麻将
    Dictionary<byte,int> cardDic;                                           //可能还剩下的未出过的牌 byte牌型 int数量

    List<byte>[] HandCard = new List<byte>[4];                              //手牌  0庄 1右 2对 3左
    List<byte[]>[] PengCard = new List<byte[]>[4];                          //碰牌 吃牌 明杠等 （下面的类似
    Stack<byte>[] AnGang = new Stack<byte>[4];                              //庄家暗杠 （下面的类似
    Stack<byte>[] OutCard = new Stack<byte>[4];                             //庄家出牌区域  (下面类似

    byte handleCard;                                                        //摸起来的牌
    byte outCard;                                                           //刚出的牌
    bool isSorted = false;                                                  //是否已经排序
    int userID;                                                             //当前玩家id

    List<int> cordActType = new List<int>();                                //玩家可操作类型 根据等级存储 吃胡>杠>碰>吃

    //初始化
    public void Reset()
    {
        cardWare = new List<byte>(GlobalData.CardWare);
        for(int i = 0; i < 4; ++i)
        {
            HandCard[i] = new List<byte>();
            PengCard[i] = new List<byte[]>();
            AnGang[i] = new Stack<byte>();
            OutCard[i] = new Stack<byte>();
        }
        for(int i= 0; i < GlobalData.CardType.Length; ++i)
        {
            cardDic.Add(GlobalData.CardType[i], 4);
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
            //排列所有玩家手中麻将
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
        int index = GetInsert(ref HandCard[userID], handleCard);
        return index;
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
            return -1;
        } else
        {
            outCard = HandCard[userID][index];
            HandCard[userID].RemoveAt(index);
            //将handleCard插入手牌中 返回插入位置
            return GetInsert(ref HandCard[userID], handleCard);
        }
    }

    //获取牌中的所有对子
    public List<byte> GetPair(List<byte> Card)
    {
        List<byte> result = new List<byte>();
        //索引
        int index = 0;
        try
        {
            byte temp = Card[index]; //取出第一个  分析开始
            do
            {
                //判断是否相同  是否连续  否则  按照这个顺序一直判断
                if(Card.Count > index + 1 && temp == Card[index + 1])
                {
                    if(result.FindIndex(cd=> cd == temp) == -1)
                    result.Add(temp);
                    ++index;

                    //如果已经迭代完成则退出循环
                    if(index + 1 >= Card.Count)
                        break;

                    ++index;
                    temp = Card[index];
                } else
                {
                    //如果已经迭代完成则退出循环
                    if(index + 1 >= Card.Count)
                        break;

                    ++index;
                    temp = Card[index];
                }
            } while(true);
        } catch
        {
            Debug.LogError("<ProcessorModel::GroupAnalyse02>: out of range, index: " + index + ", range:" + HandCard[userID].Count);
        }

        return result;
    }

    //胡牌判断
    public bool HuPaiAnalyse(List<byte> Card)
    {
        //临时存放组
        List<byte> group0 = new List<byte>(); 
        List<byte> group1 = new List<byte>(); 

        //索引
        int index = 0;
            byte temp = Card[index]; //取出第一个  分析开始
            group0.Add(temp);
        do
        {
            //判断是否相同  是否连续  否则  按照这个顺序一直判断
            if(Card.Count > index + 1 && temp == Card[index + 1])
            {
                //存入分组
                ++index;
                group0.Add(Card[index]);
                temp = Card[index];

                //如果已经存满三个 丢弃
                if(group0.Count == 3)
                {
                    group0.Clear();

                    //如果已经迭代完成则退出循环
                    if(index + 1 >= Card.Count)
                        break;

                    ++index;
                    group0.Add(Card[index]);
                    temp = Card[index];
                }
            } else if(temp < 0x31 && Card.Count > index + 1 && temp + 1 == Card[index + 1])
            {
                //如果group1不为空则表示可以有顺子
                if(group1.Count > 0)
                {
                    //取出group1的一位
                    group1.RemoveAt(0);
                    //取出group0的一位
                    group0.RemoveAt(0);
                    //加上现在判断的一位
                    ++index;

                    //大于零 表示有散牌
                    if(group1.Count > 0)
                    {
                        return false;
                    }

                    //从上一个结果开始从新一轮的判断
                    if(group0.Count > 0)
                    {
                        temp = group0[group0.Count - 1];
                    } else
                    {
                        //如果已经迭代完成则退出循环
                        if(index + 1 >= Card.Count)
                            break;

                        ++index;
                        group0.Add(Card[index]);
                        temp = Card[index];
                    }
                } else
                {
                    //将结果存入另外一个列表
                    group1.AddRange(group0);
                    group0.Clear();

                    ++index;
                    group0.Add(Card[index]);
                    temp = Card[index];
                }
            } else
            {
                //group1 大于2 是111
                if(group1.Count > 0 || group0.Count > 0)
                {
                    return false;
                }

                //如果已经迭代完成则退出循环
                if(index + 1 >= Card.Count)
                    break;

                ++index;
                group0.Add(Card[index]);
                temp = Card[index];
            }
        } while(true);
       return true;
    }

    //胡牌 听牌判断 如果返回数组大于零则听  如果输入的牌型 则返回数组大于零则胡
    public byte[] AnalyseCard(List<byte> handCard, byte card = 0)
    {
        List<byte> result = new List<byte>();
        List<byte> sup = new List<byte>();
        //记录上次移除的对子麻将
        Card cardMem;
        //如果有输入 则按照输入判断
        if(card != 0)
        {
            sup.Add(card);
        } else
        {
            //否则便利字典
            foreach(var item in cardDic)
                sup.Add(item.Key);
        }
        for(int i = 0; i < sup.Count; ++i)
        {
            //排序
            List<byte> temp = new List<byte>(handCard);
            GetInsert(ref temp, sup[i]);
            //获取所有对子
            List<byte> nPair = GetPair(temp);
            if(nPair.Count == temp.Count / 2)
            {
                //七小对
                result.Add(sup[i]);
                continue;
            }
            while(nPair.Count > 0)
            {
                //移除
                cardMem = new Card(temp.FindIndex(cd => cd == nPair[0]), nPair[0]);
                temp.RemoveRange(cardMem.index, 2);
                nPair.RemoveAt(0);
                //判断
                if(HuPaiAnalyse(temp))
                {
                    if(result.FindIndex(cd => cd == sup[i]) == -1)
                        result.Add(sup[i]);
                }
                //加回
                temp.Insert(cardMem.index, cardMem.card);
                temp.Insert(cardMem.index, cardMem.card);
            }
        }

        return result.ToArray();
    }

    //胡牌番数
    public int GetHuActionRank()
    {
        throw new NotImplementedException();
    }

    //明杠 碰分析  一次只可能出现一种杠
    public List<int> AnalyseGangPengCard(int userID, byte card)
    {
        int first = HandCard[userID].FindIndex(cd=> cd == card);
        int last = HandCard[userID].FindLastIndex(cd => cd == card);
        if(last - first > 0)
        {
            List<int> result = new List<int>();
            for(int i = 0; i < last - first; ++i)
            {
                result.Add(first + i);
            }
            return result;
        }
       return null;
    }

    //吃分析 可能有多种组合
    public List<List<int>> AnalysChiCard(int userID,byte card)
    {
        List<List<int>> result = new List<List<int>>();
        List<int> chi = new List<int>();
        //遍历所有牌 找相邻
        for(int i = 0; i < HandCard[userID].Count; ++i)
        {
            //如果存在比判断牌大1位的
           if(card +1 == HandCard[userID][i])
            {
                chi.Add(i);
                int index = i -1;
                //判断后续 如果超出范围继续
                try
                {
                    while(HandCard[userID][++index] == card + 1)
                    { }
                    //如果下一个不相同的牌是连续的 则两张牌存入结果
                    if(HandCard[userID][index] == card + 2)
                        result.Add(new List<int> { i, index });
                } catch { }

            }else if(card -1 == HandCard[userID][i])
            {
                chi.Add(i);
                int index = i + 1;
                //判断后续 如果超出范围继续
                try
                {
                    while(HandCard[userID][--index] == card - 1)
                    { }
                    //如果上一个不相同的牌是连续的 则两张牌存入结果
                    if(HandCard[userID][index] == card - 2)
                        result.Add(new List<int> { i, index });
                } catch { }
            }
        }
        if(chi.Count == 2)
            result.Add(chi);
        return result;
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

//储存手牌中的index 和牌面信息
public struct Card
{
    public int index;
    public byte card;
    public Card(int index,byte card)
    {
        this.index = index;
        this.card = card;
    }

    public static bool operator ==(Card left,Card right)
    {
        return left.card == right.card;
    }

    public static bool operator !=(Card left,Card right)
    {
        return left.card != right.card;
    }

    public static bool operator <(Card left,Card right)
    {
        return left.card < right.card;
    }

    public static bool operator >(Card left,Card right)
    {
        return left.card > right.card;
    }

    public static bool operator <=(Card left,Card right)
    {
        return left.card <= right.card;
    }

    public static bool operator >=(Card left,Card right)
    {
        return left.card >= right.card;
    }

    public override bool Equals(object obj)
    {
        return this.card.Equals(obj);
    }

    public override int GetHashCode()
    {
        return this.GetHashCode();
    }
}