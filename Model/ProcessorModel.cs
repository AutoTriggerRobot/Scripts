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

public class ProcessorModel:IMGameModel
{
    //	麻将定义
    //	01,02,03,04,05,06,07,08,09,						//万子
    //	11,12,13,14,15,16,17,18,19,						//条子
    //	21,22,23,24,25,26,27,28,29,						//同子
    //	31,32,33,34,					            				//风牌 东 南 西 北
    //	35,36,37,                         							//箭牌 中 发 白

    List<int> cardWare;                                                    //等待抽取的麻将
    Dictionary<int,int> cardDic = new Dictionary<int, int>();                                           //可能还剩下的未出过的牌 byte牌型 int数量

    List<int>[] HandCard = new List<int>[4];                              //手牌  0庄 1右 2对 3左
    List<int[]>[] PengCard = new List<int[]>[4];                          //碰牌 吃牌 明杠等 （下面的类似
    Stack<int>[] AnGang = new Stack<int>[4];                              //庄家暗杠 （下面的类似
    Stack<int>[] OutCard = new Stack<int>[4];                             //庄家出牌区域  (下面类似

    int handleCard;                                                        //摸起来的牌
    int outCard;                                                           //刚出的牌
    bool isSorted = false;                                                 //是否已经排序
    int userID;                                                            //当前玩家id

    List<int> cordActType = new List<int>();                               //玩家可操作类型 根据等级存储 吃胡>杠>碰>吃
    User[] players = new User[4];                                          //当轮玩家座位
    int turnID = 0;                                                    //轮到哪个玩家  0表示庄家  从庄家逆时针轮流



    //单机测试  单机用户信息
    OptionData.DataBase userData;

    IController control;

    public ProcessorModel(IController contro)
    {
        control = contro;
    }

    public ProcessorModel() { }

    //游戏开始用户配置（测试）
    public OptionData.DataBase GetOption()
    {
        OptionData.DataBase optionData = new OptionData.DataBase();
        optionData.card_direction = 0;
        optionData.dice_num[0] = UnityEngine.Random.Range(1, 7);
        optionData.dice_num[1] = UnityEngine.Random.Range(1, 7);
        optionData.player_priority = UnityEngine.Random.Range(0, 4);
        for(int i = 0; i < players.Length; ++i)
            players[i] = new User(i);
        return optionData;
    }

    //出牌决策（测试）
    public void WaitOrPush()
    {
        //如果该用户为自动出牌则不等待
        if(players[turnID].ID < 0 && players[turnID].TrusteeFlag && players[turnID].TingFlag)
        {
            int indexOut = UnityEngine.Random.Range(0, HandCard[turnID].Count);
            //用户随机出牌
            //并将摸的牌插入手牌
            int indexIn = UserOutCard(turnID, indexOut);
            UpdateCardLib();
            control.SetStatusFlag(turnID, UserAction.put_card, new List<int>() { indexOut, indexIn });
        }
    }

    //将出过的牌从牌字典删除
    public void UpdateCardLib()
    {
        try
        {
            --cardDic[outCard];
            if(cardDic[outCard] == 0)
            {
                cardDic.Remove(outCard);
            }
        } catch(Exception)
        {
            Debug.Log("<ProcessorModel::UpdateCardLib>: 出牌错误.");
        }
    }

    //初始化
    public void Reset()
    {
        cardWare = new List<int>(GlobalData.CardWare);
        for(int i = 0; i < 4; ++i)
        {
            HandCard[i] = new List<int>();
            PengCard[i] = new List<int[]>();
            AnGang[i] = new Stack<int>();
            OutCard[i] = new Stack<int>();
        }
        for(int i = 0; i < GlobalData.CardType.Length; ++i)
        {
            cardDic.Add(GlobalData.CardType[i], 4);
        }
        isSorted = false;
        userID = 0;
        turnID = 0;
    }

    //洗牌
    public List<int> RandCardData()
    {
        Reset();
        int i = GlobalData.MAHJONG_WARE_MAX;
        int temp;
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
    public List<int>[] SendCard()
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
    int RemoveCard(int index = -1)
    {
        if(index < 0)
            index = cardWare.Count - 1;
        int card = cardWare[index];
        cardWare.RemoveAt(index);
        return card;
    }

    //麻将数目
    public int GetCardCount()
    {
        return cardWare.Count;
    }

    //排列麻将
    public List<int> GetWeaveCard(int userID)
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
    public int GetUserCard(int index = -1)
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
    public int UserOutCard(int userID, int index)
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
    public List<int> GetPair(List<int> Card)
    {
        List<int> result = new List<int>();
        //索引
        int index = 0;
        try
        {
            int temp = Card[index]; //取出第一个  分析开始
            do
            {
                //判断是否相同  是否连续  否则  按照这个顺序一直判断
                if(Card.Count > index + 1 && temp == Card[index + 1])
                {
                    if(result.FindIndex(cd => cd == temp) == -1)
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
    public bool HuPaiAnalyse(List<int> Card)
    {
        //临时存放组
        List<int> group0 = new List<int>();
        List<int> group1 = new List<int>();

        //索引
        int index = 0;
        int temp = Card[index]; //取出第一个  分析开始
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
            } else if(temp < 31 && Card.Count > index + 1 && temp + 1 == Card[index + 1])
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
    public int[] AnalyseCard(List<int> handCard, int card = 0)
    {
        List<int> result = new List<int>();
        List<int> sup = new List<int>();
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
            List<int> temp = new List<int>(handCard);
            GetInsert(ref temp, sup[i]);
            //获取所有对子
            List<int> nPair = GetPair(temp);
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
    public List<int> AnalyseGangPengCard(int userID, int card)
    {
        int first = HandCard[userID].FindIndex(cd => cd == card);
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
    public List<List<int>> AnalysChiCard(int userID, int card)
    {
        List<List<int>> result = new List<List<int>>();
        List<int> chi = new List<int>();
        //遍历所有牌 找相邻
        for(int i = 0; i < HandCard[userID].Count; ++i)
        {
            //如果存在比判断牌大1位的
            if(card + 1 == HandCard[userID][i])
            {
                chi.Add(i);
                int index = i - 1;
                //判断后续 如果超出范围继续
                try
                {
                    while(HandCard[userID][++index] == card + 1)
                    { }
                    //如果下一个不相同的牌是连续的 则两张牌存入结果
                    if(HandCard[userID][index] == card + 2)
                        result.Add(new List<int> { i, index });
                } catch { }

            } else if(card - 1 == HandCard[userID][i])
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
    public void SelectSort(ref List<int> list)
    {
        int temp;
        int MinValue;
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
    public int GetInsert(ref List<int> list, int item)
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

    #region Model

    public void ValidLogin(string name, string password)
    {
        throw new NotImplementedException();
    }

    public void UserOperate(int ID)
    {
        throw new NotImplementedException();
    }

    public void SetStatusFlag(int userID, UserAction act, params List<int>[] arg)
    {
        switch(act)
        {
            case UserAction.ready:
                //初始化
                Reset();
                //洗牌
                RandCardData();
                //发牌
                SendCard();
                //发送配置
                userData = GetOption();
                control.OnOption(userData);
                //单机（测试）
                players[userData.player_priority] = new User(userData.player_priority, userID);
                break;
            //arg[0]
            case UserAction.put_card:
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
    }

    public void EndStatusFlag(int userID, UserAction act,params List<int>[] arg)
    {
        switch(act)
        {
            //掷色子结束
            case UserAction.turn_dice:
                //界面开始发牌
                List<int> handleCardList = new List<int>();
                //如果是庄家 则发送手牌
                if(userID == 0)
                    handleCardList.Add(this.handleCard);
                else
                    handleCardList.Add(-1); //置空
                control.SetStatusFlag(userID, UserAction.send_card_flag, HandCard[userID], handleCardList);
                break;
            //发牌结束
            case UserAction.send_card:
                //判断天胡
                int[] result = AnalyseCard(HandCard[0], handleCard);
                 if(result.Length > 0 && result[0] == handleCard)
                {
                    //天胡
                    control.SetStatusFlag(0, UserAction.hu);
                } else
                {
                    //等待判断
                    WaitOrPush();
                }
                break;
            case UserAction.put_card:
                //出牌判断
                //下一个玩家判断                                                     ----20170110
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
    }

    #endregion
}

//储存手牌中的index 和牌面信息
public struct Card
{
    public int index;
    public int card;
    public Card(int index, int card)
    {
        this.index = index;
        this.card = card;
    }

    public static bool operator ==(Card left, Card right)
    {
        return left.card == right.card;
    }

    public static bool operator !=(Card left, Card right)
    {
        return left.card != right.card;
    }

    public static bool operator <(Card left, Card right)
    {
        return left.card < right.card;
    }

    public static bool operator >(Card left, Card right)
    {
        return left.card > right.card;
    }

    public static bool operator <=(Card left, Card right)
    {
        return left.card <= right.card;
    }

    public static bool operator >=(Card left, Card right)
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

//用户
public struct User
{
    public int ID;              //唯一id  -1表示智能
    public int TurnID;          //座位id
    public bool TrusteeFlag;    //托管标志
    public bool TingFlag;       //听牌标志

    public User(int turnId,int id = -1)
    {
        this.TurnID = turnId;
        this.ID = id;
        TrusteeFlag = false;
        TingFlag = false;
    }
}