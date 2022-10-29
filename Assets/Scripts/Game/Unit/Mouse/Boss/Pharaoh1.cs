using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// BOSS法老
/// </summary>
public class Pharaoh1 : BossUnit
{
    private const string CurseKey = "诅咒"; // 诅咒修饰
    private const string IgnoreCurseKey = "免疫诅咒"; // 免疫诅咒修饰
    private const string BandageKey = "绷带单位"; // 绷带单位修饰
    private FloatModifier CurseAddDamageModifier; // 诅咒伤害增加效果
    private static FloatModifier DecDmgModifier = new FloatModifier(0); // 转场时100%减伤效果
    private static FloatModifier DecDmgWhenClosedModifier = new FloatModifier(0); // 棺材合上时50%减伤效果
    private Func<BaseUnit, BaseBullet, bool> noHitFunc = (u, b) => { return false; }; // 不可被击中

    private static RuntimeAnimatorController[] selfAnimatorController = new RuntimeAnimatorController[3]; // 自身动画控制器
    private static RuntimeAnimatorController coffinAnimatorController; // 棺材的动画控制器
    private static RuntimeAnimatorController[] mummyAnimatorController = new RuntimeAnimatorController[2]; // 木乃伊的动画控制器
    private static RuntimeAnimatorController bandageAnimatorController; // 绷带的动画控制器
    private static RuntimeAnimatorController bugAnimatorController; // 圣甲虫的动画控制器
    private static RuntimeAnimatorController curseEffectAnimatorController; // 诅咒特效动画控制器

    private CustomizationSkillAbility Skill_PharaohCurse; // 法老之咒

    private List<BaseUnit> cursedUnit = new List<BaseUnit>(); // 被诅咒的单位
    private List<BaseUnit> bandageList = new List<BaseUnit>(); // 绷带单位

    private bool isDisappear; // 是否已消失（仅法老原型可用，用于检测移动时是否要做钻地动画的）
    private bool isReal; // 是否为真身型态

    public override void Awake()
    {
        if(coffinAnimatorController == null)
        {
            for (int i = 0; i < 3; i++)
            {
                selfAnimatorController[i] = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/0/"+i);
            }
            coffinAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Coffin");
            for (int i = 0; i < 2; i++)
            {
                mummyAnimatorController[i] = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Mummy/" + i);
            }
            bandageAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Bandage");
            bugAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Bug");
            curseEffectAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/2/Curse");
        }
        base.Awake();
    }

    public override void MInit()
    {
        cursedUnit.Clear();
        bandageList.Clear();
        isDisappear = true;
        isReal = false;
        base.MInit();
        animator.runtimeAnimatorController = selfAnimatorController[0];
        CurseAddDamageModifier = new FloatModifier(GetParamValue("AddDamageRate", 0));
        SetAlpha(0);
    }

    public override void MUpdate()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var unit in cursedUnit)
        {
            if (!unit.IsAlive())
                delList.Add(unit);
        }
        foreach (var unit in delList)
        {
            cursedUnit.Remove(unit);
        }

        delList = new List<BaseUnit>();
        foreach (var unit in bandageList)
        {
            if (!unit.IsAlive())
                delList.Add(unit);
        }
        foreach (var unit in delList)
        {
            bandageList.Remove(unit);
        }

        base.MUpdate();
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    public override void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.8f, 0.65f, 0.5f, 0.2f });
        // 通用机制
        AddParamArray("AddDamageRate", new float[] { 1 }); // 诅咒增伤倍率
        AddParamArray("KilledHp", new float[] { 50 }); // 来自诅咒的斩杀血线
        AddParamArray("DropDamage", new float[] { 900 }); // 棺材掉落伤害
        AddParamArray("StunTime", new float[] { 4 }); // 棺材掉落晕眩时间

        AddParamArray("LidHp", new float[] { 900 }); // 盒盖生命值
        AddParamArray("BoxHp", new float[] { 300 }); // 盒体生命值
        AddParamArray("LidOpenTime", new float[] { 3 }); // 盒盖揭开等待时间

        AddParamArray("MummyHp", new float[] { 100 }); // 木乃伊鼠
        AddParamArray("MummyAliveTime", new float[] { 18 }); // 木乃伊自行解体时间

        AddParamArray("BugHp", new float[] { 100 }); // 圣甲虫的生命值

        // 法老之咒
        AddParamArray("r0_0", new float[] { 5, 6, 7 }); // 法老之咒作用最大列（从右向左数）
        AddParamArray("t0_0", new float[] { 3, 1.5f, 0, float.NaN, float.NaN });
        // 神秘祭祀
        AddParamArray("count1_0", new float[] { 0, 0, 0, 1, 1 }); // 重复次数
        AddParamArray("t1_0", new float[] { 2, 1, 0, 2, 1 }); // 神秘祭祀停留时间
        AddParamArray("t1_1", new float[] { float.NaN, float.NaN, float.NaN, 3, 2 }); // 真身形态下神秘祭祀移动时间
        // 唤醒仪式
        AddParamArray("t2_0", new float[] { 6, 4, 2, 2, 0 }); // 唤醒仪式停留时间
        AddParamArray("t2_1", new float[] { float.NaN, float.NaN, float.NaN, 3, 2 }); // 真身形态下唤醒仪式移动时间
        // 法老王之咒
        AddParamArray("t3_0", new float[] { float.NaN, float.NaN, float.NaN, 3, 2 }); // 法老王之咒回到中场的移动时间
        AddParamArray("t3_1", new float[] { float.NaN, float.NaN, float.NaN, 2, 0 }); // 法老王之咒停滞时间
    }

    /// <summary>
    /// 加载技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        List<SkillAbility> list = new List<SkillAbility>();

        // 法老切换为真身的过场效果
        if(mHertIndex == 3)
        {
            // 获得100%减伤
            NumericBox.DamageRate.AddModifier(DecDmgModifier);
            ChangeToRealMode(); // 添加切换为真身的任务
            mSkillQueueAbilityManager.SetNextSkillIndex(-1); // 技能下标要置-1
        }

        if(mHertIndex < 3)
        {
            isReal = false;
            // 原型技能组
            list.Add(PharaohCurseInit(infoList[0])); // 法老之咒
            list.Add(MysticalSacrificesInit(infoList[1])); // 神秘祭祀
            list.Add(AwakeningCeremonyInit(infoList[2])); // 唤醒仪式
            list.Add(MysticalSacrificesInit(infoList[1])); // 神秘祭祀
        }
        else
        {
            isReal = true;
            // 真身技能组
            list.Add(MysticalSacrificesInit(infoList[1])); // 神秘祭祀
            list.Add(PharaohKingCurseInit(infoList[3])); // 法老王之咒
            list.Add(MysticalSacrificesInit(infoList[1])); // 神秘祭祀
            list.Add(AwakeningCeremonyInit(infoList[2])); // 唤醒仪式
        }
        mSkillQueueAbilityManager.ClearAndAddSkillList(list);
    }

    /// <summary>
    /// 切换成真身
    /// </summary>
    private void ChangeToRealMode()
    {
        // 添加出现的技能
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            animator.runtimeAnimatorController = selfAnimatorController[1];
            animatorController.Play("Appear");
        };
        c.AddSpellingFunc(delegate {
            if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                return true;
            }
            return false;
        });
        c.AfterSpellFunc = delegate
        {
            animatorController.Play("Idle", true);
            // 移除减伤
            NumericBox.DamageRate.RemoveModifier(DecDmgModifier);
        };
        // 强制设制下一个技能为这个（转场）
        mSkillQueueAbilityManager.SetNextSkill(c);
    }

    /// <summary>
    /// 设置判定参数
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.98f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void OnDieStateEnter()
    {
        if (!IsRealStage())
            animatorController.Play("Disappear");
        else
            animatorController.Play("Die");
    }

    /// <summary>
    /// 是否为法老真身阶段
    /// </summary>
    /// <returns></returns>
    private bool IsRealStage()
    {
        return isReal;
    }

    /// <summary>
    /// 召唤木乃伊鼠
    /// </summary>
    private void SummonMummy(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(mummyAnimatorController, new float[] { 0.5f });
        m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // 免疫诅咒
        m.SetBaseAttribute(GetParamValue("MummyHp", 0), 10, 0.5f, 0.75f, 0, 0.9f, 0);
        m.transform.position = pos;
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.SetActionState(new MoveState(m));

        CustomizationTask t = new CustomizationTask();
        int aliveTimeLeft = Mathf.FloorToInt(GetParamValue("MummyAliveTime", 0) * 60);
        t.AddTaskFunc(delegate{
            if (aliveTimeLeft > 0)
                aliveTimeLeft--;
            else
            {
                // 时间一到就自毁
                m.ExecuteDeath();
                return true;
            }
            if (m.IsBlock())
                AddCurse(m.GetCurrentTarget());
            return false;
        });
        m.AddTask(t);
        GameController.Instance.AddMouseUnit(m);
    }

    /// <summary>
    /// 召唤一个棺材
    /// </summary>
    private void SummonCoffin(Vector2 pos)
    {
        float LidHp = GetParamValue("LidHp", 0);
        float BoxHp = GetParamValue("BoxHp", 0);

        MouseModel m = MouseModel.GetInstance(coffinAnimatorController);
        m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // 免疫诅咒
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
        m.NumericBox.AddDecideModifierToBoolDict(StringManager.BeFrightened, new BoolModifier(true)); // 免疫猫枪惊吓
        m.SetBaseAttribute(LidHp + BoxHp, 0, 1, 0, 0, 0, 0);
        m.transform.position = pos;
        m.currentYIndex = MapManager.GetYIndex(pos.y);
        m.CanBlockFuncList.Add(delegate { return false; }); // 棺材不可阻挡所以也不会攻击
        GameController.Instance.AddMouseUnit(m);

        CustomizationTask t = new CustomizationTask();
        bool dropFlag = true;
        bool isSummon = false; // 是否已经生成木乃伊鼠
        int timeLeft = Mathf.FloorToInt(GetParamValue("LidOpenTime", 0)*60);
        Func<BaseUnit, BaseBullet, bool> noHitFunc = (unit, bullet) => { return false; };

        int stunTime = Mathf.FloorToInt(GetParamValue("StunTime", 0) * 60);
        // 晕眩单位
        Action<BaseUnit> stunUnitAction = (unit) =>
        {
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(unit, stunTime, false));
        };

        t.OnEnterFunc = delegate
        {
            m.SetActionState(new IdleState(m));
            // 从天上掉下来
            m.animatorController.Play("Drop");
            m.CanHitFuncList.Add(noHitFunc); // 起初不可击中
        };
        t.AddTaskFunc(delegate {
            if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                m.animatorController.Play("Idle", true);
                return true;
            }
            else if(m.animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime()>0.33f && dropFlag)
            {
                m.CanHitFuncList.Remove(noHitFunc); // 可击中
                // 对自身为中心0.5格的单位造成范围伤害 并击晕
                DamageAreaEffectExecution e = DamageAreaEffectExecution.GetInstance();
                e.Init(m, CombatAction.ActionType.CauseDamage, GetParamValue("DropDamage", 0), GetRowIndex(), 0.5f, 0.5f, 0, 0, true, true);
                e.transform.position = m.transform.position;
                e.SetOnFoodEnterAction(stunUnitAction);
                e.SetOnEnemyEnterAction(stunUnitAction);
                e.AddExcludeMouseUnit(m); // 自身要被排除在外，不然伤敌八百自损一千
                GameController.Instance.AddAreaEffectExecution(e);
                dropFlag = false;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            if (timeLeft > 0 || m.GetCurrentHp()<=BoxHp)
                timeLeft--;
            else
            {
                if (m.GetCurrentHp() > BoxHp)
                    m.SetMaxHpAndCurrentHp(BoxHp);
                else
                    m.NumericBox.Hp.SetBase(BoxHp);
                m.animatorController.Play("Open");
                return true;
            }
            return false;
        });
        t.AddTaskFunc(delegate
        {
            if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            {
                // 此处真正生成木乃伊鼠
                isSummon = true;
                SummonMummy(m.transform.position);
                m.animatorController.Play("Idle1", true);
                return true;
            }
            return false;
        });
        m.AddTask(t);
        // 如果在没有生成木乃伊鼠的情况下棺材就爆了，那么在爆炸时生成木乃伊鼠
        m.AddBeforeDeathEvent(delegate {
            if (!isSummon)
            {
                SummonMummy(m.transform.position);
            }
        });
    }

    /// <summary>
    /// 当被诅咒的单位受到伤害后的判定
    /// </summary>
    /// <param name="unit"></param>
    private void AfterDamageWhenCursed(CombatAction action)
    {
        BaseUnit unit = action.Target;
        if(unit!=null && unit.IsAlive() && unit.GetCurrentHp() < GetParamValue("KilledHp", 0))
        {
            unit.ExecuteDeath();
        }
    }

    private void BeforeDeathWhenCursed(BaseUnit unit)
    {
        if(unit.transform.position.x > MapManager.GetColumnX(3))
            SummonCoffin(unit.transform.position); // 原地生成一个棺材
    }

    /// <summary>
    /// 为单位施加诅咒效果
    /// </summary>
    /// <param name="unit"></param>
    private void AddCurse(BaseUnit unit)
    {
        // 过滤条件
        if (!IsAlive() || unit is CharacterUnit || unit.NumericBox.GetBoolNumericValue(IgnoreCurseKey) || cursedUnit.Contains(unit))
            return;
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return;
        }

        // 以下为施加方法
        unit.NumericBox.DamageRate.AddModifier(CurseAddDamageModifier); // 受到伤害增加
        //unit.AddActionPointListener(ActionPointType.PostReceiveDamage, AfterDamageWhenCursed);
        //unit.AddActionPointListener(ActionPointType.PostReceiveReboundDamage, AfterDamageWhenCursed);
        unit.AddBeforeDeathEvent(BeforeDeathWhenCursed);
        cursedUnit.Add(unit);
        // 为目标施加诅咒特效
        BaseEffect e = BaseEffect.CreateInstance(curseEffectAnimatorController, "Appear", "Idle", "Disappear", true);
        unit.AddEffectToDict(CurseKey, e, 0f * MapManager.gridHeight * Vector2.up);
        GameController.Instance.AddEffect(e);
    }

    /// <summary>
    /// 移除单位的诅咒效果
    /// </summary>
    /// <param name="unit"></param>
    private void RemoveCurse(BaseUnit unit)
    {
        unit.NumericBox.DamageRate.RemoveModifier(CurseAddDamageModifier);
        //unit.RemoveActionPointListener(ActionPointType.PostReceiveDamage, AfterDamageWhenCursed);
        //unit.RemoveActionPointListener(ActionPointType.PostReceiveReboundDamage, AfterDamageWhenCursed);
        unit.RemoveBeforeDeathEvent(BeforeDeathWhenCursed);
        cursedUnit.Remove(unit);
        // 为目标移除诅咒特效
        unit.RemoveEffectFromDict(CurseKey);
    }

    /// <summary>
    /// 召唤圣甲虫
    /// </summary>
    private void SummonBug(Vector2 pos)
    {
        MouseModel m = MouseModel.GetInstance(bugAnimatorController);
        m.typeAndShapeValue = -1;
        m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // 免疫诅咒
        m.SetBaseAttribute(GetParamValue("BugHp", 0), 900, 1.0f, 0f, 0, 0.5f, 0);
        m.transform.position = pos;
        m.currentYIndex = MapManager.GetYIndex(pos.y);


        CustomizationTask t = new CustomizationTask();
        int accTimeLeft = 240; // 加速时间
        float acc = TransManager.TranToVelocity(6)/accTimeLeft;
        t.OnEnterFunc = delegate
        {
            m.SetActionState(new MoveState(m));
            // 自我晕眩2秒
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 120, false));
        };
        t.AddTaskFunc(delegate {
            // 这个任务会在被晕眩或冰冻时暂停
            if (m.isFrozenState)
                return false;

            if(accTimeLeft > 0)
            {
                // 加速
                m.NumericBox.MoveSpeed.SetBase(m.mBaseMoveSpeed + acc);
                accTimeLeft--;
            }
            // 如果被阻挡了，则对阻挡者造成900点伤害然后自己死亡(对人物无效）
            if (m.IsBlock())
            {
                BaseUnit target = m.GetCurrentTarget();
                if(target !=null && !(target is CharacterUnit)){
                    m.TakeDamage(target);
                    m.ExecuteDeath();
                }
            }
            return false;
        });
        m.AddTask(t);

        int decDmgTimeLeft = 240;
        FloatModifier decDmgModifier = new FloatModifier(0.01f);
        GameController.Instance.AddTasker(
            //Action InitAction, 
            delegate {
                m.NumericBox.DamageRate.AddModifier(decDmgModifier); // 获得减伤
            },
            //Action UpdateAction, 
            delegate {
                decDmgTimeLeft--;
            },
            //Func<bool> EndCondition, 
            delegate { return decDmgTimeLeft <= 0 || !m.IsAlive(); },
            //Action EndEvent
            delegate {
                m.NumericBox.DamageRate.RemoveModifier(decDmgModifier); // 移除减伤
            }
            );

        GameController.Instance.AddMouseUnit(m);
    }

    /// <summary>
    /// 创建绷带区域
    /// </summary>
    public void CreateBandageArea(BaseGrid masterGrid)
    {
        Vector2 pos = masterGrid.transform.position;
        // 生成绷带蛋
        MouseModel m = MouseModel.GetInstance(bandageAnimatorController);
        {
            m.NumericBox.AddDecideModifierToBoolDict(IgnoreCurseKey, new BoolModifier(true)); // 免疫诅咒
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true)); // 免疫冻结
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, new BoolModifier(true)); // 免疫晕眩
            m.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, new BoolModifier(true)); // 免疫冰冻减速
            m.SetBaseAttribute(900, 1, 1f, 0, 0, 0, 0);
            m.transform.position = pos;
            m.currentYIndex = MapManager.GetYIndex(pos.y);
            m.AddCanBeSelectedAsTargetFunc(delegate { return false; }); // 不可作为选取的目标
            m.AddCanBlockFunc(delegate { return false; }); // 不可被阻挡
            m.AddCanHitFunc(delegate { return false; }); // 不可被子弹击中
            m.DieClipName = "Disappear";
            // 绷带蛋正常被拆解 或 被炸毁时 会出现虫子
            Action<BaseUnit> action = delegate {
                // 位于左侧第四列中心偏左则不生成虫子
                if(m.transform.position.x > MapManager.GetColumnX(3))
                    SummonBug(m.transform.position);
            };

            m.AddBeforeDeathEvent(action);
            m.AddBeforeBurnEvent(action);

            // 出现动画
            {
                CustomizationTask t = new CustomizationTask();
                t.OnEnterFunc = delegate
                {
                    m.animatorController.Play("Appear");
                };
                t.AddTaskFunc(delegate
                {
                    if (m.animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        m.animatorController.Play("Idle", true);
                        return true;
                    }
                    return false;
                });
                m.AddTask(t);
            }

            // 依附格子
            {
                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate
                {
                    if (masterGrid.isActiveAndEnabled)
                    {
                        m.transform.position = masterGrid.transform.position;
                        return false;
                    }
                    else
                    {
                        m.ExecuteDeath();
                        return true;
                    }
                });
                m.AddTask(t);
            }

            GameController.Instance.AddMouseUnit(m);
            bandageList.Add(m);
            
            // 如果原来这格就有绷带，那么会先解除本格的绷带
            BaseUnit origin_bandage = masterGrid.GetUnitFromDict(BandageKey);
            if (origin_bandage != null)
            {
                origin_bandage.ExecuteDeath();
                bandageList.Remove(origin_bandage);
                masterGrid.RemoveUnitFromDict(BandageKey);
            }
            // 之后再把自身引用加入本格
            masterGrid.AddUnitToDict(BandageKey, m);
        }
        // 生成诅咒区域，并与绷带蛋位置绑定
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(m.transform.position, 1, 1, "BothCollide");
        {
            // 每帧做的事
            Action<BaseUnit> unitAction = (unit) => {
                if (unit is MouseUnit)
                {
                    MouseUnit m = unit as MouseUnit;
                    // 目标是BOSS或者目标高度不为0均跳过判定
                    if (m.IsBoss() || m.GetHeight()!=0)
                        return;
                }

                AddCurse(unit);
                // 持续施加晕眩效果
                StatusAbility s = unit.GetNoCountUniqueStatus(StringManager.Stun);
                if (s == null)
                {
                    s = new StunStatusAbility(unit, 1, false);
                    unit.AddNoCountUniqueStatusAbility(StringManager.Stun, s);
                }

                if (s.leftTime < 2)
                {
                    s.leftTime = 2;
                }
            };

            r.isAffectFood = true;
            r.isAffectMouse = true;
            r.SetOnFoodStayAction(unitAction);
            r.SetOnEnemyStayAction(unitAction);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                if (m!=null && m.IsAlive())
                {
                    r.transform.position = m.transform.position;
                    return false;
                }
                r.MDestory();
                return true;
            });
            r.AddTask(t);

            GameController.Instance.AddAreaEffectExecution(r);
        }
    }

    /// <summary>
    /// 解除所有绷带
    /// </summary>
    public void ReleaseAllBandage()
    {
        foreach (var item in bandageList)
        {
            item.ExecuteDeath();
        }
        bandageList.Clear();
    }

    //////////////////////////////////////////////以下为BOSS技能定义////////////////////////////////////////////////////////////

    /// <summary>
    /// 法老原型移动到某地
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Move0(Vector2 pos)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        // 实现
        bool isNoHit = false;
        bool isClose = false;
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            if(!isDisappear)
                animatorController.Play("Disappear");
            else
            {
                isNoHit = true;
                AddCanHitFunc(noHitFunc);
                transform.position = pos;
                animatorController.Play("Appear");
                SetAlpha(1);
                isDisappear = false;
            }
        };
        {
            if(!isDisappear)
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        transform.position = pos;
                        animatorController.Play("Appear");
                        return true;
                    }else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.3f && !isClose)
                    {
                        isClose = true;
                        NumericBox.DamageRate.AddModifier(DecDmgWhenClosedModifier);
                    }
                    else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.65f && !isNoHit)
                    {
                        isNoHit = true;
                        AddCanHitFunc(noHitFunc);
                    }
                    return false;
                });


            c.AddSpellingFunc(delegate {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    return true;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.3f && isNoHit)
                {
                    isNoHit = false;
                    RemoveCanHitFunc(noHitFunc);
                }
                else if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.65f && isClose)
                {
                    isClose = false;
                    NumericBox.DamageRate.RemoveModifier(DecDmgWhenClosedModifier);
                }
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }

    /// <summary>
    /// 法老真身移动到某地
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility Move1(Vector2 endPos, int moveTime)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this);
        Vector2 startPos = transform.position;
        Vector2 lastPos = transform.position;
        int timeLeft = moveTime;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            startPos = transform.position;
            lastPos = transform.position;
            timeLeft = moveTime;
            animatorController.Play("Idle");
        };
        {
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    float r = (float)timeLeft / moveTime;
                    Vector2 pos = Vector2.Lerp(startPos, endPos, 1-r);
                    SetPosition((Vector2)GetPosition() + (pos - lastPos));
                    lastPos = pos;
                    return false;
                }
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// 法老之咒
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility PharaohCurseInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        Skill_PharaohCurse = c;
        int u = -1;
        int startRowRight = 3; // 法老之咒起始列（从右向左数）
        int r0_0 = Mathf.FloorToInt(GetParamValue("r0_0", mHertIndex)); // 法老之咒作用最大列（从右向左数）
        int t0_0 = Mathf.FloorToInt(GetParamValue("t0_0", mHertIndex) * 60); // 停滞时间

        int timeLeft = 0;
        bool flag = true;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            flag = true;
            // 移至中场
            c.ActivateChildAbility(Move0(MapManager.GetGridLocalPosition(8, 3)));
        };
        {
            // 第一次拍地板
            c.AddSpellingFunc(delegate {
                if (u < 0)
                    animatorController.Play("AttackLeft");
                else
                    animatorController.Play("AttackRight");
                u = -u;
                return true;
            });
            for (int i = startRowRight; i <= r0_0; i++)
            {
                int j = i;
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                    {
                        if(j < r0_0)
                        {
                            if (u < 0)
                                animatorController.Play("AttackLeft");
                            else
                                animatorController.Play("AttackRight");
                            u = -u;
                        }
                        else
                        {
                            // 最后一次结束切换到停滞状态
                            animatorController.Play("Idle", true);
                            timeLeft = t0_0;
                        }
                        flag = true;
                        return true;
                    }
                    else if(animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5f && flag)
                    {
                        flag = false;
                        int xIndex = (9 - j);
                        // 去找当前列中生命值最高的卡片所在格子
                        List<BaseGrid> l = GridManager.GetSpecificAreaGridList(GameController.Instance.mMapController.GetGridList(), MapManager.GetColumnX(xIndex - 0.5f), MapManager.GetColumnX(xIndex + 0.5f), MapManager.GetRowY(-0.5f), MapManager.GetRowY(6.5f));
                        List<BaseGrid> l2 = GridManager.GetGridListWhichHasMaxCondition(l, (g) => {
                            List<FoodUnit> fList = g.GetAttackableFoodUnitList();
                            float max = float.MinValue;
                            foreach (var item in fList)
                            {
                                if (item.GetCurrentHp() > max)
                                    max = item.GetCurrentHp();
                            }
                            return max;
                        });
                        List<BaseGrid> l3 = GridManager.GetRandomUnitList(l2, 1);
                        // 在最终筛选出来的格子中绑上绷带
                        foreach (var g in l3)
                        {
                            CreateBandageArea(g);
                        }
                    }
                    return false;
                });
            }
            // 等待时间到了后退出
            c.AddSpellingFunc(delegate {
                if(timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// 神秘祭祀
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility MysticalSacrificesInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        int count1_0 = Mathf.FloorToInt(GetParamValue("count1_0", mHertIndex)); // 重复次数
        int t1_0 = Mathf.FloorToInt(GetParamValue("t1_0", mHertIndex)*60); // 神秘祭祀停留时间
        int t1_1 = Mathf.FloorToInt(GetParamValue("t1_1", mHertIndex)*60); // 真身形态下神秘祭祀移动时间
        int timeLeft = 0;
        bool flag = true;
        bool isRealStage = IsRealStage();
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            flag = true;
        };
        for(int k = 0; k < count1_0 + 1; k++){
            c.AddSpellingFunc(delegate
            {
                flag = true;
                SetNextGridIndexByRandom(3, 6, 1, 5);
                Vector2 v = GetNextGridIndex();
                if(isRealStage)
                    c.ActivateChildAbility(Move1(MapManager.GetGridLocalPosition(v.x, v.y), t1_1));
                else
                    c.ActivateChildAbility(Move0(MapManager.GetGridLocalPosition(v.x, v.y)));
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                if(isRealStage)
                    animatorController.Play("Attack");
                else
                    animatorController.Play("AttackLeft");
                return true;
            });
            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle", true);
                    timeLeft = t1_0;
                    return true;
                }else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.5f && flag)
                {
                    flag = false;
                    int xIndex = GetColumnIndex();
                    int yIndex = GetRowIndex();
                    for (int i = -1; i <= 1; i++)
                    {
                        BaseGrid g = GameController.Instance.mMapController.GetGrid(xIndex - 1, yIndex + i);
                        if(g!=null)
                            CreateBandageArea(g);
                    }
                        
                }
                return false;
            });
            // 等待后退出
            c.AddSpellingFunc(delegate
            {
                if(timeLeft > 0)
                    timeLeft--; 
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
    /// 唤醒仪式
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility AwakeningCeremonyInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);
        int t2_0 = Mathf.FloorToInt(GetParamValue("t2_0", mHertIndex)*60); // 唤醒仪式停留时间
        int t2_1 = Mathf.FloorToInt(GetParamValue("t2_1", mHertIndex)*60); // 真身形态下唤醒仪式移动时间
        bool flag = true;
        bool isRealStage = IsRealStage();
        int timeLeft = 0;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            flag = true;
            timeLeft = 0;
            // 移至中场
            if(isRealStage)
                c.ActivateChildAbility(Move1(MapManager.GetGridLocalPosition(8, 3), t2_1));
            else
                c.ActivateChildAbility(Move0(MapManager.GetGridLocalPosition(8, 3)));
        };
        {
            c.AddSpellingFunc(delegate
            {
                animatorController.Play("Cast");
                return true;
            });

            c.AddSpellingFunc(delegate
            {
                if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
                {
                    animatorController.Play("Idle", true);
                    timeLeft = t2_0;
                    return true;
                }
                else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > 0.35f && flag)
                {
                    flag = false;
                    ReleaseAllBandage();
                }
                return false;
            });
            // 等待退出
            c.AddSpellingFunc(delegate
            {
                if (timeLeft > 0)
                    timeLeft--;
                else
                    return true;
                return false;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }


    /// <summary>
    /// 法老王之咒
    /// </summary>
    /// <param name="info"></param>
    private CompoundSkillAbility PharaohKingCurseInit(SkillAbility.SkillAbilityInfo info)
    {
        CompoundSkillAbility c = new CompoundSkillAbility(this, info);

        int t3_0 = Mathf.FloorToInt(GetParamValue("t3_0", mHertIndex)*60); // 法老王之咒回到中场的移动时间
        int t3_1 = Mathf.FloorToInt(GetParamValue("t3_1", mHertIndex)*60); // 法老王之咒停滞时间

        int timeLeft = 0;
        bool flag = true;
        float rate = 2.0f / 7;
        // 实现
        c.IsMeetSkillConditionFunc = delegate { return true; };
        c.BeforeSpellFunc = delegate
        {
            timeLeft = 0;
            flag = true;
            // 移至中场
            c.ActivateChildAbility(Move1(MapManager.GetGridLocalPosition(8, 3), t3_0));
        };
        {
            // 缠绷带
            c.AddSpellingFunc(delegate {
                animatorController.Play("Charge", true);
                return true;
            });
            // 七行
            for (int i = 0; i < 7; i++)
            {
                int j = i;
                // 缠绷带
                c.AddSpellingFunc(delegate {
                    if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= rate * (j+1))
                    {
                        flag = true;
                        return true;
                    }
                    else if (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() > rate*j && flag)
                    {
                        BaseUnit unit = FoodManager.GetSpecificRowFarthestRightCanTargetedAlly(j, float.MinValue);
                        if(unit != null && unit.GetGrid() != null)
                        {
                            CreateBandageArea(unit.GetGrid());
                        }
                        flag = false;
                    }
                    return false;
                });
            }
            // 仅设置一个等待时间
            c.AddSpellingFunc(delegate {
                timeLeft = t3_1;
                return true;
            });
            // 等待时间到了后退出
            c.AddSpellingFunc(delegate {
                if (timeLeft > 0)
                {
                    timeLeft--;
                    return false;
                }
                return true;
            });
        }
        c.OnNoSpellingFunc = delegate { };
        c.AfterSpellFunc = delegate { };
        return c;
    }
}
