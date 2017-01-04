/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-04  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： Test01.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 一个为摇色子服务的脚本
// 色子朝上的面默认为世界空间的正方向，只用1，2，3来定义世界空间
// 的向量，比如1代表世界的上，2代表右，3代表前
public delegate void RollCompleteEvent(object sender, int faceUp);
// 这个类代表一个六面色子的行为. 当这个类加载的时候，色子会以机
//作加载在空中。
// 当色子停下后 event RollComplete 会被激活

public class Test01 : MonoBehaviour {

    #region "Events"
    public event RollCompleteEvent RollComplete;
    #endregion
    #region "Private Members"
    //色子可能的朝向
    //Vector3.up           1(+) or 6(-) 
    //Vector3.right,       2(+) or 5(-) 
    //Vector3.forward];    3(+) or 4(-) 
    Vector3[] _sides = {Vector3.up, Vector3.right, -Vector3.forward };
    //声明isSleeping变量为否，即开始加载rigidbody给色子
    private bool _isSleeping = false;
    #endregion
    #region "Private Methods"
    //寻找色子哪个面朝上，将结果返回 
    private int WhichIsUp()
    {
        //定义maxY为负无穷
        float maxY = float.NegativeInfinity;
        int result = -1;
        for(int i = 0; i < 3; i++)
        {

            //转换物体朝向到世界空间
            Vector3 worldSpace = transform.TransformDirection(_sides[i]);
            // 测试哪面的y值更高 测正方向的面 1(+) 2(+) 3(+) 
            if(worldSpace.y > maxY)
            {
                result = i + 1;
                maxY = worldSpace.y;
            }
            // 测试反方向的面 6(-) 5(-) 4(-)
            if(-worldSpace.y > maxY)
            {
                result = 6 - i;
                maxY = -worldSpace.y;
            }
        }
        return result;
    }
    // 查看色子是否停止滚动，使rigidbody睡眠，即暂停
    private bool IsAtRest()
    {
        _isSleeping = GetComponent<Rigidbody>().IsSleeping();
        return _isSleeping;
    }
    #endregion
    #region "Unity Called Methods/Events"
    private void Start()
    {
        // 以随机的方法投掷色子
        GetComponent<Rigidbody>().AddRelativeTorque(Vector3.up * ((UnityEngine.Random.value * 20) + 10));
        GetComponent<Rigidbody>().AddRelativeTorque(Vector3.forward * ((UnityEngine.Random.value * 20) + 10));
        GetComponent<Rigidbody>().AddRelativeTorque(Vector3.right * ((UnityEngine.Random.value * 20) + 10));
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * ((UnityEngine.Random.value * 120) + 30));
        GetComponent<Rigidbody>().AddRelativeForce(Vector3.left * ((UnityEngine.Random.value * 170) + 30));
    }
    private void Update()
    {
        // 仅仅投掷得到结果1次
        if(!_isSleeping)
        {
            if(IsAtRest())
            {
                if(RollComplete != null)
                    RollComplete(this, WhichIsUp());
            }
        }
    }
    #endregion
}
