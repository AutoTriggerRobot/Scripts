/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2017 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-13  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： http
* 类 名： HttpNetSys.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using UnityEngine;

namespace Assets.Scripts.Model
{
    class HttpNetSys
    {
        private static int Max_Post_Size = 10 * 1024 * 1024;//10m
        private const int BUFF_SIZE = 4096;
        private Stream inputStream;
        public StreamWriter OutputStream;
        public String http_method;
        public String http_url;
        public String http_protocol_versionstring;
        public Hashtable httpHeaders = new Hashtable();
        internal TcpClient _Socket;

        /// <summary>
        /// 这个是服务器收到有效链接初始化
        /// </summary>
        internal HttpNetSys(TcpClient client)
        {
            this._Socket = client;
            inputStream = new BufferedStream(_Socket.GetStream());
            OutputStream = new StreamWriter(new BufferedStream(_Socket.GetStream()), UTF8Encoding.Default);
            ParseRequest();
        }

        internal void process(string bind)
        {
            try
            {
                if(http_method.Equals("GET"))
                {
                    //ListenersBox.Instance.MessagePool.ActiveHttp(this, bind, GetRequestExec());
                } else if(http_method.Equals("POST"))
                {
                    //ListenersBox.Instance.MessagePool.ActiveHttp(this, bind, PostRequestExec());
                }
            } catch(Exception e)
            {
                Debug.LogError("ActiveHttp Exception: " + e);
                WriteFailure();
            }
        }

        public void Close()
        {
            OutputStream.Flush();
            inputStream.Dispose();
            inputStream = null;
            OutputStream.Dispose();
            OutputStream = null; // bs = null;            
            this._Socket.Close();
        }

        #region 读取流的一行 private string ReadLine()
        /// <summary>
        /// 读取流的一行
        /// </summary>
        /// <returns></returns>
        private string ReadLine()
        {
            int next_char;
            string data = "";
            while(true)
            {
                next_char = this.inputStream.ReadByte();
                if(next_char == '\n')
                { break; }
                if(next_char == '\r')
                { continue; }
                if(next_char == -1)
                { Thread.Sleep(1); continue; };
                data += Convert.ToChar(next_char);
            }
            return data;
        }
        #endregion

        #region 转化出 Request private void ParseRequest()
        /// <summary>
        /// 转化出 Request
        /// </summary>
        private void ParseRequest()
        {
            String request = ReadLine();
            if(request != null)
            {
                string[] tokens = request.Split(' ');
                if(tokens.Length != 3)
                {
                    throw new Exception("invalid http request line");
                }
                http_method = tokens[0].ToUpper();
                http_url = tokens[1].ToLower();
                http_protocol_versionstring = tokens[2];
            }
            String line;
            while((line = ReadLine()) != null)
            {
                if(line.Equals(""))
                {
                    break;
                }
                int separator = line.IndexOf(':');
                if(separator == -1)
                {
                    throw new Exception("invalid http header line: " + line);
                }
                String name = line.Substring(0, separator);
                int pos = separator + 1;
                while((pos < line.Length) && (line[pos] == ' '))
                {
                    pos++;//过滤键值对的空格
                }
                string value = line.Substring(pos, line.Length - pos);
                httpHeaders[name] = value;
            }
        }
        #endregion

        #region 读取Get数据 private Dictionary<string, string> GetRequestExec()
        /// <summary>
        /// 读取Get数据
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, string> GetRequestExec()
        {
            Dictionary<string, string> datas = new Dictionary<string, string>();
            int index = http_url.IndexOf("?", 0);
            if(index >= 0)
            {
                string data = http_url.Substring(index + 1);
                datas = getData(data);
            }
            WriteSuccess();
            return datas;
        }
        #endregion

        #region 读取提交的数据 private void handlePOSTRequest()
        /// <summary>
        /// 读取提交的数据
        /// </summary>
        private Dictionary<string, string> PostRequestExec()
        {
            int content_len = 0;
            MemoryStream ms = new MemoryStream();
            if(this.httpHeaders.ContainsKey("Content-Length"))
            {
                //内容的长度
                content_len = Convert.ToInt32(this.httpHeaders["Content-Length"]);
                if(content_len > Max_Post_Size)
                { throw new Exception(String.Format("POST Content-Length({0}) 对于这个简单的服务器太大", content_len)); }
                byte[] buf = new byte[BUFF_SIZE];
                int to_read = content_len;
                while(to_read > 0)
                {
                    int numread = this.inputStream.Read(buf, 0, Math.Min(BUFF_SIZE, to_read));
                    if(numread == 0)
                    {
                        if(to_read == 0)
                        { break; } else
                        { throw new Exception("client disconnected during post"); }
                    }
                    to_read -= numread;
                    ms.Write(buf, 0, numread);
                }
                ms.Seek(0, SeekOrigin.Begin);
            }

            StreamReader inputData = new StreamReader(ms);
            string data = inputData.ReadToEnd();
            WriteSuccess();
            return getData(data);
        }
        #endregion

        #region 输出状态
        /// <summary>
        /// 输出200状态
        /// </summary>
        public void WriteSuccess()
        {
            OutputStream.WriteLine("HTTP/1.0 200 OK");
            OutputStream.WriteLine("Content-Type: text/html");
            OutputStream.WriteLine("");
        }

        /// <summary>
        /// 输出状态404
        /// </summary>
        public void WriteFailure()
        {
            OutputStream.WriteLine("HTTP/1.0 404 File not found");
            OutputStream.WriteLine("Content-Type: text/html");
            OutputStream.WriteLine("Connection: close");
            OutputStream.WriteLine("");
        }
        #endregion

        /// <summary>
        /// 分析http提交数据分割
        /// </summary>
        /// <param name="rawData"></param>
        /// <returns></returns>
        private static Dictionary<string, string> getData(string rawData)
        {
            var rets = new Dictionary<string, string>();
            string[] rawParams = rawData.Split('&');
            WWWForm form = new WWWForm();
            foreach(string param in rawParams)
            {
                string[] kvPair = param.Split('=');
                string key = kvPair[0];
                string value = HttpUtility.UrlDecode(kvPair[1]);
                rets[key] = value;
            }
            return rets;
        }
    }
}
