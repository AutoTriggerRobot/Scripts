/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-04  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： JsonTest.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class JsonTest: MonoBehaviour
{
    public string url;
    public string Data;
    public List<Student> studens = new List<Student>();
    public Dictionary<string,Student> studenlib = new Dictionary<string, Student>();
    public Student stud1;
    public Student stud2;
    public List<Student> jsonTo;
    public Dictionary<string,Student> jsonTolib;
    string json;
    string jsonlib;
    string jsonMsg;
    public MahjongMessage mahjongMsg = new MahjongMessage();
    public MahjongMessage mahjongMessage;
    HTTPUnity http = new HTTPUnity();
    void Start()
    {
        stud1 = new Student(2, "haha", 22, "boy");
        stud2 = new Student(5, "BiuBiuBiu", 55, "girl");
        studens.Add(stud1);
        studens.Add(stud2);
        studenlib.Add(stud1.Name, stud1);
        studenlib.Add(stud2.Name, stud2);
        mahjongMsg.data = new Data();
        mahjongMsg.extra = new Extra();
        mahjongMsg.message = new List<ServerException>();
        mahjongMsg.status = 1;
        mahjongMsg.message.Add(new ServerException("1001", "失败"));
        mahjongMsg.message.Add(new ServerException("1002", "失败"));
    }

    void OnGUI()
    {

        if(GUILayout.Button("连接Url"))
        {
            StartCoroutine(http.GET(url));
        }

        if(GUILayout.Button("Send"))
        {
            StartCoroutine(http.PostBit(url, Data));
        }

        if(GUILayout.Button("msgTojson"))
        {
            jsonMsg = JsonUtility.ToJson(mahjongMsg);
            StartCoroutine(http.PostBit(url, jsonMsg));
            Debug.Log(jsonMsg);
        }

        if(GUILayout.Button("jsonTomsg"))
        {
            mahjongMessage = JsonUtility.FromJson<MahjongMessage>(jsonMsg);
            Debug.Log(mahjongMessage.ToString());
        }

        if(GUILayout.Button("list<t>->json"))
        {
            json = JsonUtility.ToJson(new Serialization<Student>(studens));
            http.PostBit(url, json);
            Debug.Log(json);
        }

        if(GUILayout.Button("Dictionary<k,v>->json"))
        {
            jsonlib = JsonUtility.ToJson(new Serialization<string, Student>(studenlib));
            http.PostBit(url, jsonlib);
            Debug.Log(jsonlib);
        }

        if(GUILayout.Button("json->list<t>"))
        {
            jsonTo = JsonUtility.FromJson<Serialization<Student>>(json).ToList();
            if(jsonTo != null)
                foreach(Student student in jsonTo)
                    Debug.Log(student.ToString());
        }

        if(GUILayout.Button("json->Dictionary<k,v>"))
        {
            jsonTolib = JsonUtility.FromJson<Serialization<string, Student>>(jsonlib).ToDictionary();
            if(jsonTolib != null)
                Debug.Log(jsonTolib.Count);
        }
    }

}

[Serializable]
public struct Student
{
    public int ID;

    public string Name;

    public int Age;

    public string Sex;

    public Student(int id,string name,int age,string sex)
    {
        this.ID = id;
        this.Name = name;
        this.Age = age;
        this.Sex = sex;

    }

    public override string ToString()
    {
        return "ID:"+ID+": Name:" + Name+": Age:"+ Age + ": Sex:" + Sex;
    }
}
