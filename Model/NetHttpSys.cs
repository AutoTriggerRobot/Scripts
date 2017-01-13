/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2017-01-13  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 
* 类 名： NetHttpSys.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using System.Threading;
using System.Net;

public class NetHttpSys: IDisposable
{
    //服务端是否是Java
    public bool isJavaServer = true;
    //释放标志
    public volatile bool isDispose;

    public TcpClient socket;
    //回调
    AsyncCallback aCallback;
    //缓冲
    byte[] buffers;
    //缓冲大小
    int bufferSize = 4 * 1024;
    //收到消息状态
    SocketError receiveError;
    //发送状态
    SocketError senderError;
    //接收
    NetworkStream recStream;
    //发送
    NetworkStream writerStream;
    //接收到的字节数
    int receiveSize = 0;
    //接收空消息次数
    byte zeroCount = 0;
    IPAddress IP;
    string Url;
    int Port = 0;

    //一个消息字节
    const long ConstLength = 4L;
    //消息缓冲
    List<byte> L_Buff = new List<byte>();

    //客户端
    INetworkCop client;

    //创建
    public NetHttpSys(INetworkCop client, string url,int port)
    {
        this.client = client;
        this.Url = url;
        this.Port = port;
    }

    //连接
    public void Connect()
    {
        try
        {
            Debug.Log("尝试连接：" + IP + ":" + Port);
            this.socket = new TcpClient();
            this.socket.Connect(Url, Port);
            SetSocket();
            client.SuccessInfo();
            Debug.Log("连接成功...");
        } catch(SocketException e)
        {
            client.ErrorInfo(GlobalData.NET_MSG_Try);
            Debug.Log("连接中断,2秒后重连..." + e);
            //重连
            Thread.Sleep(2000);
            this.Connect();
        } catch(Exception e)
        {
            Debug.LogError("连接错误：" + e);
        }
    }

    IEnumerator Reconnect()
    {
        yield return new WaitForSeconds(2f);
        this.Connect();
    }

    //初始化
    void SetSocket()
    {
        isDispose = false;
        socket.ReceiveBufferSize = bufferSize;
        socket.SendBufferSize = bufferSize;
        buffers = new byte[bufferSize];
        ReceiveAsync();
    }

    //消息接收递归
    void ReceiveAsync()
    {
        try
        {
            if(!isDispose && socket.Connected)
            {
                recStream = socket.GetStream();
                recStream.Read(buffers, 0, bufferSize);
                Message msg = new Message(buffers);
                ReadMsg(msg);
            }
        } catch
        {
            client.ErrorInfo(GlobalData.NET_MSG_Fail);
            Close("连接已经被关闭");
        }
    }

    //读取消息
    public void ReadMsg(Message msg)
    {
        client.ReadMsg(msg);
    }

    //发送消息
    public void SendMsg(Message msg)
    {
        try
        {
            if(!isDispose)
            {
                byte[] buff = Encoder(msg);
                writerStream = socket.GetStream();
                writerStream.Write(buff, 0, buff.Length);
                writerStream.Flush();
            }
        } catch(ObjectDisposedException e)
        {
            client.ErrorInfo(GlobalData.NET_MSG_Fail);
            Close("连接已被关闭" + e);
        } catch(SocketException e)
        {
            client.ErrorInfo(GlobalData.NET_MSG_Fail);
            Close("连接已被关闭" + e);
        } catch
        {
            Debug.LogError("状态未初始化");
        }
    }

    //状态检查
    bool CheckSocketError(SocketError socketError)
    {
        switch((socketError))
        {
            case SocketError.SocketError:
            case SocketError.VersionNotSupported:
            case SocketError.TryAgain:
            case SocketError.ProtocolFamilyNotSupported:
            case SocketError.ConnectionAborted:
            case SocketError.ConnectionRefused:
            case SocketError.ConnectionReset:
            case SocketError.Disconnecting:
            case SocketError.HostDown:
            case SocketError.HostNotFound:
            case SocketError.HostUnreachable:
            case SocketError.NetworkDown:
            case SocketError.NetworkReset:
            case SocketError.NetworkUnreachable:
            case SocketError.NoData:
            case SocketError.OperationAborted:
            case SocketError.Shutdown:
            case SocketError.SystemNotReady:
            case SocketError.TooManyOpenSockets:
                client.ErrorInfo(socketError.ToString());
                this.Close(socketError.ToString());
                return true;
        }
        return false;
    }

    //关闭
    public void Close(string msg)
    {
        lock(this)
        {
            if(!isDispose)
            {
                isDispose = true;
                try
                {
                    Debug.Log("关闭Tcp Socket:" + msg);
                    try
                    {
                        socket.Close();
                    } catch { }
                    IDisposable disposable = socket;
                    if(disposable != null)
                        disposable.Dispose();
                    IDisposable disposableThis = this;
                    buffers = null;
                    if(disposableThis != null)
                        disposableThis.Dispose();
                    client.Stop();
                } catch(Exception) { }
            }
        }
    }

    /// <summary>
    /// 读取大端序的int
    /// </summary>
    /// <param name="value"></param>
    public int ReadInt(byte[] intbytes)
    {
        if(isJavaServer)
            Array.Reverse(intbytes);
        return BitConverter.ToInt32(intbytes, 0);
    }

    /// <summary>
    /// 写入大端序的int
    /// </summary>
    /// <param name="value"></param>
    public byte[] WriterInt(int value)
    {
        byte[] bs = BitConverter.GetBytes(value);
        if(isJavaServer)
            Array.Reverse(bs);
        return bs;
    }

    //消息解码
    List<Message> Decoder(byte[] buff, int len)
    {
        byte[] rbuff = new byte[len];
        Array.Copy(buff, 0, rbuff, 0, rbuff.Length);
        buff = rbuff;
        if(L_Buff.Count > 0)
        {
            L_Buff.AddRange(rbuff);
            buff = L_Buff.ToArray();
            L_Buff.Clear();
            L_Buff = new List<byte>();
        }
        List<Message> msglist = new List<Message>();
        MemoryStream ms = new MemoryStream(buff);
        BinaryReader br = new BinaryReader(ms, UTF8Encoding.Default);
        try
        {
            byte[] _buff;
            Tag_back:
            //判断字节数
            if((br.BaseStream.Length - br.BaseStream.Position) < ConstLength)
            {
                buff = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                L_Buff.AddRange(buff);
            } else
            {
                long offset = 0;
                offset = ReadInt(br.ReadBytes(4));
                //消息接收足够
                if(offset <= (br.BaseStream.Length - br.BaseStream.Position))
                {
                    //消息
                    _buff = br.ReadBytes((int)(offset));
                    msglist.Add(new Message(_buff));
                    goto Tag_back;
                } else
                {
                    //继续存储
                    br.BaseStream.Seek(ConstLength, SeekOrigin.Current);
                    _buff = br.ReadBytes((int)(br.BaseStream.Length - br.BaseStream.Position));
                    L_Buff.AddRange(_buff);
                }
            }
        } catch { } finally
        {
            br.Close();
            ms.Close();
            if(ms != null)
                ms.Dispose();
        }
        return msglist;
    }

    //消息编码
    public byte[] Encoder(Message msg)
    {
        byte[] bit = null;
        using(MemoryStream ms = new MemoryStream())
        {
            using(BinaryWriter bw = new BinaryWriter(ms, UTF8Encoding.Default))
            {
                byte[] msgBuff = msg.BMsg;

                if(msgBuff != null)
                {
                    //长度
                    bw.Write(WriterInt(msgBuff.Length));
                    //命令内容
                    bw.Write(msgBuff);
                    bit = ms.ToArray();
                }
            }
        }
        return bit;
    }

    void Dispose(bool flag)
    {
        if(flag)
        {
            IDisposable disposable = this.L_Buff as IDisposable;
            if(disposable != null)
            { disposable.Dispose(); }
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }
}
