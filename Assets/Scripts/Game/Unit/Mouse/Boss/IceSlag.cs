using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 冰渣
/// </summary>
public class IceSlag : BossUnit
{
    // 专属效果
    private static string IceMagicKey = "冰渣的冰魔法";
    private static string FireMagicKey = "冰渣的火魔法";
    private Sprite Sprite_IceMagic;
    private Sprite Sprite_FireMagic;
    private RuntimeAnimatorController Run_IceBullet;
    private RuntimeAnimatorController Run_FireBullet;
    private RuntimeAnimatorController Run_LightBullet;
    private static BoolModifier IceMagic = new BoolModifier(true); // 冰魔法DEBUFF
    private static BoolModifier FireMagic = new BoolModifier(true); // 火魔法DEBUFF

    // 其他
    private bool isFireMode; // 是否为火魔法师形态（非黑即白，不是火就是冰）
    private List<BaseUnit> affectedUnitList = new List<BaseUnit>(); // 受魔法影响的单位表

    // 技能组
    private CustomizationSkillAbility Skill_CastBall; // 施放元素球
    private CustomizationSkillAbility Skill_LightHit; // 光击

    public override void Awake()
    {
        base.Awake();
        Sprite_IceMagic = GameManager.Instance.GetSprite("Boss/3/IceIcon");
        Sprite_FireMagic = GameManager.Instance.GetSprite("Boss/3/FireIcon");
        Run_IceBullet = GameManager.Instance.GetRuntimeAnimatorController("Boss/3/IceBullet");
        Run_FireBullet = GameManager.Instance.GetRuntimeAnimatorController("Boss/3/FireBullet");
        Run_LightBullet = GameManager.Instance.GetRuntimeAnimatorController("Boss/3/LightBullet");
    }

    public override void MInit()
    {
        affectedUnitList.Clear();
        isFireMode = true;
        base.MInit();

        Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate { return false; };
        // 添加出现的技能
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            if(isFireMode)
                animatorController.Play("Appear1");
            else
                animatorController.Play("Appear0");
            // 不可被子弹打中
            AddCanHitFunc(noHitFunc);
        };
        c.AddSpellingFunc(delegate {
            if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
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
    public override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });

        AddParamArray("dmg_boom", new float[] { 900, 900, 900 });
        // 施放元素球
        AddParamArray("t0_0", new float[] { 5, 3, 1.334f }); // 集气时间
        AddParamArray("t0_1", new float[] { 2f, 1.75f, 1.5f }); // 放完球后的原地停滞时间
        AddParamArray("t0_2", new float[] { 3, 4.5f, 6 }); // 球碾压和爆炸后对卡片造成晕眩时间
        AddParamArray("v0_0", new float[] { 3, 3, 3 }); // 元素球初速度
        AddParamArray("v0_1", new float[] { 6, 9, 12 }); // 元素球末速度
        // 光击
        AddParamArray("t1_0", new float[] { 3, 1.5f, 0 }); // 发光球前的蓄力时间
        AddParamArray("t1_1", new float[] { 2.25f, 2.25f, 1.5f }); // 光弹的攻击间隔
        AddParamArray("t1_2", new float[] { 1.5f, 0.75f, 0 }); // 光弹结束后停滞时间
        AddParamArray("num1_0", new float[] { 4, 3, 5 }); // 光弹数
        AddParamArray("num1_1", new float[] { 0, 1, 1 }); // 元素弹数
        AddParamArray("dmg1_0", new float[] { 900, 900, 900 }); // 光弹伤害
        AddParamArray("stun1_0", new float[] { 9, 9, 9 }); // 光弹对人物的晕眩时间
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        list.Add(CastBallInit(infoList[0])); // 搓球
        list.Add(LightHitInit(infoList[1])); // 光击

        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }


    public override void MUpdate()
    {
        base.MUpdate();
    }

    /// <summary>
    /// 为某个目标施加冰魔法debuff
    /// </summary>
    private void AddIceMagic(BaseUnit unit)
    {
        // 如果自己趋势了则不会施加魔法效果
        if (!IsAlive())
            return;

        float dmg_boom = GetParamValue("dmg_boom", mHertIndex);

        // 非自己的BOSS单位不受该BUFF影响
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss() && m != this)
                return;
        }

        // 如果目标已被施加火魔法，则造成爆炸效果并移除火魔法
        if (unit.NumericBox.GetBoolNumericValue(FireMagicKey))
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, null, unit, dmg_boom * (mCurrentAttack / 10)).ApplyAction();
            RemoveFireMagic(unit);
        }
        else if (!unit.NumericBox.GetBoolNumericValue(IceMagicKey))
        {
            // 否则施加冰魔法
            unit.NumericBox.AddDecideModifierToBoolDict(IceMagicKey, IceMagic);
            affectedUnitList.Add(unit);
            // 添加特效
            BaseEffect e = BaseEffect.CreateInstance(Sprite_IceMagic);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 4);
            }
            GameController.Instance.AddEffect(e);
            unit.AddEffectToDict(IceMagicKey, e, 0f*MapManager.gridHeight*Vector2.up);
        }
    }

    /// <summary>
    /// 移除冰魔法
    /// </summary>
    /// <param name="unit"></param>
    private void RemoveIceMagic(BaseUnit unit)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(IceMagicKey, IceMagic);
        affectedUnitList.Remove(unit);
        // 移除特效
        unit.RemoveEffectFromDict(IceMagicKey);
    }

    /// <summary>
    /// 为某个目标施加火魔法debuff
    /// </summary>
    private void AddFireMagic(BaseUnit unit)
    {
        // 如果自己趋势了则不会施加魔法效果
        if (!IsAlive())
            return;

        float dmg_boom = GetParamValue("dmg_boom", mHertIndex);

        // 非自己的BOSS单位不受该BUFF影响
        if (unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss() && m != this)
                return;
        }

        // 如果目标已被施加冰魔法，则造成爆炸效果并移除冰魔法
        if (unit.NumericBox.GetBoolNumericValue(IceMagicKey))
        {
            new BombDamageAction(CombatAction.ActionType.CauseDamage, null, unit, dmg_boom * (mCurrentAttack / 10)).ApplyAction();
            RemoveIceMagic(unit);
        }
        else if (!unit.NumericBox.GetBoolNumericValue(FireMagicKey))
        {
            // 否则施加火魔法
            unit.NumericBox.AddDecideModifierToBoolDict(FireMagicKey, FireMagic);
            affectedUnitList.Add(unit);
            // 添加特效
            BaseEffect e = BaseEffect.CreateInstance(Sprite_FireMagic);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 4);
            }
            GameController.Instance.AddEffect(e);
            unit.AddEffectToDict(FireMagicKey, e, 0f * MapManager.gridHeight * Vector2.up);
        }
    }

    /// <summary>
    /// 移除火魔法
    /// </summary>
    /// <param name="unit"></param>
    private void RemoveFireMagic(BaseUnit unit)
    {
        unit.NumericBox.RemoveDecideModifierToBoolDict(FireMagicKey, FireMagic);
        affectedUnitList.Remove(unit);
        // 移除特效
        unit.RemoveEffectFromDict(FireMagicKey);
    }

    public override void OnDieStateEnter()
    {
        if(isFireMode)
            animatorController.Play("Die1");
        else
            animatorController.Play("Die0");
    }

    /// <summary>
    /// 死亡时移除目标的冰火魔法
    /// </summary>
    public override void AfterDeath()
    {
        base.AfterDeath();
        List<BaseUnit> l = new List<BaseUnit>();
        foreach (var unit in affectedUnitList)
        {
            l.Add(unit);
        }
        foreach (var unit in l)
        {
            RemoveIceMagic(unit);
            RemoveFireMagic(unit);
        }
    }

    /// <summary>
    /// 目标的元素是否与自身相反
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    private bool IsOpposite(BaseUnit unit)
    {
        if ((isFireMode && unit.NumericBox.GetBoolNumericValue(IceMagicKey)) || (!isFireMode && unit.NumericBox.GetBoolNumericValue(FireMagicKey)))
            return true;
        return false;
    }

    //////////////////////////////////////////////以下为BOSS技能定义////////////////////////////////////////////////////////////

    /// <summary>
    /// 搓球球
    /// </summary>
    private CustomizationSkillAbility CastBallInit(SkillAbility.SkillAbilityInfo info)
    {
        // 判断方法：目标是与自身元素相异的可攻击友方单位
        Func<BaseUnit, bool> IsOppositeAndCanTargetedAlly = (unit) =>
        {
            if (unit is FoodUnit)
            {
                FoodUnit f = unit as FoodUnit;
                FoodInGridType t = f.GetFoodInGridType();
                if (IsOpposite(unit) && (t.Equals(FoodInGridType.Default) || t.Equals(FoodInGridType.Shield)))
                    return true;
            }
            return false;
        };

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_CastBall = c;
        bool isNoHit = false;
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60); // 集气时间
        int t0_1 = Mathf.FloorToInt(GetParamValue("t0_1", mHertIndex) * 60); // 放完球后的原地停滞时间
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex) * 60); // 球爆炸后对卡片造成晕眩时间

        int timeLeft = 0;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            isNoHit = false;
        };
        {
            // 消失动画
            c.AddSpellingFunc(delegate {
                if(isFireMode)
                    animatorController.Play("Disappear1");
                else
                    animatorController.Play("Disappear0");
                isNoHit = true;
                CloseCollision();
                return true;
            });
            // 结束确定瞬移位置然后出现
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    isFireMode = !isFireMode; // 切换模式
                    // 赋予自身 冰/火 魔法
                    if (isFireMode)
                    {
                        RemoveIceMagic(this);
                        AddFireMagic(this);
                    }
                    else
                    {
                        RemoveFireMagic(this);
                        AddIceMagic(this);
                    }

                    List<int> list = FoodManager.GetRowListWhichHasMaxConditionAllyCount(IsOppositeAndCanTargetedAlly);
                    if (list.Count < GameController.Instance.mAllyList.Length)
                    {
                        // 如果结果少于7行，那么就在里面随机取一行作为目标行
                        int selectRowIndex = list[GameController.Instance.GetRandomInt(0, list.Count)];
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }
                    else
                    {
                        // 如果发现结果是7行一致，那么可能的情况应该是全场都没有卡片被上魔法效果（即第一次施放该技能时）或者就是刚刚好每行数量一样多
                        // 在这种情况下，优先选取可作为攻击目标的卡片数量最多的行作为目标行
                        List<int> list2 = FoodManager.GetRowListWhichHasMaxCanTargetedAllyCount();
                        int selectRowIndex = list2[GameController.Instance.GetRandomInt(0, list2.Count)];
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }

                    if (isFireMode)
                        animatorController.Play("Appear1");
                    else
                        animatorController.Play("Appear0");
                    return true;
                }
                return false;
            });
            // 准备搓球球
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isNoHit)
                {
                    isNoHit = false;
                    OpenCollision();
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if (isFireMode)
                        animatorController.Play("Charge1", true);
                    else
                        animatorController.Play("Charge0", true);
                    timeLeft = t0_0;
                    return true;
                }
                return false;
            });
            // 搓t0_0时间后把球放出来，然后停滞
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                }
                else
                {
                    CreateBall();
                    if (isFireMode)
                        animatorController.Play("Idle1", true);
                    else
                        animatorController.Play("Idle0", true);
                    timeLeft = t0_1;
                    return true;
                }
                return false;
            });
            // 停滞完退出
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
    /// 光击
    /// </summary>
    private CustomizationSkillAbility LightHitInit(SkillAbility.SkillAbilityInfo info)
    {
        // 判断方法：取当前行最靠左一个
        Func<BaseUnit, BaseUnit, bool> RowCompareFunc = (u1, u2) =>
        {
            if (u1 == null)
                return true;
            else if (u2 == null)
                return false;
            return u2.transform.position.x < u1.transform.position.x;
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
            else if(u2 == null)
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

        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_LightHit = c;
        bool isNoHit = false;

        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex) * 60); // 发光球前的蓄力时间
        int t1_1 = Mathf.FloorToInt(GetParamValue("t1_1", mHertIndex) * 60); // 光弹的攻击间隔
        int t1_2 = Mathf.FloorToInt(GetParamValue("t1_2", mHertIndex) * 60); // 光弹结束过停滞时间
        int num1_0 = Mathf.FloorToInt(GetParamValue("num1_0", mHertIndex)); // 光弹数
        int num1_1 = Mathf.FloorToInt(GetParamValue("num1_1", mHertIndex)); // 元素弹数
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex); // 光弹伤害

        //int attack_time = Mathf.FloorToInt(animatorController.GetAnimatorStateRecorder("Attack0").aniTime); // 光弹的动作时间
        //int ExcuteDamage_time = Mathf.FloorToInt(0.5f*animatorController.GetAnimatorStateRecorder("Attack0").aniTime); // 光弹的发射触发时机
        int attack_time = 85; // 光弹的动作时间
        int ExcuteDamage_time = Mathf.FloorToInt(0.5f * attack_time); // 光弹的发射触发时机

        int n = 0;
        int timeLeft = 0;
        float minLeftX = 0; // 光弹最远可以打到的位置

        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            isNoHit = true;
            CloseCollision();
            n = 0;
            timeLeft = 0;
            minLeftX = MapManager.GetColumnX(0);
        };
        {
            // 消失动画
            c.AddSpellingFunc(delegate {
                if (isFireMode)
                    animatorController.Play("Disappear1");
                else
                    animatorController.Play("Disappear0");
                isNoHit = true;
                CloseCollision();
                return true;
            });
            // 结束确定瞬移位置然后出现
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    List<int> list = FoodManager.GetRowListBySpecificConditions(RowCompareFunc, LastCompareFunc);
                    int selectRowIndex;
                    if (list.Count == 0)
                    {
                        // 如果没有结果，就随机取一行吧
                        selectRowIndex = GameController.Instance.GetRandomInt(0, GameController.Instance.mAllyList.Length);
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }
                    else
                    {
                        // 否则在结果集里随机取一行
                        selectRowIndex = list[GameController.Instance.GetRandomInt(0, list.Count)];
                        transform.position = new Vector2(MapManager.GetColumnX(8.5f), MapManager.GetRowY(selectRowIndex));
                    }   

                    if (isFireMode)
                        animatorController.Play("Appear1");
                    else
                        animatorController.Play("Appear0");
                    return true;
                }
                return false;
            });
            // 蓄力
            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= 0.5 && isNoHit)
                {
                    isNoHit = false;
                    OpenCollision();
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    if (isFireMode)
                        animatorController.Play("Idle1", true);
                    else
                        animatorController.Play("Idle0", true);
                    timeLeft = t1_0;
                    return true;
                }
                return false;
            });
            // 
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
            // 每隔t1_1扔光弹
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    if(timeLeft == t1_1 - attack_time)
                    {
                        if (isFireMode)
                            animatorController.Play("Idle1", true);
                        else
                            animatorController.Play("Idle0", true);
                    }
                    else if(timeLeft == t1_1 - attack_time + ExcuteDamage_time)
                    {
                        // 发射弹体
                        CreateLightBullet(minLeftX);
                    }
                }else if(n < num1_0)
                {
                    n++;
                    timeLeft = t1_1;
                    if (isFireMode)
                        animatorController.Play("Attack1");
                    else
                        animatorController.Play("Attack0");
                }
                else if(n == num1_0)
                {
                    n = 0;
                    // timeLeft = t1_1;
                    //if (isFireMode)
                    //    animatorController.Play("Attack1");
                    //else
                    //    animatorController.Play("Attack0");
                    return true;
                }
                return false;
            });

            // 扔元素光弹，然后停滞
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    if (timeLeft == t1_1 - attack_time)
                    {
                        if (isFireMode)
                            animatorController.Play("Idle1", true);
                        else
                            animatorController.Play("Idle0", true);
                    }
                    else if (timeLeft == t1_1 - attack_time + ExcuteDamage_time)
                    {
                        // 发射弹体
                        CreateElementalBullet(minLeftX);
                    }
                }
                else if (n < num1_1)
                {
                    n++;
                    timeLeft = t1_1;
                    if (isFireMode)
                        animatorController.Play("Attack1");
                    else
                        animatorController.Play("Attack0");
                }else
                {
                    timeLeft = t1_2;
                    if(isFireMode)
                        animatorController.Play("Idle1", true);
                    else
                        animatorController.Play("Idle0", true);
                    return true;
                }
                return false;
            });
            // 停滞完退出
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
    /// 产生元素球
    /// </summary>
    private void CreateBall()
    {
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex) * 60); // 球爆炸后对卡片造成晕眩时间
        int v0_0 = Mathf.FloorToInt(GetParamValue("v0_0", mHertIndex));
        int v0_1 = Mathf.FloorToInt(GetParamValue("v0_1", mHertIndex));

        // 生成一个隐形挨炸的老鼠
        MouseUnit m = MouseManager.GetBombedToolMouse();

        string ball_res;
        bool isFireBall = isFireMode;
        // 对单位施加元素效果 以及 控制效果
        Action<BaseUnit> unitAction = (unit) =>
        {
            if (isFireBall)
            {
                AddFireMagic(unit);
                if(unit != m)
                    unit.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, t0_2, false));
            }
            else
            {
                AddIceMagic(unit);
                if (unit != m)
                    unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, t0_2, false));
            }
        };

        if (isFireMode)
            ball_res = "FireBall";
        else
            ball_res = "IceBall";



        {
            if (!isFireBall)
            {
                m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
                m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冰冻
            }
            m.NumericBox.Defense.SetBase(100); // 100%减伤

            // 添加一个出现、加速的任务
            CustomizationTask t = new CustomizationTask();
            float v0 = TransManager.TranToVelocity(v0_0);
            float v1 = TransManager.TranToVelocity(v0_1);
            int accTime = 180;
            float acc = (v1 - v0) / accTime;
            int timeLeft = accTime;
            t.AddOnEnterAction(delegate
            {
                m.transform.position = transform.position;
                m.SetMoveRoate(moveRotate);
            });
            // 加速
            t.AddTaskFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    v0 += acc;
                    m.SetPosition(m.GetPosition() + v0 * (Vector3)m.moveRotate);
                }
                m.SetPosition(m.GetPosition() + v0 * (Vector3)m.moveRotate);
                return false;
            });
            m.AddTask(t);

            // 再添加一个旋转和到达左四列爆炸的效果
            CustomizationTask t2 = new CustomizationTask();
            t2.AddTaskFunc(delegate {
                if (m.transform.position.x < MapManager.GetColumnX(3))
                    return true;
                // 如果目标被冰冻了且是火球则直接自爆
                if (isFireBall)
                {
                    if (m.GetNoCountUniqueStatus(StringManager.Frozen) != null)
                    {
                        return true;
                    }
                    else if (m.GetNoCountUniqueStatus(StringManager.Stun) != null)
                    {
                        // 清除晕眩效果
                        m.GetNoCountUniqueStatus(StringManager.Stun).ClearLeftTime();
                    }
                }
                return false;
            });
            t2.AddOnExitAction(delegate
            {
                m.ExecuteDeath();
            });
            m.AddTask(t2);
        }

        // 外形为球的敌方子弹
        EnemyBullet b = EnemyBullet.GetInstance(GameManager.Instance.GetRuntimeAnimatorController("Boss/3/"+ ball_res), this, 0);
        {
            b.CloseCollision(); // 球本质是个无判定的子弹，判定写在矩形范围效果里
            // 添加一个出现、加速的任务
            CustomizationTask t = new CustomizationTask();
            int totalTime = 30;
            int timeLeft = totalTime;
            t.AddOnEnterAction(delegate
            {
                b.transform.localScale = Vector2.zero;
                b.SetRotate(moveRotate);
            });
            // 变大
            t.AddTaskFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    b.transform.localScale = Vector2.one * (1 - (float)timeLeft/totalTime);
                }
                else
                {
                    return true;
                }
                return false;
            });
            b.AddTask(t);

            // 再添加一个跟随、旋转和到达左四列爆炸的效果
            CustomizationTask t2 = new CustomizationTask();
            float w0 = 0;
            float dw = -Mathf.Sign(moveRotate.x)*3.5f/180*Mathf.PI;
            t2.AddTaskFunc(delegate {
                if (!m.IsAlive())
                    return true;
                w0 += dw;
                b.transform.right = new Vector2(Mathf.Cos(w0), Mathf.Sin(w0));
                b.transform.position = m.transform.position;
                return false;
            });
            t2.AddOnExitAction(delegate
            {
                // 纠正方向
                b.transform.right = Vector2.right;
                // 自爆
                b.KillThis();
                // 产生爆炸效果
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 3, 3, "BothCollide");
                r.isAffectFood = true;
                r.isAffectMouse = true;
                r.SetOnFoodEnterAction(unitAction);
                r.SetOnEnemyEnterAction(unitAction);
                r.SetInstantaneous();
                GameController.Instance.AddAreaEffectExecution(r);
            });
            b.AddTask(t2);

            GameController.Instance.AddBullet(b);
        }
        
        // 该矩形范围效果能够吸收友方子弹、并对碾压的单位施加对应的元素效果，并且会跟随球体移动
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1, 1, "CollideTriple");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.isAffectBullet = true;
            r.SetOnFoodEnterAction(unitAction);
            r.SetOnEnemyEnterAction(unitAction);

            RetangleAreaEffectExecution r2 = RetangleAreaEffectExecution.GetInstance(b.transform.position, 1, 1, "Enemy");
            r2.isAffectBullet = true;
            r2.SetOnBulletEnterAction((bullet) => {
                // 摧毁接触的子弹
                bullet.TakeDamage(null);
                bullet.KillThis();
            });

            GameController.Instance.AddTasker(
            //Action InitAction,
                null,
            //Action UpdateAction, 
                delegate
                {
                // 每帧坐标同步跟随就行了
                    r.transform.position = b.transform.position;
                    r2.transform.position = b.transform.position;
                },
            //Func<bool> EndCondition, 
                delegate
                {
                // 如果绑定的子弹爆了，那么自己也会被摧毁
                    return !b.IsAlive();
                },
            //Action EndEvent
                delegate
                {
                    r.MDestory();
                    r2.MDestory();
                }
            );
            GameController.Instance.AddAreaEffectExecution(r);
            GameController.Instance.AddAreaEffectExecution(r2);
        }
    }

    /// <summary>
    /// 产生光弹
    /// </summary>
    private void CreateLightBullet(float minLeftX)
    {
        // 先去获取当前行最靠左的可攻击美食单位
        BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
        Vector3 targetPos;
        if (targetUnit == null)
            targetPos = new Vector2(minLeftX, MapManager.GetRowY(GetRowIndex()));
        else
            targetPos = new Vector2(Mathf.Max(minLeftX, targetUnit.transform.position.x), MapManager.GetRowY(GetRowIndex()));

        // 以下是产生弹体
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        int stun1_0 = Mathf.FloorToInt(GetParamValue("stun1_0", mHertIndex) * 60); // 对人物的晕眩时间
        RuntimeAnimatorController run = Run_LightBullet;
        float dmg = dmg1_0 * (mCurrentAttack / 10); // 最终打到人身上的伤害
        EnemyBullet b = EnemyBullet.GetInstance(run, this, 0);
        // 修改攻击优先级，这种投掷攻击优先攻击护罩里的东西
        b.GetTargetFunc = (unit) => {
            BaseGrid g = unit.GetGrid();
            if (g != null)
            {
                return g.GetThrowHighestAttackPriorityUnitInclude(this);
            }
            return unit;
        };
        b.AddHitAction((b, u)=> {
            if(u is CharacterUnit)
            {
                // 如果击中人物，则为人物施加晕眩效果
                u.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(u, stun1_0, false));
            }
            else
            {
                new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
            }
        });
        TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(48f), 1.5f, transform.position, targetPos, true);
        GameController.Instance.AddBullet(b);
    }

    /// <summary>
    /// 产生元素弹
    /// </summary>
    private void CreateElementalBullet(float minLeftX)
    {
        int t0_2 = Mathf.FloorToInt(GetParamValue("t0_2", mHertIndex) * 60); // 球爆炸后对卡片造成晕眩时间
        bool isFireBullet = isFireMode;

        // 对单位施加元素效果 以及 控制效果
        Action<BaseUnit> unitAction = (unit) =>
        {
            if (isFireBullet)
            {
                AddFireMagic(unit);
                unit.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, t0_2, false));
            }
            else
            {
                AddIceMagic(unit);
                unit.AddNoCountUniqueStatusAbility(StringManager.Frozen, new FrozenStatusAbility(unit, t0_2, false));
            }
        };

        // 先去获取当前行最靠左的可攻击美食单位
        BaseUnit targetUnit = FoodManager.GetSpecificRowFarthestLeftCanTargetedAlly(GetRowIndex(), transform.position.x, true);
        Vector3 targetPos;
        if (targetUnit == null)
            targetPos = new Vector2(minLeftX, MapManager.GetRowY(GetRowIndex()));
        else
            targetPos = new Vector2(Mathf.Max(minLeftX, targetUnit.transform.position.x), MapManager.GetRowY(GetRowIndex()));

        // 以下是产生弹体
        float dmg1_0 = GetParamValue("dmg1_0", mHertIndex);
        RuntimeAnimatorController run;
        if (isFireMode)
            run = Run_FireBullet;
        else
            run = Run_IceBullet;
        EnemyBullet b = EnemyBullet.GetInstance(run, this, 0);
        TaskManager.AddParabolaTask(b, TransManager.TranToVelocity(48f), 1.5f, transform.position, targetPos, true);
        GameController.Instance.AddBullet(b);
        // 添加击中时效果
        b.AddHitAction((bullet, unit) => {
            // 产生3*3赋予元素效果
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(b.GetColumnIndex()), MapManager.GetRowY(b.GetRowIndex())), 3, 3, "BothCollide");
            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetOnFoodEnterAction(unitAction);
            r.SetOnEnemyEnterAction(unitAction);
            r.SetInstantaneous();
            GameController.Instance.AddAreaEffectExecution(r);
        });
    }
}
