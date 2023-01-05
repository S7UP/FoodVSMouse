using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 敌人的BOSS单位
/// </summary>
public class BossUnit : MouseUnit
{
    public SkillQueueAbilityManager mSkillQueueAbilityManager; // 技能队列管理器
    private static BoolModifier IgnoreSomeEffectModifier = new BoolModifier(true);
    private System.Random rand; // 随机数生成器
    private Dictionary<int, int> seedDict = new Dictionary<int, int>(); // 作用于本随机数生成器的种子字典，key表示初始行，value表示对应种子号
    private Stack<Vector2> nextGridStack = new Stack<Vector2>(); // 下一个格子坐标栈，如果栈为空则让随机数生成器生成一个值并进栈 
    protected Dictionary<string, float[]> BossParamArrayDict = new Dictionary<string, float[]>(); // boss参数字典

    private BoolModifier BossNoTargetAttackModeModifier = new BoolModifier(true); // BOSS无限攻击tag

    public override void Awake()
    {
        mSkillQueueAbilityManager = new SkillQueueAbilityManager(this);
        base.Awake();
    }

    public override void MInit()
    {
        mSkillQueueAbilityManager.Initial();
        rand = null;
        seedDict.Clear();
        nextGridStack.Clear();
        BossParamArrayDict.Clear();
        InitBossParam(); // 初始化BOSS参数字典，由具体的BOSS类内部实现
        base.MInit();
        InitHertRateList();
        isBoss = true;
        // 免疫控制效果
        AddBossIgnoreDebuffEffect(this);
        // BOSS不执行正常敌人前进逻辑
        NumericBox.MoveSpeed.SetBase(0);
        // 为全场添加无限攻击TAG
        GameController.Instance.AddNoTargetAttackModeModifier(BossNoTargetAttackModeModifier);
    }

    public override void MUpdate()
    {
        base.MUpdate();
        mSkillQueueAbilityManager.Update();
    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        mSkillQueueAbilityManager.Initial();
    }

    public override void OnMoveStateEnter()
    {
        
    }

    public override void OnMoveState()
    {
        
    }

    public override void ExecuteRecycle()
    {
        // 移除全场无限攻击TAG
        GameController.Instance.RemoveNoTargetAttackModeModifier(BossNoTargetAttackModeModifier);
        mSkillQueueAbilityManager.Initial();
        // BOSS剩余数-1
        GameController.Instance.mCurrentStage.DecBossCount();
        base.ExecuteRecycle();
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Boss;
    }

    /// <summary>
    /// 获取种子表
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
    /// 通过初始行获取特定随机数种子，以设置该BOSS随机数生成器的种子
    /// </summary>
    /// <param name="rowIndex">若key不存在则采用默认的随机构造方法</param>
    public void SetRandSeedByRowIndex(int rowIndex)
    {
        if(seedDict.ContainsKey(rowIndex))
            rand = new System.Random(seedDict[rowIndex]);
        else
            rand = new System.Random();
    }

    /// <summary>
    /// 设置下一个格子的格子坐标
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
    /// 通过随机的方法设置下一个格子的格子坐标(左右都是开区间）
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
    /// 获取下一个前往的格子
    /// </summary>
    /// <returns></returns>
    public Vector2 GetNextGridIndex()
    {
        if (nextGridStack.Count > 0)
            return nextGridStack.Pop();
        return new Vector2(8, 3);
    }

    /// <summary>
    /// 获取BOSS自身随机数发射器的下一个随机数(左开右闭)
    /// </summary>
    /// <returns></returns>
    public int GetRandomNext(int min, int max)
    {
        return rand.Next(min, max);
    }

    /// <summary>
    /// 自动更新贴图
    /// </summary>
    public override void UpdateRuntimeAnimatorController()
    {
        //AnimatorStateRecorder a = animatorController.GetCurrentAnimatorStateRecorder(); // 获取当前在播放的动画
        //animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/" + mType + "/" + mShape + "/" + mHertIndex);
        //animatorController.ChangeAnimator(animator);
        //// 保持当前动画播放
        //if (a != null)
        //{
        //    animatorController.Play(a.aniName, a.isCycle, a.GetNormalizedTime());
        //}
        OnUpdateRuntimeAnimatorController();
        // 锁定前几个阶段
        for (int i = 0; i < mHertIndex; i++)
        {
            mHertRateList[i] = float.MaxValue;
        }
        // 重载技能组（狂暴）
        LoadSkillAbility();
    }

    /// <summary>
    /// 技能初始化模板（勿删）
    /// </summary>
    /// <param name="info"></param>
    //private CompoundSkillAbility PharaohCurseInit(SkillAbility.SkillAbilityInfo info)
    //{
    //    CompoundSkillAbility c = new CompoundSkillAbility(this, info);
    //    Skill_PharaohCurse = c;
    //    // 实现
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
    /// 为某个对象添加BOSS免疫异常状态效果
    /// </summary>
    public static void AddBossIgnoreDebuffEffect(BaseUnit unit)
    {
        // 免疫控制效果
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozenSlowDown, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreSlowDown, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, IgnoreSomeEffectModifier);
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreWaterGridState, IgnoreSomeEffectModifier);
    }

    public override bool IsOutOfBound()
    {
        return false;
    }

    /// <summary>
    /// 在与猫判定时是否能触发猫
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerCat()
    {
        return false;
    }

    /// <summary>
    /// 在越过失败判定线后是否会触发游戏失败判定
    /// </summary>
    /// <returns></returns>
    public override bool CanTriggerLoseWhenEnterLoseLine()
    {
        return false;
    }

    /// <summary>
    /// 初始化BOSS的参数
    /// </summary>
    public virtual void InitBossParam()
    {
        // 切换阶段血量百分比
        AddParamArray("hpRate", new float[] { 0.5f, 0.2f });
    }

    /// <summary>
    /// 根据给定的hpRate的值来设置BOSS切换阶段点
    /// </summary>
    private void InitHertRateList()
    {
        mHertRateList.Clear();
        float[] hpRate = BossParamArrayDict["hpRate"];
        foreach (var item in hpRate)
        {
            mHertRateList.Add(item);
        }
    }

    /// <summary>
    /// 添加一个参数
    /// </summary>
    public void AddParamArray(string key, float[] arr)
    {
        if (!BossParamArrayDict.ContainsKey(key))
            BossParamArrayDict.Add(key, arr);
        else
        {
            Debug.LogWarning("BOSS中已存在名为“" + key + "”的参数！传进来的新参数数组会覆盖原来的数组！");
            BossParamArrayDict[key] = arr;
        }

        // 特殊参数处理
        if (key.Equals("hpRate"))
        {
            InitHertRateList();
            UpdateHertMap();
        }
    }

    public void AddParamArray(string key, List<float> list)
    {
        AddParamArray(key, list.ToArray());
    }

    /// <summary>
    /// 获取参数值
    /// </summary>
    /// <param name="key"></param>
    /// <returns>如果没有key或者其中的数组为0，则返回0且报warning，如果有且没有越界，则正常返回，否则返回数组最后一位</returns>
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
            Debug.LogWarning("BOSS中未发现名为“"+key+"”的参数！");
            return 0;
        }
    }

    /// <summary>
    /// 是否不存在或者越界
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
}
