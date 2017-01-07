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
        //发牌方向
        public byte card_direction;         //逆时针发牌 0表示从右家开始 依次类推 主角最大为3
        //取牌次序
        public byte get_card_priority;      //顺时针取牌  0表示从主角开始 1表示从右家开始 依次类推 右家最大为3
        //出牌次序
        public byte send_card_priority;     //出牌次序  取值同上
        public byte[] dice_num;             //色子数字
        public byte get_card_offset;        //取牌偏移值
        public byte player_priority;        //发牌顺序 顺时针发牌顺序  0从主角开始发牌  1从右家开始发牌  2从对家开始发牌  3从左家开始发牌
        public DataBase(byte i = 0)
        {
            card_direction = 0;
            get_card_priority = 0;
            send_card_priority = 0;
            dice_num = new byte[2] { 6, 6 };
            get_card_offset = 6;
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
    public string passport = "admin";
    public string passwd =  "1234";
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