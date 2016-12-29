/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-29  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 回馈错误警告
* 类 名： Debuger.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debuger : System.Exception
{
    public Debuger(string message)
    {
        Debug.LogWarning(message);
    }
}
