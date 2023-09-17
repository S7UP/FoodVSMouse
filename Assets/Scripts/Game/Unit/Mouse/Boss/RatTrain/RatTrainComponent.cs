
using UnityEngine;
/// <summary>
/// ����г���������Ϊ��ͷ�ͳ���ĸ��ࣩ
/// </summary>
public abstract class RatTrainComponent : MouseModel
{
    private BaseRatTrain master; // �󶨵�BOSS����
    private float dmgRate; // �˺���������
    private float burnDmgRate; // �ҽ��˺���������
    protected int bodyLayerIndex; // λ�����˵ĵڼ���

    public override void MInit()
    {
        bodyLayerIndex = -1;
        master = null;
        dmgRate = 0;
        burnDmgRate = 0;
        base.MInit();
        // ��ӵ�ͬ��BOSS�����߻���
        BossUnit.AddBossIgnoreDebuffEffect(this);
        canTriggerCat = false;
        canTriggerLoseWhenEnterLoseLine = false;
        SetIsMouseUnit(false);

        isBoss = true; // ����Ҳ��BOSS�ж�
        SetActionState(new MoveState(this));
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    public override void LoadSkillAbility()
    {

    }

    public override void OnMoveStateEnter()
    {
        animatorController.Play("Move", true);
    }

    public override void OnMoveState()
    {

    }

    /// <summary>
    /// Ҫ�ڷ�Χ�ڲ��ܱ�ѡΪ����Ŀ��
    /// </summary>
    /// <param name="otherUnit"></param>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// ��ȡ��ǰ�ƶ��ٶ�
    /// </summary>
    /// <returns></returns>
    public override float GetMoveSpeed()
    {
        if(master!=null)
            return master.GetMoveSpeed();
        return 0;
    }

    public override float OnDamage(float dmg)
    {
        return 0;
    }

    public override float OnAoeDamage(float dmg)
    {
        return 0;
    }

    public override float OnDamgeIgnoreShield(float dmg)
    {
        return 0;
    }

    public override float OnRealDamage(float dmg)
    {
        return 0;
    }

    public override float OnBurnDamage(float dmg)
    {
        return 0;
    }

    public override float OnBombBurnDamage(float dmg)
    {
        return 0;
    }

    public override void AddRecordDamage(float value)
    {

    }

    public override void OnCure(float cure)
    {
        // Ȼ��ʲôҲ���ᷢ��������ζ�Ž̻ʵĻظ�Ч��������ͨ�����ᴫ����BOSS
    }

    public BaseRatTrain GetMaster()
    {
        return master;
    }

    public void SetMaster(BaseRatTrain master)
    {
        this.master = master;
        SetMaxHpAndCurrentHp(master.mMaxHp);
        SetGetCurrentHpFunc(delegate { return master.mCurrentHp; });
        SetGetBurnRateFunc(delegate { return master.mBurnRate; });
        // �˺����ݻ���
        AddActionPointListener(ActionPointType.PreReceiveDamage, (combat) =>
        {
            if (combat is DamageAction)
            {
                
                DamageAction dmgAction = combat as DamageAction;
                if (dmgAction.IsDamageType(DamageAction.DamageType.BombBurn))
                {
                    DamageAction d = DamageActionManager.Copy(dmgAction, dmgAction.Creator, master);
                    d.DamageValue = dmgAction.DamageValue * burnDmgRate;
                    d.ApplyAction();
                }
                else
                {
                    DamageAction d = DamageActionManager.Copy(dmgAction, dmgAction.Creator, master);
                    d.DamageValue = dmgAction.DamageValue * dmgRate;
                    d.ApplyAction();
                }
            }
        });
    }

    public float GetDmgRate()
    {
        return dmgRate;
    }

    public void SetDmgRate(float dmgrate, float burnrate)
    {
        dmgRate = dmgrate;
        burnDmgRate = burnrate;
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


    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, -1, 0, bodyLayerIndex);
    }
}
