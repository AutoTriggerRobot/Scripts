/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 用户设置&可变参数
* 类 名： OptionData.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OptionData
{
    
    //配置
    public struct Configuration
    {
        
    }

    //基本参数
    public struct DataBase
    {
        public int[] dice_num;             //色子数字
        public int player_priority;        //发牌顺序 顺时针发牌顺序  0从主角开始发牌  1从右家开始发牌  2从对家开始发牌  3从左家开始发牌

        public DataBase(int i = 0)
        {
            dice_num = new int[2] { 6, 6 };
            player_priority = 3;
         }
    }

    //用户设置
    public struct UserSeting
    {

    }
}

[Serializable]
public struct MahjongMessage
{
    public int status;
    public Data data;
    public List<ServerException> message;
    public Extra extra;

    public override string ToString()
    {
        return JsonUtility.ToJson(this);
    }
}

[Serializable]
public class Data
{

}


[Serializable]
public struct ServerException
{
    public string code;
    public string msg;
    public ServerException(string code, string msg)
    {
        this.code = code;
        this.msg = msg;
    }
}

[Serializable]
public struct Extra
{

}