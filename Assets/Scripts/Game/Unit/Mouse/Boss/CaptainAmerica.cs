using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 美队鼠
/// </summary>
public class CaptainAmerica : BossUnit
{
    // 技能组
    private CustomizationSkillAbility TeleportSkill; // 瞬间移动
    private CustomizationSkillAbility BoomerangShield; // 回旋圆盾
    private CustomizationSkillAbility ShieldStrike; // 圆盾拍击
    private CustomizationSkillAbility SummonSoldiers; // 召唤士兵

    // 盾牌子弹
    private EnemyBullet ShieldBullet;
    private static RuntimeAnimatorController ShieldBulletAnimaotrController;

    // 其他状态
    private bool isHasShield; // 是否持有盾牌
    private static int[] DropWaitTimeArray = new int[] { 300, 180, 60 }; // 丢盾牌前的蓄力时间（帧）
    private int DropWaitTimeLeft = 0; // 蓄力剩余时间
    private static int[] ShieldMovementTimeArray = new int[] { 480, 360, 240 }; // 盾牌运动时间
    private int ShieldMovementTimeLeft = 0; // 盾牌剩余运动时间
    private static int[] AfterBoomerangShieldWaitTimeArray = new int[] { 240, 120, 0 }; // 接到盾牌后站桩停滞时间
    private bool isPreDrop; // 是否在蓄力丢盾牌时间

    private List<Vector2> pathList = new List<Vector2>() // 盾牌的运动轨迹表
    {
        new Vector2(MapManager.GetColumnX(7), MapManager.GetRowY(6)),
        new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(6)),
        new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(0)),
        new Vector2(MapManager.GetColumnX(7), MapManager.GetRowY(0)),
    }; 

    private static int[] StrikeCountArray = new int[] { 2, 3, 4 }; // 圆盾拍击次数
    private static int[] StrikeWaitTimeArray = new int[] { 180, 90, 0 }; // 圆盾拍击结束后停滞时间

    private static int[] StandTimeArray = new int[] { 360, 240, 120 }; // 敬礼的站场时间
    private int StandTimeLeft = 0; // 经历站场剩余时间

    public override void Awake()
    {
        if (ShieldBulletAnimaotrController == null)
            ShieldBulletAnimaotrController = GameManager.Instance.GetRuntimeAnimatorController("Boss/20/ShieldBullet");
        base.Awake();
        ShieldBullet = transform.Find("ShieldBullet").GetComponent<EnemyBullet>();
    }

    public override void MInit()
    {
        isHasShield = true;
        isPreDrop = false;
        DropWaitTimeLeft = 0;
        ShieldMovementTimeLeft = 0;
        ShieldBullet.gameObject.SetActive(false);
        ShieldBullet.isnKillSelf = true;
        ShieldBullet.isAffectCharacter = false; // 不影响人
        base.MInit();
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        //skillAbilityManager.Initialize();
        TeleportInit();
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        
        list.Add(ShieldStrikeInit(infoList[2]));
        list.Add(BoomerangShieldInit(infoList[1]));
        list.Add(new IdleSkillAbility(this, AfterBoomerangShieldWaitTimeArray[mHertIndex])); // 接收盾牌后的站场时间
        list.Add(ShieldStrikeInit(infoList[2]));
        list.Add(SummonSoldiersInit(infoList[3]));

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }


    public override void MUpdate()
    {
        if(ShieldMovementTimeLeft>0)
            ShieldMovementTimeLeft--;
        base.MUpdate();
    }

    public override void OnIdleStateEnter()
    {

    }

    public override void OnMoveStateEnter()
    {
        
    }

    /// <summary>
    /// 准备扔盾牌时的动作
    /// </summary>
    public override void OnCastStateEnter()
    {
        animatorController.Play("PreDrop");
        DropWaitTimeLeft = DropWaitTimeArray[mHertIndex];
    }

    /// <summary>
    /// 在施法动作时计算CD减少
    /// </summary>
    public override void OnCastState()
    {
        if(DropWaitTimeLeft>0)
            DropWaitTimeLeft--;
    }

    /// <summary>
    /// 敬礼
    /// </summary>
    public override void OnTransitionStateEnter()
    {
        animatorController.Play("PreStand");
    }

    public override void OnTransitionState()
    {
        if (StandTimeLeft > 0)
            StandTimeLeft--;
    }

    /// <summary>
    /// 当处于蓄力丢盾牌状态下被炸弹攻击会被击晕3s
    /// </summary>
    /// <param name="dmg"></param>
    public override void OnBombBurnDamage(float dmg)
    {
        if (isPreDrop)
            AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(this, 180, true));
        base.OnBombBurnDamage(dmg);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    /// <summary>
    /// 回收圆盾子弹
    /// </summary>
    public override void BeforeDeath()
    {
        RecycleShieldBullet();
        base.BeforeDeath();
    }

    /// <summary>
    /// 回收圆盾子弹
    /// </summary>
    public override void ExecuteRecycle()
    {
        RecycleShieldBullet();
        base.ExecuteRecycle();
    }

    //////////////////////////////////////////////以下为BOSS技能定义////////////////////////////////////////////////////////////
    /// <summary>
    /// 瞬间移动
    /// </summary>
    private CustomizationSkillAbility TeleportInit()
    {
        ////
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        TeleportSkill = c;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate 
        {
            if (isHasShield)
                animatorController.Play("PreMove0");
            else
                animatorController.Play("PreMove1");
            CloseCollision();
        };
        {
            // 瞬移前摇
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    // 如果是移动前摇结束，则立即瞬移变更位置然后启动移动后摇
                    Vector2 v = GetNextGridIndex();
                    transform.position = MapManager.GetGridLocalPosition(v.x, v.y);
                    if (isHasShield)
                        animatorController.Play("PostMove0");
                    else
                        animatorController.Play("PostMove1");
                    return true;
                }
                return false;
            });
            // 前摇转后摇
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    return true;
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate 
        {
            if (isHasShield)
                animatorController.Play("Idle0", true);
            else
                animatorController.Play("Idle1", true);
            OpenCollision();
        };
        return c;
    }

    /// <summary>
    /// 回旋圆盾
    /// </summary>
    /// <param name="info"></param>
    private CustomizationSkillAbility BoomerangShieldInit(SkillAbility.SkillAbilityInfo info)
    {
        // 当前状态 0 瞬移 1 前摇 2 蓄力 3 后摇 4 瞬移 5 停滞 6 接收
        ////
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        BoomerangShield = c;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            SetNextGridIndex(8, 6); // 去7路
            c.ActivateChildAbility(TeleportSkill);
        };
        {
            // 瞬移转前摇
            c.AddSpellingFunc(delegate {
                if (true || !TeleportSkill.isSpelling)
                {
                    SetActionState(new CastState(this));
                    return true;
                }
                return false;
            });
            // 前摇转蓄力动作
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    isPreDrop = true;
                    animatorController.Play("Drop", true);
                    return true;
                }
                return false;
            });
            // 蓄力动作转后摇
            c.AddSpellingFunc(delegate {
                if (DropWaitTimeLeft <= 0)
                {
                    isPreDrop = false;
                    isHasShield = false;
                    animatorController.Play("PostDrop");
                    return true;
                }
                return false;
            });
            // 发射弹体帧
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>=0.5f)
                {
                    DropShieldBullet();
                    return true;
                }
                return false;
            });
            // 后摇结束后转瞬移
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    ShieldMovementTimeLeft = ShieldMovementTimeArray[mHertIndex];
                    SetNextGridIndex(8, 0); // 去1路
                    c.ActivateChildAbility(TeleportSkill);
                    return true;
                }
                return false;
            });
            // 瞬移到一路后转停滞， 准备接收盾牌
            c.AddSpellingFunc(delegate {
                if (ShieldMovementTimeLeft<=0)
                {
                    animatorController.Play("Recieve");
                    return true;
                }
                return false;
            });
            // 接收完盾牌后播放停滞动作同时退出该技能
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    isHasShield = true;
                    if (isHasShield)
                        animatorController.Play("Idle0", true);
                    else
                        animatorController.Play("Idle1", true);
                    return true;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate{ };
        return c;
    }

    /// <summary>
    /// 圆盾拍击技能定义
    /// </summary>
    private CustomizationSkillAbility ShieldStrikeInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        ShieldStrike = c;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {  };
        {
            for (int i = 0; i < StrikeCountArray[mHertIndex]; i++)
            {
                // 先瞬移
                c.AddSpellingFunc(delegate {
                    SetNextGridIndexByRandom(2, 8, 0, 6);
                    c.ActivateChildAbility(TeleportSkill);
                    return true;
                });
                // 瞬移转攻击
                c.AddSpellingFunc(delegate {
                    if (!TeleportSkill.isSpelling)
                    {
                        animatorController.Play("Attack");
                        return true;
                    }
                    return false;
                });
                // 攻击判定产生同时出判定
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.5f)
                    {
                        // 特效
                        BaseEffect effect = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/20/Strike"), null, "StrikeEffect", null, false);
                        effect.transform.position = transform.position;
                        GameController.Instance.AddEffect(effect);
                        // 对前方两格卡片造成900点伤害
                        DamageAreaEffectExecution dmgEffect = DamageAreaEffectExecution.GetInstance();
                        dmgEffect.Init(this, CombatAction.ActionType.CauseDamage, 900, GetRowIndex(), 2, 1, -1.5f, 0, true, false);
                        dmgEffect.transform.position = transform.position;
                        GameController.Instance.AddAreaEffectExecution(dmgEffect);
                        return true;
                    }
                    return false;
                });
                // 攻击结束后等待
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        c.ActivateChildAbility(new IdleSkillAbility(this, StrikeWaitTimeArray[mHertIndex]));
                        if (isHasShield)
                            animatorController.Play("Idle0", true);
                        else
                            animatorController.Play("Idle1", true);
                        return true;
                    }
                    return false;
                });
                // 这个是用来卡等待的
                c.AddSpellingFunc(delegate {
                    return true;
                });
            }
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 召唤士兵
    /// </summary>
    /// <param name="info"></param>
    /// <returns></returns>
    private CustomizationSkillAbility SummonSoldiersInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        SummonSoldiers = c;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate 
        {
            SetNextGridIndex(7, 3);
            c.ActivateChildAbility(TeleportSkill);
        };
        {
            // 瞬移转行礼前摇
            c.AddSpellingFunc(delegate {
                if (!TeleportSkill.isSpelling)
                {
                    StandTimeLeft = StandTimeArray[mHertIndex];
                    SetActionState(new TransitionState(this));
                    return true;
                }
                return false;
            });
            // 行礼前摇转行礼站桩
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Stand", true);
                    return true;
                }
                return false;
            });
            // 逐个召唤士兵
            {
                int max = 5;
                int i = 0;
                int interval = 12;
                int time = 0;
                c.AddSpellingFunc(delegate {
                    time++;
                    if (time == interval)
                    {
                        time = 0;
                        i++;
                        CreateEnemySpawner(6, i);
                    }
                    if (i == max)
                    {
                        // 再额外召唤两个航母
                        for (int i = 0; i < 2; i++)
                        {
                            GameController.Instance.CreateMouseUnit(1+4*i, new BaseEnemyGroup.EnemyInfo() { type = (int)MouseNameTypeMap.AirTransportMouse, shape = 0 });
                        }
                        return true;
                    }
                    return false;
                });
            }
            

            // 行礼站桩转行礼后摇
            c.AddSpellingFunc(delegate {
                if (StandTimeLeft <= 0)
                {
                    animatorController.Play("PostStand");
                    return true;
                }
                return false;
            });
            // 后摇转退出
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
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
    /// 扔出圆盾
    /// </summary>
    private void DropShieldBullet()
    {
        float s = 0;
        for (int i = 1; i < pathList.Count; i++)
        {
            s += (pathList[i] - pathList[i - 1]).magnitude;
        }
        float v = s/ShieldMovementTimeArray[mHertIndex];
        // 计算出每段的方向与时间
        List<Vector2> rotList = new List<Vector2>();
        List<int> timeList = new List<int>();
        for (int i = 1; i < pathList.Count; i++)
        {
            rotList.Add((pathList[i] - pathList[i - 1]).normalized);
            timeList.Add(Mathf.CeilToInt((pathList[i] - pathList[i - 1]).magnitude/v));
        }
        // 挂载一个任务在自己身上
        CustomizationTask task = new CustomizationTask();
        task.OnEnterFunc = delegate 
        {
            // 圆盾子弹出现
            CreateShieldBullet();
            // 设置位置为自身前方一格
            ShieldBullet.transform.localPosition = MapManager.gridWidth * Vector2.left;
            // 
            ShieldBullet.transform.SetParent(GameController.Instance.transform);
            GameController.Instance.AddBullet(ShieldBullet);
        };
        for (int i = 0; i < rotList.Count; i++)
        {
            Vector3 rot = rotList[i];
            int time = timeList[i];
            int t = 0;
            task.AddTaskFunc(delegate 
            {
                ShieldBullet.transform.position += v * rot;
                t++;
                if (t >= time)
                {
                    return true;
                }
                return false;
            });
        }
        task.OnExitFunc = delegate 
        {
            RecycleShieldBullet();
        };
        AddTask(task);
    }

    /// <summary>
    /// 创建圆盾子弹
    /// </summary>
    private void CreateShieldBullet()
    {
        ShieldBullet.gameObject.SetActive(true);
        ShieldBullet.MInit();
        ShieldBullet.SetRuntimeAnimatorController(ShieldBulletAnimaotrController);
        ShieldBullet.isAffectCharacter = false; // 不影响人
        ShieldBullet.isnKillSelf = true;
        ShieldBullet.SetDamage(900);
        ShieldBullet.animatorController.Play("Fly", true);
    }

    /// <summary>
    /// 回收圆盾子弹
    /// </summary>
    private void RecycleShieldBullet()
    {
        ShieldBullet.gameObject.SetActive(false);
        ShieldBullet.transform.SetParent(transform);
        ShieldBullet.transform.localPosition = Vector3.zero;
        GameController.Instance.RemoveBullet(ShieldBullet);
    }

    /// <summary>
    /// 产生一个敌人生成器
    /// </summary>
    private void CreateEnemySpawner(int xIndex, int yIndex)
    {
        CustomizationItem item = CustomizationItem.GetInstance(MapManager.GetGridLocalPosition(xIndex, yIndex), GameManager.Instance.GetRuntimeAnimatorController("Boss/20/RainBow"));
        CustomizationTask task = new CustomizationTask();
        task.OnEnterFunc = delegate 
        {
            item.animatorController.Play("RainBow");
        };
        task.AddTaskFunc(delegate 
        {
            // 生成怪
            if (item.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.8f)
            {
                GameController.Instance.CreateMouseUnit(xIndex, yIndex, new BaseEnemyGroup.EnemyInfo() { type=(int)MouseNameTypeMap.PandaMouse, shape = 1 });
                return true;
            }
            return false;
        });
        task.AddTaskFunc(delegate
        {
            // 等待结束跳出
            if (item.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        task.OnExitFunc = delegate 
        {
            item.ExecuteRecycle();
        };
        item.AddTask(task);
        GameController.Instance.AddItem(item);
    }
}
