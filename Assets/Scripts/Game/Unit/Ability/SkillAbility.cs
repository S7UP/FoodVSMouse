using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
/// <summary>
/// ���ܴ洢ʵ��
/// </summary>
public abstract class SkillAbility : AbilityEntity
{
    public SkillAbilityManager skillAbilityManager { get; set; }
    public bool isSpelling { get; set; } // �Ƿ��ڼ�����Ч״̬
    public FloatNumeric needEnergy = new FloatNumeric(); // ʩ����������ֵ
    public FloatNumeric startEnergy = new FloatNumeric(); // ��ʼ����ֵ
    public FloatNumeric energyRegeneration = new FloatNumeric(); // ����ֵ�ظ�/֡
    public float currentEnergy; // ��ǰ����ֵ
    public SkillAbility.Type skillType; // ��������
    public int priority; // �������ȼ�
    public bool isExclusive = true; // �Ƿ�����
    public bool canActiveInDeathState; // ������״̬���ܷ񴥷�
    public bool enableEnergyRegeneration = true; // �Ƿ����������ظ�

    public enum Type
    {
        GeneralAttack = 0, // ��ͨ����
        SpecialAbility = 1, // �ؼ�
    }

    /// <summary>
    /// �洢�ڱ��ص�JSON�ļ�
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
    /// �ӱ��ض�ȡ�������ݣ��Զ�����
    /// </summary>
    /// <param name="unitType">��λ����</param>
    /// <param name="type">�ô��൥λ��С��</param>
    /// <param name="shape">��С�൥λ�ı���</param>
    /// <param name="index">�õ�λ���ܱ��</param>
    public void Load(UnitType unitType, int type, int shape, int index)
    {
        SkillAbilityInfo info = AbilityManager.Instance.AbilityDict[unitType][type][shape][index];
        Init(info.name, info.needEnergy, info.startEnergy, info.energyRegeneration, info.skillType, info.canActiveInDeathState);
    }

    /// <summary>
    /// �������ĳ�ʼ��
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
    /// ���õ�ǰ����ֵ
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
        // ������⣬���Ŀ��������״̬��������ܲ����������ڼ��ͷ���ֱ���˳�
        if (!IsActiveInDeath())
            return;
        // �����ظ�
        if (enableEnergyRegeneration)
        {
            currentEnergy += energyRegeneration.Value;
            if (currentEnergy > needEnergy.Value)
            {
                currentEnergy = needEnergy.Value;
            }
        }
        // ������Ч�ڼ�
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
    /// �ڼ����ڼ䣬����ʵ�����������
    /// </summary>
    public virtual void OnSpelling()
    {

    }

    /// <summary>
    /// �ڷǼ����ڼ䣬����ʵ�����������
    /// </summary>
    public virtual void OnNoSpelling()
    {

    }

    /// <summary>
    /// �ڼ����ڼ䣬������������Ҫ������
    /// </summary>
    public virtual bool IsMeetCloseSpellingCondition()
    {
        return true;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public override void ActivateAbility()
    {
        // ������⣬���Ŀ��������״̬��������ܲ����������ڼ��ͷ���ֱ���˳�
        if (!IsActiveInDeath())
            return;
        isSpelling = true;
        currentEnergy = 0; // ����
        // ��������
        if (isExclusive && skillAbilityManager !=null)
        {
            skillAbilityManager.SetExclusiveLock(true);
        }
        BeforeSpell();
    }

    /// <summary>
    /// ����ǰ�������ǿ�ʼ����
    /// </summary>
    public virtual void BeforeSpell()
    {

    }


    public override AbilityExecution CreateAbilityExecution()
    {
        return null;
    }

    /// <summary>
    /// ��������
    /// </summary>
    public override void EndActivate()
    {
        isSpelling = false;
        // ��������
        if (isExclusive && skillAbilityManager != null)
        {
            skillAbilityManager.SetExclusiveLock(false);
        }
        AfterSpell();
    }

    /// <summary>
    /// ���ܺ󣬻����ǽ�������
    /// </summary>
    public virtual void AfterSpell()
    {

    }

    /// <summary>
    /// �ܷ�ż���
    /// </summary>
    public override bool CanActive()
    {
        return (IsActiveInDeath() && !isSpelling && currentEnergy >= needEnergy.Value && IsMeetSkillCondition());
    }

    /// <summary>
    /// �Ƿ����㼼���ͷ���������������д����������
    /// </summary>
    /// <returns></returns>
    public virtual bool IsMeetSkillCondition()
    {
        return true;
    }

    /// <summary>
    /// ����ܷ�������ʱʹ��
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
    /// �����ù��ܣ�������ʽ��Ϸʵװ
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
        // ��Update�������������Insert
        if(!AbilityManager.Instance.Update(skillAbilityInfo, master.mUnitType, master.mType, master.mShape))
            AbilityManager.Instance.Insert(skillAbilityInfo, master.mUnitType, master.mType, master.mShape);
        AbilityManager.Instance.SaveAll();
    }
}