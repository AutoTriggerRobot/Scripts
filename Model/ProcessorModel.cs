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
    List<List<int>>[] PengAre = new List<List<int>>[4];                          //碰牌 吃牌 明杠等 （下面的类似
    Stack<int>[] AnGangAre = new Stack<int>[4];                              //庄家暗杠 （下面的类似
    Stack<int>[] OutCard = new Stack<int>[4];                             //庄家出牌区域  (下面类似

    int handleCard;                                                        //摸起来的牌
    int outCard;                                                           //刚出的牌
    bool isSorted = false;                                                 //是否已经排序

    List<int> playerAct = new List<int>();                               //玩家可操作类型 存储 胡 杠
    User[] players = new User[4];                                          //当轮玩家座位
    int turnID = 0;                                                    //轮到哪个玩家  0表示庄家  从庄家逆时针轮流
    int analysID = 0;                                                      //判断哪个玩家

    UserAction state;                                                      //状态

    //出牌判断标志
    bool isOutChihu;
    bool isOutGang;
    bool isOutPeng;
    bool isOutChi;

    PlayerAct gameTip;

    List<int> exebuff;

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
        optionData.dice_num = new int[2]{ UnityEngine.Random.Range(1, 7), UnityEngine.Random.Range(1, 7) };
        //optionData.player_priority = UnityEngine.Random.Range(0, 4);
        optionData.player_priority = 0;
        for(int i = 0; i < players.Length; ++i)
        {
            players[i] = new User(i);
        }
        return optionData;
    }

    //出牌等待（测试）
    public void WaitOrPush(int userID)
    {
        //如果该用户为自动出牌则不等待
        if(players[userID].ID < 0 || players[userID].TrusteeFlag || players[userID].TingFlag)
        {
            switch(gameTip)
            {
                case PlayerAct.add_gang:
                    EndStatusFlag(userID, UserAction.add_gang);
                    break;
                case PlayerAct.gang:
                    EndStatusFlag(userID, UserAction.gang_flag);
                    break;
                case PlayerAct.zi_mo:
                    EndStatusFlag(userID, UserAction.hu_flag);
                    break;
                default:
                    int indexOut = UnityEngine.Random.Range(0, HandCard[userID].Count);
                    //用户随机出牌
                    //并将摸的牌插入手牌
                    int indexIn = UserOutCard(userID, indexOut);
                    //更新字典
                    UpdateCardLib(outCard);
                    //传入 出牌id  插入id  出牌值
                    SetStatusFlag(userID, UserAction.put_card, new List<int>() { indexOut, indexIn , outCard});
                    break;
            }
        }
    }

    //吃牌和碰牌都是要打一张，杠牌是杠了后摸一张再打一张
    //出牌判断
    public void OutCardAnalys()
    {
        analysID = (analysID + 1) % 4;
        if(isOutChihu)
        {
            //吃胡判断
            if(!OutCardChiHuAnalys())
            {
                isOutChihu = false;
                //没有结果继续判断
                OutCardAnalys();
            }
        }else if(isOutGang)
        {
            //杠判断
            if(!OutCardGangAnalys())
            {
                isOutGang = false;
                //没有结果继续判断
                OutCardAnalys();
            }

        }else if(isOutPeng)
        {
            //碰判断
            if(!OutCardPengAnalys())
            {
                isOutPeng = false;
                //没有结果继续判断
                OutCardAnalys();
            }

        } else
        {
            //吃牌判断
            OutCardChiAnalys();
        }
    }

    //出牌判断 吃胡
    public bool OutCardChiHuAnalys()
    {
        //吃胡判断
        if(analysID != turnID)
        {
            players[analysID].HuPaiList = AnalyseCard(HandCard[analysID]);
            //查看出牌是否在胡牌列表 如果在就表示吃胡
            int index = players[analysID].HuPaiList.FindIndex(cd => cd == outCard);
            if(index > 0)
            {
                //吃胡 传入被吃的id 被吃的牌
                SetStatusFlag(analysID, UserAction.chi_hu_flag, new List<int>() { turnID });
            } else
            {
                //没有结果继续判断
                OutCardAnalys();
            }
        } else
        {
            return false;
        }
        return true;
    }

    //出牌判断 杠
    public bool OutCardGangAnalys()
    {
        List<int> resultList;
        if(analysID != turnID)
        {
            resultList = AnalyseGangPengCard(analysID, outCard);
            if(resultList.Count == 3)
            {
                //杠 传入被杠玩家id  传入杠牌index
                SetStatusFlag(analysID, UserAction.gang_flag, new List<int>() { turnID }, resultList);
            } else
            {
                //没有结果继续判断
                OutCardAnalys();
            }
        } else
        {
            return false;
        }
        return true;
    }

    //出牌判断 碰
    public bool OutCardPengAnalys()
    {
        List<int> resultList;
        if(analysID != turnID)
        {
            resultList = AnalyseGangPengCard(analysID, outCard);
            if(resultList.Count == 2)
            {
                //碰 传入被碰玩家id  传入碰牌index
                SetStatusFlag(analysID, UserAction.peng_card_flag, new List<int>() { turnID }, resultList);

                //等待用户操作
               // WaitOrPush();
            } else
            {
                //没有结果继续判断
                OutCardAnalys();
            }
        } else
        {
            return false;
        }

        return true;
    }

    //出牌判断 吃
    public void OutCardChiAnalys()
    {
        //吃牌可能有多种组合
        List<List<int>> chiList;

        analysID = (turnID + 1) % 4;
        chiList = AnalysChiCard(analysID, outCard);
        if(chiList.Count > 0)
        {
            //吃 传入被吃玩家id  传入吃牌index 
            //将被吃玩家id插入到传输列表第一位
            chiList.Insert(0, new List<int>() { turnID });
            SetStatusFlag(analysID, UserAction.chi_card_flag, chiList.ToArray());
        } else
        {
            //如果都没有 则自然轮到下一家出牌
            turnID = (turnID + 1) % 4;
            analysID = turnID;
            //用户摸牌
            UserGetCard(turnID);
        }
    }

    //将出过的牌从牌字典删除
    public void UpdateCardLib(int card)
    {
        try
        {
            if(cardDic.ContainsKey(card))
            {
                --cardDic[card];
                if(cardDic[card] == 0)
                {
                    cardDic.Remove(card);
                }
            }
        } catch(Exception e)
        {
            Debug.Log("<ProcessorModel::UpdateCardLib>: 出牌错误." + e);
        }
    }

    //初始化
    public void Reset()
    {
        cardWare = new List<int>(GlobalData.CardWare);
        for(int i = 0; i < 4; ++i)
        {
            HandCard[i] = new List<int>();
            PengAre[i] = new List<List<int>>();
            AnGangAre[i] = new Stack<int>();
            OutCard[i] = new Stack<int>();
        }
        for(int i = 0; i < GlobalData.CardType.Length; ++i)
        {
            cardDic.Add(GlobalData.CardType[i], 4);
        }
        isSorted = false;
        turnID = 0;
        analysID = 0;
    }

    //洗牌
    public List<int> RandCardData()
    {
        Reset();
        RandData(ref cardWare);
        return cardWare;
    }

    public void RandData(ref List<int> cards)
    {
        int index;
        int value;
        int Temp;

        if(cards == null || cards.Count == 0)
        {
            return;
        }

        for(index = 0; index < cards.Count; index++)
        {
            // 随机数是为了取余
            value = index + new System.Random().Next(0, 10000000) % (cards.Count - index);

            Temp = cards[index];
            cards[index] = cards[value];
            cards[value] = Temp;
        }
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

        //排列所有玩家手中麻将
        for(int i = 0; i < 4; ++i)
        {
            SelectSort(ref HandCard[i]);
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

    //摸牌
    public void UserGetCard(int userID, int index = -1)
    {
        //初始化出牌判断标志
        isOutChihu = true;
        isOutGang = true;
        isOutPeng = true;

        handleCard = RemoveCard(index);
        playerAct.Clear();
        playerAct.Add(handleCard);
        //自摸判断
        if(players[userID].HuPaiList == null)
        {
            players[userID].HuPaiList = AnalyseCard(HandCard[userID]);
            //查看出牌是否在胡牌列表 如果在就表示吃胡
            int id = players[userID].HuPaiList.FindIndex(cd => cd == outCard);
            if(id > 0)
            {
                playerAct.Add((int)PlayerAct.zi_mo);
            }
        }
        //杠判断 可能有多种
        List<int> card = new List<int>(HandCard[userID]);
        card.Add(handleCard);
        List<List<int>> gangGroup = GetGang(card);
        if(gangGroup.Count >0)
        {
            playerAct.Add((int)PlayerAct.gang);
        }
        //补杠判断
        for(int j = 0; j < PengAre[userID].Count; ++j)
        {
            for(int i = 0; i < HandCard[userID].Count; ++i)
            {
                if(PengAre[userID][j][0] == PengAre[userID][j][1] && HandCard[userID][i] == PengAre[userID][j][0])
                {
                    playerAct.Add((int)PlayerAct.add_gang);
                    gangGroup.Add(new List<int>(i)); //加杠手牌id
                    break;
                } else
                {
                    break;
                }
            }

            if(PengAre[userID][j][0] == PengAre[userID][j][1] && handleCard == PengAre[userID][j][0])
            {
                playerAct.Add((int)PlayerAct.add_gang);
                gangGroup.Add(new List<int>(-1)); //加杠手牌id -1表示手中的牌
                break;
            }
        }
        //将 要摸的牌  摸后的可行动作加到前面
        gangGroup.Insert(0, playerAct);
        gangGroup.Insert(0, new List<int>() { handleCard});

        //摸牌  传入要摸的牌  摸牌后的可行动作 能杠的牌
        SetStatusFlag(userID, UserAction.get_card,gangGroup.ToArray());
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
    /// <returns>返回插入index  如果不用插入则返回 -1</returns>
    public int UserOutCard(int userID, int index)
    {
        if(index < 0)
        {
            //出的是handlecard
            outCard = handleCard;
            handleCard = -1;
            return -1;
        } else if(handleCard >=0)
        {
            outCard = HandCard[userID][index];
            HandCard[userID].Remove(outCard);
            //将handleCard插入手牌中 返回插入位置
            int id = GetInsert(ref HandCard[userID], handleCard);
            handleCard = -1;
            return id;
        } else
        {
            outCard = HandCard[userID][index];
            HandCard[userID].Remove(outCard);
            //这是没有摸牌出牌的情况 一般碰或吃后
            return -1;
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
            Debug.LogError("<ProcessorModel::GroupAnalyse02>: out of range, index: " + index + ", range:" + Card.Count);
        }

        return result;
    }

    //获取牌中的所有四个相同的牌
    public List<List<int>> GetGang(List<int> Card)
    {
        List<List<int>> result = new List<List<int>>();
        List<int> group = new List<int>();
        //索引
        int index = 0;
        try
        {
            int temp = Card[index]; //取出第一个  分析开始
            group.Add(temp);
            do
            {
                //判断是否相同  是否连续  否则  按照这个顺序一直判断
                if(Card.Count > index + 1 && temp == Card[index + 1])
                {
                    ++index;
                    temp = Card[index];
                    group.Add(temp);
                } else
                {
                    if(group.Count == 4)
                    {
                        result.Add(group);
                    }
                    group.Clear();

                    //如果已经迭代完成则退出循环
                    if(index + 1 >= Card.Count)
                        break;

                    ++index;
                    temp = Card[index];
                    group.Add(temp);
                }
            } while(true);
        } catch
        {
            Debug.LogError("<ProcessorModel::GroupAnalyse02>: out of range, index: " + index + ", range:" + Card.Count);
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
    public List<int> AnalyseCard(List<int> handCard, int card = 0)
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
                //胡牌判断
                if(HuPaiAnalyse(temp))
                {
                    //保证不重复
                    if(result.FindIndex(cd => cd == sup[i]) == -1)
                        result.Add(sup[i]);
                }
                //加回
                temp.Insert(cardMem.index, cardMem.card);
                temp.Insert(cardMem.index, cardMem.card);
            }
        }

        return result;
    }

    //明杠 碰分析  一次只可能出现一种杠
    public List<int> AnalyseGangPengCard(int userID, int card)
    {
        List<int> result = new List<int>();
        int first = HandCard[userID].FindIndex(cd => cd == card);
        int last = HandCard[userID].FindLastIndex(cd => cd == card);
        if(last - first > 0)
        {
            for(int i = 0; i < last - first +1; ++i)
            {
                result.Add(first + i);
            }
        }
        return result;
    }

    //吃分析 可能有多种组合
    public List<List<int>> AnalysChiCard(int userID, int card)
    {
        List<List<int>> result = new List<List<int>>();
        //花牌没有吃
        if(card > 30)
            return result;
        //相间的吃牌
        int[] chi = new int[2] { -1, -1 };
        //遍历所有牌 找相邻
        for(int i = 0; i < HandCard[userID].Count; ++i)
        {
            //如果存在比判断牌大1位的
            if(card + 1 == HandCard[userID][i])
            {
                chi[1] = i;
                int index = i - 1;
                //判断后续 如果超出范围继续
                try
                {
                    while(HandCard[userID][++index] <= card + 1)
                    { }
                    //如果下一个不相同的牌是连续的 则两张牌存入结果
                    if(HandCard[userID][index] == card + 2)
                        result.Add(new List<int> { i, index });
                } catch { }

            } else if(card - 1 == HandCard[userID][i])
            {
                chi[0] = i;
                int index = i + 1;
                //判断后续 如果超出范围继续
                try
                {
                    while(HandCard[userID][--index] == card - 1)
                    { }
                    //如果上一个不相同的牌是连续的 则两张牌存入结果
                    if(HandCard[userID][index] == card - 2)
                        result.Add(new List<int> { index, i });
                } catch { }
            }
        }
        //有相间隔的吃牌
        if(chi[0] >= 0 && chi[1] >= 0)
            result.Add(new List<int>(chi));
        return result;
    }

    int GetHandCard(List<int> cards,int index)
    {
        int cd = cards[index];
        UpdateCardLib(cd);
        return cd;
    }

    /*胡牌分析*/

    //胡牌番数
    public int GetHuActionRank()
    {
        throw new NotImplementedException();
    }

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


    //发送
    public void SetStatusFlag(int userID, UserAction act, params List<int>[] arg)
    {
        Debug.Log("用户：" + userID + "-" + act.ToString() + "开始");
        //托管拦截
        if(players[userID].ID < 0 || players[userID].TrusteeFlag || players[userID].TingFlag)
        {
            switch(act)
            {
                case UserAction.get_card:
                    control.SetStatusFlag(userID, act, arg);
                    if(arg[1].Count > 1)
                    {
                        int n = UnityEngine.Random.Range(0,arg[1].Count);
                        //随机获取操作状态
                        gameTip = (PlayerAct)arg[1][n];
                    } else
                    {
                        gameTip = PlayerAct.empty;
                    }
                    break;
                //arg[0]被吃玩家id arg[1]...arg[n]吃牌组合
                case UserAction.chi_card_flag:
                case UserAction.peng_card_flag:
                case UserAction.gang_flag:
                    exebuff = arg[1];
                    EndStatusFlag(userID, act, exebuff);
                    break;
                case UserAction.chi_hu_flag:
                    EndStatusFlag(userID, UserAction.hu);
                    break;
                default:
                    control.SetStatusFlag(userID, act, arg);
                    break;
            }
        } else
        {
            //发送
            control.SetStatusFlag(userID, act, arg);
        }
    }

    //接收
    public void EndStatusFlag(int userID, UserAction act,params List<int>[] arg)
    {
        Debug.Log("用户：" + userID + "-" + act.ToString() + "完成");
        switch(act)
        {
            case UserAction.ready:
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
            //掷色子结束
            case UserAction.turn_dice:
                //界面开始发牌
                SetStatusFlag(userID, UserAction.send_card_flag, HandCard[userID]);
                break;
            //发牌结束
            case UserAction.send_card:
                //摸牌
                UserGetCard(turnID);
                break;
            //摸牌完成 等待用户出牌
            case UserAction.get_card:
                WaitOrPush(userID);
                break;
            case UserAction.put_card:
                string s = "ID:" + userID + "[";
                foreach(var item in HandCard[userID])
                    s += item +",";
                s += "]";
                Debug.Log("<color=red>" +s + HandCard[userID].Count +"</color>");
                //出牌判断
                OutCardAnalys();
                //下一个玩家判断                                                     
                break;
            case UserAction.insert_card:
                break;
            //吃碰等待出牌
            case UserAction.chi:
            case UserAction.peng:
                WaitOrPush(userID);
                break;
            //杠后摸牌再出
            case UserAction.ming_gang:
            case UserAction.add_gang:
            case UserAction.an_gang:
                //摸牌
                UserGetCard(turnID);
                break;
            //arg[0]
            case UserAction.chi_hu:
            case UserAction.hu:
                //游戏结束(测试 通知玩家可以开始新一局)
                SetStatusFlag(userID, UserAction.end);
                break;
            case UserAction.ting:
                break;
            case UserAction.ting_cancel:
                break;
            case UserAction.trustee:
                break;
            case UserAction.AI_cancel:
                break;
            
            //带flag为请求操作
            case UserAction.pass_flag:
                if(isOutChihu || isOutGang || isOutPeng || isOutChi)
                {
                    //出牌判断
                    OutCardAnalys();
                } else
                {
                    //如果都没有 则自然轮到下一家出牌
                    turnID = (turnID + 1) % 4;
                    analysID = turnID;
                    //用户摸牌
                    UserGetCard(turnID);
                }
                break;
            //出牌请求
            case UserAction.put_card_flag:
                if(userID == turnID)
                {
                    int index = UserOutCard(userID, arg[0][0]); //手中的牌插入id 如果没有则为-1
                    //更新字典
                    UpdateCardLib(outCard);
                    //传入 出牌id 手牌插入id 出牌牌型
                    SetStatusFlag(userID, UserAction.put_card, new List<int>() { arg[0][0], index , outCard });
                }
                break;
            //玩家要吃
            case UserAction.chi_card_flag:
                if(userID == analysID)
                {
                    List<int> card = new List<int>();
                    card.Add(GetHandCard(HandCard[userID], arg[0][0]));
                    card.Add(GetHandCard(HandCard[userID], arg[0][1]));
                    HandCard[userID].Remove(card[0]);
                    HandCard[userID].Remove(card[1]);
                    card.Add(outCard);
                    PengAre[userID].Add(card);
                    //传入 被吃的id  吃的牌id  吃牌的牌型
                    SetStatusFlag(userID, UserAction.chi,new List<int>() { turnID },arg[0], card);
                    //设置当前出牌用户
                    turnID = analysID;
                }
                break;
             //玩家要碰
            case UserAction.peng_card_flag:
                //玩家验证
                if(userID == analysID)
                {
                    List<int> card = new List<int>();
                    card.Add(GetHandCard(HandCard[userID], arg[0][0]));
                    card.Add(GetHandCard(HandCard[userID], arg[0][1]));
                    HandCard[userID].Remove(card[0]);
                    HandCard[userID].Remove(card[1]);
                    card.Add(outCard);
                    PengAre[userID].Add(card);
                    //传入 被碰的id  碰的牌id  碰牌的牌型
                    SetStatusFlag(userID, UserAction.peng, new List<int>() { turnID }, arg[0], card);
                    //设置当前出牌用户
                    turnID = analysID;
                }
                break;
            //玩家要家杠
            case UserAction.add_gang_flag:
                //玩家验证
                if(userID == analysID)
                {
                    
                    //大于零是手牌 小于零是手中的牌
                    int jiaCard;
                    if(arg[0][0] > 0)
                    {
                        jiaCard = GetHandCard(HandCard[userID], arg[0][0]);
                        HandCard[userID].Remove(jiaCard);
                    } else
                    {
                        jiaCard = handleCard;
                        handleCard = -1;
                    }
                    for(int i = 0; i < PengAre[userID].Count; ++i)
                    {
                        //寻找加牌的位置
                        if(jiaCard == PengAre[userID][i][0])
                        {
                            PengAre[userID][i].Add(jiaCard);
                            //告诉用户加牌 哪个位置的牌 并且加哪 是什么牌
                            SetStatusFlag(userID, UserAction.add_gang, new List<int>() { arg[0][0],i ,jiaCard});
                            break;
                        }
                    }
                    //设置当前出牌用户
                    turnID = analysID;
                }
                break;
            //玩家要杠
            case UserAction.gang_flag:
                //判断杠 并传回杠牌
                if(userID == analysID)
                {
                    
                    //传回三个以上的index
                    List<int> card = new List<int>();
                    card.Add(GetHandCard(HandCard[userID], arg[0][0]));
                    card.Add(GetHandCard(HandCard[userID], arg[0][1]));
                    card.Add(GetHandCard(HandCard[userID], arg[0][2]));
                    
                    //有手牌则是暗杠 没有则是明杠的回馈  
                    if(handleCard > 0)
                    {
                        //第四个参数有可能是手中的牌
                        if(arg[0][3] > 0)
                        {
                            card.Add(GetHandCard(HandCard[userID], arg[0][3]));
                            //从手牌中移除杠过的牌
                            HandCard[userID].Remove(card[0]);
                            HandCard[userID].Remove(card[1]);
                            HandCard[userID].Remove(card[2]);
                            HandCard[userID].Remove(card[3]);
                        } else
                        {
                            //从手牌中移除杠过的牌
                            HandCard[userID].Remove(card[0]);
                            HandCard[userID].Remove(card[1]);
                            HandCard[userID].Remove(card[2]);
                            //小于零表示手中的牌
                            card.Add(handleCard);
                            handleCard = -1;
                        }
                        PengAre[userID].Add(card);
                        //传入 杠的牌型
                        SetStatusFlag(userID, UserAction.an_gang, new List<int>() { turnID }, arg[0], card);
                    } else
                    {
                        //从手牌中移除杠过的牌
                        HandCard[userID].Remove(card[0]);
                        HandCard[userID].Remove(card[1]);
                        HandCard[userID].Remove(card[2]);
                        //第四个参数是刚出的牌
                        card.Add(outCard);
                        PengAre[userID].Add(card);
                        //传入 杠的牌型
                       SetStatusFlag(userID, UserAction.ming_gang, new List<int>() { turnID }, arg[0], card);
                    }
                    //设置当前出牌用户
                    turnID = analysID;
                }
                break;
            case UserAction.hu_flag:
                if(userID == analysID)
                {
                    //打包牌型数据
                     List<List<int>> cardData = new List<List<int>>(HandCard);
                    cardData.Add(new List<int> { handleCard });
                    //自摸
                    if(handleCard > 0)
                    {
                        SetStatusFlag(userID, UserAction.hu, cardData.ToArray());
                    } else
                    {
                        //吃胡 最前面加入被吃的id
                        cardData.Insert(0, new List<int>() { turnID });
                        SetStatusFlag(userID, UserAction.chi_hu, cardData.ToArray());
                    }
                    turnID = analysID;
                }
                break;
            case UserAction.ting_flag:
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
    public List<int> HuPaiList;     //如果听 能胡的牌列表

    public User(int turnId,int id = -1)
    {
        this.TurnID = turnId;
        this.ID = id;
        TrusteeFlag = false;
        TingFlag = false;
        HuPaiList = null;
    }
}

public enum PlayerAct
{
    empty,
    zi_mo,
    gang,
    add_gang,//补杠
    ting,
}