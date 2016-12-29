/* 
*┌──────────────────────────────────┐
*│　         Copyright(C) 2016 by Antiphon.All rights reserved.       │
*│                Author:by Locke Xie 2016-12-25  　　　　　　　　  　│
*└──────────────────────────────────┘
*
* 功 能： 处理界面动作逻辑
* 类 名： MGameClientAction.cs
* 
* 修改历史：
* 
* 
*/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathologicalGames;

public class MGameClientAction:IDisposable
{
    public struct UserCard
    {
        public Container handleCard;        //刚摸到的牌
        public Container handCard;          //手牌
        public Container mingGangPoint;     //明杠
        public Container anGangPoint;       //暗杠
        public Container spacialCard;       //功能牌
        public Container outCardPoint;      //出牌区域

        public UserCard(MahjongGrooves groove,Container outCardPoint)
        {
            handleCard =new Container(groove.handleCard,GlobalData.HANDLECARD_Count);
            //声明向右排序普通容器
            handCard = new Container(groove.handCard, GlobalData.HANDCARD_Count);
            //左排序普通容器
            mingGangPoint = new Container(groove.mingGangPoint, GlobalData.MING_GANG_Count, 1);
            anGangPoint = new Container(groove.anGangPoint, GlobalData.AN_GANG_Count, 1);
            //右排序普通容器
            spacialCard = new Container(groove.spacialCard, GlobalData.SPACIAL_CARD_Count);
            //绑定外部声明的出牌区域
            this.outCardPoint = outCardPoint;
        }

        //重置容器
        public void Reset()
        {
            handleCard.Reset();
            handCard.Reset();
            mingGangPoint.Reset();
            anGangPoint.Reset();
            spacialCard.Reset();
        }

        //清理容器
        public void Dispose()
        {
            handleCard.Dispose();
            handCard.Dispose();
            mingGangPoint.Dispose();
            anGangPoint.Dispose();
            spacialCard.Dispose();
            outCardPoint.Dispose();
        }
    }

    //主角
    public UserCard hostUser;
    //对家
    public UserCard frontUser;
    //右家
    public UserCard rightUser;
    //左家
    public UserCard leftUser;

    /*公用牌*/

    //主角区域公用牌
    public Container outCardPoint_H;
    public Container group_H;

    //对家区域公用牌
    public Container outCardPoint_F;
    public Container group_F;

    //右家区域公用牌
    public Container outCardPoint_R;
    public Container group_R;

    //左角区域公用牌
    public Container outCardPoint_L;
    public Container group_L;

    //视图中的部件(obj)
    MGameClientView mGameClientView;

    //麻将的对象池
    SpawnPool spawnPool;

    //所有出牌区域的牌旋转角都为零，即面对主角
    Quaternion qua = Quaternion.Euler(0,0,0);


    //初始化
    void Init()
    {
        //初始化界面绑定
        mGameClientView = new MGameClientView();

        #region 主角区域
        //出牌区域容器
        outCardPoint_H = new Container(mGameClientView.Host.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_HorF);
        //抽牌区域容器
        group_H = new Container(mGameClientView.Host.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //主玩家
        hostUser = new UserCard(mGameClientView.Host, outCardPoint_H);
        #endregion

        #region 对家区域
        //出牌区域容器
        outCardPoint_F = new Container(mGameClientView.Front.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_HorF);
        //抽牌区域容器
        group_F = new Container(mGameClientView.Front.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //对面家
        frontUser = new UserCard(mGameClientView.Front, outCardPoint_F);
        #endregion

        #region 右家区域
        //出牌区域容器
        outCardPoint_R = new Container(mGameClientView.Right.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_LorR);
        //抽牌区域容器
        group_R = new Container(mGameClientView.Right.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //右玩家
        rightUser = new UserCard(mGameClientView.Right, outCardPoint_R);
        #endregion

        #region 左家区域
        //出牌区域容器
        outCardPoint_L = new Container(mGameClientView.Left.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_LorR);
        //抽牌区域容器
        group_L = new Container(mGameClientView.Left.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //左玩家
        leftUser = new UserCard(mGameClientView.Left, outCardPoint_L);
        #endregion
    }

    public MGameClientAction()
    {
        //初始化
        Init();
        //找到名为MahjongPool的对象池
        spawnPool = PoolManager.Pools["MahjongPool"];
    }

    //重置所有牌
    public void Reset()
    {
        //玩家手牌
        hostUser.Reset();
        frontUser.Reset();
        leftUser.Reset();
        rightUser.Reset();
        //抽牌区域
        group_R.Reset();
        group_L.Reset();
        group_H.Reset();
        group_F.Reset();
        //出牌区域
        outCardPoint_F.Reset();
        outCardPoint_H.Reset();
        outCardPoint_L.Reset();
        outCardPoint_R.Reset();
    }

    //断开所有连接
    public void BreakAllLink()
    {
        group_F.BreakLink();
        group_H.BreakLink();
        group_L.BreakLink();
        group_R.BreakLink();
    }

    //手牌的容量会更具情况改变
    public void SetHandCardCapacity(UserCard user,int len)
    {
        user.handCard.SetCapacity(len);
    }

    //洗牌（发牌前要确认好发牌顺序这会影响取牌方向)
    public IEnumerator AddGroup()
    {
        //判断整条链表是否不饱和
        while(group_R.IsAllNotFull)
        {
            //获取要加入容器的预设
            Transform tran = spawnPool.Spawn("Mahjong");
            //加入容器 并获取容器为预设分配的位置
            Vector3 target = group_R.AddItem(tran);
            //预设移动到分配位置
            iTween.MoveTo(tran.gameObject, target, .5f);
            //可以降低获取的速度
            //yield return new WaitForSeconds(.01f);
        }
        yield return 0;
    }

    //开始发牌
    public IEnumerator AddHandCard(UserCard user, MahjongPrefab card)
    {

        //判断手牌是否已满并且牌库未空
        if(user.handCard.IsNotFull && !group_H.IsAllEmpty)
        {
            MahjongPrefab mahjong;
            //传入牌优先
            if(card.transform)
                mahjong = card;
            //默认取头牌
            else
                mahjong = group_H.GetMahjongCard();
            Vector3 target = user.handCard.AddMahjong(mahjong);
            //先让牌提高一定高度（避免穿过其他牌获取）
            //随便计算个与目的地方向相同位置为三分一的高增加0.5f的坐标
            Vector3 temp = (target - mahjong.transform.position).normalized * (target - mahjong.transform.position).magnitude * .75f + new Vector3(mahjong.transform.position.x, mahjong.transform.position.y + .1f, mahjong.transform.position.z);
            iTween.MoveTo(mahjong.transform.gameObject, temp, .2f);
            yield return new WaitForSeconds(.1f);
            //移动到分配位置
            iTween.MoveTo(mahjong.transform.gameObject, target, .3f);
        }else
        Debug.Log("手牌已满");
    }

    //发完牌后为玩家显示手牌
    public IEnumerator DisplayCard(UserCard user)
    {
        foreach(MahjongPrefab item in user.handCard)
        {
            item.animator.Play(GlobalData.ANIMA_GetCard);
        }
        yield return 0;
    }

    //翻开某个玩家的所有手牌
    public IEnumerator TurnOverCard(UserCard user)
    {
        foreach(MahjongPrefab item in user.handCard)
        {
            item.animator.Play(GlobalData.ANIMA_TurnOverCard);
        }
        yield return 0;
    }

    //摸牌 user：用户  card：指定要摸的牌
    public IEnumerator GetCard(UserCard user,MahjongPrefab card)
    {
        if(user.handleCard.IsNotFull && !group_H.IsAllEmpty)
        {
            MahjongPrefab mahjong;
            //传入牌优先
            if(card.transform)
                mahjong = card;
            //默认取头牌
            else
                mahjong = group_H.GetMahjongCard();
            Vector3 target = user.handleCard.AddMahjong(mahjong);
            mahjong.animator.Play(GlobalData.ANIMA_GetCard);
            yield return new WaitForSeconds(.2f);
            //先让牌提高一定高度（避免穿过其他牌获取）
            //随便计算个与目的地方向相同位置为三分一的高增加0.5f的坐标
            Vector3 temp = (target - mahjong.transform.position).normalized * (target - mahjong.transform.position).magnitude * .75f + new Vector3(mahjong.transform.position.x, mahjong.transform.position.y + .1f, mahjong.transform.position.z);
            iTween.MoveTo(mahjong.transform.gameObject, temp, .1f);
            yield return new WaitForSeconds(.1f);
            //移动到分配位置
            iTween.MoveTo(mahjong.transform.gameObject, target, .3f);
        }else
        Debug.Log("手中的牌不能超过一个");
    }

    //将摸到的牌插入手牌中
    public IEnumerator InsertToHandCard(UserCard user,int index, MahjongPrefab mahjong)
    {
        Vector3 target = user.handCard.InsertAt(index, mahjong);

        //此处可播放特效start
        mahjong.animator.Play(GlobalData.ANIMA_InsertCard);
        yield return new WaitForSeconds(.1f);

        //特效end

        //开始移动到出牌区域
        iTween.MoveTo(mahjong.transform.gameObject, target, .4f);
    }

    //添加暗杠
    public IEnumerator AddAnGang(UserCard user)
    {
        if(user.anGangPoint.IsNotFull)
        {
            //获取预设
            Transform tran = spawnPool.Spawn("AnGang");
            //加入容器并获取分配坐标
            Vector3 target = user.anGangPoint.AddItem(tran, GlobalData.AN_GANG_Width);
            //只使用本地坐标x轴做动画
            tran.position = target + tran.right*.5f;
            //tran.position = target + tran.right*Vector3.Dot(tran.right,(target - Vector3.zero));
            //此处可播放特效start 

            //特效end
            //开始移动到暗杠区域
            iTween.MoveTo(tran.gameObject, target, .5f);
            yield return 0;
        } else
        {
            throw new Debuger("<MGameClientAction::AddAnGang>: 暗杠区域已满.");
        }
    }

    //添加明杠
    public IEnumerator AddMingGang(UserCard user)
    {
        if(user.mingGangPoint.IsNotFull)
        {
            //获取预设
            Transform tran = spawnPool.Spawn("MingGang");
            //加入容器并获取分配坐标
            Vector3 target = user.mingGangPoint.AddItem(tran, GlobalData.MING_GANG_Width);
            //只使用本地坐标x轴做动画
            tran.position = target + tran.right*.5f;
            //tran.position = target + tran.right * Vector3.Dot(tran.right, (target - Vector3.zero));
            //此处可播放特效start 

            //特效end
            //开始移动到明杠区域
            iTween.MoveTo(tran.gameObject, target, .5f);
        } else
        {
            Debug.LogWarning("<MGameClientAction::AddAnGang>: 明杠区域已满.");
            yield return 0;
        }
    }

    //特殊牌 & 功能牌
    public IEnumerator AddSpacialCard(UserCard user,MahjongPrefab card)
    {
        if(user.spacialCard.IsNotFull)
        {
            //加入容器并获取分配坐标
            Vector3 target = user.spacialCard.AddMahjong(card);
            //此处可播放特效start 
            card.animator.Play(GlobalData.ANIMA_TurnOverCard);

            //特效end
            //开始移动到功能牌区域
            iTween.MoveTo(card.transform.gameObject, target, .5f);
            yield return 0;
        } else
        {
            throw new Debuger("<MGameClientAction::AddAnGang>: 功能牌区域已满.");
        }
    }

    //出牌区域
    public IEnumerator AddOutCard(UserCard user, MahjongPrefab card)
    {
        if(user.outCardPoint.IsNotFull)
        {
            if(card.transform != null)
            {
                //需要换行的排序需要最后一个参数用来确定换行距离
                Vector3 target = user.outCardPoint.AddMahjong(card);

                //此处可播放特效start
                card.animator.Play(GlobalData.ANIMA_OutCard);
                yield return new WaitForSeconds(.2f);
                //使出牌区域的牌面向主角
                card.transform.rotation = qua;

                //特效end

                //开始移动到出牌区域
                iTween.MoveTo(card.transform.gameObject, target, .5f);
            }
        }
    }

    //清理所有容器
    public void Dispose()
    {
        hostUser.Dispose();
        leftUser.Dispose();
        rightUser.Dispose();
        frontUser.Dispose();
        group_F.Dispose();
        group_H.Dispose();
        group_L.Dispose();
        group_R.Dispose();
    }
}
