using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 酒杯灯
/// </summary>
public class CupLight : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier = new FloatModifier(0);
    private float fireValue; // 单个火苗价值（34-44)
    private int growTimeLeft; // 成长剩余时间
    private int fireCount; // 火苗数
    private float lastProductivity; // 上一帧生产效率

    // 生产类卡片，没有攻击能力，在技能里不填写相关信息即可无法攻击
    public override void MInit()
    {
        fireCount = 0;
        timer = 0;
        lastProductivity = 0;
        fireValue = 34;
        growTimeLeft = 1;
        base.MInit();
        // 剩余成长时间
        growTimeLeft = GetGrowTime(mShape, mLevel);
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // 如果原来的存在，要先移除原来的
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        if (mShape > 1)
            fireCount = 2;
        else
            fireCount = 1;
        // 根据星级计算出新的  算法为 生产效率（攻击力）*火苗数*fireValue/间隔（秒）/60帧
        floatModifier.Value = GetCurrentProductivity();
        lastProductivity = floatModifier.Value;
        // 加回去
        GameController.Instance.AddCostResourceModifier("Fire", floatModifier);
    }

    // 死亡前就清除帧回效果
    public override void BeforeDeath()
    {
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        base.BeforeDeath();
    }

    /// <summary>
    /// 再移除一次，以防止强制移除效果不触发产能清空判定bug
    /// </summary>
    public override void AfterDeath()
    {
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
    }

    public override void MUpdate()
    {
        if (GetCurrentProductivity() != lastProductivity)
        {
            UpdateAttributeByLevel();
        }
        base.MUpdate();
    }

    public override void OnIdleStateEnter()
    {
        if(growTimeLeft > 0)
            animatorController.Play("Idle", true);
        else
            animatorController.Play("Idle2", true);
    }

    public override void OnIdleState()
    {
        // 第60帧时回复火苗数*fireValue火
        if (timer == 60)
        {
            float replyCount = Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack / 10 * fireCount * fireValue;
            SmallStove.CreateAddFireEffect(transform.position, replyCount);
        }
        timer++;
        growTimeLeft--;
        if (growTimeLeft == 0)
            SetActionState(new TransitionState(this));
        base.OnIdleState();
    }

    public override void OnTransitionStateEnter()
    {
        fireValue = 44;
        animatorController.Play("Grow");
        GameManager.Instance.audioSourceController.PlayEffectMusic("Grow");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            growTimeLeft = 0;
            SetActionState(new IdleState(this));
        }
    }

    /// <summary>
    /// 获取当前生产力（每帧回复量）
    /// </summary>
    /// <returns></returns>
    public float GetCurrentProductivity()
    {
        if (isFrozenState)
            return 0;
        else
            return Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack/10 * (float)(fireCount * fireValue) / attr.valueList[mLevel] / 60;
    }

    /// <summary>
    /// 获取成长时间
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static int GetGrowTime(int shape, int level)
    {
        FoodUnit.Attribute attr = GameManager.Instance.attributeManager.GetFoodUnitAttribute((int)FoodNameTypeMap.CupLight, shape);
        if (shape < 2)
            return Mathf.CeilToInt(90*attr.valueList[level]);
        else
            return Mathf.CeilToInt(15*attr.valueList[level]);
    }
}
