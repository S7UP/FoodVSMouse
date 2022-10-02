using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 列车终极
/// </summary>
public class RatTrain2 : BaseRatTrain
{
    private CustomizationSkillAbility SummonSoldiers; // 召唤士兵
    private CustomizationSkillAbility OrbitalMovement; // 定轨移动
    private CustomizationSkillAbility TimeBomb; // 定时炸弹

    private int leftBombCount; // 剩余炸弹车厢数（当进入该表的车厢不处于下怪与传送状态时会立即脱落并转换为定时炸弹）
    private float moveSpeedIncrease; // 移速提升值
    private FloatModifier moveSpeedModifier; // 移速提升标签
    private FloatModifier BlockedMoveSpeedModifier; // 二阶段被阻挡时移速改变标签

    // 其它状态
    private static int[] SpawnWaitTimeArray = new int[] { 300, 240, 180, 120 }; // 下怪后的等待站桩时间

    private float oilDistLeft = 0; // 离倒油还差多少距离

    public override void MInit()
    {
        moveSpeedIncrease = 0;
        moveSpeedModifier = null;
        leftBombCount = 0;
        oilDistLeft = 0;
        base.MInit();
        // 创建8节车厢
        CreateHeadAndBody(8);
        // 设置车头1倍伤害传递，车厢0.5倍伤害传递，车尾1倍普通伤害，10倍灰烬伤害传递
        GetHead().SetDmgRate(1.0f, 1.0f);
        foreach (var item in GetBodyList())
        {
            item.SetDmgRate(0.5f, 0.5f);
        }
        GetBody(GetBodyList().Count - 1).SetDmgRate(1.0f, 10f);
        // 动起来了
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(9f));
        BlockedMoveSpeedModifier = new FloatModifier(100*(TransManager.TranToVelocity(3f)/ NumericBox.MoveSpeed.Value)-100);
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        if (mHertIndex < 4)
        {
            // 狂暴之前
            SummonSoldiersInit(infoList[1]);
            OrbitalMovementInit(infoList[3]);
            List<SkillAbility> list = new List<SkillAbility>();
            list.Add(OrbitalMovement);
            mSkillQueueAbilityManager.ClearAndAddSkillList(list);
        }
        else if (mHertIndex == 4)
        {
            // 开始狂暴
            mSkillQueueAbilityManager.SetCurrentSkill(EnterRampageInit(SkillAbility.GetDefaultSkillInfoInstance("传送")));
            mSkillQueueAbilityManager.SetNextSkillIndex(0);
            List<SkillAbility> list = new List<SkillAbility>();
            list.Add(SupersonicRocketHead(SkillAbility.GetDefaultSkillInfoInstance("超音速火箭头")));
            mSkillQueueAbilityManager.ClearAndAddSkillList(list);
        }
    }

    public override void UpdateRuntimeAnimatorController()
    {
        base.UpdateRuntimeAnimatorController();
        // 总生命值每降低20%则会<准备脱落>最后一节车厢,并获得16.7%移动速度提升
        if (mHertIndex >= 1 && mHertIndex < 4)
        {
            if (GetBodyList().Count - leftBombCount>0)
            {
                SetPctAddMoveSpeed(moveSpeedIncrease + 16.7f);
                leftBombCount++;
            }
        }else if(mHertIndex == 4)
        {
            // 移动速度+100%
            SetPctAddMoveSpeed(100f);
            // 剩余车厢全部转化为炸弹
            int c = GetBodyList().Count;
            for (int i = 0; i < c; i++)
            {
                TimeBombAction();
            }
            leftBombCount = 0;
        }
    }

    /// <summary>
    /// 设置移动速度增加的百分比值
    /// </summary>
    private void SetPctAddMoveSpeed(float percent)
    {
        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        moveSpeedIncrease = percent;
        moveSpeedModifier = new FloatModifier(moveSpeedIncrease);
        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
        // 修改穿越的播放速度
        foreach (var item in GetAllRatTrainComponent())
        {
            item.animatorController.SetSpeed("Appear", 1.0f + (float)moveSpeedIncrease / 100);
            item.animatorController.SetSpeed("Disappear", 1.0f + (float)moveSpeedIncrease / 100);
        }
    }

    /// <summary>
    /// 在移动状态时会不断检测炸弹车厢的状态并将满足条件的车厢脱落
    /// </summary>
    public override void OnMoveState()
    {
        base.OnMoveState();
        // 若<准备脱落>的车厢数大于0，则在自身移动时不断检测最后一节车厢的条件，若满足则将其脱落并转化为定时炸弹
        while(leftBombCount > 0)
        {
            // 只要最后一节车厢未满足条件就停止检测更前面的车厢，此目的是防止偶然情况下前面的车厢满足条件脱落从而形成断裂
            RatTrainBody b = GetBody(GetBodyList().Count - 1);
            if(!(b.mCurrentActionState is TransitionState))
            {
                TimeBombAction();
                // 注：执行上面TimeBombAction()的方法时已使leftBombCount--了
            }
            else
            {
                break;
            }
        }
    }

    /// <summary>
    /// 到站下车（召怪）
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SummonSoldiersInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        SummonSoldiers = c;
        // 相关变量
        int WaitTime = SpawnWaitTimeArray[mHertIndex];
        int waitTimeLeft = 0; // 剩余等待时间
        List<RatTrainBody> bodyList = new List<RatTrainBody>();
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            waitTimeLeft = WaitTime;
            SetActionState(new IdleState(this));
            animatorController.Play("PreSpawn2");
            // 最后五节车厢执行下怪
            bodyList.Clear();
            int start = Mathf.Max(0, GetBodyList().Count - 1);
            int end = Mathf.Max(0, GetBodyList().Count - 5);
            for (int i = start; i >= end; i--)
            {
                if(GetBodyList()[i].mCurrentActionState is MoveState)
                {
                    GetBodyList()[i].animatorController.Play("PreSpawn2");
                    bodyList.Add(GetBodyList()[i]);
                }
            }
        };
        {
            // 下怪前摇
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Spawn2", true);
                    foreach (var item in bodyList)
                    {
                        if (item.IsAlive())
                        {
                            item.animatorController.Play("Spawn2", true);
                            SummonAction(item.transform.position, 2);
                            // 产生黄油加速带
                            for (int i = -1; i <= 1; i+=2)
                            {
                                for (int j = -1; j <= 1; j+=2)
                                {
                                    CreateOil(item.transform.position + MapManager.gridHeight * i * Vector3.up + 0.5f*MapManager.gridWidth * j * Vector3.right);
                                }
                                
                            }
                        }
                    }
                    return true;
                }
                return false;
            });
            // 下怪等待
            c.AddSpellingFunc(delegate {
                if (waitTimeLeft < 0)
                {
                    animatorController.Play("PostSpawn2");
                    foreach (var item in bodyList)
                    {
                        if (item.IsAlive())
                            item.animatorController.Play("PostSpawn2", true);
                    }
                    return true;
                }
                waitTimeLeft--;
                return false;
            });
            // 下怪后摇
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 直接润
                    SetActionState(new MoveState(this));
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 定轨移动
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility OrbitalMovementInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        OrbitalMovement = c;
        // 相关变量

        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isMoveToDestination = false;
            List<Vector2[]> list = new List<Vector2[]>()
            {
                new Vector2[]{ new Vector2(10, 1f), new Vector2(-1, 1f) },
            };
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
        };
        {
            // 是否抵达终点
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    c.ActivateChildAbility(SummonSoldiers);
                    return true;
                }
                return false;
            });
            // 召唤完士兵后继续前进
            c.AddSpellingFunc(delegate {
                if (!SummonSoldiers.isSpelling)
                {
                    List<Vector2[]> list = new List<Vector2[]>()
                    {
                        new Vector2[]{ new Vector2(-1, 3), new Vector2(10, 3) },
                        new Vector2[]{ new Vector2(10, 5f), new Vector2(-1, 5f) },
                    };
                    isMoveToDestination = false;
                    AddRouteListByGridIndex(list);
                    SetActionState(new MoveState(this));
                    return true;
                }
                return false;
            });
            // 是否抵达终点
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    isMoveToDestination = false;
                    c.ActivateChildAbility(SummonSoldiers);
                    return true;
                }
                return false;
            });
            // 召唤完士兵后继续前进
            c.AddSpellingFunc(delegate {
                if (!SummonSoldiers.isSpelling)
                {
                    List<Vector2[]> list = new List<Vector2[]>()
                    {
                        new Vector2[]{ new Vector2(-1, 3), new Vector2(10, 3) },
                    };
                    isMoveToDestination = false;
                    AddRouteListByGridIndex(list);
                    SetActionState(new MoveState(this));
                    return true;
                }
                return false;
            });
            // 是否抵达终点，抵达后退出，进入下一个循环
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 定时炸弹触发
    /// </summary>
    private void TimeBombAction()
    {
        leftBombCount--;
        int count = GetBodyList().Count;
        if (count > 0)
        {
            RatTrainBody b = GetBody(count - 1);
            CustomizationTask task = new CustomizationTask();
            RemoveTheEndOfBody();
            // 设置新的车尾1倍伤害，10倍灰烬伤害
            if(GetBodyList().Count >= 1)
                GetBody(GetBodyList().Count - 1).SetDmgRate(1.0f, 10f);
            b.SetMaster(null);
            // 如果列车不在传送状态，则转化为炸弹，否则直接消失
            if (!(b.mCurrentActionState is TransitionState))
            {
                task.OnEnterFunc = delegate
                {
                    // 丢弃的车厢任务
                    b.animatorController.Play("Bomb");
                };
                task.AddTaskFunc(delegate
                {
                    if (b.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        // 自爆动画播放，同时销毁3*3所有美食与老鼠单位，然后取消自身判定
                        b.animatorController.Play("BoomDie");
                        b.CloseCollision();
                        // 产生一个爆炸效果
                        BombAreaEffectExecution bombEffect = BombAreaEffectExecution.GetInstance();
                        bombEffect.Init(b, 3600, b.GetRowIndex(), 3, 3, 0, 0, true, true);
                        bombEffect.transform.position = b.transform.position;
                        bombEffect.SetOnEnemyEnterAction((m)=> m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 360, false)));
                        GameController.Instance.AddAreaEffectExecution(bombEffect);
                        return true;
                    }
                    return false;
                });
                // 动画播放完就可以消失了
                task.AddTaskFunc(delegate
                {
                    if (b.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
            }
            else
            {
                task.OnEnterFunc = delegate
                {

                };
                // 动画播放完就可以消失了
                task.AddTaskFunc(delegate
                {
                    if (b.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        return true;
                    }
                    return false;
                });
            }

            // 回收
            task.OnExitFunc = delegate {
                b.SetMaster(null);
                // 自爆动画播放完后直接回收
                b.DeathEvent();
            };
            b.AddTask(task);
        }
    }

    /// <summary>
    /// 进入狂暴状态
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility EnterRampageInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 相关变量

        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            ClearRouteList(); // 清空当前路径表
            ClearTransferQueue(); // 清空当前转移队列
            isMoveToDestination = false;
            // 原地消失
            AddRoute(new BaseRatTrain.RoutePoints()
            {
                start = GetHead().transform.position,
                end = GetHead().transform.position + (Vector3)moveRotate*0.01f,
            });
            SetActionState(new MoveState(this));
        };
        {
            // 是否抵达终点，抵达后退出，进入下一个循环
            c.AddSpellingFunc(delegate {
                if (isMoveToDestination)
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 超音速火箭头
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SupersonicRocketHead(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 相关变量
        bool lastBlock = false;
        // 攻击间隔
        int cd = 30;
        int cdLeft = cd;
        // 下怪间隔
        int spawnCd = 120;
        int spawnCdLeft = spawnCd;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isMoveToDestination = false;
            List<Vector2[]> list = new List<Vector2[]>()
                    {
                        new Vector2[]{ new Vector2(8, 6), new Vector2(-2, 6) }, // 7路从右到左
                        new Vector2[]{ new Vector2(0, 4), new Vector2(10, 4) }, // 5路从左到右
                        new Vector2[]{ new Vector2(8, 2), new Vector2(-2, 2) }, // 3路从右到左
                        new Vector2[]{ new Vector2(0, 0), new Vector2(10, 0) }, // 1路从左到右
                        new Vector2[]{ new Vector2(8, 1), new Vector2(-2, 1) }, // 2路从右到左
                        new Vector2[]{ new Vector2(0, 3), new Vector2(10, 3) }, // 4路从左到右
                        new Vector2[]{ new Vector2(8, 5), new Vector2(-2, 5) }, // 6路从右到左
                        // 和上面是镜像的
                        new Vector2[]{ new Vector2(0, 6), new Vector2(10, 6) }, // 7路从左到右
                        new Vector2[]{ new Vector2(8, 4), new Vector2(-2, 4) }, // 5路从右到左
                        new Vector2[]{ new Vector2(0, 2), new Vector2(10, 2) }, // 3路从左到右
                        new Vector2[]{ new Vector2(8, 0), new Vector2(-2, 0) }, // 1路从右到左
                        new Vector2[]{ new Vector2(0, 1), new Vector2(10, 1) }, // 2路从左到右
                        new Vector2[]{ new Vector2(8, 3), new Vector2(-2, 3) }, // 4路从右到左
                        new Vector2[]{ new Vector2(0, 5), new Vector2(10, 5) }, // 6路从左到右
                    };
            AddRouteListByGridIndex(list);
            SetActionState(new MoveState(this));
            GetHead().animatorController.Play("Dash", true); // 火车头直接冲刺
        };
        {
            // 是否抵达终点，抵达后退出，进入下一个循环
            c.AddSpellingFunc(delegate {
                // 如果被阻挡且阻挡目标不是人物，则使列车移速降低至 0.5格/s，并且每30帧（0.5秒）对阻挡者造成 20点伤害
                cdLeft--;
                bool currentBlock = GetHead().IsHasTarget() && !(GetHead().GetCurrentTarget() is CharacterUnit);
                if (currentBlock)
                {
                    if (cdLeft <= 0)
                    {
                        new DamageAction(CombatAction.ActionType.CauseDamage, this, GetHead().GetCurrentTarget(), 20.0f).ApplyAction();
                        cdLeft = cd;
                    }
                        
                    if (!lastBlock)
                    {
                        // 移速标签换成被阻挡的
                        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
                        NumericBox.MoveSpeed.RemovePctAddModifier(BlockedMoveSpeedModifier);
                        NumericBox.MoveSpeed.AddPctAddModifier(BlockedMoveSpeedModifier);
                    }
                }
                else
                {
                    if (lastBlock)
                    {
                        // 移速标签换成未阻挡的
                        NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
                        NumericBox.MoveSpeed.RemovePctAddModifier(BlockedMoveSpeedModifier);
                        NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
                    }
                }
                lastBlock = currentBlock;
                
                if(GetHead().mCurrentActionState is MoveState)
                {
                    GetHead().animatorController.Play("Dash", true); // 火车头直接冲刺
                    // 每走一格就在后方留下一个黄油
                    oilDistLeft -= GetMoveSpeed();
                    if(oilDistLeft <= 0)
                    {
                        CreateOil(GetHead().transform.position - (MapManager.gridWidth-oilDistLeft)*(Vector3)GetHead().moveRotate);
                        oilDistLeft += MapManager.gridWidth;
                    }
                    // 下怪
                    spawnCdLeft--;
                    if (spawnCdLeft <= 0)
                    {
                        if (GetHead().transform.position.x > MapManager.GetColumnX(1) && GetHead().transform.position.x < MapManager.GetColumnX(9))
                        {
                            SummonAction(GetHead().transform.position, 1);
                            spawnCdLeft = spawnCd;
                        }
                    }
                }   

                if (isMoveToDestination)
                {
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// 召唤士兵实体
    /// </summary>
    private void SummonAction(Vector2 center, int count)
    {
        for (int i = -(count-1); i <= count-1; i+=count)
        {
            int j = i;
            MouseUnit m = GameController.Instance.CreateMouseUnit(GetColumnIndex(), 0,
                new BaseEnemyGroup.EnemyInfo() { type = ((int)MouseNameTypeMap.NormalMouse), shape = 8 });
            // 确定初始坐标和最终坐标
            Vector2 startV2 = new Vector2(center.x, center.y);
            Vector2 endV2 = new Vector2(center.x, center.y + MapManager.gridHeight * j);

            CustomizationTask task = new CustomizationTask();
            int totalTime = 90;
            int currentTime = 0;
            task.OnEnterFunc = delegate
            {
                m.transform.position = startV2; // 对初始坐标进行进一步修正
                m.CloseCollision(); // 关闭判定
                m.SetAlpha(0); // 0透明度
            };
            task.AddTaskFunc(delegate
            {
                if(currentTime <= totalTime)
                {
                    currentTime++;
                    float t = (float)currentTime / totalTime;
                    m.SetPosition(Vector2.Lerp(startV2, endV2, t));
                    m.SetAlpha(t);
                    return false;
                }
                return true;
            });
            task.OnExitFunc = delegate
            {
                // 启用判定
                m.OpenCollision();
            };
            m.AddTask(task);
        }
    }

    /// <summary>
    /// 产生能使老鼠单位加速的黄油
    /// </summary>
    private void CreateOil(Vector2 pos)
    {
        // 产生黄油实体
        TimelinessShiftZone t = (TimelinessShiftZone)GameController.Instance.CreateItem(pos, (int)ItemNameTypeMap.ShiftZone, 0);
        t.SetLeftTime(900); // 持续15s
        t.SetChangePercent(100.0f); // 增加当前100%基础移速
        t.SetSkin(1);// 设置为黄油外观加速带
    }

    /// <summary>
    /// 是否狂暴
    /// </summary>
    /// <returns></returns>
    private bool IsRampage()
    {
        return mHertIndex >= 4;
    }
}
