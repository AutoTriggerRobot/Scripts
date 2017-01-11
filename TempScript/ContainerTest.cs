/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-27  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 测试麻将牌容器（Capacity.cs）的各个功能
* 类 名： ContainerTest.cs
* 
* 修改历史： 2016/12/28  完成初步功能测试
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;

public class ContainerTest : MonoBehaviour
{
    MGameClientAction GameAct;
    ProcessorModel processorModel;
    List<int> test;


    void Start()
    {
        GameAct = new MGameClientAction(this);
        processorModel = new ProcessorModel();
    }

    void OnGUI ()
    {
        GUILayout.BeginHorizontal();
        #region 麻将抽发测试  已完成 2016/12/28
        GUILayout.BeginVertical();
        if(GUILayout.Button("发牌顺序:R-F-L-H"))
        {
            GameAct.CardDirection(1);
        }

        if(GUILayout.Button("发牌顺序:H-R-F-L"))
        {
            GameAct.CardDirection(0);
        }

        if(GUILayout.Button("断开连接"))
        {
            GameAct.BreakAllLink();
        }

        if(GUILayout.Button("洗牌"))
        {
            StartCoroutine(GetTest(CardTest.group));
        }

        if(GUILayout.Button("掷色子"))
            StartCoroutine(GetTest(CardTest.diceRoll));

        if(GUILayout.Button("移除色子"))
            StartCoroutine(GetTest(CardTest.displayDice));

        if(GUILayout.Button("清空"))
        {
            GameAct.Reset();

        }
        GUILayout.EndVertical();
        #endregion

        #region 玩家牌测试 所有玩家测试完成 2016/12/28
        GUILayout.BeginVertical();
        if(GUILayout.Button("发牌"))
        {
            MahjongPrefab tran = new MahjongPrefab();
            if(GameAct.hostUser.handCard.IsNotFull)
                tran = GameAct.group_H.GetMahjongCard(6);
            if(tran.transform != null)
                StartCoroutine(GetTest(CardTest.handCard,tran));
        }

        if(GUILayout.Button("获取手牌"))
        {
            StartCoroutine(GetTest(CardTest.getCard));
        }

        if(GUILayout.Button("从最后获取手牌"))
        {
            MahjongPrefab tran = new MahjongPrefab();
            if(GameAct.hostUser.handCard.IsNotFull)
                tran = GameAct.group_H.GetMahjongCard(5,0);
            if(tran.transform != null)
                StartCoroutine(GetTest(CardTest.getCard,tran));
                //mGameClientAction.GetCard(mGameClientAction.hostUser, tran);
        }

        if(GUILayout.Button("加入手牌"))
        {
                StartCoroutine(GetTest(CardTest.insertCard));
        }

        if(GUILayout.Button("打出第一张牌"))
        {
            MahjongPrefab tran = GameAct.hostUser.handCard.GetMahjongCard();
            StartCoroutine(GetTest(CardTest.outCard,tran));
        }

        if(GUILayout.Button("打出最后一张牌"))
        {
            MahjongPrefab tran = GameAct.hostUser.handCard.GetMahjongCard(0,0);
            StartCoroutine(GetTest(CardTest.outCard, tran));
        }

        if(GUILayout.Button("明杠"))
        {
            //左边第2个开始 杠四个
            MahjongPrefab tran1 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            MahjongPrefab tran2 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            MahjongPrefab tran3 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            MahjongPrefab tran4 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            StartCoroutine(GameAct.AddGang(GameAct.hostUser, CardActType.MingGang, tran1, tran2, tran3, tran4,()=>{
                GameAct.hostUser.handCard.ReSort();
            }));
        }

        if(GUILayout.Button("暗杠"))
        {
            //左边第2个开始 杠四个
            MahjongPrefab tran1 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            MahjongPrefab tran2 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            MahjongPrefab tran3 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            MahjongPrefab tran4 = GameAct.hostUser.handCard.GetMahjongCard(2, 0, false);
            StartCoroutine(GameAct.AddGang(GameAct.hostUser, CardActType.AnGang, tran1, tran2, tran3, tran4, () => {
                GameAct.hostUser.handCard.ReSort();
            }));
        }

        if(GUILayout.Button("碰"))
        {
            //出牌最后一个
            MahjongPrefab tran1 = GameAct.hostUser.outCardPoint.GetMahjongCard();
            //左边第1个开始  碰两个
            MahjongPrefab tran2 = GameAct.hostUser.handCard.GetMahjongCard(1, 0, false);
            MahjongPrefab tran3 = GameAct.hostUser.handCard.GetMahjongCard(1, 0, false);
            StartCoroutine(GameAct.AddPengChi(GameAct.hostUser, CardActType.Peng, tran1, tran2, tran3, () =>
            {
                GameAct.hostUser.handCard.ReSort();
            }));
        }

        if(GUILayout.Button("吃"))
        {
            //出牌最后一个
            MahjongPrefab tran1 = GameAct.hostUser.outCardPoint.GetMahjongCard();
            //左边第1个开始  手牌两个
            MahjongPrefab tran2 = GameAct.hostUser.handCard.GetMahjongCard(1, 0, false);
            MahjongPrefab tran3 = GameAct.hostUser.handCard.GetMahjongCard(1, 0, false);
            StartCoroutine(GameAct.AddPengChi(GameAct.hostUser, CardActType.Chi, tran1, tran2, tran3, () =>
            {
                GameAct.hostUser.handCard.ReSort();
            }));
        }

        if(GUILayout.Button("重排出牌区域"))
        {
            GameAct.hostUser.outCardPoint.ReSort();
        }

        if(GUILayout.Button("胡"))
        {
            StartCoroutine(GameAct.TurnOverCard(GameAct.hostUser));
        }

        if(GUILayout.Button("功能牌"))
        {
            MahjongPrefab tran = GameAct.hostUser.handCard.GetMahjongCard();
            StartCoroutine(GetTest(CardTest.spacialCard,tran));
        }

        GUILayout.EndVertical();
        GUILayout.BeginVertical();
        if(GUILayout.Button("洗牌"))
        {
            test = processorModel.RandCardData();
            string s = "[ ";
            foreach(byte item in test)
            {
                s += item + " : ";
            }
            s += " ]";
            Debug.Log(s);
        }
        if(GUILayout.Button("排序"))
        {
            processorModel.SelectSort(ref test);
            string s = "[ ";
            foreach(byte item in test)
            {
                s += item + " : ";
            }
            s += " ]";
            Debug.Log(s);
        }
        if(GUILayout.Button("插入"))
        {
            byte item = 10;
            Debug.Log(item);
            int index = processorModel.GetInsert(ref test, item);
            Debug.Log(index);
            string s = "[ ";
            foreach(byte it in test)
            {
                s += it + " : ";
            }
            s += " ]";
            Debug.Log(s);
        }
        GUILayout.EndVertical();
        #endregion
        GUILayout.EndHorizontal();
	}

    IEnumerator GetTest(CardTest type,MahjongPrefab carditem = new MahjongPrefab())
    {
        switch(type)
        {
            case CardTest.group:
                yield return GameAct.AddGroup();

                break;
            case CardTest.handCard:
                yield return GameAct.AddHandCard(GameAct.hostUser,carditem);
                yield return GameAct.DisplayCard(GameAct.hostUser);
              
                break;
            case CardTest.outCard:
                yield return GameAct.AddOutCard(GameAct.hostUser, carditem);

                break;
            case CardTest.spacialCard:
                yield return GameAct.AddSpacialCard(GameAct.hostUser, carditem);

                break;
            case CardTest.getCard:
                yield return GameAct.AddHandleCard(GameAct.hostUser,carditem);
                break;
            case CardTest.insertCard:
                int index = GameAct.hostUser.handCard.Count;
                if(index < 3)
                    --index;
                else
                    index = 2;
                yield return GameAct.InsertToHandCard(GameAct.hostUser, index, GameAct.hostUser.handleCard.GetMahjongCard());
                break;
            case CardTest.diceRoll:
                yield return GameAct.TurnDice(Random.Range(1, 7), Random.Range(1, 7));
                break;
            case CardTest.displayDice:
                yield return GameAct.DisapperDice();
                break;
        }
    }

    enum CardTest
    {
        handCard,
        spacialCard,
        outCard,
        group,
        getCard,
        insertCard,
        diceRoll,
        displayDice,
    }
}
