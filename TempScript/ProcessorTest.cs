/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-09  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： ProcessorTest.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ProcessorTest: MonoBehaviour
{
    public string Card;
    public string Analys;
    public int HandleCard;
    public  List<List<int>> result;
    public bool re;
    ProcessorModel pro = new ProcessorModel();
    List<int> card = new List<int>();
    List<int> analys = new List<int>();


    void OnGUI()
    {
        if(GUILayout.Button("Init"))
        {
            card.Clear();
            foreach(char c in Card)
                card.Add(byte.Parse(c.ToString()));
        }

        if(GUILayout.Button("GetResult"))
        {
            //result = pro.AnalyseCard(card, HandleCard).ToArray();
            result = AnalysChiCard(card, HandleCard);
            string s = "";
            foreach(var item in result)
            {
                foreach(var it in item)
                    s += it;
                s += ",";
            }
            Debug.Log(s);
        }

        if(GUILayout.Button("result"))
        {
            Debug.Log(pro.HuPaiAnalyse(card));
        }
    }

    
    public List<List<int>> AnalysChiCard(List<int> list, int card)
    {
        List<List<int>> result = new List<List<int>>();
        //花牌没有吃
        if(card > 30)
            return result;
        //相间的吃牌
        int[] chi = new int[2] { -1,-1};
        //遍历所有牌 找相邻
        for(int i = 0; i < list.Count; ++i)
        {
            //如果存在比判断牌大1位的
            if(card + 1 == list[i])
            {
                 chi[1] = i;
                int index = i - 1;
                //判断后续 如果超出范围继续
                try
                {
                    while(list[++index] <= card + 1)
                    { }
                    //如果下一个不相同的牌是连续的 则两张牌存入结果
                    if(list[index] == card + 2)
                        result.Add(new List<int> { i, index });
                } catch { }

            } else if(card - 1 == list[i])
            {
                    chi[0] = i;
                int index = i + 1;
                //判断后续 如果超出范围继续
                try
                {
                    while(list[--index] == card - 1)
                    { }
                    //如果上一个不相同的牌是连续的 则两张牌存入结果
                    if(list[index] == card - 2)
                        result.Add(new List<int> {index ,i});
                } catch { }
            }
        }
        //有相间隔的吃牌
        if(chi[0] >= 0 && chi[1] >= 0)
            result.Add(new List<int>(chi));
        return result;
    }
}