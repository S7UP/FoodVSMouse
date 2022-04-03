using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// 技能存储实体
/// </summary>
public abstract class SkillAbility : AbilityEntity
{
    public SkillAbilityManager skillAbilityManager { get; set; }
    public bool isSpelling { get; set; } // 是否处在技能生效状态
    public FloatNumeric needEnergy = new FloatNumeric(); // 施放需求能量值
    public FloatNumeric startEnergy = new FloatNumeric(); // 初始能量值
    public FloatNumeric energyRegeneration = new FloatNumeric(); // 能量值回复/帧
    public float currentEnergy; // 当前能量值
    public SkillAbility.Type skillType; // 技能类型
    public int priority; // 技能优先级
    public bool isExclusive = true; // 是否排他
    public bool canActiveInDeathState; // 在死亡状态下能否触发
    public bool enableEnergyRegeneration = true; // 是否启动能量回复

    public enum Type
    {
        GeneralAttack = 0, // 普通攻击
        SpecialAbility = 1, // 特技
    }

    /// <summary>
    /// 存储在本地的JSON文件
    /// </summary>
    public struct SkillAbilityInfo
    {
        public string name;
        public float needEnergy;
        public float startEnergy;
        public float energyRegeneration;
        public SkillAbility.Type skillType;
        public bool isExclusive;
        public bool canActiveInDeathState;
    }

    public SkillAbility()
    {

    }

    public SkillAbility(BaseUnit pmaster):base(pmaster)
    { 

    }

    public SkillAbility(BaseUnit pmaster, SkillAbilityInfo info): base(pmaster)
    {
        Init(info.name, info.needEnergy, info.startEnergy, info.energyRegeneration, info.skillType, info.canActiveInDeathState);
    }

    /// <summary>
    /// 从本地读取技能数据，自动配置
    /// </summary>
    /// <param name="unitType">单位大类</param>
    /// <param name="type">该大类单位的小类</param>
    /// <param name="shape">该小类单位的变种</param>
    /// <param name="index">该单位技能编号</param>
    public void Load(UnitType unitType, int type, int shape, int index)
    {
        SkillAbilityInfo info = AbilityManager.Instance.AbilityDict[unitType][type][shape][index];
        Init(info.name, info.needEnergy, info.startEnergy, info.energyRegeneration, info.skillType, info.canActiveInDeathState);
    }

    /// <summary>
    /// 带参数的初始化
    /// </summary>
    /// <param name="name"></param>
    /// <param name="need"></param>
    /// <param name="start"></param>
    /// <param name="regeneration"></param>
    /// <param name="type"></param>
    public void Init(string name, float need, float start, float regeneration, SkillAbility.Type type, bool canActiveInDeathState)
    {
        this.name = name;
        needEnergy.SetBase(need);
        startEnergy.SetBase(start);
        energyRegeneration.SetBase(regeneration);
        currentEnergy = startEnergy.Value;
        skillType = type;
        this.canActiveInDeathState = canActiveInDeathState;
    }

    /// <summary>
    /// 重置当前能量值
    /// </summary>
    public void ResetCurrentEnergy()
    {
        currentEnergy = startEnergy.Value;
    }

    public override void Init()
    {
        isSpelling = false;
        needEnergy.Initialize();
        startEnergy.Initialize();
        energyRegeneration.Initialize();
        currentEnergy = 0;
    }


    public override void Update()
    {
        // 死亡检测，如果目标在死亡状态且这个技能不能在死亡期间释放则直接退出
        if (!IsActiveInDeath())
            return;
        // 能量回复
        if (enableEnergyRegeneration)
        {
            currentEnergy += energyRegeneration.Value;
            if (currentEnergy > needEnergy.Value)
            {
                currentEnergy = needEnergy.Value;
            }
        }
        // 技能生效期间
        if (isSpelling)
        {
            OnSpelling();
            if (IsMeetCloseSpellingCondition())
                EndActivate();
        }
        else
            OnNoSpelling();
    }

    /// <summary>
    /// 在技能期间，具体实现由子类完成
    /// </summary>
    public virtual void OnSpelling()
    {

    }

    /// <summary>
    /// 在非技能期间，具体实现由子类完成
    /// </summary>
    public virtual void OnNoSpelling()
    {

    }

    /// <summary>
    /// 在技能期间，结束技能所需要的条件
    /// </summary>
    public virtual bool IsMeetCloseSpellingCondition()
    {
        return true;
    }

    /// <summary>
    /// 激活能力
    /// </summary>
    public override void ActivateAbility()
    {
        // 死亡检测，如果目标在死亡状态且这个技能不能在死亡期间释放则直接退出
        if (!IsActiveInDeath())
            return;
        isSpelling = true;
        currentEnergy = 0; // 清零
        // 排它设置
        if (isExclusive && skillAbilityManager !=null)
        {
            skillAbilityManager.SetExclusiveLock(true);
        }
        BeforeSpell();
    }

    /// <summary>
    /// 技能前，或者是开始技能
    /// </summary>
    public virtual void BeforeSpell()
    {

    }


    public override AbilityExecution CreateAbilityExecution()
    {
        return null;
    }

    /// <summary>
    /// 结束能力
    /// </summary>
    public override void EndActivate()
    {
        isSpelling = false;
        // 排它设置
        if (isExclusive && skillAbilityManager != null)
        {
            skillAbilityManager.SetExclusiveLock(false);
        }
        AfterSpell();
    }

    /// <summary>
    /// 技能后，或者是结束技能
    /// </summary>
    public virtual void AfterSpell()
    {

    }

    /// <summary>
    /// 能否放技能
    /// </summary>
    public override bool CanActive()
    {
        return (IsActiveInDeath() && !isSpelling && currentEnergy >= needEnergy.Value && IsMeetSkillCondition());
    }

    /// <summary>
    /// 是否满足技能释放条件，由子类重写其特殊条件
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetSkillCondition()
    {
        return true;
    }

    /// <summary>
    /// 检测能否在死亡时使用
    /// </summary>
    /// <returns></returns>
    private bool IsActiveInDeath()
    {
        return master.IsAlive() || canActiveInDeathState;
    }

    public static bool operator >(SkillAbility a, SkillAbility b)
    {
        return a.priority > b.priority;
    }

    public static bool operator <(SkillAbility a, SkillAbility b)
    {
        return a.priority < b.priority;
    }

    /// <summary>
    /// 测试用功能，不在正式游戏实装
    /// </summary>
    public void SaveInfo()
    {
        SkillAbilityInfo skillAbilityInfo = new SkillAbilityInfo()
        {
            name = name,
            needEnergy = needEnergy.baseValue,
            startEnergy = startEnergy.baseValue,
            energyRegeneration = energyRegeneration.baseValue,
            skillType = skillType,
            canActiveInDeathState = canActiveInDeathState
        };
        // 先Update，如果不存在再Insert
        if(!AbilityManager.Instance.Update(skillAbilityInfo, master.mUnitType, master.mType, master.mShape))
            AbilityManager.Instance.Insert(skillAbilityInfo, master.mUnitType, master.mType, master.mShape);
        AbilityManager.Instance.SaveAll();
    }
}