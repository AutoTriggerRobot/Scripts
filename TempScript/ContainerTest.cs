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
    public SpawnPool spawnPool;

    //只有单个用户可操控的牌
    MGameClientAction mGameClientAction;

    Quaternion qua = Quaternion.Euler(0, 0, 0);

    void Awake ()
    {
        mGameClientAction = new MGameClientAction();
	}
    void Start()
    {
        spawnPool = PoolManager.Pools["MahjongPool"];
    }

    void OnGUI ()
    {
        GUILayout.BeginHorizontal();
        #region 麻将抽发测试  已完成 2016/12/28
        GUILayout.BeginVertical();
        if(GUILayout.Button("发牌顺序:R-F-L-H"))
        {
            bool b;
            b = mGameClientAction.group_R + mGameClientAction.group_F;
            if(b)
                b = mGameClientAction.group_F + mGameClientAction.group_L;
            if(b)
                b = mGameClientAction.group_L + mGameClientAction.group_H;
            if(b)
            {
                Debug.Log("连接成功");
            } else
                Debug.Log("连接失败");
        }

        if(GUILayout.Button("发牌顺序:H-R-F-L"))
        {
            bool b;
            b = mGameClientAction.group_H + mGameClientAction.group_R;
            if(b)
                b = mGameClientAction.group_R + mGameClientAction.group_F;
            if(b)
                b = mGameClientAction.group_F + mGameClientAction.group_L;
            if(b)
                Debug.Log("连接成功.");
            else
                Debug.Log("连接失败");
        }

        if(GUILayout.Button("断开连接"))
        {
            mGameClientAction.BreakAllLink();
        }

        if(GUILayout.Button("洗牌"))
        {
            StartCoroutine(GetTest(CardTest.group));
        }

        if(GUILayout.Button("获取最后一张牌并移除"))
        {
            Transform tran = mGameClientAction.group_R.GetCard(0);
            if(tran != null)
                spawnPool.Despawn(tran);
        }

        if(GUILayout.Button("清空"))
        {
            spawnPool.DespawnAll();
            mGameClientAction.Reset();

        }
        GUILayout.EndVertical();
        #endregion

        #region 玩家牌测试 所有玩家测试完成 2016/12/28
        GUILayout.BeginVertical();
        if(GUILayout.Button("发牌"))
        {
            StartCoroutine(GetTest(CardTest.handCard));
        }

        if(GUILayout.Button("获取手牌"))
        {
            StartCoroutine(GetTest(CardTest.getCard));
        }

        if(GUILayout.Button("从最后获取手牌"))
        {
            MahjongPrefab tran = mGameClientAction.group_R.GetMahjongCard(0);
            if(tran.transform != null)
                //StartCoroutine(GetTest(CardTest.getCard,tran));
                mGameClientAction.GetCard(mGameClientAction.hostUser, tran);
        }

        if(GUILayout.Button("加入手牌"))
        {
                StartCoroutine(GetTest(CardTest.insertCard));
        }

        if(GUILayout.Button("打出第一张牌"))
        {
            MahjongPrefab tran = mGameClientAction.hostUser.handCard.GetMahjongCard();
            StartCoroutine(GetTest(CardTest.outCard,tran));
        }

        if(GUILayout.Button("打出最后一张牌"))
        {
            MahjongPrefab tran = mGameClientAction.hostUser.handCard.GetMahjongCard(0);
            StartCoroutine(GetTest(CardTest.outCard, tran));
        }

        if(GUILayout.Button("明杠"))
        {
            StartCoroutine(GetTest(CardTest.mingGang));
        }

        if(GUILayout.Button("暗杠"))
        {
            StartCoroutine(GetTest(CardTest.anGang));
        }

        if(GUILayout.Button("功能牌"))
        {
            MahjongPrefab tran = mGameClientAction.hostUser.handCard.GetMahjongCard();
            StartCoroutine(GetTest(CardTest.spacialCard,tran));
        }

        GUILayout.EndVertical();

        #endregion
        GUILayout.EndHorizontal();
	}

    IEnumerator GetTest(CardTest type,MahjongPrefab carditem = new MahjongPrefab())
    {
        switch(type)
        {
            case CardTest.anGang:
                yield return mGameClientAction.AddAnGang(mGameClientAction.hostUser);

                break;
            case CardTest.group:
                yield return mGameClientAction.AddGroup();

                break;
            case CardTest.handCard:
                yield return mGameClientAction.AddHandCard(mGameClientAction.hostUser,carditem);
                yield return mGameClientAction.DisplayCard(mGameClientAction.hostUser);
              
                break;
            case CardTest.mingGang:
                yield return mGameClientAction.AddMingGang(mGameClientAction.hostUser);

                break;
            case CardTest.outCard:
                yield return mGameClientAction.AddOutCard(mGameClientAction.hostUser, carditem);

                break;
            case CardTest.spacialCard:
                yield return mGameClientAction.AddSpacialCard(mGameClientAction.hostUser, carditem);

                break;
            case CardTest.getCard:
                yield return mGameClientAction.GetCard(mGameClientAction.hostUser,carditem);
                break;
            case CardTest.insertCard:
                int index = mGameClientAction.hostUser.handCard.Count;
                if(index < 3)
                    --index;
                else
                    index = 2;
                yield return mGameClientAction.InsertToHandCard(mGameClientAction.hostUser, index, mGameClientAction.hostUser.handleCard.GetMahjongCard());
                break;
        }
    }

    enum CardTest
    {
        handCard,
        mingGang,
        anGang,
        spacialCard,
        outCard,
        group,
        getCard,
        insertCard,
    }
}
