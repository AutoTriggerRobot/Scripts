/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 网络通讯连接
* 类 名： NetworkSys.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.Serialization;
using System.Threading;

public class NetworkSys : IDisposable
{
    //是否收到服务器消息
    public bool isServer = false;
    //释放标志
    public volatile bool isDispose;
    //id
    public string ID = string.Empty;

    Socket socket;
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
    //接收到的字节数
    int receiveSize = 0;
    //接收空消息次数
    byte zeroCount = 0;
    string IP;
    int Port = 0;

    //一个消息字节
    const long ConstLength = 4L;
    //消息缓冲
    List<byte> L_Buff = new List<byte>();

    //客户端
    Test client;

    //创建
    public NetworkSys(Test client,string ip = "127.0.0.1",int port = 9527)
    {
        this.client = client;
        this.IP = ip;
        this.Port = port;
    }
    
    //测试服务器
    public NetworkSys(Socket socket)
    {
        isServer = true;
        this.socket = socket;
        SetSocket();
    }

    //连接
    public void Connect()
    {
        try
        {
            Debug.Log("尝试连接：" + IP + ":" + Port);
            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socket.Connect(IP, Port);
            Debug.Log("远程EndPoint：" + socket.RemoteEndPoint.ToString() + " 本地EndPoint：" + socket.LocalEndPoint.ToString());
            SetSocket();
            Debug.Log("连接成功...");
        }catch(SocketException e)
        {
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

    void SetSocket()
    {
        aCallback = new AsyncCallback(ReceiveCallback);
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
                socket.BeginReceive(buffers, 0, bufferSize, SocketFlags.None, out senderError, aCallback, this);
                CheckSocketError(receiveError);
            }
        } catch
        {
            Close("连接已经被关闭");
        }
    }

    //接收消息回调
    void ReceiveCallback(IAsyncResult iar)
    {
        if(!isDispose)
        {
            try
            {
                //接收消息
                receiveSize = socket.EndReceive(iar, out receiveError);
                if(!CheckSocketError(receiveError)&& receiveError == SocketError.Success)
                {
                    if(receiveSize > 0)
                    {
                        byte[] rbuff = new byte[receiveSize];
                        Array.Copy(buffers, rbuff, receiveSize);
                        var msgs = Decoder(rbuff, receiveSize);
                        foreach(var msg in msgs)
                        {
                            try
                            {
                                //读取消息
                                ReadMsg(msg);
                                
                            }catch(Exception e)
                            {
                                Debug.Log("读取消息错误：" + e);
                            }
                        }
                        //重置空字节数
                        zeroCount = 0;
                        //继续递归接收
                        ReceiveAsync();
                    } else
                    {
                        zeroCount++;
                        if(zeroCount == 10)
                            this.Close("连接错误");
                    }
                }
            } catch{ Close("连接已经被关闭"); }
        }
    }

    //读取消息
    public void ReadMsg(MahjongMessage msg)
    {
        using(MemoryStream msReader = new MemoryStream(msg.Msg))
        {
            using(BinaryReader srReader = new BinaryReader(msReader, UTF8Encoding.Default))
            {
                client.ReadMsg(msg.MsgID, srReader.ReadString());
            }
        }
    }

    //发送消息
    public int SendMsg(int i,string msg)
    {
        MahjongMessage mms;
        using(MemoryStream msw = new MemoryStream())
        {
            using(BinaryWriter sw = new BinaryWriter(msw, UTF8Encoding.Default))
            {
                sw.Write(msg);
                mms = new MahjongMessage(i, msw.GetBuffer());
            }
        }
        int size = 0;
        try
        {
            if(!isDispose)
            {
                byte[] buff = Encoder(mms);
                size = socket.Send(buff, 0, buff.Length, SocketFlags.None, out senderError);
                CheckSocketError(senderError);
            }
        } catch(ObjectDisposedException e)
        {
            Close("连接已被关闭" + e);
        } catch(SocketException e)
        {
            Close("连接已被关闭" + e);
        } catch
        {
            Debug.LogError("状态未初始化");
        }
        return size;
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
                    Debug.Log("关闭连接远程EndPoint：" + socket.RemoteEndPoint.ToString() + " 本地EndPoint：" + socket.LocalEndPoint.ToString());
                    Debug.Log("关闭Tcp Socket" + msg);
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
        Array.Reverse(bs);
        return bs;
    }

    //消息解码
    List<MahjongMessage> Decoder(byte[] buff,int len)
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
        List<MahjongMessage> msglist = new List<MahjongMessage>();
        MemoryStream ms = new MemoryStream(buff);
        BufferReader br = new BufferReader(ms, UTF8Encoding.Default);
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
                    int msgID = 1;
                    //命令
                    //msgID = ReadInt(br.ReadBytes(4));
                    //消息
                    _buff = br.ReadBytes((int)(offset));
                    msglist.Add(new MahjongMessage(msgID, _buff));
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
            if(br != null)
                br.Dispose();
            ms.Close();
            if(ms != null)
                ms.Dispose();
        }
        return msglist;
    }

    //消息编码
    public byte[] Encoder(MahjongMessage msg)
    {
        byte[] bit = null;
        using(MemoryStream ms = new MemoryStream())
        {
            using(BinaryWriter bw = new BinaryWriter(ms, UTF8Encoding.Default))
            {
                byte[] msgBuff = msg.Msg;

                if(msgBuff != null)
                {
                    //长度
                    bw.Write(WriterInt(msgBuff.Length));
                    //命令id
                    //bw.Write(WriterInt(msg.MsgID));
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

public struct MahjongMessage
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public int MsgID;
    /// <summary>
    /// 消息内容
    /// </summary>
    public byte[] Msg;

    public MahjongMessage(int msgID,byte[] msg)
    {
        MsgID = msgID;
        Msg = msg;
    }
}