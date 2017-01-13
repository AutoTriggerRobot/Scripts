/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2017 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-13  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： HTTPUnity.cs
* 
* 修改历史：
* 
* 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HTTPUnity
{
    //POST请求
    public IEnumerator POST(string url, Dictionary<string, string> post)
    {
        WWWForm form = new WWWForm();
        foreach(KeyValuePair<string, string> post_arg in post)
        {
            form.AddField(post_arg.Key, post_arg.Value);
        }

        WWW www = new WWW(url, form);
        yield return www;

        if(www.error != null)
        {
            //POST请求失败
            Debug.Log("error is :" + www.error);

        } else
        {
            //POST请求成功
            Debug.Log("request ok : " + www.text);
        }
    }

    //GET请求
    public IEnumerator GET(string url)
    {

        WWW www = new WWW(url);
        yield return www;

        if(www.error != null)
        {
            //GET请求失败
            Debug.Log("error is :" + www.error);

        } else
        {
            //GET请求成功
            Debug.Log("request ok : " + www.text);
        }
    }

    //转byte流
    public IEnumerator PostBit(string url,string Data)
    {
        WWWForm wwwForm = new WWWForm();
        byte[] byteStream = System.Text.Encoding.Default.GetBytes(Data);
        wwwForm.AddBinaryData("post", byteStream);

        WWW www = new WWW(url, wwwForm);
        yield return www;
        if(www.error != null)
        {
            //POST请求失败
            Debug.Log("error is :" + www.error);

        } else
        {
            //POST请求成功
            Debug.Log("request ok : " + www.text);
        }
    }
}
