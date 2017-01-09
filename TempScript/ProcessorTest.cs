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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessorTest : MonoBehaviour
{
    public string Card;
    public byte HandleCard;
    public byte[] result;
    ProcessorModel pro = new ProcessorModel();
    List<byte> card = new List<byte>();
    

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
            result = pro.AnalyseCard(card, HandleCard);
        }

        if(GUILayout.Button("result"))
        {
            Debug.Log(pro.HuPaiAnalyse(card));
        }
    }
}
