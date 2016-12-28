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
            mGameClientAction.group_R.BreakLink();
            mGameClientAction.group_L.BreakLink();
            mGameClientAction.group_H.BreakLink();
            mGameClientAction.group_F.BreakLink();
        }

        if(GUILayout.Button("发牌"))
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
        if(GUILayout.Button("获取手牌"))
        {
            StartCoroutine(GetTest(CardTest.handCard));
        }

        if(GUILayout.Button("从最后获取手牌"))
        {
            Transform tran = mGameClientAction.group_R.GetCard(0);
            if(tran != null)
                StartCoroutine(GetTest(CardTest.handCard,tran));
        }

        if(GUILayout.Button("打出第一张牌"))
        {
            Transform tran = mGameClientAction.hostUser.handCard.GetCard();
            StartCoroutine(GetTest(CardTest.outCard,tran));
        }

        if(GUILayout.Button("打出最后一张牌"))
        {
            Transform tran = mGameClientAction.hostUser.handCard.GetCard(0);
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
            Transform tran = mGameClientAction.hostUser.handCard.GetCard();
            StartCoroutine(GetTest(CardTest.spacialCard,tran));
        }

        GUILayout.EndVertical();

        #endregion
        GUILayout.EndHorizontal();
	}

    IEnumerator GetTest(CardTest type,Transform carditem = null)
    {
        //bool isFull = false;
        switch(type)
        {
            case CardTest.anGang:
                yield return mGameClientAction.AddAnGang(mGameClientAction.hostUser);
                /*
                if(mGameClientAction.rightUser.anGangPoint.IsNotFull)
                {
                    Transform tran = spawnPool.Spawn("AnGang");
                    Vector3 target = mGameClientAction.rightUser.anGangPoint.AddItem(tran, GlobalData.AN_GANG);
                    iTween.MoveTo(tran.gameObject, target, .5f);
                }
                */
                break;
            case CardTest.group:
                yield return mGameClientAction.AddGroup();
                /*
                while(mGameClientAction.group_R.IsAllNotFull)
                {
                    //要加入容器的预设
                    Transform tran = spawnPool.Spawn("Mahjong");
                    //加入容器 并获取容器为预设分配的位置
                    Vector3 target = mGameClientAction.group_R.AddItem(tran, GlobalData.MAHJONG_Width,GlobalData.MAHJONG_Thickness);
                    //预设移动到分配位置
                    iTween.MoveTo(tran.gameObject, target, .5f);
                    //yield return new WaitForSeconds(.01f);
                };
                */
                break;
            case CardTest.handCard:
                yield return mGameClientAction.AddHandCard(mGameClientAction.hostUser,carditem);
                yield return mGameClientAction.GetCard(mGameClientAction.hostUser);
                /*
                while(!isFull)
                {
                    if(mGameClientAction.rightUser.handCard.IsNotFull)
                    {
                        Transform tran;
                        //如果有传入牌则获取传入牌
                        if(carditem)
                            tran = carditem;
                        //从牌库获取牌
                        else
                            tran = mGameClientAction.group_R.GetCard();
                        //如果没有取完则继续获取否则退出获取
                        if(tran != null)
                        {
                            Vector3 target = mGameClientAction.rightUser.handCard.AddItem(tran, GlobalData.MAHJONG_Width);
                            //先让牌提高一定高度（避免穿过其他牌获取）
                            //计算个与目的地方向相同位置为四分三的高增加0.1f的坐标
                            Vector3 temp = (target - tran.position).normalized * (target - tran.position).magnitude * .75f + new Vector3(tran.position.x,tran.position.y+.1f,tran.position.z);
                            iTween.MoveTo(tran.gameObject,temp, .2f);
                            yield return new WaitForSeconds(.15f);
                            //移动到分配位置
                            iTween.MoveTo(tran.gameObject, target, .5f);
                        } else
                        {
                            isFull = true;
                        }
                    } else
                    {
                        Debug.Log("手牌已满");
                        isFull = true;
                    }
                };
                //获取完后翻牌
                foreach(MahjongPrefab item in mGameClientAction.rightUser.handCard)
                {
                    item.transform.GetComponentInChildren<Animator>().SetBool("GetCard", true);
                }*/
                break;
            case CardTest.mingGang:
                yield return mGameClientAction.AddMingGang(mGameClientAction.hostUser);
                /*
                if(mGameClientAction.rightUser.mingGangPoint.IsNotFull)
                {
                    Transform tran = spawnPool.Spawn("MingGang");
                    Vector3 target = mGameClientAction.rightUser.mingGangPoint.AddItem(tran, GlobalData.MING_GANG);
                    iTween.MoveTo(tran.gameObject, target, .5f);
                    //在上面判断会多出刷出一个
                    if(isFull)
                        tran.gameObject.SetActive(false);
                }
                */
                break;
            case CardTest.outCard:
                yield return mGameClientAction.AddOutCard(mGameClientAction.hostUser, carditem);
                /*
                if(mGameClientAction.outCardPoint_H.IsNotFull)
                {
                    Transform tran = carditem;
                    if(tran != null)
                    {
                        //需要换行的排序 需要最后一个参数
                        Vector3 target = mGameClientAction.outCardPoint_H.AddItem(tran, GlobalData.MAHJONG_High, GlobalData.MAHJONG_Width);
                        tran.GetComponentInChildren<Animator>().SetBool("OutCard",true);
                        yield return new WaitForSeconds(.2f);
                        //使出牌区域的牌面向主角
                        tran.rotation = qua;
                        iTween.MoveTo(tran.gameObject, target, .5f);
                    }
                }*/
                break;
            case CardTest.spacialCard:
                yield return mGameClientAction.AddSpacialCard(mGameClientAction.hostUser, carditem);
                /*
                if(mGameClientAction.rightUser.spacialCard.IsNotFull)
                {
                    Transform tran = carditem;
                    if(carditem != null)
                    {
                        Vector3 target = mGameClientAction.rightUser.spacialCard.AddItem(tran, GlobalData.MAHJONG_Width);
                        tran.GetComponentInChildren<Animator>().SetBool("TurnOverCard", true);
                        iTween.MoveTo(tran.gameObject, target, .5f);
                    }
                }*/
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
    }
}
