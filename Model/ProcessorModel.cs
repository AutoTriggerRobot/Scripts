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

    /*
    //记录牌型 从内到外 
    //Card为分组的牌  储存手牌中的index 和牌面信息
    //list<byte[]>为分组的牌(如111,123,11) 
    //list<list<byte[]>>能分组的牌的集合 
    //list<list<byte[]>>[]玩家id
    */
    List<List<Card>>[] nSequence = new List<List<Card>>[4];             
    List<List<Card>>[] nPieces = new List<List<Card>>[4];               //散牌
    List<int> cordActType = new List<int>();                                //玩家可操作类型 根据等级存储 碰 胡 吃 杠

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
            //排列所有玩家手中麻将
            for(int i = 0; i < 4; ++i)
            {
                SelectSort(ref HandCard[i]);
                //排序完后分组
                GroupAnalyse(i, HandCard[i]);
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
        //重新分组
        GroupAnalyse(userID, HandCard[userID]);
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

    //出入牌分析
    public int AnalyseCard(int userID, byte card)
    {
        //杠>碰>吃>胡>没操作   判断顺序  
    }

    //手牌分组 判断顺序 是否是111 否则是否是11 否则是否是 123  循环判断
    public void GroupAnalyse(int userID,List<byte> handCard)
    {
        //保证排数为3n+1张
        if(handCard.Count%3 == 1)
        {
            nSequence[userID].Clear();
            nPieces[userID].Clear();
            //临时存放组
            List<Card> group = new List<Card>();
            //临时存放散牌
            List<Card> piec = new List<Card>();
            //索引
            int index = 0;
            try
            {
                byte temp = handCard[index]; //取出第一个  分析开始
                do
                {
                    //对比下一个 判断111 否则 11
                    if(handCard.Count > index + 1 && temp == handCard[index+1])
                    {
                        //存入分组
                        group.Add(new Card(index,temp));
                        ++index;
                        group.Add(new Card(index, handCard[index]));
                        //再判断下一个是 否相等
                        if(handCard.Count > index + 1 && temp == handCard[index + 1])
                        {
                            //继续存入
                            ++index;
                            group.Add(new Card(index, handCard[index]));
                        }
                        //将分组存入牌型
                        nSequence[userID].Add(group);
                        //清空组
                        group.Clear();
                        //如果已经迭代完成则退出循环
                        if(index + 1 == handCard.Count)
                            break;
                        //准备下一次判断
                        temp = handCard[++index];
                    } else if(handCard.Count > index + 1 && temp + 1 == handCard[index + 1])
                    {
                        //如果两个连续
                        //暂时存入分组
                        group.Add(new Card(index, temp));
                        ++index;
                        group.Add(new Card(index, handCard[index]));
                        //再判断下一个 是否是123
                        if(handCard.Count > index + 1 && temp + 2 == handCard[index + 1])
                        {
                            //如果是继续存入
                            ++index;
                            group.Add(new Card(index, handCard[index]));
                            //将分组存入牌型
                            nSequence[userID].Add(group);
                            //清空组
                            group.Clear();
                            //如果已经迭代完成则退出循环
                            if(index + 1 == handCard.Count)
                                break;
                            //准备下一次判断
                            temp = handCard[++index];
                        } else
                        {
                            //如果不能组成顺子 则将第一个加入散牌 从第二个开始迭代新判断
                            piec.Add(group[0]);
                            //准备下一次判断
                            temp = group[1].card;
                            group.Clear();
                        }
                    } else
                    {
                        //加入散牌
                        piec.Add(new Card(index, temp));
                        //如果已经迭代完成则退出循环
                        if(index +1 == handCard.Count)
                            break;
                        //准备下一次判断
                        temp = handCard[++index];
                    }
                } while(handCard.Count > index +1);
                //散牌分析 相邻的组一组
                for(int i = 0; i < piec.Count; ++i)
                {
                    if(piec[i] == piec[i + 1])
                    {
                        //存入散牌并跳过下一个判断
                        nPieces[userID].Add(new List<Card>() { piec[i], piec[++i] });
                    } else
                    {
                        //存入散牌
                        nPieces[userID].Add(new List<Card>() { piec[i] });
                    }
                }
            } catch
            {
                Debug.LogError("<ProcessorModel::GroupAnalyse>: out of range, index: " + index + ", range:" + HandCard[userID].Count);
            }
        }
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

    //明杠分析  一次只可能出现一种杠
    public List<int> AnalyseGangCard(int userID, byte card)
    {
        for(int i = 0; i < nSequence[userID].Count; ++i)
        {
            //找出三个相等的组
            if(nSequence[userID][i].Count == 3 &&　nSequence[userID][i][0] == nSequence[userID][i][1])
            {
                //查看值是否相等
                if(nSequence[userID][i][0].Equals(card))
                {
                    //返回手牌中的索引
                    return new List<int> { nSequence[userID][i][0].index, nSequence[userID][i][1].index, nSequence[userID][i][2].index };
                }
            }
        }
        return null;
    }

    //碰分析 只有一种结果
    public List<int> AnalysPengCard(int userID, byte card)
    {
        for(int i = 0; i < nSequence[userID].Count; ++i)
        {
            //找出2个相等的组
            if(nSequence[userID][i].Count == 2 && nSequence[userID][i][0] == nSequence[userID][i][1])
            {
                //查看值是否相等
                if(nSequence[userID][i][0].Equals(card))
                {
                    //返回手牌中的索引
                    return new List<int> { nSequence[userID][i][0].index, nSequence[userID][i][1].index};
                }
            }
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

    //听牌分析  返回能胡的牌型
    public List<byte> AnalyseTingCard(int userID)
    {
        List<byte> result = new List<byte>();
        //只有散牌数量少于3的情况下才可能听
        if(nPieces[userID].Count < 3)
        {
            //先判断散牌 
            //如果为两张 则必须要相间隔
            if(nPieces[userID].Count == 2)
            {
                //如果其中有一个个数大于1则不会听
                if(nPieces[userID][0].Count >1 || nPieces[userID][1].Count > 1)
                {
                    return null;
                } else
                {
                    //否则判断是否是间隔 不是间隔则不会听
                    if(nPieces[userID][0][0].card != nPieces[userID][1][0].card + 2)
                    {
                        return null;
                    } else
                    {
                        //开始分析成组的牌 将中间的间隔牌加入可能胡的列表
                        result.Add((byte)(nPieces[userID][0][0].card + 1));
                    }
                }
            }else if(nPieces[userID].Count == 1)
            {
                //待完成
            }
        } else
        {
            return null;
        }
        return result;
    }

    //胡分析
    public bool AnalyseHuCard()
    {
        
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