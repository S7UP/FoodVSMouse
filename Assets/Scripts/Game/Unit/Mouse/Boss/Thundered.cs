using System.Collections.Generic;
using System;
using UnityEngine;
/// <summary>
/// 轰隆隆
/// </summary>
public class Thundered : BossUnit
{
    // 技能组
    private CustomizationSkillAbility Skill_PushDown; // 高空压制
    private CustomizationSkillAbility Skill_Missile; // 导弹攻击
    private CustomizationSkillAbility Skill_Laser; // 毁灭激光

    public override void MInit()
    {
        base.MInit();

        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        // 添加出现的技能
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animatorController.Play("Appear");
            // 不可被子弹打中
            AddCanHitFunc(noHitFunc);
        };
        c.AddSpellingFunc(delegate {
            if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                return true;
            return false;
        });
        c.AfterSpellFunc = delegate
        {
            RemoveCanHitFunc(noHitFunc);
        };
        // 强制设制当前技能为这个
        mSkillQueueAbilityManager.SetCurrentSkill(c);
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    protected override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        foreach (var keyValuePair in BossManager.GetParamDict(BossNameTypeMap.Thundered, 0))
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
        
        list.Add(PushDownInit(infoList[0])); // 高空压制
        list.Add(MissileInit(infoList[1])); // 导弹发射
        list.Add(PushDownInit(infoList[0])); // 高空压制
        list.Add(LasterInit(infoList[2])); // 毁灭激光

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0.49f * MapManager.gridWidth, 0);
        mBoxCollider2D.size = new Vector2(1.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }


    public override void MUpdate()
    {
        base.MUpdate();
    }

    //////////////////////////////////////////////以下为BOSS技能定义////////////////////////////////////////////////////////////
    
    /// <summary>
    /// 高空压制
    /// </summary>
    private CustomizationSkillAbility PushDownInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_PushDown = c;
        bool isFly = false; // 是否处于飞行状态
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex)*60); // 升空时的观察时间
        int t0_1 = Mathf.FloorToInt(GetParamValue("t0_1", mHertIndex)*60); // 移动时间
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex)*60); // 下降前停留时间
        int t0_3 = Mathf.FloorToInt(GetParamValue("t0_3", mHertIndex)*60); // 下压完后原地停滞时间
        int timeLeft = 0;
        Vector2 startPos = Vector2.zero; // 起始点
        Vector2 targetPos = Vector2.zero; // 目标压制点
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isFly = false;
            timeLeft = 0;
            startPos = Vector2.zero;
            targetPos = Vector2.zero;
        };
        {
            // 上升
            c.AddSpellingFunc(delegate {
                animatorController.Play("Up");
                return true;
            });
            // 上升到一半半时将高度置为1，上升结束后动画设为空中待机
            c.AddSpellingFunc(delegate {
                if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>=0.5 && !isFly)
                {
                    isFly = true;
                    mHeight = 1;
                }else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t0_0;
                    return true;
                }
                return false;
            });
            // 在空中观察t0_0时间后确定目标点
            c.AddSpellingFunc(delegate
            {
                if(timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // 设置取随机算法，保证2*2压制的区域位于左一列到右三列之间
                    //SetNextGridIndexByRandom(0, 5, 1, 6);
                    //Vector2 v2 = GetNextGridIndex();
                    int rowIndex;
                    int colIndex;
                    FindR0C0(out rowIndex, out colIndex);
                    targetPos = MapManager.GetGridLocalPosition(colIndex, rowIndex);
                    startPos = transform.position;
                    if((targetPos - startPos).x <= 0)
                        animatorController.Play("FlyAhead", true);
                    else
                        animatorController.Play("FlyBack", true);
                    timeLeft = t0_1;
                    return true;
                }
                return false;
            });
            // 在空中移动t0_1时间后到达目标点
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    transform.position = targetPos + (startPos - targetPos) * timeLeft / t0_1;
                }
                else
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t0_2;
                    return true;
                }
                return false;
            });
            // 在空中停滞t0_2时间后准备下压
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    animatorController.Play("PushDown");
                    return true;
                }
                return false;
            });
            // 下压到一半半时将高度置为0，并标记为未升空，同时结算伤害判定
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isFly)
                {
                    isFly = false;
                    mHeight = 0;

                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position + 0.5f*new Vector3(MapManager.gridWidth, MapManager.gridHeight), 1.75f, 1.75f, "EnemyAllyGrid");
                    r.SetInstantaneous();
                    r.isAffectMouse = true;
                    r.SetOnEnemyEnterAction((u) => {
                        if (u.IsBoss())
                            return;
                        UnitManager.Execute(this, u);
                    });

                    r.isAffectGrid = true;
                    r.SetOnGridEnterAction((g) => {
                        g.TakeAction(this, (u) => {
                            DamageAction action = UnitManager.Execute(this, u);
                            new DamageAction(CombatAction.ActionType.CauseDamage, this, this, action.RealCauseValue * GetParamValue("dmg_trans0") / 100).ApplyAction();
                        }, false);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle0", true);
                    timeLeft = t0_3;
                    return true;
                }
                return false;
            });
            // 在停滞t0_3时间后结束该技能
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
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
    /// 导弹发射
    /// </summary>
    private CustomizationSkillAbility MissileInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_Missile = c;
        bool isFly = false; // 是否处于飞行状态
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex)*60); // 升空时的观察时间
        int t1_1 = Mathf.FloorToInt(GetParamValue("t1_1", mHertIndex)*60); // 移动时间
        
        int t1_2 = Mathf.FloorToInt(GetParamValue("t1_2", mHertIndex)*60); // 导弹发射完后的停滞时间

        int timeLeft = 0;
        int missileCounter = 0; // 导弹计数器

        Vector2 startPos = Vector2.zero; // 起始点
        Vector2 targetPos = Vector2.zero; // 目标点
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {
            timeLeft = 0;
            missileCounter = 0;
            isFly = false;
        };
        {
            // 上升
            c.AddSpellingFunc(delegate {
                animatorController.Play("Up");
                return true;
            });
            // 上升到一半半时将高度置为1，上升结束后动画设为空中待机
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && !isFly)
                {
                    isFly = true;
                    mHeight = 1;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t1_0;
                    return true;
                }
                return false;
            });
            // 在空中观察t1_0时间后确定目标点
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // 获取可作为目标的美食数最少的几行
                    List<int> rowList = FoodManager.GetRowListWhichHasMinCanTargetedAllyCount();
                    // 从中随机取一行（这个随机与BOSS情况完全无关也不影响当前BOSS情况的种子）
                    int selectRowIndex = rowList[GameController.Instance.GetRandomInt(0, rowList.Count)];
                    targetPos = new Vector2(MapManager.GetColumnX(8), MapManager.GetRowY(selectRowIndex));
                    startPos = transform.position;
                    if ((targetPos - startPos).x <= 0)
                        animatorController.Play("FlyAhead", true);
                    else
                        animatorController.Play("FlyBack", true);
                    timeLeft = t1_1;
                    return true;
                }
                return false;
            });
            // 在空中移动t1_1时间后到达目标点
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    transform.position = targetPos + (startPos - targetPos) * timeLeft / t1_1;
                }
                else
                {
                    animatorController.Play("Down");
                    return true;
                }
                return false;
            });
            // 下降到一半半时将高度置为0，并标记为未升空，然后发导弹
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isFly)
                {
                    isFly = false;
                    mHeight = 0;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Missile");
                    return true;
                }
                return false;
            });

            // 发导弹要分阶段
            if(mHertIndex == 0)
            {
                // 发一发导弹
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.3 && missileCounter == 0)
                    {
                        // 先去获取当前行最靠左的可攻击美食单位
                        BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
                        if (targetUnit == null)
                            CreateMissile(new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex())));
                        else
                            CreateMissile(targetUnit.transform.position);
                        missileCounter++;
                    }
                    else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle0", true);
                        timeLeft = t1_2;
                        return true;
                    }
                    return false;
                });
            }else if(mHertIndex == 1)
            {
                // 三发导弹落于同一位置
                c.AddSpellingFunc(delegate {
                    for (int i = 0; i < 3; i++)
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.3*(i+1) && missileCounter == i)
                        {
                            // 先去获取当前行最靠左的可攻击美食单位
                            BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
                            if (targetUnit == null)
                                CreateMissile(new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex())));
                            else
                                CreateMissile(targetUnit.transform.position);
                            missileCounter++;
                        }
                    }

                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle0", true);
                        timeLeft = t1_2;
                        return true;
                    }
                    return false;
                });
            }
            else
            {
                // 三发导弹落于三行
                c.AddSpellingFunc(delegate {
                    for (int i = 0; i < 3; i++)
                    {
                        if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.3 * (i + 1) && missileCounter == i)
                        {
                            if (i == 0)
                            {
                                // 先去获取当前行最靠左的可攻击美食单位
                                BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
                                if (targetUnit == null)
                                    targetPos = new Vector2(MapManager.GetColumnX(0), MapManager.GetRowY(GetRowIndex()));
                                else
                                    targetPos = targetUnit.transform.position;
                            }
                            CreateMissile(targetPos + Vector2.up * MapManager.gridHeight * (i - 1));
                            missileCounter++;
                        }
                    }

                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        animatorController.Play("Idle0", true);
                        timeLeft = t1_2;
                        return true;
                    }
                    return false;
                });
            }




            // 在停滞t1_2时间后结束该技能
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
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
    /// 发射一发导弹
    /// </summary>
    private void CreateMissile(Vector2 targetpos)
    {
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60); // 对人物的晕眩时间
        EnemyBullet b = EnemyBullet.GetInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/4/Missile"), this, 0);
        // 修改攻击优先级，这种投掷攻击优先攻击护罩里的东西
        b.GetTargetFunc = (unit) => {
            BaseGrid g = unit.GetGrid();
            if (g != null)
            {
                return g.GetThrowHighestAttackPriorityUnitInclude(this);
            }
            return unit;
        };
        b.AddHitAction((b, u) => {
            if (u is CharacterUnit)
            {
                // 如果击中人物，则为人物施加晕眩效果
                u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun1_0, false));
            }
            else
            {
                if (u != null && u.IsAlive())
                    BurnManager.BurnDamage(this, u);
            }
        });
        b.taskController.AddTask(TaskManager.GetParabolaTask(b, TransManager.TranToVelocity(48f), 1.5f, transform.position, targetpos, true));
        GameController.Instance.AddBullet(b);
    }

    /// <summary>
    /// 毁灭激光
    /// </summary>
    private CustomizationSkillAbility LasterInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_Laser = c;
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex)*60); // 升空时的观察时间
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex)*60); // 移动时间
        int t2_2 = Mathf.FloorToInt(GetParamValue("t2_2", mHertIndex)*60); // 激光蓄力时间
        int t2_3 = Mathf.FloorToInt(GetParamValue("t2_3", mHertIndex)*60); // 激光后原地停滞时间

        bool isFly = false; // 是否处于飞行状态
        int timeLeft = 0;

        Vector2 startPos = Vector2.zero; // 起始点
        Vector2 targetPos = Vector2.zero; // 目标点
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate {
            isFly = false;
            timeLeft = 0;
        };
        {
            // 上升
            c.AddSpellingFunc(delegate {
                animatorController.Play("Up");
                return true;
            });
            // 上升到一半半时将高度置为1，上升结束后动画设为空中待机
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && !isFly)
                {
                    isFly = true;
                    mHeight = 1;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle1", true);
                    timeLeft = t2_0;
                    return true;
                }
                return false;
            });
            // 在空中观察t2_0时间后确定目标点
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // 获取可作为目标的美食数最多的几行
                    List<int> rowList = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
                    // 从中随机取一行（这个随机与BOSS情况完全无关也不影响当前BOSS情况的种子）
                    int selectRowIndex = rowList[GameController.Instance.GetRandomInt(0, rowList.Count)];
                    targetPos = new Vector2(MapManager.GetColumnX(8), MapManager.GetRowY(selectRowIndex));
                    startPos = transform.position;
                    if ((targetPos - startPos).x <= 0)
                        animatorController.Play("FlyAhead", true);
                    else
                        animatorController.Play("FlyBack", true);
                    timeLeft = t2_1;
                    return true;
                }
                return false;
            });
            // 在空中移动t2_1时间后到达目标点
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    transform.position = targetPos + (startPos - targetPos) * timeLeft / t2_1;
                }
                else
                {
                    animatorController.Play("Down");
                    return true;
                }
                return false;
            });
            // 下降到一半半时将高度置为0，并标记为未升空，然后发激光
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isFly)
                {
                    isFly = false;
                    mHeight = 0;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Laser", true);
                    timeLeft = t2_2;
                    return true;
                }
                return false;
            });
            // 蓄力t2_2s后发射激光，然后停滞
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    // 搞一个激光特效
                    BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("BOSS/4/Laser"), null, "Laser", null, false);
                    e.transform.position = transform.position;
                    GameController.Instance.AddEffect(e);
                    // 一行灰烬效果
                    {
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(4.0f), MapManager.GetRowY(GetRowIndex())), 9, 0.5f, "EnemyAllyGrid");
                        r.SetInstantaneous();
                        r.isAffectFood = false;
                        r.isAffectCharacter = false;
                        r.isAffectMouse = true;
                        r.isAffectGrid = true;
                        r.SetOnGridEnterAction((g) => {
                            BaseUnit u = g.GetHighestAttackPriorityUnit(this);
                            if (u != null && !(u is CharacterUnit))
                                BurnManager.BurnDamage(this, u);
                        });
                        r.SetOnEnemyEnterAction((u) => {
                            BurnManager.BurnDamage(this, u);
                        });
                        r.AddExcludeMouseUnit(this);
                        GameController.Instance.AddAreaEffectExecution(r);
                    }

                    animatorController.Play("Idle0", true);
                    timeLeft = t2_3;
                    return true;
                }
                return false;
            });
            // 在停滞t2_3时间后结束该技能
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
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


    private void FindR0C0(out int rowIndex, out int colIndex)
    {
        // 然后找有最右边卡的行
        List<int> list = new List<int>();
        float max = float.MinValue;
        for (int row = 0; row < 7; row++)
        {
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(row, float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit != null)
            {
                if (unit.transform.position.x > max)
                {
                    max = unit.transform.position.x;
                    list.Clear();
                    list.Add(row);
                }
                else if (unit.transform.position.x == max)
                {
                    list.Add(row);
                }
            }
        }
        // 在这些行里随机挑一行吧
        if (list.Count > 0)
        {
            rowIndex = Mathf.Max(1, Mathf.Min(6, list[GetRandomNext(0, list.Count)]));
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit !=null)
                colIndex = Mathf.Max(0, Mathf.Min(5, unit.GetColumnIndex() - 1));
            else
                colIndex = 5;
        }
        else
        {
            // 这里是保险处理，真没有合适的结果的话那就固定四路出现吧
            GetRandomNext(0, 1);
            rowIndex = 3;
            BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(rowIndex, float.MinValue, MapManager.GetColumnX(6.5f), false);
            if (unit != null)
                colIndex = Mathf.Max(1, Mathf.Min(5, unit.GetColumnIndex() - 1));
            else
                colIndex = 5;
        }
    }
}
