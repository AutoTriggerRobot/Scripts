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
        public byte card_direction;          //逆时针发牌 0表示从右家开始 依次类推 主角最大为3
        //取牌次序
        public byte get_card_priority;       //顺时针取牌  0表示从主角开始 1表示从右家开始 依次类推 右家最大为3
        //出牌次序
        public byte send_card_priority;      //出牌次序  取值同上

        public byte[] dice_num;              //色子数字

        public byte get_card_offset;         //取牌偏移值

        public DataBase(byte i = 0)
        {
            card_direction = 0;
            get_card_priority = 0;
            send_card_priority = 0;
            dice_num = new byte[2] { 6, 6 };
            get_card_offset = 6;
         }
    }

    //用户设置
    public struct UserSeting
    {

    }
}
