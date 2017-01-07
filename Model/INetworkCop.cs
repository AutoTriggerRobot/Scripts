/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-07  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 网络连接控制组件
* 类 名： INetworkCop.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkCop
{
    void Connect();
    void Stop();
    void ErrorInfo(string msg);
    void SuccessInfo();
    int SendMsg(string msg);
    void ReadMsg(Message msg);
}
