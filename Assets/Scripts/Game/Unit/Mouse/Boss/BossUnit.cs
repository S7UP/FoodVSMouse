using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ���˵�BOSS��λ
/// </summary>
public class BossUnit : MouseUnit
{
    public SkillQueueAbilityManager mSkillQueueAbilityManager; // ���ܶ��й�����
    private static BoolModifier IgnoreSomeEffectModifier = new BoolModifier(true);
    private System.Random rand; // �����������
    private Dictionary<int, int> seedDict = new Dictionary<int, int>(); // �����ڱ�������������������ֵ䣬key��ʾ��ʼ�У�value��ʾ��Ӧ���Ӻ�
    private int SeedValue; // ��ǰ����ֵ
    private Stack<Vector2> nextGridStack = new Stack<Vector2>(); // ��һ����������ջ�����ջΪ���������������������һ��ֵ����ջ 
    protected Dictionary<string, float[]> BossParamArrayDict = new Dictionary<string, float[]>(); // boss�����ֵ�
    private Dictionary<string, List<Action<float[]>>> ParamChangeActionListDict = new Dictionary<string, List<Action<float[]>>>(); // �������ֵ��еĲ��������ı�ʱ���¼�

    private BoolModifier BossNoTargetAttackModeModifier = new BoolModifier(true); // BOSS���޹���tag
    private FloatModifier burnRateMod = new FloatModifier(0.05f); // BOSS�ܻҽ�Ч������

    public override void Awake()
    {
        mSkillQueueAbilityManager = new SkillQueueAbilityManager(this);
        base.Awake();
    }

    public override void MInit()
    {
        ParamChangeActionListDict.Clear();
        SeedValue = 0;
        mSkillQueueAbilityManager.Initial();
        rand = null;
        seedDict.Clear();
        nextGridStack.Clear();
        BossParamArrayDict.Clear();
        BossParamInit(); // ��ʼ��BOSS����
        base.MInit();
        InitHertRateList();
        isBoss = true;
        // ���߿���Ч��
        AddBossIgnoreDebuffEffect(this);
        // BOSS��ִ����������ǰ���߼�
        NumericBox.Attack.SetBase(10);
        NumericBox.AttackSpeed.SetBase(1);
        NumericBox.MoveSpeed.SetBase(0);
        // Ϊȫ��������޹���TAG
        GameController.Instance.AddNoTargetAttackModeModifier(BossNoTargetAttackModeModifier);
    }

    public override void MUpdate()
    {
        base.MUpdate();
        mSkillQueueAbilityManager.Update();
    }

    public override void MDestory()
    {
        // �Ƴ�ȫ�����޹���TAG
        GameController.Instance.RemoveNoTargetAttackModeModifier(BossNoTargetAttackModeModifier);
        mSkillQueueAbilityManager.Initial();
        base.MDestory();
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        mSkillQueueAbilityManager.Initial();
    }


    public override void BeforeBurn()
    {
        BeforeDeath();
    }

    public override void OnMoveStateEnter()
    {
        
    }

    public override void OnMoveState()
    {
        
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Boss;
    }

    /// <summary>
    /// ��ȡ���ӱ�
    /// </summary>
    public void LoadSeedDict()
    {
        seedDict.Clear();
        for (int i = 0; i < 7; i++)
        {
            seedDict.Add(i, i);
        }
    }

    /// <summary>
    /// ͨ����ʼ�л�ȡ�ض���������ӣ������ø�BOSS�����������������
    /// </summary>
    /// <param name="rowIndex">��key�����������Ĭ�ϵ�������췽��</param>
    public void SetRandSeedByRowIndex(int rowIndex)
    {
        SeedValue = rowIndex;
        if (seedDict.ContainsKey(rowIndex))
            rand = new System.Random(seedDict[rowIndex]);
        else
            rand = new System.Random();
    }

    /// <summary>
    /// ������һ�����ӵĸ�������
    /// </summary>
    /// <param name="xIndex"></param>
    /// <param name="yIndex"></param>
    public void SetNextGridIndex(int xIndex, int yIndex, float xIndexOffset, float yIndexOffset)
    {
        nextGridStack.Push(new Vector2(xIndex + xIndexOffset, yIndex + yIndexOffset));
    }

    public void SetNextGridIndex(int xIndex, int yIndex)
    {
        SetNextGridIndex(xIndex, yIndex, 0, 0);
    }

    /// <summary>
    /// ͨ������ķ���������һ�����ӵĸ�������(���Ҷ��ǿ����䣩
    /// </summary>
    public void SetNextGridIndexByRandom(int xIndexMin, int xIndexMax, int yIndexMin, int yIndexMax, float xIndexOffset, float yIndexOffset)
    {
        nextGridStack.Push(new Vector2(rand.Next(xIndexMin, xIndexMax + 1) + xIndexOffset, rand.Next(yIndexMin, yIndexMax + 1) + yIndexOffset));
    }

    public void SetNextGridIndexByRandom(int xIndexMin, int xIndexMax, int yIndexMin, int yIndexMax)
    {
        SetNextGridIndexByRandom(xIndexMin, xIndexMax, yIndexMin, yIndexMax, 0, 0);
    }

    /// <summary>
    /// ��ȡ��һ��ǰ���ĸ���
    /// </summary>
    /// <returns></returns>
    public Vector2 GetNextGridIndex()
    {
        if (nextGridStack.Count > 0)
            return nextGridStack.Pop();
        return new Vector2(8, 3);
    }

    /// <summary>
    /// ��ȡBOSS�������������������һ�������(���ұ�)
    /// </summary>
    /// <returns></returns>
    public int GetRandomNext(int min, int max)
    {
        return rand.Next(min, max);
    }

    /// <summary>
    /// �Զ�������ͼ
    /// </summary>
    public override void OnHertStageChanged()
    {
        OnUpdateRuntimeAnimatorController();
        // ����ǰ�����׶�
        for (int i = 0; i < mHertIndex; i++)
        {
            mHertRateList[i] = float.MaxValue;
        }
        burnRateMod.Value = 1 - GetParamValue("burn_defence");
        NumericBox.BurnRate.RemoveModifier(burnRateMod);
        NumericBox.BurnRate.AddModifier(burnRateMod);
        // ���ؼ����飨�񱩣�
        LoadSkillAbility();
    }

    /// <summary>
    /// ���ܳ�ʼ��ģ�壨��ɾ��
    /// </summary>
    /// <param name="info"></param>
    //private CompoundSkillAbility PharaohCurseInit(SkillAbility.SkillAbilityInfo info)
    //{
    //    CompoundSkillAbility c = new CompoundSkillAbility(this, info);
    //    // ʵ��
    //    c.IsMeetSkillConditionFunc = delegate { return true; };
    //    c.BeforeSpellFunc = delegate
    //    {

    //    };
    //    {
    //        c.AddSpellingFunc(delegate {
    //            return true;
    //        });

    //    }
    //    c.OnNoSpellingFunc = delegate { };
    //    c.AfterSpellFunc = delegate { };
    //    return c;
    //}

    /// <summary>
    /// Ϊĳ���������BOSS�����쳣״̬Ч��
    /// </summary>
    public static void AddBossIgnoreDebuffEffect(BaseUnit unit)
    {
        // ���߿���Ч��
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreSomeEffectModifier);
    }

    public override bool IsOutOfBound()
    {
        return false;
    }

    /// <summary>
    /// ����è�ж�ʱ�Ƿ��ܴ���è
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerCat()
    {
        return false;
    }

    /// <summary>
    /// ��Խ��ʧ���ж��ߺ��Ƿ�ᴥ����Ϸʧ���ж�
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return false;
    }

    private void BossParamInit()
    {
        // �л��׶�Ѫ���ٷֱ�
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
        AddParamArray("burn_defence", new float[] { 0.95f });

        // Ѫ���׶ε�����ı��¼�
        AddParamChangeAction("hpRate", delegate {
            InitHertRateList();
            UpdateHertMap();
        });

        InitBossParam();
    }



    /// <summary>
    /// ��ʼ��BOSS�Ĳ���
    /// </summary>
    protected virtual void InitBossParam()
    {

    }

    /// <summary>
    /// ���ݸ�����hpRate��ֵ������BOSS�л��׶ε�
    /// </summary>
    public void InitHertRateList()
    {
        mHertRateList.Clear();
        float[] hpRate = BossParamArrayDict["hpRate"];
        foreach (var item in hpRate)
        {
            mHertRateList.Add(item);
        }
    }

    /// <summary>
    /// ���һ������
    /// </summary>
    public void AddParamArray(string key, float[] arr)
    {
        if (!BossParamArrayDict.ContainsKey(key))
            BossParamArrayDict.Add(key, arr);
        else
        {
            Debug.LogWarning("BOSS���Ѵ�����Ϊ��" + key + "���Ĳ��������������²�������Ḳ��ԭ�������飡");
            BossParamArrayDict[key] = arr;
        }

        // �����������
        if (ParamChangeActionListDict.ContainsKey(key))
            foreach (var action in ParamChangeActionListDict[key])
                action(arr);
    }

    public void AddParamArray(string key, List<float> list)
    {
        AddParamArray(key, list.ToArray());
    }

    /// <summary>
    /// ��ȡ����ֵ
    /// </summary>
    /// <param name="key"></param>
    /// <returns>���û��key�������е�����Ϊ0���򷵻�0�ұ�warning���������û��Խ�磬���������أ����򷵻��������һλ</returns>
    public float GetParamValue(string key, int stage)
    {
        if (BossParamArrayDict.ContainsKey(key) && BossParamArrayDict[key].Length>0)
        {
            float[] arr = BossParamArrayDict[key];
            if (stage < arr.Length)
                return arr[stage];
            else
                return arr[arr.Length-1];
        }
        else
        {
            Debug.LogWarning("BOSS��δ������Ϊ��"+key+"���Ĳ�����");
            return 0;
        }
    }

    public float GetParamValue(string key)
    {
        return GetParamValue(key, mHertIndex);
    }

    public float[] GetParamArray(string key)
    {
        if (BossParamArrayDict.ContainsKey(key) && BossParamArrayDict[key].Length > 0)
        {
            return BossParamArrayDict[key];
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// �Ƿ񲻴��ڻ���Խ��
    /// </summary>
    /// <param name="key"></param>
    /// <param name="stage"></param>
    /// <returns></returns>
    public bool IsParamValueInValidOrOutOfBound(string key, int stage)
    {
        if (BossParamArrayDict.ContainsKey(key) && BossParamArrayDict[key].Length > 0)
        {
            float[] arr = BossParamArrayDict[key];
            if (stage < arr.Length)
                return false;
            else
                return true;
        }
        return true;
    }

    public int GetSeedValue()
    {
        return SeedValue;
    }

    /// <summary>
    /// ��ȡ��ǰ�׶���
    /// </summary>
    /// <returns></returns>
    public int GetStage()
    {
        return Mathf.Max(0, Mathf.Min(mHertIndex, GetParamArray("hpRate").Length));
    }

    /// <summary>
    /// ��ȡ��ǰ�׶ε㵽��һ���׶ε�����Ҫ�ﵽ������ֵ�ٷֱȣ�С����
    /// </summary>
    /// <param name="stage"></param>
    /// <returns></returns>
    private float GetHpPercent(int stage)
    {
        float[] arr = GetParamArray("hpRate");
        stage = Mathf.Max(0, Mathf.Min(stage, arr.Length));
        if (stage == arr.Length)
            return 0;
        return Mathf.Max(0, Mathf.Min(arr[stage], 1));
    }

    public float GetStageTotalHp(int stage)
    {
        float[] arr = GetParamArray("hpRate");
        if (stage == 0)
        {
            if (arr.Length == 0)
                return mMaxHp;
            else
                return mMaxHp * (1 - GetHpPercent(stage));
        }
        else if (stage == arr.Length)
            return mMaxHp * GetHpPercent(stage - 1);
        return mMaxHp * (GetHpPercent(stage - 1) - GetHpPercent(stage));
    }

    /// <summary>
    /// ��ȡ��ǰ�׶ε�������ֵ
    /// </summary>
    /// <returns></returns>
    public float GetCurrentStageTotalHp()
    {
        return GetStageTotalHp(GetStage());
    }

    public float GetStageLeftHp(int stage)
    {
        float[] arr = GetParamArray("hpRate");
        if (arr.Length == 0)
            return mCurrentHp;
        else
            return mCurrentHp - mMaxHp * GetHpPercent(stage);
    }

    /// <summary>
    /// ��ȡ��ǰ�׶ε�ʣ������ֵ
    /// </summary>
    /// <returns></returns>
    public float GetCurrentStageLeftHp()
    {
        return GetStageLeftHp(GetStage());
    }

    /// <summary>
    /// ������һ�׶�
    /// </summary>
    public void SkipStage()
    {
        if (mHertIndex == 0)
            mHertRateList.Insert(mHertIndex, 1.0f);
        else
            mHertRateList.Insert(mHertIndex, mHertRateList[mHertIndex - 1]);
        mHertIndex++;
        OnHertStageChanged();
    }

    /// <summary>
    /// ��ӵ������ֵ䷢���ı�ʱ���¼�
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    public void AddParamChangeAction(string key, Action<float[]> action)
    {
        if (!ParamChangeActionListDict.ContainsKey(key))
            ParamChangeActionListDict.Add(key, new List<Action<float[]>>());
        ParamChangeActionListDict[key].Add(action);
    }

    /// <summary>
    /// �Ƴ��������ֵ䷢���ı�ʱ���¼�
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    public void RemoveParamChangeAction(string key, Action<float[]> action)
    {
        if (ParamChangeActionListDict.ContainsKey(key))
            ParamChangeActionListDict[key].Remove(action);
    }
}
