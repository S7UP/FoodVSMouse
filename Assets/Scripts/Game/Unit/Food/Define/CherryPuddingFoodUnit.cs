using System.Collections.Generic;
/// <summary>
/// ӣ�ҷ�������
/// </summary>
public class CherryPuddingFoodUnit : FoodUnit
{
    /// <summary>
    /// ���Ա��������ӵ����ͱ�
    /// </summary>
    public static List<BulletStyle> canReboundBulletStyleList = new List<BulletStyle>() {
        BulletStyle.Normal, BulletStyle.NoStrengthenNormal
    };

    public override void MInit()
    {
        base.MInit();
        CreateReboundArea();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        if(mShape >= 1)
            SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel])*1.25f);
        else
            SetMaxHpAndCurrentHp((float)(attr.baseAttrbute.baseHP + attr.valueList[mLevel]));
    }

    /// <summary>
    /// ��⴫���ӵ��Ƿ��ܷ���
    /// </summary>
    /// <param name="baseBullet"></param>
    /// <returns></returns>
    public bool CanThrought(BaseBullet b)
    {
        // ����������
        if (isFrozenState)
            return false;
        // �ӵ������ѷ��ӵ�
        if (!(b is AllyBullet))
            return false;
        // �ж��ӵ��Ƿ��ѱ�����
        if (b.GetTagCount(StringManager.BulletRebound) > 0)
            return false;
        // �ж��ӵ������ܷ񷴵�
        foreach (var item in canReboundBulletStyleList)
        {
            if (item == b.style)
                return true;
        }
        return false;
    }

    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        // ������Ϊ����״̬�£������ѷ�Ͷ��ѡΪ����Ŀ��
        if(otherUnit is FoodUnit)
        {
            FoodUnit f = otherUnit as FoodUnit;
            if (GetNoCountUniqueStatus(StringManager.Stun) != null && PitcherManager.IsPitcher(f))
                return false;
        }
        return base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// Ѱ�ұ��ض���Ͷ�����Ŀ���㷨��������������ģ�������������ģ�
    /// </summary>
    /// <returns></returns>
    public BaseUnit FindRedirectThrowingObjectTarget(BaseBullet b)
    {
        // ���������������ض���Ͷ����
        if (GetNoCountUniqueStatus(StringManager.Stun)!=null)
            return null;

        // ��Ѱ����Ч��λ
        BaseUnit target = MouseManager.FindTheMostLeftTarget(b.mMasterBaseUnit, float.MinValue, b.transform.position.x, b.GetRowIndex());
        if (target != null)
        {
            return target;
        }
        else
        {
            // ���ûĿ���Ǿ�����һ�Σ���δ��Ҳ���
            target = MouseManager.FindTheMostLeftTarget(b.mMasterBaseUnit, b.transform.position.x, float.MaxValue, b.GetRowIndex());
            return target;
        }
    }

    /// <summary>
    /// ���������ӵ����Ȧ
    /// </summary>
    private void CreateReboundArea()
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(transform.position, 0.5f, 0.5f, "ItemCollideBullet");
        r.isAffectBullet = true;
        r.SetOnBulletEnterAction(OnCollision);
        r.SetOnBulletStayAction(OnCollision);
        {
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                if(this.IsAlive())
                {
                    r.transform.position = transform.position;
                    return false;
                }
                else
                {
                    return true;
                }
            });
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.AddTask(t);
        }
        GameController.Instance.AddAreaEffectExecution(r);
    }

    public void OnCollision(BaseBullet b)
    {
        if (CanThrought(b)) // ����ӵ��ܷ񴩹�
        {
            // �ӵ�����
            b.SetRotate(-b.GetRotate());
            b.AddTag(StringManager.BulletRebound); // Ϊ�ӵ������ѷ����ı�ǣ���ֹ��η���
        }
    }

    /////////////////////////////////���¹��ܾ�ʧЧ������Ҫ���·���/////////////////////////////////////

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        // �����Ϳ�Ƭ����Ҫ
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ����Ҫ
        return true;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �����Ϳ�Ƭ�޹���״̬
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        // �����Ϳ�Ƭ��
        return true;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �����Ϳ�Ƭ��
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        // �����Ϳ�Ƭ��
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // �����Ϳ�Ƭ��
    }
}
