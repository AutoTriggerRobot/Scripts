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

    /*公用区域*/

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

    //可接收一个int类型的回调委托
    public delegate void Callback(int i = -1);

    //色子运行时间
    public float rollTime = .5f;

    //游戏主程
    MonoBehaviour gameClient;

    //视图中的部件(obj)
    MGameClientView viewObj;

    //麻将的对象池
    SpawnPool spawnPool;

    //所有出牌区域的牌旋转角都为零，即面对主角
    Quaternion qua = Quaternion.Euler(0,0,0);

    //色子
    Transform diceA;
    Transform diceB;

    //色子旋转方向
    int xEuler;
    int yEuler;
    int zEuler;
    int supEuler;

    //储存iTween参数设置
    Hashtable diceAHash;
    Hashtable diceBHash;

    //色子旋转方向  输入点数
    Quaternion SetDice(int num)
    {
        xEuler = UnityEngine.Random.Range(2, 6) * 360;
        yEuler = UnityEngine.Random.Range(2, 6) * 360;
        zEuler = UnityEngine.Random.Range(2, 6) * 360;
        supEuler = UnityEngine.Random.Range(0, 360);
        switch(num)
        {
            case 1:
                return Quaternion.Euler(xEuler, supEuler + yEuler, zEuler);
            case 2:
                return Quaternion.Euler(xEuler + 90, supEuler + yEuler, supEuler + zEuler);
            case 3:
                return Quaternion.Euler(xEuler + 180, supEuler + yEuler, zEuler - 90);
            case 4:
                return Quaternion.Euler(xEuler, supEuler + yEuler, zEuler-90);
            case 5:
                return Quaternion.Euler(xEuler-90, supEuler + yEuler, zEuler);
            case 6:
                return Quaternion.Euler(xEuler, supEuler + yEuler, zEuler+180);
            default:
                Debug.Log(num + " 超出色子取值范围.");
                return Quaternion.identity;
        }
    }

    //初始化
    void Init()
    {
        //初始化界面绑定
        viewObj = new MGameClientView();

        #region 色子动画设置
        diceAHash = new Hashtable();
        diceBHash = new Hashtable();

        //设置路径
        diceAHash.Add("path", viewObj.PathA);
        diceBHash.Add("path", viewObj.PathB);

        //移动特效
        diceAHash.Add("easeType", iTween.EaseType.linear);
        diceBHash.Add("easeType", iTween.EaseType.linear);

        //移动时间
        diceAHash.Add("time", rollTime);
        diceBHash.Add("time", rollTime);

        //结束时调用函数
        //可以调用AnimationStart update end，默认调用自身，可以自定义设置
        diceAHash.Add("oncomplete", "AnimationEnd");
        diceBHash.Add("oncomplete", "AnimationEnd");

        //结束时调用主程的 AnimationEnd
        diceAHash.Add("oncompletetarget", gameClient.gameObject);
        diceBHash.Add("oncompletetarget", gameClient.gameObject);

        #endregion

        #region 主角区域
        //出牌区域容器
        outCardPoint_H = new Container(viewObj.Host.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_HorF);
        //抽牌区域容器
        group_H = new Container(viewObj.Host.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //主玩家
        hostUser = new UserCard(viewObj.Host, outCardPoint_H);
        #endregion

        #region 对家区域
        //出牌区域容器
        outCardPoint_F = new Container(viewObj.Front.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_HorF);
        //抽牌区域容器
        group_F = new Container(viewObj.Front.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //对面家
        frontUser = new UserCard(viewObj.Front, outCardPoint_F);
        #endregion

        #region 右家区域
        //出牌区域容器
        outCardPoint_R = new Container(viewObj.Right.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_LorR);
        //抽牌区域容器
        group_R = new Container(viewObj.Right.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //右玩家
        rightUser = new UserCard(viewObj.Right, outCardPoint_R);
        #endregion

        #region 左家区域
        //出牌区域容器
        outCardPoint_L = new Container(viewObj.Left.outCardPoint, GlobalData.OUT_CARD_Count, -1, ContainerTypes.OutCard_LorR);
        //抽牌区域容器
        group_L = new Container(viewObj.Left.group, GlobalData.GROUP_Count, -1, ContainerTypes.Group);
        //左玩家
        leftUser = new UserCard(viewObj.Left, outCardPoint_L);
        #endregion
    }

    public MGameClientAction(MonoBehaviour gameClient)
    {
        this.gameClient = gameClient;
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
        //重置对象池
        spawnPool.DespawnAll();
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

    //绑定洗牌顺序
    public void CardDirection(byte i)
    {
        //绑定前断开所有连接
        BreakAllLink();
        Container temp;
        switch(i)
        {
            case 0:
                //主角开始洗牌
                temp = group_H + group_R + group_F + group_L;
                break;
            case 1:
                //右家开始
                temp = group_R + group_F + group_L + group_H;
                break;
            case 2:
                //对家开始
                temp = group_F + group_L + group_H + group_R;
                break;
            case 3:
                //左家开始
                temp = group_L + group_H + group_R + group_F;
                break;
            default:
                throw new Debuger("MGameClientAction::CardDirection: 发牌顺序超出范围.");
        }
    }

    //洗牌（发牌前要确认好发牌顺序这会影响取牌方向)
    public IEnumerator AddGroup(Callback callback = null)
    {
        //判断整条链表是否不饱和
        while(group_R.IsAllNotFull)
        {
            //获取要加入容器的预设
            Transform tran = spawnPool.Spawn("Mahjong");
            //加入容器 并获取容器为预设分配的位置
            Vector3 target = group_R.AddItem(tran,GlobalData.MAHJONG_Width);
            //预设移动到分配位置
            iTween.MoveTo(tran.gameObject, target, .5f);
            //可以降低获取的速度
            //yield return new WaitForSeconds(.01f);
        }
        yield return 0;
        callback();
    }

    //转色子
    public IEnumerator TurnDice(int A_Result,int B_Result,Callback callback = null)
    {
        diceA = spawnPool.Spawn("Dice");
        diceB = spawnPool.Spawn("Dice");
        diceA.position = viewObj.PathA[0].position;
        diceB.position = viewObj.PathB[0].position;
        //旋转 已经有回调了 不再添加回调函数 
        iTween.MoveTo(diceA.gameObject, diceAHash);
        iTween.MoveTo(diceB.gameObject, diceBHash);

        float currentTime = Time.time + rollTime;
        while(currentTime > Time.time)
        {
            diceA.rotation = Quaternion.Lerp(diceA.rotation, SetDice(A_Result), .2f);
            diceB.rotation = Quaternion.Lerp(diceB.rotation, SetDice(B_Result), .2f);
            yield return 0;
        }
        Debug.Log("色子点数为：DiceA: " + A_Result + "    DiceB: " + B_Result);
    }

    //清除色子
    public IEnumerator DisplayDice()
    {
        if(diceA)
            spawnPool.Despawn(diceA);
        if(diceB)
            spawnPool.Despawn(diceB);
        yield return 0;
    }

    //开始发牌
    public IEnumerator AddHandCard(UserCard user, MahjongPrefab card,Callback callback = null)
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
        callback();
    }

    //发完牌后为玩家显示手牌
    public IEnumerator DisplayCard(UserCard user,Callback callback = null)
    {
        foreach(MahjongPrefab item in user.handCard)
        {
            item.animator.Play(GlobalData.ANIMA_GetCard);
        }
        yield return 0;
        callback();
    }

    //翻开某个玩家的所有手牌
    public IEnumerator TurnOverCard(UserCard user, Callback callback = null)
    {
        foreach(MahjongPrefab item in user.handCard)
        {
            item.animator.Play(GlobalData.ANIMA_TurnOverCard);
        }
        yield return 0;
        callback();
    }

    //摸牌 user：用户  card：指定要摸的牌
    public IEnumerator GetCard(UserCard user,MahjongPrefab card, Callback callback = null)
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
            yield return new WaitForSeconds(.3f);
        }else
        Debug.Log("手中的牌不能超过一个");
        callback();
    }

    //将摸到的牌插入手牌中
    public IEnumerator InsertToHandCard(UserCard user,int index, MahjongPrefab mahjong, Callback callback = null)
    {
        Vector3 target = user.handCard.InsertAt(index, mahjong);

        //此处可播放特效start
        mahjong.animator.Play(GlobalData.ANIMA_InsertCard);
        yield return new WaitForSeconds(.1f);

        //特效end

        //开始移动到出牌区域
        iTween.MoveTo(mahjong.transform.gameObject, target, .4f);
        yield return new WaitForSeconds(.4f);
        callback();
    }

    //添加暗杠
    public IEnumerator AddAnGang(UserCard user,Callback callback = null)
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
            yield return new WaitForSeconds(.5f);
            callback();
        } else
        {
            throw new Debuger("<MGameClientAction::AddAnGang>: 暗杠区域已满.");
        }
    }

    //添加明杠
    public IEnumerator AddMingGang(UserCard user, Callback callback = null)
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
            yield return new WaitForSeconds(.5f);
            callback();
        } else
        {
            throw new Debuger("<MGameClientAction::AddAnGang>: 明杠区域已满.");
        }
    }

    //添加碰
    public IEnumerator AddPengChi(UserCard user, Callback callback = null)
    {
        if(user.mingGangPoint.IsNotFull)
        {
            //获取预设
            Transform tran = spawnPool.Spawn("PengChi");
            //加入容器并获取分配坐标
            Vector3 target = user.mingGangPoint.AddItem(tran, GlobalData.PENG_CHI_Width);
            //只使用本地坐标x轴做动画
            tran.position = target + tran.right * .5f;
            //tran.position = target + tran.right * Vector3.Dot(tran.right, (target - Vector3.zero));
            //此处可播放特效start 

            //特效end
            //开始移动到明杠区域
            iTween.MoveTo(tran.gameObject, target, .5f);
            yield return new WaitForSeconds(.5f);
            callback();
        } else
        {
            throw new Debuger("<MGameClientAction::AddAnGang>: 明杠区域已满.");
        }
    }

    //特殊牌 & 功能牌
    public IEnumerator AddSpacialCard(UserCard user,MahjongPrefab card, Callback callback = null)
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
            yield return new WaitForSeconds(.5f);
            callback();
        } else
        {
            throw new Debuger("<MGameClientAction::AddAnGang>: 功能牌区域已满.");
        }
    }

    //出牌区域
    public IEnumerator AddOutCard(UserCard user, MahjongPrefab card, Callback callback = null)
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
                yield return new WaitForSeconds(.5f);
            }
        }
        callback();
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
