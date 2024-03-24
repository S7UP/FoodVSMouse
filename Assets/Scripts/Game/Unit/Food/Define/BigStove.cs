using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 大火炉
/// </summary>
public class BigStove : FoodUnit
{
    private int timer;
    private FloatModifier floatModifier = new FloatModifier(0);
    private int fireCount; // 火苗数
    private float lastProductivity; // 上一帧生产效率

    // 生产类卡片，没有攻击能力，在技能里不填写相关信息即可无法攻击
    public override void MInit()
    {
        fireCount = 0;
        timer = 0;
        lastProductivity = 0;
        base.MInit();
    }

    /// <summary>
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        // 如果原来的存在，要先移除原来的
        GameController.Instance.RemoveCostResourceModifier("Fire", floatModifier);
        // 根据星级计算出新的  算法为 生产效率（攻击力）*火苗数*44/间隔（秒）/60帧
        switch (mShape)
        {
            case 0:
                fireCount = 2;
                break;
            case 1:
                fireCount = 3;
                break;
            default:
                fireCount = 4;
                break;
        }
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
        base.AfterDeath();
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

    public override void OnIdleState()
    {
        // 第60帧时回复火苗数*44火
        if (timer == 60)
        {
            float replyCount = Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack / 10 * fireCount * 44;
            SmallStove.CreateAddFireEffect(transform.position, replyCount);
        }
        timer++;
        base.OnIdleState();
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
            return Mathf.Min(1, mCurrentAttackSpeed) * mCurrentAttack / 10 * (float)(fireCount * 44) / attr.valueList[mLevel] / 60;
    }
}
