
using UnityEngine;
/// <summary>
/// ��ɫ��λ
/// </summary>
public class CharacterUnit : BaseUnit
{
    // Awake��ȡ�����
    protected Animator animator;
    private SpriteRenderer spriteRenderer;
    public Material defaultMaterial;
    public BoxCollider2D mBoxCollider2D;
    public Transform spriteTrans;

    // �������
    public BaseWeapons weapons; // ��������

    // ����
    public int typeAndShapeToLayer = 0; // ��������ֶ�ͼ��ļ�Ȩ�ȼ�

    private BaseGrid mGrid; // ��Ƭ���ڵĸ��ӣ�����)


    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("SpriteGo");
        animator = spriteTrans.gameObject.GetComponent<Animator>();
        spriteRenderer = spriteTrans.gameObject.GetComponent<SpriteRenderer>();
        mBoxCollider2D = transform.GetComponent<BoxCollider2D>();
        defaultMaterial = spriteRenderer.material;  // װ���������ܻ�����
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
        PlayerData data = PlayerData.GetInstance();
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Character/" + data.GetCharacter());
        // ������������animator
        animatorController.ChangeAnimator(animator);
        // ��������
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        SetActionState(new IdleState(this));
        // �Ƴ�ԭ�����������ã������
        if (weapons != null)
            weapons.DeathEvent();
        // �������
        int type = data.GetWeapons();
        weapons = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Weapons/"+type+"/0").GetComponent<BaseWeapons>();
        weapons.MInit();
        weapons.transform.SetParent(transform);
        weapons.master = this;
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Character;
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
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
        // if (action.Creator != null)
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
        animatorController.Play("Idle", true);
    }

    public override void OnAttackStateEnter()
    {
        // ÿ�ι���ʱ����ø��ݹ���������һ�²����ٶȣ�Ȼ��ı䲥���ٶ�
        UpdateAttackAnimationSpeed();
        animatorController.Play("Attack");
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
        // PauseCurrentAnimatorState(new BoolModifier(true));
    }

    public override void DuringBurn(float _Threshold)
    {
        DeathEvent();
    }

    private BoolModifier boolModifier = new BoolModifier(true);

    /// <summary>
    /// ˤ������˲��
    /// </summary>
    public override void OnDropStateEnter()
    {
        // ��ֹ���Ŷ���
        PauseCurrentAnimatorState(boolModifier);
    }

    /// <summary>
    /// ˤ����������
    /// </summary>
    public override void OnDropState(float r)
    {
        SetAlpha(1-r);
        spriteRenderer.transform.localPosition = spriteRenderer.transform.localPosition + 0.25f * MapManager.gridHeight * r * Vector3.down;
        spriteRenderer.transform.localScale = Vector3.one * (1 - r);
        // ����1�Ϳ��Ի�����
        if (r >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            SetAlpha(1);
            spriteRenderer.transform.localPosition = Vector3.zero;
            spriteRenderer.transform.localScale = Vector3.one;
            DeathEvent();
        }
    }

    /// <summary>
    /// ���ݹ����ٶ������¹����������ٶ�
    /// </summary>
    private void UpdateAttackAnimationSpeed()
    {
        AnimatorStateRecorder a = animatorController.GetAnimatorStateRecorder("Attack");
        float time = a.aniTime; // һ������һ�ι��������Ĳ���ʱ�䣨֡��
        float interval = 1 / NumericBox.AttackSpeed.Value*60;  // ���������֡��
        float speed = Mathf.Max(1, time / interval); // ���㶯��ʵ�ʲ����ٶ�
        animatorController.SetSpeed("Attack", speed);
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

    // �����󣬽�������Ϣ�Ӷ�Ӧ�����Ƴ������ڳ��ռ����������ͬ���ӷ����Ϳ�Ƭʹ��
    public override void AfterDeath()
    {
        RemoveFromGrid();
        if (weapons != null)
        {
            weapons.DeathEvent();
            weapons = null;
        }
        // ���½�ɫ����������Ϣ
        if (GameController.Instance.mCharacterController != null)
        {
            GameController.Instance.mCharacterController.AfterCharacterDeath();
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
        mBoxCollider2D.enabled = true;
    }

    /// <summary>
    /// �ر��ж�
    /// </summary>
    public override void CloseCollision()
    {
        mBoxCollider2D.enabled = false;
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
        if(weapons!=null)
            weapons.transform.localPosition = vector2;
    }

    /// <summary>
    /// ��ȡ��ͼ
    /// </summary>
    public SpriteRenderer GetSpriteRender()
    {
        return spriteRenderer;
    }

    /// <summary>
    /// �ɷ�ѡ��ΪĿ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return mBoxCollider2D.enabled && base.CanBeSelectedAsTarget(otherUnit);
    }

    public override void MPause()
    {
        base.MPause();
        // ��ͣ��������
        if(weapons!=null)
            weapons.MPause();
    }

    public override void MResume()
    {
        base.MResume();
        // ȡ����ͣ��������
        if (weapons != null)
            weapons.MResume();
    }

    /// <summary>
    /// ��ȡ��ͼ����
    /// </summary>
    public override Sprite GetSpirte()
    {
        return spriteRenderer.sprite;
    }

    /// <summary>
    /// ��ȡSpriterRenderer
    /// </summary>
    /// <returns></returns>
    public override SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer;
    }

    private WeaponsFrozenState frozenStatus;

    /// <summary>
    /// ������ʱ��������һ�𱻶���
    /// </summary>
    public override void OnFrozenStateEnter()
    {
        base.OnFrozenStateEnter();
        frozenStatus = new WeaponsFrozenState(weapons, weapons.mCurrentActionState);
        weapons.SetActionState(frozenStatus);
    }

    /// <summary>
    /// �ⶳʱ��������һ��ⶳ
    /// </summary>
    public override void OnFrozenStateExit()
    {
        base.OnFrozenStateExit();
        frozenStatus.TryExitCurrentState();
    }
}
