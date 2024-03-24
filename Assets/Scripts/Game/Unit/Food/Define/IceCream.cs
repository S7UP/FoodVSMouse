using S7P.Numeric;

using System.Collections.Generic;
/// <summary>
/// 冰淇淋
/// </summary>
public class IceCream : FoodUnit
{
    private Dictionary<FoodInGridType, bool> IsCdDict = new Dictionary<FoodInGridType, bool>()
    {
        { FoodInGridType.Default, false }, { FoodInGridType.Bomb, false }, { FoodInGridType.Shield, false }, { FoodInGridType.LavaVehicle, false }, { FoodInGridType.WaterVehicle, false }, { FoodInGridType.NoAttach, false },
    };

    public override void MInit()
    {
        base.MInit();
        {
            List<FoodInGridType> list = new List<FoodInGridType>();
            foreach (var keyValuePair in IsCdDict)
                list.Add(keyValuePair.Key);
            foreach (var key in list)
                IsCdDict[key] = false;
        }

        // 获取100%减伤，接近无限的生命值，以及免疫灰烬秒杀效果
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // 不可选取
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        return false;
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new CastState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 伤害判定帧应当执行判定
        //if (IsDamageJudgment())
        //{
        //    mAttackFlag = false;
        //    ExecuteDamage();
        //}
        // 二转以后是其他功能
        if (mShape >= 2)
            return;
        // 以下是一转的功能
        BaseGrid g = GetGrid();
        if (g == null)
            return;
        // 持续重置不同类型卡的CD
        foreach (var f in g.GetFoodUnitList())
        {
            if (!IsCdDict[f.GetFoodInGridType()] && !CardBuilderManager.IsGoldenCard((FoodNameTypeMap)f.mType) && f.GetCardBuilder() != null && !f.GetCardBuilder().IsColdDown() && f.mType != mType)
            {
                BaseCardBuilder builder = f.GetCardBuilder();
                builder.ResetCD();
                IsCdDict[f.GetFoodInGridType()] = true;
            }
        }
    }

    /// <summary>
    /// 是否为伤害判定时刻
    /// </summary>
    /// <returns></returns>
    //public override bool IsDamageJudgment()
    //{
    //    BaseGrid g = GetGrid();
    //    if (g == null)
    //        return false;
    //    bool flag = false;
    //    foreach (var f in g.GetAttackableFoodUnitList())
    //    {
    //        if(!CardBuilderManager.IsGoldenCard((FoodNameTypeMap)f.mType) && f.GetCardBuilder()!=null && !f.GetCardBuilder().IsColdDown())
    //        {
    //            flag = true;
    //            break;
    //        }
    //    }
    //    return (mAttackFlag && flag);
    //}

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce());
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // 如果不小心空放了冰淇淋还会重置CD
        bool flag = true;
        if (mShape < 2)
        {
            foreach (var keyValuePair in IsCdDict)
            {
                if(keyValuePair.Value)
                {
                    flag = false;
                    break;
                }
            }
            if(flag)
                GetCardBuilder().ResetCD();
        }
        else
        {
            foreach (var builder in GameController.Instance.mCardController.mCardBuilderList)
            {
                if (!CardBuilderManager.IsGoldenCard((FoodNameTypeMap)builder.mType) && builder.mType != mType)
                    builder.ResetCD();
            }
        }
        // 灰烬型卡片直接销毁自身
        ExecuteDeath();
    }
}
