/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 数据处理接口
* 类 名： IMGameModel.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMGameModel
{
    //登录验证
    void ValidLogin(string name, string password);

    //界面操作
    void UserOperate(int ID);

    //状态标志
    void SetStatusFlag(int userID, UserAction act, params List<int>[] arg);

    void EndStatusFlag(int userID, UserAction act, params List<int>[] arg);

}
