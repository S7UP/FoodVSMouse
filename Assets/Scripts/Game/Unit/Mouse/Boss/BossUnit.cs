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
        base.MInit();
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
    /// 自动更新贴图
    /// </summary>
    /// <param name="collision"></param>
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
    //private void SkillModelInit(SkillAbility.SkillAbilityInfo info)
    //{
    //    CustomizationSkillAbility c = new CustomizationSkillAbility(info);
    //    TeleportSkill = c;
    //    skillAbilityManager.AddSkillAbility(c);
    //    // 实现
    //    c.IsMeetSkillConditionFunc = delegate { return false; };
    //    c.BeforeSpellFunc = delegate
    //    {

    //    };
    //    c.OnSpellingFunc = delegate
    //    {

    //    };
    //    c.OnNoSpellingFunc = delegate { };
    //    c.IsMeetCloseSpellingConditionFunc = delegate { return true; };
    //    c.AfterSpellFunc = delegate
    //    {

    //    };
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
}
