using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 阿诺
/// </summary>
public class ANuo : BossUnit
{
    private static RuntimeAnimatorController Canister_AnimatorController; // 霰弹

    public override void Awake()
    {
        if (Canister_AnimatorController == null)
        {
            Canister_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/1/Canister");
        }
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();

        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        // 添加出现的技能
        {
            CompoundSkillAbility c = new CompoundSkillAbility(this);
            c.IsMeetSkillConditionFunc = delegate { return true; };
            c.BeforeSpellFunc = delegate
            {
                animatorController.Play("Enter");
            };
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                return false;
            });
            // 强制设制当前技能为这个
            mSkillQueueAbilityManager.SetCurrentSkill(c);
        }
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        // 读取参数
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.ANuo, 0))
        {
            AddParamArray(keyValuePair.Key, keyValuePair.Value);
        }
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        list.Add(CanisterInit(infoList[0])); // 霰弹枪
        list.Add(DragRacingInit(infoList[1])); // 飙车

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(1.99f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    /// <summary>
    /// 产生射弹
    /// </summary>
    private void CreateBullet(float dmg, float areaDmg, float childDmg)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Canister_AnimatorController, this, dmg);
        b.transform.position = transform.position + 0.5f * MapManager.gridWidth * Vector3.left;
        b.isAffectCharacter = false;
        b.isAffectFood = true;
        b.SetStandardVelocity(18);
        b.SetRotate(Vector2.left);
        // 击中后造成3*3爆炸
        b.AddHitAction(delegate {
            DamageAreaEffectExecution e = DamageAreaEffectExecution.GetInstance(this, b.transform.position, 3, 3, CombatAction.ActionType.CauseDamage, areaDmg);
            GameController.Instance.AddAreaEffectExecution(e);
        });
        GameController.Instance.AddBullet(b);

        // 添加一个任务，该任务记录射弹的路程，路程超过一格就会自爆并散开
        {
            float s = 0.75f*MapManager.gridWidth; // 路程
            Vector3 lastPos = b.transform.position; // 上一帧所在位置

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate { 
                if(s > 0)
                {
                    s -= (b.transform.position - lastPos).magnitude;
                    lastPos = b.transform.position;
                    return false;
                }
                else
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        CreateSmallBullet(b.transform.position, childDmg, i);
                    }
                    b.ExecuteRecycle();
                    return true;
                }
            });
            b.AddTask(t);
        }
    }

    /// <summary>
    /// 产生小射弹
    /// </summary>
    /// <param name="dmg"></param>
    private void CreateSmallBullet(Vector3 pos, float dmg, int i)
    {
        EnemyBullet b = EnemyBullet.GetInstance(Canister_AnimatorController, this, dmg);
        b.transform.position = pos;
        b.transform.localScale = Vector2.one * 0.75f;
        b.isAffectCharacter = false;
        b.isAffectFood = true;
        b.SetStandardVelocity(18);
        b.SetRotate(Vector2.left);
        // 添加一个纵向位移的任务
        b.AddTask(new StraightMovePresetTask(b.transform, MapManager.gridHeight / 15, 0, Vector3.up * i, MapManager.gridHeight));
        // GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridHeight / 15, 0, Vector3.up * i, MapManager.gridHeight));
        GameController.Instance.AddBullet(b);
    }

    /// <summary>
    /// 产生碾压友方的伤害判定区域
    /// </summary>
    /// <param name="pos"></param>
    private void CreateDamageAllyArea(Vector3 pos, float dmgRate)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 2, 1, "ItemCollideAlly");
        r.SetAffectHeight(0);
        r.isAffectFood = true;
        r.isAffectMouse = false;
        r.isAffectCharacter = false;
        r.SetInstantaneous();
        // 添加进入造成50%最大生命值伤害的效果，同时反弹相当于200%造成伤害给自己
        r.SetOnFoodEnterAction((u)=>{
            float dmg = 0.5f*u.mMaxHp;
            new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
            if(!u.NumericBox.GetBoolNumericValue(StringManager.Invincibility))
                new DamageAction(CombatAction.ActionType.ReboundDamage, u, this, dmg*dmgRate).ApplyAction();
        });
        GameController.Instance.AddAreaEffectExecution(r);
    }

    /// <summary>
    /// 产生碾压敌方的伤害判定区域
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="dmgRate"></param>
    private RetangleAreaEffectExecution CreateDamageEnemyArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f * MapManager.gridWidth * Vector3.right, 2, 1, "ItemCollideEnemy");
        r.SetAffectHeight(0);
        r.isAffectFood = false;
        r.isAffectMouse = true;
        r.isAffectCharacter = false;
        r.AddExcludeMouseUnit(this);
        // 添加进入造成伤害效果
        r.SetOnEnemyEnterAction((u) => {
            if (u.IsBoss())
                return;
            UnitManager.Execute(this, u);
        });
        GameController.Instance.AddAreaEffectExecution(r);
        // 添加跟随任务
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                r.transform.position = transform.position + 0.5f * MapManager.gridWidth * Vector3.right;
                return false;
            });
            r.AddTask(t);
        }

        return r;
    }

    ///////////////////////////////////////////////////////////////以下是BOSS的技能定义////////////////////////////////////////////////////

    /// <summary>
    /// 瞬移
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Move(Vector3 pos)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Disappear");
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    transform.position = pos;
                    animatorController.Play("Appear");
                    return true;
                }
                return false;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    return true;
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 寻找一个合适的开霰弹枪的位置
    /// 在右三列到右五列、第二行到第六行所围成的矩形内 所有 可攻击卡片数量最少的所有格子 中 随机选取某格的右一格作为目标格
    /// </summary>
    /// <returns></returns>
    private Vector3 FindGunPosition()
    {
        // l1：矩形范围的所有格子
        List<BaseGrid> l1 = GridManager.GetSpecificAreaGridList(GameController.Instance.mMapController.GetGridList(), MapManager.GetColumnX(4 - 0.5f), MapManager.GetColumnX(6 + 0.5f), MapManager.GetRowY(0.5f), MapManager.GetRowY(5.5f));
        // l2：可攻击卡片数量最少的所有格子
        List<BaseGrid> l2 = GridManager.GetGridListWhichHasMinCondition(l1, (g) => {
            return g.GetAttackableFoodUnitList().Count;
        });
        // 随机取一格，用的算法是BOSS自身的取随机数算法
        if(l2.Count > 0)
        {
            return l2[GetRandomNext(0, l2.Count)].transform.position + MapManager.gridWidth*Vector3.right;
        }
        else
        {
            // 真要是一格都没有的话。。。。那你随便在这个区域找个位置安分点吧，但是这种情况我认为不可能发生，除非你地图设计这块区域真就一格没有
            int xIndex = GetRandomNext(4, 6);
            int yIndex = GetRandomNext(1, 5);
            return MapManager.GetGridLocalPosition(xIndex, yIndex);
        }
    }


    /// <summary>
    /// 寻找一个适合飙车的位置
    /// 从七行中选取含有最靠右侧的可攻击卡片的一行作为目标行
    /// </summary>
    /// <returns></returns>
    private Vector3 FindDragRacingPosition()
    {
        // 判断方法：取当前行最靠右一个
        Func<BaseUnit, BaseUnit, bool> RowCompareFunc = (u1, u2) =>
        {
            if (u1 == null)
                return true;
            else if (u2 == null)
                return false;
            return u2.transform.position.x > u1.transform.position.x;
        };

        // 判断方法：取最靠右的
        Func<BaseUnit, BaseUnit, int> LastCompareFunc = (u1, u2) =>
        {
            if (u1 == null)
            {
                if (u2 == null)
                    return -1;
                else
                    return 1;
            }
            else if (u2 == null)
                return -1;

            if (u2.transform.position.x > u1.transform.position.x)
            {
                return 1;
            }
            else if (u2.transform.position.x == u1.transform.position.x)
            {
                return 0;
            }
            else
                return -1;
        };
        // 获取目标行
        List<int> list = FoodManager.GetRowListBySpecificConditions(RowCompareFunc, LastCompareFunc);
        if (list.Count > 0)
        {
            int selectedRow = list[GetRandomNext(0, list.Count)];
            return MapManager.GetGridLocalPosition(8, selectedRow);
        }
        else
        {
            // 我反正不能想象什么时候会出现这种情况
            // 也许哪天脑抽了没给地图分配行可能会出现吧
            GetRandomNext(0, 2);
            return MapManager.GetGridLocalPosition(8, 3);
        }
    }

    /// <summary>
    /// 开一次枪
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Shoot(float dmg, float areaDmg, float childDmg)
    {
        bool attackFlag = true;
        int num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // 连射次数
        int count = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Attack");
            num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // 连射次数(动态刷新！）
            count = 1;
            attackFlag = true;
        };
        {
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if (count >= num0_0)
                        return true;
                    else
                    {
                        animatorController.Play("Attack", false, 0);
                        count++;
                        attackFlag = true;
                        return false;
                    }
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.57f && attackFlag)
                {
                    // 生成射弹
                    CreateBullet(dmg, areaDmg, childDmg);
                    attackFlag = false;
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 霰弹枪
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility CanisterInit(SkillAbility.SkillAbilityInfo info)
    {
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex)*60); // 准备时间
        int num0_0 = Mathf.FloorToInt(GetParamValue("num0_0", mHertIndex)); // 连射次数
        float dmg0_0 = GetParamValue("dmg0_0", mHertIndex); // 单发大霰弹伤害
        float dmg0_1 = GetParamValue("dmg0_1", mHertIndex); // 霰弹爆破范围伤害
        float dmg0_2 = GetParamValue("dmg0_2", mHertIndex); // 单发小霰弹伤害
        int count0_0 = Mathf.FloorToInt(GetParamValue("count0_0", mHertIndex)); // 重复施放次数

        // 变量定义
        int timeLeft = 0;

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate{ };
        for (int j = 0; j <= count0_0; j++)
        {
            c.AddSpellingFunc(delegate
            {
                // 初始化变量
                timeLeft = 0;

                // 去找位置
                Vector3 v3 = FindGunPosition();
                // 然后瞬移到那个位置
                c.ActivateChildAbility(Move(v3));
                return true;
            });

            c.AddSpellingFunc(delegate
            {
                // 停滞，玩具车呀摇摇车
                animatorController.Play("Idle", true);
                timeLeft = t0_0;
                return true;
            });
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                else
                {
                    // 然后开枪了
                    return true;
                }
            });
            // 开枪
            c.AddSpellingFunc(delegate {
                c.ActivateChildAbility(Shoot(dmg0_0, dmg0_1, dmg0_2));
                return true;
            });
            // 占位（必须）
            c.AddSpellingFunc(delegate {
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 飙车
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility DragRacingInit(SkillAbility.SkillAbilityInfo info)
    {
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex)*60); // 飙车前摇
        float v1_0 = TransManager.TranToVelocity(GetParamValue("v1_0", mHertIndex)); // 速度
        float dmgRate1_0 = GetParamValue("dmgRate1_0", mHertIndex)/100; // 对自身反伤转化比率
        int moveTime = 4 * 60 - 55;
        int dmgAllyInterval = 20; // 每1/3秒进行一次范围伤害判定

        // 变量定义
        int timeLeft = 0;
        int dmgTimeLeft = 0;

        // 前进更新
        Action moveUpdate = delegate {
            // 前进更新（被阻挡时降低75%速度）
            if (IsBlock())
                transform.position += 0.75f * Vector3.left * v1_0;
            else
                transform.position += Vector3.left * v1_0;

            // 伤害判定
            if (dmgTimeLeft > 0)
                dmgTimeLeft--;
            else
            {
                // 产生碾压判定区
                CreateDamageAllyArea(transform.position + 0.5f * MapManager.gridWidth * Vector3.right, dmgRate1_0);
                dmgTimeLeft += dmgAllyInterval;
            }
        };

        RetangleAreaEffectExecution dmgEnemyArea = null; // 伤害敌人的判定区

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            dmgTimeLeft = 0;
            dmgEnemyArea = null;

            Vector3 v3 = FindDragRacingPosition(); // 去找一个合适的飙车的行
            c.ActivateChildAbility(Move(v3));
        };
        {
            // 翘起车子前摇
            c.AddSpellingFunc(delegate
            {
                animatorController.Play("PreDrag");
                return true;
            });
            // 翘起车子等待时间
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Drag", true);
                    timeLeft = t1_0;
                    return true;
                }
                return false;
            });
            // 翘起车子后摇
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    animatorController.Play("PostDrag");
                    return true;
                }
                return false;
            });
            // 前进前摇
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("PreMove");
                    timeLeft = moveTime;
                    // 生成伤害敌人的判定区
                    dmgEnemyArea = CreateDamageEnemyArea();
                    return true;
                }
                return false;
            });
            // 前进过程
            c.AddSpellingFunc(delegate {
                moveUpdate();
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Move", true);
                    return true;
                }
                return false;
            });
            // 前进时每帧更新
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // 飙车结束时删除对敌人的判定区
                    if (dmgEnemyArea != null)
                        dmgEnemyArea.MDestory();
                    animatorController.Play("PostMove", true);
                    return true;
                }
                moveUpdate();
                return false;
            });

            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                return false;
            });

            //c.AddSpellingFunc(delegate {
            //    if (timeLeft > 0)
            //    {
            //        timeLeft--;
            //    }
            //    else
            //    {
            //        return true;
            //    }
            //    return false;
            //});

            //c.AddSpellingFunc(delegate {
            //    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            //    {
            //        return true;
            //    }
            //    return false;
            //});

        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

}
