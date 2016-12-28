/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-26  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 可组成链表的麻将牌并可分配麻将位置的功能容器（存储麻将和麻将排所需要的信息）
* 类 名： Capacity.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Container:IEnumerable
{
    Container previousContainer = null;
    Container nextContainer = null;

    //子物体
    List<MahjongPrefab> childrenList;
    //容器位置
    Transform transform;
    //容器大小
    int capacity;
    //容器下一个空位置坐标
    Vector3 itemnPoint;
    //方向  +向左排  -向右排
    int direction;
    //容器种类
    ContainerTypes type;
    //遍历标志(用于遍历容器的时候做标记)
    bool isChange = false;

    public Container NextContainer
    {
        get
        {
            return nextContainer;
        }
        set
        {
            nextContainer = value;
        }
    }

    public Container PreviousContainer
    {
        get
        {
            return previousContainer;
        }
        set
        {
            previousContainer = value;
        }
    }

    //重载运算附  用+表示连接两表  连接成功返回true
    public static bool operator +(Container Head, Container Tail)
    {
        if(Head.NextContainer != null || Tail.previousContainer != null)
            return false;
        Head.NextContainer = Tail;
        Tail.previousContainer = Head;
        return true;
    }

    /// <summary>
    /// 容器
    /// </summary>
    /// <param name="tran">容器位置</param>
    /// <param name="capacity">容量</param>
    /// <param name="direction">方向 +1 向左排  -1 向右排</param>
    /// <param name="type">容器类型</param>
    public Container(Transform tran,int capacity,int direction = -1, ContainerTypes type = ContainerTypes.Nomal)
    {
        this.direction = direction;
        this.capacity = capacity;
        this.type = type;
        transform = tran;
        childrenList = new List<MahjongPrefab>();
        itemnPoint = tran.position;
    }

    //初始化容器
    public void ResetContainer()
    {
        childrenList.Clear();
        itemnPoint = transform.position;
    }

    //断开所有连接
    public void BrokenLink()
    {
        previousContainer = null;
        nextContainer = null;
    }

    //只读遍历标志
    public bool IsChange
    {
        get
        {
            return isChange;
        }
    }

    //如果当前容器未满 则返回true  否则 false
    public bool IsNotFull
    {
        get
        {
            if(childrenList.Count < capacity)
                return true;
            return false;
        }
    }

    //如果当前所在的链表中所有的容器中有一个存在未满的情况则返回true
    public bool IsAllNotFull
    {
        get
        {
            //改变标志表示已经遍历过了
            isChange = false;
            //如果当前容器未满则返回true
            if(IsNotFull)
            {
                //重置标志后返回结果
                ResetChange();
                return true;
            }
            //否则遍历排列在后面的容器
            if(nextContainer != null && isChange != nextContainer.IsChange)
            {
                return nextContainer.IsAllNotFull;
            }
            //后面的容器都满了遍历前面的容器
            if(previousContainer != null && isChange != previousContainer.isChange)
            {
                return previousContainer.IsAllNotFull;
            }
            //重置标志后返回结果
            ResetChange();
            return false;
        }
    }

    //如果当前所在的链表中所有的容器都空的情况则返回true
    public bool IsAllEmpty
    {
        get
        {
            //改变标志表示已经遍历过了
            isChange = false;
            //如果当前容器未空则返回false
            if(!IsEmpty)
            {
                ResetChange();
                return false;
            }
            //否则遍历排列在后面的容器
            if(nextContainer != null && isChange != nextContainer.IsChange)
            {
                return nextContainer.IsAllEmpty;
            }
            //后面的容器都满了遍历前面的容器
            if(previousContainer != null && isChange != previousContainer.isChange)
            {
                return previousContainer.IsAllNotFull;
            }
            //重置标志后返回结果
            ResetChange();
            return true;
        }
    }

    //重置链表中所有容器的标志
    void ResetChange()
    {
        if(isChange != true)
            isChange = true;
        if(nextContainer != null && nextContainer.IsChange != true)
            nextContainer.ResetChange();
        if(previousContainer != null && previousContainer.IsChange != true)
            previousContainer.ResetChange();
    }

    //是否为空
    public bool IsEmpty
    {
        get
        {
            if(childrenList.Count > 0)
                return false;
            return true;
        }
    }

    public void SetCapacity(int len)
    {
        capacity = len;
    }

    //返回容器子物体数量
    public int Count
    {
        get
        {
            return childrenList.Count;
        }
    }

    /// <summary>
    /// 输出物体位置坐标 如果所有容器都满了返回true 
    /// </summary>
    /// <param name="tran">预设</param>
    /// <param name="interest">间隔</param>
    /// <param name="pos">将加入在当前容器的位置</param>
    /// <param name="exp">换行占用间隔</param>
    /// <returns></returns>
    public Vector3 AddItem(Transform tran,float interest,float exp = 0)
    {
        //如果不是头容器则上一个容器执行添加操作
        if(previousContainer != null && previousContainer.IsNotFull)
            return previousContainer.AddItem(tran, interest, exp);
        else
        {
            //调整子物体旋转与容器一致
            tran.rotation = transform.rotation;
            //待返回变量
            Vector3 reTran;
            //容器未满
            if(IsNotFull)
            {
                reTran = itemnPoint;
                childrenList.Add(new MahjongPrefab(tran, interest, exp));
                switch(type)
                {
                    case ContainerTypes.Nomal:
                        //更改本地x轴偏移量
                        itemnPoint += (direction * interest)*transform.right;
                        break;
                    case ContainerTypes.Group:
                        //横排
                        if(childrenList.Count % (int)type == 0)
                        {
                            //更改本地x轴偏移量
                            itemnPoint += (direction * interest) * transform.right;
                            //更改本地y轴偏移量
                            itemnPoint -= exp * transform.up;
                        } else
                        //竖排
                        {
                            //更改本地y轴偏移量
                            itemnPoint += exp * transform.up;
                        }
                        break;
                    default:
                        //单行满就就换行
                        if(childrenList.Count % (int)type == 0)
                        {
                            itemnPoint = transform.position;
                            //更改本地z轴偏移量
                            itemnPoint += exp * transform.forward * (childrenList.Count / (int)type);
                        } else
                        {
                            itemnPoint += (direction * interest) * transform.transform.right;
                        }
                        break;
                }
                return reTran;
            }else if(nextContainer != null)
            {
                  return nextContainer.AddItem(tran, interest, exp);
            }
            return Vector3.zero;
        }
    }

    //移除容器中index的物体 操作成功返回false  只有普通容器可以使用
    public bool RemoveAt(int index)
    {
        if(childrenList.Count > 0)
        {
            childrenList.RemoveAt(index);
            //如果是普通容器则重排列表
            if(type != ContainerTypes.Group)
            {
                //重排前初始化最后一个子物体位置 之前的位置已经无效了
                itemnPoint = transform.position;
                ReSort(type);
            }
            return true;
        }
        Debug.LogWarning("<Capaity::RemoveItem>: index out of range");
        return false;
    }

    /// <summary>
    /// 按序列获取物体并返回
    /// </summary>
    /// <param name="direction">方向 大于0 从尾部获取   小于等于0 从头部获取</param>
    /// <param name="len">从起始位置算起 第几个</param>
    /// <returns></returns>
    public Transform GetCard(int direction = 1, int len = 0)
    {
        Transform tran;
        try
        {
            if(direction > 0)
            {
                //如果当前容器不是末端则执行下一个容器
                if(nextContainer != null && !nextContainer.IsEmpty)
                {
                    return nextContainer.GetCard(direction, len);
                }
                if(!IsEmpty)
                {
                    tran = childrenList[childrenList.Count - 1 - len].transform;
                    //childrenList.RemoveAt(childrenList.Count - 1 - len);
                    RemoveAt(childrenList.Count - 1 - len);
                    return tran;
                }
                if(previousContainer != null && !previousContainer.IsEmpty)
                {
                    return previousContainer.GetCard(direction, len);
                }
                Debug.Log("麻将已经抽完");
                return null;
            } else
            {
                //如果当前容器不是头 则执行上一个容器
                if(previousContainer != null && !previousContainer.IsEmpty)
                {
                    return previousContainer.GetCard(direction, len);
                }
                if(!IsEmpty)
                {
                    //从1开始算 默认把零当成1
                    if(len == 0)
                        ++len;
                    //判断是不是最后一个 如果是就返回
                    if(childrenList.Count == 1)
                    {
                        tran = childrenList[0].transform;
                        //childrenList.Clear();
                        ResetContainer();
                        return tran;
                    }
                    //判断最后一个的麻将上面有没麻将 
                    if(childrenList[0].transform.position.y < childrenList[1].transform.position.y)
                    {
                        tran = childrenList[len].transform;
                        //childrenList.RemoveAt(len);
                        RemoveAt(len);
                    }
                    //如果没有且取的是最后一个麻将
                    else if(len <= 1)
                    {
                        tran = childrenList[len - 1].transform;
                        //childrenList.RemoveAt(len - 1);
                        RemoveAt(len - 1);
                    }
                    //如果最后一个麻将上面没有麻将 并且不是取最后一个
                    else
                    {
                        tran = childrenList[len].transform;
                        childrenList.RemoveAt(len);
                    }
                    return tran;
                }
                if(nextContainer != null)
                {
                    return nextContainer.GetCard(direction, len);
                }

                Debug.Log("麻将已经抽完");
                return null;
            }
        } catch
        {
            Debug.LogWarning("<Capaity::GetCard>: index out of range");
            tran = null;
            return tran;
        }
    }

    //根据Transform获取列表位置index
    public int FindTransform(Transform tran)
    {
        return childrenList.FindIndex(pre=>pre.transform == tran);
    }

    //在列表中间移出物体后重新排列
    void ReSort(ContainerTypes type)
    {
        if(type == ContainerTypes.Nomal)
        {
            for(int i = 0; i < childrenList.Count; ++i)
            {
                Vector3 target = itemnPoint;
                iTween.MoveTo(childrenList[i].transform.gameObject, target, .2f);
                itemnPoint += (direction * childrenList[i].interest) * transform.right;
            }
        } else
        {
            for(int i = 0;i < childrenList.Count; ++i)
            {
                Vector3 target = itemnPoint;
                iTween.MoveTo(childrenList[i].transform.gameObject, target, .2f);
                if(i+1 % (int)type == 0)
                {
                    itemnPoint = this.transform.position;
                    itemnPoint -= childrenList[i].exp * ((i + 1 )/ (int)type) * transform.forward;
                } else
                {
                    itemnPoint += (direction * childrenList[i].interest) * transform.right;
                }
            }
        }
    }

    //实现迭代器
    public IEnumerator GetEnumerator()
    {
        for(int i = 0; i < childrenList.Count; ++i)
        {
            yield return childrenList[i];
        }
    }

    //可调用资源清理
    public void Dispose()
    {
        childrenList.Clear();
        childrenList = null;
        //原头接原尾
        if(previousContainer != null)
            previousContainer.NextContainer = NextContainer;
        //清除链表头
        previousContainer = null;
        //清除链表尾
        NextContainer = null;
        GC.SuppressFinalize(this);
    }
}

//容器子物体内容
public struct MahjongPrefab
{
    public Transform transform; //容器子物体
    public float interest;      //容器子物体占用空间
    public float exp;  //容器子物体高（用于竖排）
    public Animator animator;

    public MahjongPrefab(Transform tran, float interest, float exp = 0)
    {
        this.transform = tran;
        this.animator = transform.GetComponentInChildren<Animator>();
        this.interest = interest;
        this.exp = exp;
    }
}

public enum ContainerTypes
{
    Nomal = 0,
    Group = 2,          //牌库 每2张换行
    OutCard_HorF = 9,   //主角或对家出牌列表 每9张换行 
    OutCard_LorR = 6,   //右家或左家出牌列表 每6张换行
}