using S7P.Numeric;

using System.Collections.Generic;
/// <summary>
/// �����
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

        // ��ȡ100%���ˣ��ӽ����޵�����ֵ���Լ����߻ҽ���ɱЧ��
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // ����ѡȡ
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new CastState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �˺��ж�֡Ӧ��ִ���ж�
        //if (IsDamageJudgment())
        //{
        //    mAttackFlag = false;
        //    ExecuteDamage();
        //}
        // ��ת�Ժ�����������
        if (mShape >= 2)
            return;
        // ������һת�Ĺ���
        BaseGrid g = GetGrid();
        if (g == null)
            return;
        // �������ò�ͬ���Ϳ���CD
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
    /// �Ƿ�Ϊ�˺��ж�ʱ��
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
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce());
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �����С�Ŀշ��˱���ܻ�������CD
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
        // �ҽ��Ϳ�Ƭֱ����������
        ExecuteDeath();
    }
}
