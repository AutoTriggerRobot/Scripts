/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-03  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： NetTest.cs
* 
* 修改历史：
* 
* 
*/

using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class NetTest: MonoBehaviour,INetworkCop
{
    public string IP = "127.0.0.1";
    public int Prot = 6650;
    public bool isJava;
    public InputField input;
    public InputField read;
    public Thread thread;
    NetworkSys network;
    int status;
    public string msg = "";
    public byte[] msgB;
    string msgbuff = "";
    bool isReceive = false;
    bool isConect = false;
    bool connect;

    void Awake()
    {
        network = new NetworkSys(this, IP, Prot);
        network.isJavaServer = isJava;
        thread = new Thread(new ThreadStart(network.Connect));
    }

    void OnGUI()
    {
        if(GUILayout.Button("连接"))
        {
            Connect();
        }
        if(GUILayout.Button("断开连接"))
        {
            Stop();
        }

        if(isReceive)
        {
            isReceive = false;
            msgbuff = msg;
            read.text +="Server: "+ msg + "\n";
            Debug.Log("已收到服务器消息：" + "<color=green>" + msg + "</color>");
        }

        if(connect && isConect != network.socket.Connected)
        {
            isConect = network.socket.Connected;
            Debug.Log(isConect);
        }
    }

    void OnDisable()
    {
        Stop();
    }

    public void SendMsg()
    {
        SendMsg(input.text);
        Debug.Log("已发送："+input.text);
        input.text = "";
    }

    public void Connect()
    {
        connect = true;
        if(thread == null)
        {
            network = new NetworkSys(this, IP, Prot);
            thread = new Thread(new ThreadStart(network.Connect));
        }
        if(!thread.IsAlive)
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

    public void Stop()
    {
        connect = false;
        if(!network.isDispose)
        {
            network.Close("手动断开连接");
        }
        if(thread != null)
        {
            thread.Abort();
            thread = null;
        }
    }

    public void ReadMsg(Message msg)
    {
        isReceive = true;
        this.msgB = msg.BMsg;
        this.msg = msg.SMsg;
    }

    public int SendMsg(string msg)
    {
        return network.SendMsg(new Message(msg));
    }

    public void ErrorInfo(string msg)
    {
        Debug.Log("Error：" + "<color=red>" + msg + "</color>");
        //StartCoroutine(WaitReConnect());
    }

    IEnumerator WaitReConnect()
    {
        int i = 0;
        while(thread != null)
        {
            yield return new WaitForSeconds(0.02f);
            ++i;
            if(i > 100)
            {
                break;
            }
        }
        Connect();
    }

    public void SuccessInfo()
    {

    }
}
