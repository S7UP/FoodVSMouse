using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEditor.Experimental.GraphView.GraphView;
/// <summary>
/// ��ɫ��λ
/// </summary>
public class CharacterUnit : BaseUnit
{
    // Awake��ȡ�����
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    public Material defaultMaterial;
    public Collider2D mCollider2D;
    public Transform spriteTrans;

    // �������
    public BaseWeapons weapons; // ��������

    // ����
    // type: 0 = female��1 = male
    public int typeAndShapeToLayer = 0; // ��������ֶ�ͼ��ļ�Ȩ�ȼ�

    private BaseGrid mGrid; // ��Ƭ���ڵĸ��ӣ�����)


    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("SpriteGo");
        animator = spriteTrans.gameObject.GetComponent<Animator>();
        spriteRenderer = spriteTrans.gameObject.GetComponent<SpriteRenderer>();
        mCollider2D = transform.GetComponent<Collider2D>();
        defaultMaterial = spriteRenderer.material;  // װ���������ܻ�����
        AnimatorContinue(); // �ָ�����
    }

    // ��λ������ػ���ʱ����
    public override void OnDisable()
    {
        base.OnDisable();
        mGrid = null; // ���ڵĸ���
        spriteRenderer.material = defaultMaterial; // ������
    }

    // ÿ�ζ��󱻴���ʱҪ���ĳ�ʼ������
    public override void MInit()
    {
        base.MInit();
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Character/" + mType + "/" + mShape);
        // ��������
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        AnimatorContinue(); // �ָ����Ŷ���
        SetActionState(new IdleState(this));
        // �Ƴ�ԭ�����������ã������
        if (weapons != null)
            weapons.DeathEvent();
        // �������test
        weapons = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Weapons/0/0").GetComponent<BaseWeapons>();
        weapons.MInit();
        weapons.transform.SetParent(transform);
        weapons.master = this;
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Character;
    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ���������弼�ܼ���ʵ��������������д
    /// </summary>
    public override void LoadSkillAbility()
    {

    }

    /// <summary>
    /// ��������ʱ����
    /// </summary>
    /// <param name="action"></param>
    public void FlashWhenHited(CombatAction action)
    {
        // �����ڹ�����Դʱ
        if (action.Creator != null)
        {
            hitBox.OnHit();
        }
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsHasTarget()
    {
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        return false;
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {

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
        return false;
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {

    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public virtual void ExecuteDamage()
    {

    }

    // �ڴ���״̬ʱÿ֡Ҫ������
    public override void OnIdleState()
    {

    }

    // �ڹ���״̬ʱÿ֡Ҫ������
    public override void OnAttackState()
    {

    }

    public override void BeforeDeath()
    {
        base.BeforeDeath();
        // ����Ҳ��֮����
        if (weapons != null)
        {
            weapons.ExecuteDeath();
        }
    }

    /// <summary>
    /// �����ڼ�
    /// </summary>
    public override void DuringDeath()
    {
        DeathEvent();
    }

    /// <summary>
    /// ��ȡ��Ƭ���ڵĸ���
    /// </summary>
    /// <returns></returns>
    public override BaseGrid GetGrid()
    {
        return mGrid;
    }

    public override void SetGrid(BaseGrid grid)
    {
        mGrid = grid;
    }

    /// <summary>
    /// ���뵥λ������ײʱ�ܷ��赲���ж�
    /// Ĭ��������������ͬ��ʱ�����赲����
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanBlock(BaseUnit unit)
    {
        if (unit is MouseUnit)
        {
            return GetRowIndex() == unit.GetRowIndex();
        }
        return false; // ��ĵ�λ��ʱĬ�ϲ����赲
    }

    /// <summary>
    /// ��ʳ��λĬ�ϱ�����ͬ�е��ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return GetRowIndex() == bullet.GetRowIndex();
    }

    /// <summary>
    /// ״̬���
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animator.Play("Idle");
    }

    public override void OnAttackStateEnter()
    {
        // ÿ�ι���ʱ����ø��ݹ���������һ�²����ٶȣ�Ȼ��ı䲥���ٶ�
        UpdateAttackAnimationSpeed();
        animator.Play("Attack");
    }

    public override void OnAttackStateExit()
    {
        // ���������󲥷��ٶȸĻ���
        animator.speed = 1;
    }

    public override void OnAttackStateContinue()
    {
        // ÿ�ι���ʱ����ø��ݹ���������һ�²����ٶȣ�Ȼ��ı䲥���ٶ�
        UpdateAttackAnimationSpeed();
    }

    public override void OnDieStateEnter()
    {
        // ������ʳ��˵û�����������Ļ���ֱ�ӻ��ն�����У�����Ϸ������־���ֱ����ʧ�����ն��������duringDeath��һ֡ȥ��
    }

    public override void OnBurnStateEnter()
    {
        // ��ֹ���Ŷ���
        AnimatorStop();
    }

    public override void DuringBurn(float _Threshold)
    {
        DeathEvent();
    }

    /// <summary>
    /// ���ݹ����ٶ������¹����������ٶ�
    /// </summary>
    private void UpdateAttackAnimationSpeed()
    {
        float time = AnimatorManager.GetClipTime(animator, "Attack"); // 1������£�һ�ι�����Ĭ��ʱ�� ��
        float interval = 1 / NumericBox.AttackSpeed.Value; // �������  ��
        float rate = Mathf.Max(1, time / interval);
        AnimatorManager.SetClipSpeed(animator, "Attack", rate);
    }

    /// <summary>
    /// ������Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), typeAndShapeToLayer, 2 * arrayIndex);
        UpdateSpecialRenderLayer();
        // ���������㼶
        if (weapons != null)
            weapons.SetSpriteRenderSortingOrder(spriteRenderer.sortingOrder+1);
    }

    /// <summary>
    /// ������ʵ�֣�����������������Ĳ���
    /// </summary>
    public virtual void UpdateSpecialRenderLayer()
    {

    }

    public override void AnimatorStop()
    {
        animator.speed = 0;
    }

    public override void AnimatorContinue()
    {
        animator.speed = 1;
    }

    // �����󣬽�������Ϣ�Ӷ�Ӧ�����Ƴ������ڳ��ռ����������ͬ���ӷ����Ϳ�Ƭʹ��
    public override void AfterDeath()
    {
        RemoveFromGrid();
        if (weapons != null)
        {
            weapons.DeathEvent();
            weapons = null;
        }
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // ���������߼�
        if (weapons != null)
            weapons.MUpdate();
        // �����ܻ���˸״̬
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer.material.SetFloat("_FlashRate", 0.5f * hitBox.GetPercent());
        }
    }

    /// <summary>
    /// ���ñ�������Ч��
    /// </summary>
    /// <param name="enable"></param>
    public override void SetFrozeSlowEffectEnable(bool enable)
    {
        if (enable)
        {
            spriteRenderer.material.SetFloat("_IsSlow", 1);
        }
        else
        {
            spriteRenderer.material.SetFloat("_IsSlow", 0);
        }
    }

    /// <summary>
    /// �������Ƴ�������
    /// </summary>
    public virtual void RemoveFromGrid()
    {
        CharacterUnit c = mGrid.RemoveCharacterUnit();
        if (c != this)
            Debug.LogWarning("�������Ƴ���ɫ�Ǳ���ɫ����");
    }

    /// <summary>
    /// �����ж�
    /// </summary>
    public override void OpenCollision()
    {
        mCollider2D.enabled = true;
    }

    /// <summary>
    /// �ر��ж�
    /// </summary>
    public override void CloseCollision()
    {
        mCollider2D.enabled = false;
    }

    /// <summary>
    /// ����͸����
    /// </summary>
    public override void SetAlpha(float a)
    {
        spriteRenderer.material.SetFloat("_Alpha", a);
    }

    /// <summary>
    /// ��ȡ���±�
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        // �����ڸ����򷵻ض�Ӧ���ӵ��±�
        if (GetGrid() != null)
        {
            return GetGrid().GetRowIndex();
        }
        return base.GetRowIndex();
    }

    /// <summary>
    /// ��ȡ���±�
    /// </summary>
    /// <returns></returns>
    public override int GetColumnIndex()
    {
        // �����ڸ����򷵻ض�Ӧ���ӵ��±�
        if (GetGrid() != null)
        {
            return GetGrid().GetColumnIndex();
        }
        return base.GetColumnIndex();
    }

    /// <summary>
    /// ������ͼ��������
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteTrans.localPosition = vector2;
    }

    /// <summary>
    /// ��ȡ��ͼ
    /// </summary>
    public SpriteRenderer GetSpriteRender()
    {
        return spriteRenderer;
    }
}
