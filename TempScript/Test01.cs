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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;

public class Test01: MonoBehaviour
{
    public InputField input;
    public Text read;

    public List<Student> studens = new List<Student>();
    public Dictionary<string,Student> studenlib = new Dictionary<string, Student>();
    public Student stud1;
    public Student stud2;
    public List<Student> jsonTo;
    public Dictionary<string,Student> jsonTolib;
    string json;
    string jsonlib;

    void Start()
    {
        stud1 = new Student(2, "haha", 22, "boy");
        stud2 = new Student(5, "BiuBiuBiu", 55, "girl");
        studens.Add(stud1);
        studens.Add(stud2);
        studenlib.Add(stud1.Name, stud1);
        studenlib.Add(stud2.Name, stud2);
        Debug.Log(JsonUtility.ToJson(stud1));
        Debug.Log(JsonUtility.ToJson(studens));
        Debug.Log(JsonUtility.ToJson(studenlib));
    }

    void OnGUI()
    {
        if(GUILayout.Button("json->list<t>"))
        {
            
        }

        if(GUILayout.Button("序列化1"))
        {
            json = JsonUtility.ToJson(new Serialization<Student>(studens));
            Debug.Log(json);
        }

        if(GUILayout.Button("序列化2"))
        {
            jsonlib = JsonUtility.ToJson(new Serialization<string, Student>(studenlib));
            Debug.Log(jsonlib);
        }

        if(GUILayout.Button("反序列化1"))
        {
            jsonTo = JsonUtility.FromJson<Serialization<Student>>(json).ToList();
            if(jsonTo != null)
                foreach(Student student in jsonTo)
                    Debug.Log(student.ToString());
        }

        if(GUILayout.Button("反序列化2"))
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