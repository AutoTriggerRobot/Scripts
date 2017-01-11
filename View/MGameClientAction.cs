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

public class MGameClientAction
{
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

    //普通回调委托
    public delegate void Callback();

    //迭代回调委托
    public delegate IEnumerator IEnumeratorCallback();

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
        xEuler = 0;
        yEuler = 0;
        zEuler = 0;
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

    //吃 碰 杠时 判断下一个动画状态 -1表示不做任何动作
    int CheckState(MahjongPrefab card,CardActType type)
    {
        int currenState = card.animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        if(currenState == GlobalData.ANIMA_GetCard)
            switch(type)
            {
                case CardActType.AnGang:
                    return GlobalData.ANIMA_CloseCard;
                default:
                    return GlobalData.ANIMA_TurnOverCard;
            }
        if(currenState == GlobalData.ANIMA_OutCard)
            return GlobalData.ANIMA_ChiPengCard;
        if(currenState == GlobalData.ANIMA_TurnOverCard)
            return -1;
        if(currenState == GlobalData.ANIMA_InsertCard)
            switch(type)
            {
                case CardActType.AnGang:
                    return GlobalData.ANIMA_CloseCard;
                default:
                    return GlobalData.ANIMA_TurnOverCard;
            }
        if(currenState == GlobalData.ANIMA_CardIdle)
            switch(type)
            {
                case CardActType.AnGang:
                    return -1;
                default:
                    return GlobalData.ANIMA_TurnOverCard;
            }
        if(currenState == GlobalData.ANIMA_CloseCard)
            switch(type)
            {
                case CardActType.AnGang:
                    return -1;
                default:
                    return GlobalData.ANIMA_TurnOverCard;
            }
        if(currenState == GlobalData.ANIMA_ChiPengCard)
            return -1;
        return -1;
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

    //构造函数
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
        //重置色子
        diceA = null;
        diceB = null;
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

    //绑定顺序
    public void CardDirection(int i)
    {
        //绑定前断开所有连接
        BreakAllLink();
        Container temp;
        switch(i)
        {
            case 0:
                //主角开始取牌
                temp = group_R + group_F + group_L + group_H;
                break;
            case 1:
                //右家开始
                temp = group_F + group_L + group_H + group_R;
                break;
            case 2:
                //对家开始
                temp = group_L + group_H + group_R + group_F;
                break;
            case 3:
                //左家开始
                temp = group_H + group_R + group_F + group_L;
                break;
            default:
                Debug.Log("MGameClientAction::CardDirection: 发牌顺序超出范围.");
                break;
        }
    }

    //洗牌（发牌前要确认好发牌顺序这会影响取牌方向)
    public IEnumerator AddGroup()
    {
        Reset();
        //判断整条链表是否不饱和
        while(group_H.IsAllNotFull)
        {
            //获取要加入容器的预设
            Transform tran = spawnPool.Spawn("Mahjong");
            //加入容器 并获取容器为预设分配的位置
            Vector3 target = group_H.AddItem(tran,GlobalData.MAHJONG_Width);
            //预设移动到分配位置
            iTween.MoveTo(tran.gameObject, target, .5f);
            //可以降低获取的速度
            //yield return new WaitForSeconds(.01f);
        }
        yield return 0;
    }

    //显示色子
    public IEnumerator DisplayDice()
    {
        if(!diceA)
            diceA = spawnPool.Spawn("Dice");
        if(!diceB)
            diceB = spawnPool.Spawn("Dice");
        yield return 0;
    }

    //转色子
    public IEnumerator TurnDice(int A_Result, int B_Result)
    {
        if(!diceA)
            diceA = spawnPool.Spawn("Dice");
        if(!diceB)
            diceB = spawnPool.Spawn("Dice");
        diceA.position = viewObj.PathA[0].position;
        diceB.position = viewObj.PathB[0].position;
        
        iTween.MoveTo(diceA.gameObject, diceAHash);
        iTween.MoveTo(diceB.gameObject, diceBHash);

        float currentTime = Time.time + rollTime * .8f;
        //随便旋转 4/5个旋转时间
        while(currentTime > Time.time)
        {
            diceA.rotation = UnityEngine.Random.rotation;
            diceB.rotation = UnityEngine.Random.rotation;
            yield return 0;
        }
        //最后 1/5旋转时间 旋转至指定角度
        currentTime = Time.time + rollTime * .2f;
        while(currentTime > Time.time)
        {
            diceA.rotation = Quaternion.Lerp(diceA.rotation, SetDice(A_Result), Time.time / currentTime);
            diceB.rotation = Quaternion.Lerp(diceB.rotation, SetDice(B_Result), Time.time / currentTime);
            yield return 0;
        }
    }

    //清除色子
    public IEnumerator DisapperDice(float delayTime = 0)
    {
        yield return new WaitForSeconds(delayTime);
        if(diceA)
        {
            spawnPool.Despawn(diceA);
            diceA = null;
        }
        if(diceB)
        {
            spawnPool.Despawn(diceB);
            diceB = null;
        }

        yield return 0;
    }

    //开始发牌
    public IEnumerator AddHandCard(UserCard user, MahjongPrefab card)
    {

        //判断手牌是否已满并且牌库未空
        if(user.handCard.IsNotFull)
        {
            MahjongPrefab mahjong;
            //传入牌优先
            if(card.transform)
                mahjong = card;
            //默认取头牌
            else
                mahjong = group_H.GetMahjongCard();

            //检查牌库是否空了
            if(mahjong.transform)
            {
                Vector3 target = user.handCard.AddMahjong(mahjong);
                //先让牌提高一定高度（避免穿过其他牌获取）
                //随便计算个与目的地方向相同位置为三分一的高增加0.5f的坐标
                Vector3 temp = (target - mahjong.transform.position).normalized * (target - mahjong.transform.position).magnitude * .75f + new Vector3(mahjong.transform.position.x, mahjong.transform.position.y + .1f, mahjong.transform.position.z);
                //创建路径
                Hashtable arg = new Hashtable();
                Vector3[] path =
                {
                temp,target
            };
                arg.Add("path", path);
                arg.Add("time", .3f);
                arg.Add("easeType", iTween.EaseType.linear);
                //移动到分配位置
                iTween.MoveTo(mahjong.transform.gameObject, arg);
                yield return 0;
            } else
            {
                Debug.Log("麻将已经抽完");
            }
        } else
            Debug.Log("手牌已满");
    }

    //发完牌后为玩家显示手牌
    public IEnumerator DisplayCard(UserCard user,List<int> card = null)
    {
        for(int i = 0;i<user.handCard.Count;++i)
        {
            //只有在初始状态的牌才需要翻开
            if(user.handCard[i].animator.GetCurrentAnimatorStateInfo(0).fullPathHash == GlobalData.ANIMA_CardIdle)
            {
                if(card!=null)
                user.handCard[i].mesh.mesh = ResoucreMtr.Instance.GetMesh(card[i]);
                user.handCard[i].animator.Play(GlobalData.ANIMA_GetCard);
            }
        }
        yield return 0;
    }

    //整理手牌
    public IEnumerator SortCard(UserCard user, List<int> card)
    {
        for(int i = 0;i< user.handCard.Count; ++i)
        {
            user.handCard[i].mesh.mesh = ResoucreMtr.Instance.GetMesh(card[i]);
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
        if(!user.handleCard.IsEmpty)
            user.handleCard[0].animator.Play(GlobalData.ANIMA_TurnOverCard);
        yield return 0;
    }

    //摸牌 user：用户  card：指定要摸的牌
    public IEnumerator AddHandleCard(UserCard user, MahjongPrefab card)
    {
        if(user.handleCard.IsNotFull)
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
            //计算目的地距离三分二处高增加0.1f的坐标
            Vector3 temp = (target - mahjong.transform.position).normalized * (target - mahjong.transform.position).magnitude * .75f + new Vector3(mahjong.transform.position.x, mahjong.transform.position.y + .1f, mahjong.transform.position.z);
            //创建路径
            Hashtable arg = new Hashtable();
            Vector3[] path =
            {
                temp,target
            };
            arg.Add("path", path);
            arg.Add("time", .3f);
            arg.Add("easeType", iTween.EaseType.linear);
            //移动到分配位置
            iTween.MoveTo(mahjong.transform.gameObject, arg);
            yield return new WaitForSeconds(.3f);
        } else
            Debug.Log("手中的牌不能超过一个");
    }

    //将摸到的牌插入手牌中
    public IEnumerator InsertToHandCard(UserCard user, int index, MahjongPrefab mahjong)
    {
        Vector3 target = user.handCard.InsertAt(index, mahjong);

        //此处可播放特效start
        mahjong.animator.Play(GlobalData.ANIMA_InsertCard);
        yield return new WaitForSeconds(.1f);

        //特效end

        //开始移动到出牌区域
        iTween.MoveTo(mahjong.transform.gameObject, target, .4f);
        yield return new WaitForSeconds(.4f);
    }

    //添加明杠 或 暗杠
    public IEnumerator AddGang(UserCard user, CardActType type, MahjongPrefab card1, MahjongPrefab card2, MahjongPrefab card3, MahjongPrefab card4)
    {
        if(user.mingGangPoint.IsNotFull || user.anGangPoint.IsNotFull)
        {
            //预设
            Transform tran = null;
            //分配位置
            Vector3 target = new Vector3();

            //输入检查
            if(!card1.transform || !card2.transform || !card3.transform || !card4.transform)
                Debug.Log("<MGameClientAction::AddMingGang>:输入参数为空.");
            switch(type)
            {
                case CardActType.AnGang:
                    //获取预设
                    tran = spawnPool.Spawn("AnGang");
                    //加入容器并获取分配坐标
                    target = user.anGangPoint.AddItem(tran, GlobalData.AN_GANG_Width);
                    break;
                case CardActType.MingGang:
                    //获取预设
                    tran = spawnPool.Spawn("MingGang");
                    //加入容器并获取分配坐标
                    target = user.mingGangPoint.AddItem(tran, GlobalData.MING_GANG_Width);
                    break;
                default:
                    Debug.Log("<MGameClientAction::AddMingGang>:输入参数过多.");
                    break;
            }
            //只使用本地坐标x轴做动画
            tran.position = target + tran.right * .5f;

            //各个牌的位置
            Transform card1Point = tran.GetChild(0);
            Transform card2Point = tran.GetChild(1);
            Transform card3Point = tran.GetChild(2);
            Transform card4Point = tran.GetChild(3);

            //调整各个牌的缩放
            card1.transform.localScale = card1Point.localScale;
            card2.transform.localScale = card2Point.localScale;
            card3.transform.localScale = card3Point.localScale;
            card4.transform.localScale = card4Point.localScale;

            //下一个动画
            int card1Anima = CheckState(card1, type);
            int card2Anima = CheckState(card2, type);
            int card3Anima = CheckState(card3, type);
            int card4Anima = CheckState(card4, type);

            //播放动画
            if(card1Anima != -1)
                card1.animator.Play(card1Anima);
            if(card2Anima != -1)
                card2.animator.Play(card2Anima);
            if(card3Anima != -1)
                card3.animator.Play(card3Anima);
            if(card4Anima != -1)
                card4.animator.Play(card4Anima);

            //移动到预设位置
            iTween.MoveTo(card1.transform.gameObject, card1Point.position, .5f);
            iTween.MoveTo(card2.transform.gameObject, card2Point.position, .5f);
            iTween.MoveTo(card3.transform.gameObject, card3Point.position, .5f);
            iTween.MoveTo(card4.transform.gameObject, card4Point.position, .5f);

            //旋转角度调整至预设角度
            card1.transform.rotation = card1Point.rotation;
            card2.transform.rotation = card2Point.rotation;
            card3.transform.rotation = card3Point.rotation;
            card4.transform.rotation = card4Point.rotation;

            //等待预设移动
            yield return new WaitForSeconds(.5f);

            //绑定到为预设
            card1.transform.SetParent(card1Point);
            card2.transform.SetParent(card2Point);
            card3.transform.SetParent(card3Point);
            card4.transform.SetParent(card4Point);

            //开始预设动作

            //此处可播放特效start 

            //特效end
            //开始移动到明杠区域
            iTween.MoveTo(tran.gameObject, target, .5f);
            yield return new WaitForSeconds(.5f);
        } else
        {
            Debug.Log("<MGameClientAction::AddAnGang>: " + type.ToString() + "区域已满.");
        }
    }

    //添加碰 吃  角色 动作类型  牌1   牌2   牌3 
    public IEnumerator AddPengChi(UserCard user, CardActType type, MahjongPrefab card1, MahjongPrefab card2, MahjongPrefab card3)
    {
        if(user.mingGangPoint.IsNotFull)
        {
            //预设
            Transform tran = null;
            //分配位置
            Vector3 target = new Vector3();

            //输入检查
            if(!card1.transform || !card2.transform || !card3.transform)
                Debug.Log("<MGameClientAction::AddMingGang>:输入参数为空.");

            switch(type)
            {
                case CardActType.Peng:
                    //获取预设
                    tran = spawnPool.Spawn("Peng");
                    //加入容器并获取分配坐标
                    target = user.mingGangPoint.AddItem(tran, GlobalData.PENG_Width);
                    break;
                case CardActType.Chi:
                    //获取预设
                    tran = spawnPool.Spawn("Chi");
                    //加入容器并获取分配坐标
                    target = user.mingGangPoint.AddItem(tran, GlobalData.CHI_Width);
                    break;
                default:
                    Debug.Log("<MGameClientAction::AddPengChi>:输入参数过少.");
                    break;
            }
            //只使用本地坐标x轴做动画
            tran.position = target + tran.right * .5f;

            //各个牌的位置
            Transform card1Point = tran.GetChild(0);
            Transform card2Point = tran.GetChild(1);
            Transform card3Point = tran.GetChild(2);

            //调整各个牌的缩放
            card1.transform.localScale = card1Point.localScale;
            card2.transform.localScale = card2Point.localScale;
            card3.transform.localScale = card3Point.localScale;

            //下一个动画
            int card1Anima = CheckState(card1, type);
            int card2Anima = CheckState(card2, type);
            int card3Anima = CheckState(card3, type);

            //播放动画
            if(card1Anima != -1)
                card1.animator.Play(card1Anima);
            if(card2Anima != -1)
                card2.animator.Play(card2Anima);
            if(card3Anima != -1)
                card3.animator.Play(card3Anima);

            //移动到预设位置
            iTween.MoveTo(card1.transform.gameObject, card1Point.position, .5f);
            iTween.MoveTo(card2.transform.gameObject, card2Point.position, .5f);
            iTween.MoveTo(card3.transform.gameObject, card3Point.position, .5f);

            //旋转角度调整至预设角度
            card1.transform.rotation = card1Point.rotation;
            card2.transform.rotation = card2Point.rotation;
            card3.transform.rotation = card3Point.rotation;

            //等待预设移动
            yield return new WaitForSeconds(.5f);

            //绑定到为预设
            card1.transform.SetParent(card1Point);
            card2.transform.SetParent(card2Point);
            card3.transform.SetParent(card3Point);

            //开始预设动作


            //此处可播放特效start 

            //特效end
            //开始移动到明杠区域
            iTween.MoveTo(tran.gameObject, target, .5f);
            yield return new WaitForSeconds(.5f);
        } else
        {
            Debug.Log("<MGameClientAction::AddAnGang>: 明杠区域已满.");
        }
    }

    //特殊牌 & 功能牌
    public IEnumerator AddSpacialCard(UserCard user, MahjongPrefab card)
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
        } else
        {
            Debug.Log("<MGameClientAction::AddAnGang>: 功能牌区域已满.");
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

                //先让牌提高一定高度（避免穿过其他牌获取）
                //计算目的地距离三分一处高增加0.1f的坐标
                Vector3 temp = (target - card.transform.position).normalized * (target - card.transform.position).magnitude * .25f + new Vector3(card.transform.position.x, card.transform.position.y + .1f, card.transform.position.z);
                //创建路径
                Hashtable arg = new Hashtable();
                Vector3[] path =
                {
                temp,target
            };
                arg.Add("path", path);
                arg.Add("time", .3f);
                arg.Add("easeType", iTween.EaseType.linear);
                //动画
                card.animator.Play(GlobalData.ANIMA_OutCard);

                //使出牌区域的牌面向主角
                card.transform.rotation = qua;

                //特效end

                //开始移动到出牌区域
                iTween.MoveTo(card.transform.gameObject, arg);
                yield return new WaitForSeconds(.3f);
            }
        }
    }

    #region 回调重载 没什么卵用的函数模块
    //普通委托 
    public IEnumerator AddGroup(Callback callback)
    {
        yield return AddGroup();
        yield return new WaitForSeconds(1f);
        callback();
    }

    //迭代委托
    public IEnumerator AddGroup(IEnumeratorCallback callback)
    {
        yield return AddGroup();
        yield return new WaitForSeconds(1f);
        yield return callback();
    }


    public IEnumerator DisplayDice(Callback callback)
    {
        yield return DisplayDice();
        callback();
    }

    public IEnumerator DisplayDice(IEnumeratorCallback callback)
    {
        DisplayDice();
        yield return callback();
    }

    public IEnumerator TurnDice(int A_Result, int B_Result, Callback callback)
    {
        yield return TurnDice(A_Result, B_Result);
        callback();
    }

    public IEnumerator TurnDice(int A_Result, int B_Result, IEnumeratorCallback callback)
    {
        yield return TurnDice(A_Result, B_Result);
        yield return  callback();
    }

    public IEnumerator DisapperDice(Callback callback)
    {
        yield return DisplayDice();
        callback();
    }

    public IEnumerator DisapperDice(IEnumeratorCallback callback)
    {
        yield return DisplayDice();
        yield return callback();
    }

    public IEnumerator AddHandCard(UserCard user, MahjongPrefab card,Callback callback)
    {
        yield return AddHandCard(user, card);
        callback();
    }

    public IEnumerator AddHandCard(UserCard user, MahjongPrefab card,IEnumeratorCallback callback)
    {
        yield return AddHandCard(user, card);
        yield return callback();
    }

    public IEnumerator DisplayCard(UserCard user, Callback callback)
    {
        yield return DisplayCard(user);
        callback();
    }

    public IEnumerator DisplayCard(UserCard user, IEnumeratorCallback callback)
    {
        yield return DisplayCard(user);
        yield return callback();
    }

    public IEnumerator TurnOverCard(UserCard user, Callback callback)
    {
        yield return TurnOverCard(user);
        callback();
    }

    public IEnumerator TurnOverCard(UserCard user, IEnumeratorCallback callback)
    {
        yield return TurnOverCard(user);
        yield return callback();
    }

    public IEnumerator GetCard(UserCard user, MahjongPrefab card, Callback callback)
    {
        yield return AddHandleCard(user, card);
        callback();
    }

    public IEnumerator GetCard(UserCard user, MahjongPrefab card, IEnumeratorCallback callback)
    {
        yield return AddHandleCard(user, card);
        yield return callback();
    }

    public IEnumerator InsertToHandCard(UserCard user, int index, MahjongPrefab card, Callback callback)
    {
        yield return InsertToHandCard(user,index, card);
        callback();
    }

    public IEnumerator InsertToHandCard(UserCard user, int index, MahjongPrefab card, IEnumeratorCallback callback)
    {
        yield return InsertToHandCard(user,index, card);
        yield return callback();
    }

    public IEnumerator AddGang(UserCard user, CardActType type, MahjongPrefab card1, MahjongPrefab card2, MahjongPrefab card3, MahjongPrefab card4, Callback callback)
    {
        yield return AddGang(user, type, card1,card2,card3,card4);
        callback();
    }

    public IEnumerator AddGang(UserCard user, CardActType type, MahjongPrefab card1, MahjongPrefab card2, MahjongPrefab card3, MahjongPrefab card4, IEnumeratorCallback callback)
    {
        yield return AddGang(user, type, card1, card2, card3, card4);
        yield return callback();
    }

    public IEnumerator AddPengChi(UserCard user, CardActType type, MahjongPrefab card1, MahjongPrefab card2, MahjongPrefab card3, Callback callback)
    {
        yield return AddPengChi(user, type, card1, card2, card3);
        callback();
    }

    public IEnumerator AddPengChi(UserCard user, CardActType type, MahjongPrefab card1, MahjongPrefab card2, MahjongPrefab card3, IEnumeratorCallback callback)
    {
        yield return AddPengChi(user, type, card1, card2, card3);
        yield return callback();
    }

    public IEnumerator AddSpacialCard(UserCard user, MahjongPrefab card, Callback callback)
    {
        yield return AddSpacialCard(user, card);
        callback();
    }

    public IEnumerator AddSpacialCard(UserCard user, MahjongPrefab card, IEnumeratorCallback callback)
    {
        yield return AddSpacialCard(user, card);
        yield return callback();
    }

    public IEnumerator AddOutCard(UserCard user, MahjongPrefab card, Callback callback)
    {
        yield return AddOutCard(user, card);
        callback();
    }

    public IEnumerator AddOutCard(UserCard user, MahjongPrefab card, IEnumeratorCallback callback)
    {
        yield return AddOutCard(user, card);
        yield return callback();
    }

    #endregion

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

public struct UserCard
{
    public bool operateFlag;
    public Container handleCard;        //刚摸到的牌
    public Container handCard;          //手牌
    public Container mingGangPoint;     //明杠
    public Container anGangPoint;       //暗杠
    public Container spacialCard;       //功能牌
    public Container outCardPoint;      //出牌区域

    public UserCard(MahjongGrooves groove, Container outCardPoint)
    {
        operateFlag = false;
        handleCard = new Container(groove.handleCard, GlobalData.HANDLECARD_Count);
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

public enum CardActType
{
    Peng,
    Chi,
    MingGang,
    AnGang,
}