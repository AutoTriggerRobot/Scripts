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

public class Test: MonoBehaviour
{
    public Vector3 initPos;
    float initXpose;
    public Camera cam;
    int diceCount;

    void Update()
    {

        if(Input.GetMouseButtonDown(0))
        {
            //initial click to roll a dice  
            initPos = Input.mousePosition;

            //return x component of dice from screen to view point  
            initXpose = cam.ScreenToViewportPoint(Input.mousePosition).x;
        }

        //current position of mouse  
        Vector3 currentPos = Input.mousePosition;

        //get all position along with mouse pointer movement  
        Vector3 newPos = cam.ScreenToWorldPoint(new Vector3(currentPos.x, currentPos.y, Mathf.Clamp(currentPos.y / 10, 10, 50)));

        //translate from screen to world coordinates    
        newPos = cam.ScreenToWorldPoint(currentPos);

        if(Input.GetMouseButtonUp(0))
        {
            initPos = cam.ScreenToWorldPoint(initPos);

            //Method use to roll the dice  
            RollTheDice(newPos);
            //use identify face value on dice  
            StartCoroutine(GetDiceCount());
        } 
    }

    //Method Roll the Dice  
    void RollTheDice(Vector3 lastPos)
    {
        GetComponent<Rigidbody>().AddTorque(Vector3.Cross(lastPos, initPos) * 100, ForceMode.Impulse);
        lastPos.y += 12;
        GetComponent<Rigidbody>().AddForce(((lastPos - initPos).normalized) * (Vector3.Distance(lastPos, initPos)) * 25 * GetComponent<Rigidbody>().mass);
    }

    //Coroutine to get dice count  
    IEnumerator GetDiceCount()
    {
        if(Vector3.Dot(transform.forward, Vector3.up) > 1)
            diceCount = 5;
        if(Vector3.Dot(-transform.forward, Vector3.up) > 1)
            diceCount = 2;
        if(Vector3.Dot(transform.up, Vector3.up) > 1)
            diceCount = 3;
        if(Vector3.Dot(-transform.up, Vector3.up) > 1)
            diceCount = 4;
        if(Vector3.Dot(transform.right, Vector3.up) > 1)
            diceCount = 6;
        if(Vector3.Dot(-transform.right, Vector3.up) > 1)
            diceCount = 1;
        Debug.Log("diceCount :" + diceCount);
        yield return 0;
    }
}
