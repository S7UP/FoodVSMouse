/// <summary>
/// ���ȷ�
/// </summary>
public class CoffeePowder : FoodUnit
{
    private static FloatModifier attackSpeedModifier = new FloatModifier(50);
    private static FloatModifier attackModifier = new FloatModifier(100);
    private static BoolModifier boolModifier = new BoolModifier(true);

    public override void MInit()
    {
        base.MInit();
        // ��ȡ100%���ˣ��Լ����߻ҽ���ɱЧ��
        NumericBox.Defense.SetBase(1);
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
        // ����ѡȡ
        CloseCollision();
    }

    public override void OnCastStateEnter()
    {
        animatorController.Play("Cast");
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

    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        AnimatorStateRecorder a = animatorController.GetCurrentAnimatorStateRecorder();
        if (a != null)
        {
            return a.IsFinishOnce();
        }
        return false;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        // �Ե�ǰ��Ƭʩ��Ч��
        ExecuteDamage();
        // ֱ����������
        ExecuteDeath();
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        // ʹ��ǰ��ӵ������ܻ����ȼ��Ŀ�ƬCD����
        BaseGrid g = GetGrid();
        if (g != null)
        {
            foreach (var unit in g.GetAttackableFoodUnitList())
            {
                // �Ƴ���Щ���ĸ������Ч��
                StatusManager.RemoveAllSettleDownDebuff(unit);
                // ���ӹ�������Ч��
                int timeLeft = 10 * 60;
                CustomizationTask t = new CustomizationTask();
                t.OnEnterFunc = delegate {
                    unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
                    unit.NumericBox.Attack.AddPctAddModifier(attackModifier);

                    unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
                    unit.NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier);
                };
                t.AddTaskFunc(delegate {
                    if (timeLeft > 0)
                        timeLeft--;
                    else
                        return true;
                    return false;
                });
                t.OnExitFunc = delegate {
                    unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
                    unit.NumericBox.Attack.RemovePctAddModifier(attackModifier);

                    unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreFrozen, boolModifier);
                    unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.IgnoreStun, boolModifier);
                };
                unit.AddTask(t);
            }
        }
    }
}
