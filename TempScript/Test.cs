/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-03  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： Test.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading;
using System;

public class Test: MonoBehaviour
{
    public InputField input;
    public Text read;
    public Thread thread;
    NetworkSys network;
    int status;
    string msg;
    void Start()
    {
        network = new NetworkSys(this, "192.168.1.194", 8765);
        thread = new Thread(new ThreadStart(network.Connect));
    }

    void OnGUI()
    {
        if(GUILayout.Button("连接"))
        {
            if(!network.isDispose)
            {
                try
                {
                    thread.Start();
                } catch(Exception e)
                {
                    Debug.Log(e);
                }
            }
        }
        if(GUILayout.Button("断开连接"))
        {
            if(!network.isDispose)
            {
                network.Close("手动断开连接");
                thread.Suspend();
            }
        }

        if(GUILayout.Button("发送"))
        {
            if(!network.isDispose)
            {
                SendMsg();
            }
        }
    }

    void OnDisable()
    {
        thread.Abort();
    }
    public void SendMsg()
    {
       Debug.Log(network.SendMsg(1, input.text));
    }

    //读取消息
    public void ReadMsg(int i, string msg)
    {
        read.text = msg;
    }
}
