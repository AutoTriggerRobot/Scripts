/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： ResoucreMtr.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResoucreMtr
{
   ResoucreMtr()
    {
        for(int i = 0; i < GlobalData.CardType.Length; ++i)
        {
            meshNameLib.Add(GlobalData.CardType[i], GlobalData.MashType[i]);
        }
        mesh = Resources.LoadAll<Mesh>("Model");
        for(int i = 0; i < mesh.Length; ++i)
        {
            meshLib.Add(mesh[i].name, mesh[i]);
        }
    }
    public static readonly ResoucreMtr Instance = new ResoucreMtr();

    Dictionary<int,string> meshNameLib = new Dictionary<int, string>();
    Dictionary<string, Mesh> meshLib = new Dictionary<string, Mesh>();
    Mesh[] mesh;

    public Mesh GetMesh(int card)
    {
        return meshLib[meshNameLib[card]];
    }

}