/// <summary>
/// ������
/// </summary>
public class IceEggBullet : ParabolaBullet
{
    public override void MInit()
    {
        base.MInit();
        SetCanAttackFood(false);
        SetCanAttackMouse(true);
    }

    /// <summary>
    /// ����Χ���AOE����Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // ԭ�ز���һ������Ч��
        IceSlowDownAreaEffectExecution iceSlowEffect = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/IceSlowDownAreaEffect").GetComponent<IceSlowDownAreaEffectExecution>();
        iceSlowEffect.Init(this.mMasterBaseUnit, 120, GetRowIndex(), 3, 3, -0.5f, 0, false, true); // �ڶ�������Ϊ����ʱ�䣨֡��
        iceSlowEffect.SetAffectHeight(0); // ֻ�е��浥λ��Ӱ��
        // �ٲ���һ����Χ�˺�Ч��
        DamageAreaEffectExecution dmgEffect = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/DamageAreaEffect").GetComponent<DamageAreaEffectExecution>();
        dmgEffect.Init(this.mMasterBaseUnit, CombatAction.ActionType.CauseDamage, 0.25f*GetDamage(), GetRowIndex(), 3, 3, -0.5f, 0, false, true);
        dmgEffect.SetAffectHeight(0);
        if (baseUnit != null && baseUnit.IsValid())
        {
            // �����λ���ڣ����ڵ�λλ�ñ�ը�����ҵ�λ�ܵ�ȫ���˺�
            new DamageAction(CombatAction.ActionType.CauseDamage, this.mMasterBaseUnit, baseUnit, 0.75f * GetDamage()).ApplyAction();
            iceSlowEffect.transform.position = baseUnit.transform.position;
            dmgEffect.transform.position = baseUnit.transform.position;
        }
        else
        {
            // ����λ�ڸ��������ı�ը
            iceSlowEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
            dmgEffect.transform.position = iceSlowEffect.transform.position;
        }
        GameController.Instance.AddAreaEffectExecution(iceSlowEffect);
        GameController.Instance.AddAreaEffectExecution(dmgEffect);
        ExecuteHitAction(baseUnit);
        KillThis();
    }

    //public override void OnTriggerEnter2D(Collider2D collision)
    //{
        
    //}

    //public override void OnTriggerStay2D(Collider2D collision)
    //{
        
    //}

    //public override void OnTriggerExit2D(Collider2D collision)
    //{

    //}
}
