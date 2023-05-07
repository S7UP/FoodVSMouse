using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;

public class MouseUnit : BaseUnit
{
    // ����λ������
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public double[] hertRateList;
    }

    // Awake��Findһ�ε����
    public Rigidbody2D rigibody2D;
    public BoxCollider2D mBoxCollider2D;
    public SpriteRenderer spriteRenderer;
    protected Animator animator;
    public Transform spriteTrans;
    
    // ��������
    protected List<double> mHertRateList = new List<double>(); // �л���ͼʱ�����˱��ʣ���->��)
    public int mHertIndex; // ������ͼ�׶�
    public int currentXIndex; // ��ǰ�ر�X�±�
    public int currentYIndex; // ��ǰ�ر�Y�±�
    public bool isBoss; // �Ƿ���BOSS��λ
    public bool canDrivenAway; // �ܷ�ǿ�ƻ���

    public string AttackClipName;
    public string IdleClipName;
    public string MoveClipName;
    public string DieClipName;

    // �������
    protected bool isBlock { get; set; } // �Ƿ��赲
    protected BaseUnit mBlockUnit; // �赲��

    // �������
    // ����Σ�ն�Ȩ�ر������޸��ض�ֵʹ��ĳЩ���͵ĵ�����������ε�����
    public Dictionary<GridType, int> GridDangerousWeightDict;

    /// <summary>
    /// ֻ�ж��󱻴���ʱ��һ�Σ���Ҫ��������ȡ�����������
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        // �����ȡ
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.transform.Find("Ani_Mouse").gameObject.GetComponent<Animator>();
        spriteRenderer = gameObject.transform.Find("Ani_Mouse").gameObject.GetComponent<SpriteRenderer>();
        mBoxCollider2D = gameObject.GetComponent<BoxCollider2D>();
        spriteTrans = transform.Find("Ani_Mouse");
    }

    /// <summary>
    /// ����ÿ�α�Ͷ��ս��ʱҪ���ĳ�ʼ��������Ҫȷ�����������
    /// </summary>
    public override void MInit()
    {
        isBlock = false; // �Ƿ��赲
        mBlockUnit = null; // �赲��
        isBoss = false;

        AttackClipName = "Attack";
        IdleClipName = "Idle";
        MoveClipName = "Move";
        DieClipName = "Die";

        base.MInit();

        // ������������animator
        animatorController.ChangeAnimator(animator);

        mHertRateList.Clear();
        mHertIndex = 0;
        currentXIndex = 0;
        currentYIndex = 0;
        moveRotate = Vector2.left;
        canDrivenAway = true;

        // ��ʼ��
        GridDangerousWeightDict = new Dictionary<GridType, int>()
        {
            { GridType.Default, 10},
            { GridType.NotBuilt, 10},
            { GridType.Water, 12}, // ������֪ˮ�Ǿ綾��
            { GridType.Lava, 11},
            { GridType.Sky, 13}, // ������ָ�
            { GridType.Teleport, 10 }, // ����װ�ã�̫cool�ˣ���
        };

        // ��ʼΪ�ƶ�״̬
        SetActionState(new MoveState(this));

        // ��Ӽ����ж�����Ӧ�¼�
        // ���ܵ��˺�����֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        // װ���������ܻ�����
        spriteRenderer.material = GameManager.Instance.GetMaterial("Hit");
        OnHertStageChanged(); // ������ͼ
        // �޸ĵ���Σ�ն�Ȩֵ��
        SetGridDangerousWeightDict();
    }

    public override void MUpdate()
    {
        base.MUpdate();
        int lastXIndex = currentXIndex;
        int lastYIndex = currentYIndex;
        currentXIndex = MapManager.GetXIndex(transform.position.x);
        currentYIndex = MapManager.GetYIndex(transform.position.y);
        // �����ӵ��ж����귢���ı�
        if (lastYIndex != currentYIndex)
        {
            // ����
            GameController.Instance.ChangeEnemyRow(lastYIndex, this);
        }
        // �����ܻ���˸״̬
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer.material.SetFloat("_FlashRate", 0.75f * hitBox.GetPercent());
        }
        // �����ж�
        if (CanTriggerLoseWhenEnterLoseLine() && transform.position.x < MapManager.GetColumnX(-1.5f))
        {
            GameController.Instance.Lose();
        }
        else
        {
            // �����ж�
            if (IsOutOfBound())
            {
                DeathEvent();
            }
        }

    }

    public override void MDestory()
    {
        NumericBox.Initialize();
        base.MDestory();
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Mouse;
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f*MapManager.gridWidth, 0.49f*MapManager.gridHeight);
    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ���������弼�ܼ���ʵ��������������д
    /// </summary>
    public override void LoadSkillAbility()
    {
        foreach (var item in AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape))
        {
            if (item.skillType == SkillAbility.Type.GeneralAttack)
            {
                skillAbilityManager.AddSkillAbility(new GeneralAttackSkillAbility(this, item));
            }
        }
    }

    /// <summary>
    /// ���ٴ󲿷�����Ӧ����һ����ֱ�ӹ������ֶ�
    /// </summary>
    /// <param name="unit"></param>
    public void TakeDamage(BaseUnit unit)
    {
        new DamageAction(CombatAction.ActionType.CauseDamage, this, unit, mCurrentAttack).ApplyAction();
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ���赲�� �� �赲��������Ч��
        return IsHasTarget();
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        UpdateBlockState(); // �����赲״̬
        // ����п��Թ�����Ŀ�꣬��ͣ�����ȴ���һ�ι���������ǰ��
        if (IsHasTarget())
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
    }

    /// <summary>
    /// �ж�������Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public virtual bool IsHasTarget()
    {
        if (IsBlock())
        {
            // ��Ŀ�������ڸ��ӣ���Ŀ���л�ΪĿ�����ڸ����߹������ȼ�Ŀ��
            BaseGrid g = mBlockUnit.GetGrid();
            if (g != null)
            {
                mBlockUnit = g.GetHighestAttackPriorityUnit(this);
                UpdateBlockState();
            }
            return IsBlock();
        }
        return false;
    }

    /// <summary>
    /// ��ȡ��ǰ����Ŀ�꣬�����ж�һ��ʹ��
    /// </summary>
    /// <returns></returns>
    public virtual BaseUnit GetCurrentTarget()
    {
        return mBlockUnit;
    }

    /// <summary>
    /// �����赲��״̬
    /// </summary>
    protected virtual void UpdateBlockState()
    {
        if (mBlockUnit != null && mBlockUnit.IsAlive() && UnitManager.CanBeSelectedAsTarget(this, mBlockUnit) && UnitManager.CanBlock(this, mBlockUnit))
        {
            isBlock = true;
        }
        else
            SetNoCollideAllyUnit();
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public virtual void ExecuteDamage()
    {
        if (IsHasTarget())
            TakeDamage(GetCurrentTarget());
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Chomp" + GameManager.Instance.rand.Next(0, 3));
    }


    public override void OnIdleState()
    {
        UpdateBlockState();
        if (!isBlock)
            SetActionState(new MoveState(this));
    }

    public override void OnMoveState()
    {
        // �ƶ�����
        SetPosition((Vector2)GetPosition() + moveRotate * GetMoveSpeed());
    }

    public override void OnAttackState()
    {

    }

    /// <summary>
    /// ����λĬ�϶��Ǵ���ͬ����ͬ�߶�ʱ����ʳ�������ﵥλ�赲
    /// ��ʾ�������������������д���������ǿ����Ϊ�����赲����������
    /// </summary>
    public override bool CanBlock(BaseUnit unit)
    {
        return ((unit is FoodUnit || unit is CharacterUnit) && UnitManager.CanBeSelectedAsTarget(this, unit) && GetRowIndex() == unit.GetRowIndex() && mHeight == unit.mHeight);
    }

    /// <summary>
    /// ����λĬ�ϱ�����ͬ�е��ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return IsAlive();
    }

    /// <summary>
    /// �����˻��߱�����ʱ�����µ�λ��ͼ״̬
    /// </summary>
    public virtual void UpdateHertMap()
    {
        // Ҫ�����˵Ļ������˰�
        if (isDeathState)
            return;

        // �Ƿ�Ҫ�л���������flag
        bool flag = false;
        // �ָ�����һ��������ͼ���
        while (mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex--;
            flag = true;
        }
        // ��һ��������ͼ�ļ��
        while (mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex++;
            flag = true;
        }
        // ���л�֪ͨʱ���л�
        if (flag)
            OnHertStageChanged();
    }

    /// <summary>
    /// �����˽׶ε��л�ʱ
    /// </summary>
    /// <param name="collision"></param>
    public virtual void OnHertStageChanged()
    {
        AnimatorStateRecorder a = animatorController.GetCurrentAnimatorStateRecorder(); // ��ȡ��ǰ�ڲ��ŵĶ���
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + mType + "/" + mShape + "/" + mHertIndex);
        animatorController.ChangeAnimator(animator);
        // ���ֵ�ǰ��������
        if (a != null)
        {
            animatorController.Play(a.aniName, a.isCycle, a.GetNormalizedTime());
        }
        OnUpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// ��������ʱ����
    /// </summary>
    /// <param name="action"></param>
    public void FlashWhenHited(CombatAction action)
    {
        hitBox.OnHit();
    }

    /// <summary>
    /// ��������ʱ����
    /// </summary>
    public void FlashWhenHited()
    {
        {
            hitBox.OnHit();
        }
    }

    /// <summary>
    /// ����ͼ����ʱҪ�����£�������override
    /// </summary>
    public virtual void OnUpdateRuntimeAnimatorController()
    {

    }

    /// <summary>
    /// ȡ���Ӵ��ѷ���λ
    /// </summary>
    /// <param name="unit"></param>
    public void SetNoCollideAllyUnit()
    {
        isBlock = false;
        mBlockUnit = null;
    }

    /// <summary>
    /// �Ƿ��赲
    /// </summary>
    /// <returns></returns>
    public bool IsBlock()
    {
        return isBlock && mBlockUnit!=null && mBlockUnit.IsAlive() && UnitManager.CanBeSelectedAsTarget(this, mBlockUnit);
    }

    /// <summary>
    /// ��ײ�¼�
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        // ��������ʱ�������κ���ײ�¼�
        if (isDeathState)
        {
            return;
        }

        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            // ��⵽�ѷ���λ��ײ�ˣ�
            OnAllyCollision(collision.GetComponent<BaseUnit>());
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // ��⵽�ӵ���λ��ײ��
            OnBulletCollision(collision.GetComponent<BaseBullet>());
        }
    }

    /// <summary>
    /// �����ѷ���λ����ʳ���������������ײ�ж�ʱ
    /// </summary>
    public virtual void OnAllyCollision(BaseUnit unit)
    {
        // ��Ȿ����ʳ����ܻ����ȼ���λ
        BaseUnit target = unit.GetGrid().GetHighestAttackPriorityUnit(this);
        if (target!=null && !isBlock && UnitManager.CanBlock(this, target)) // ���˫���ܷ����赲���������ܷ񽫸�Ŀ����Ϊ����Ŀ��(�����GetHighestAttackPriorityUnit�Ѿ����ˣ�
        {
            isBlock = true;
            mBlockUnit = target;
        }
    }

    /// <summary>
    /// ���ѷ���λ�뿪ʱ
    /// </summary>
    /// <param name="collision"></param>
    public virtual void OnAllyTriggerExit(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Character"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            if (mBlockUnit == unit)
            {
                SetNoCollideAllyUnit();
            }
        }
    }

    /// <summary>
    /// �����ӵ���λ����������ײ�ж�ʱ
    /// </summary>
    public virtual void OnBulletCollision(BaseBullet bullet)
    {
        if (UnitManager.CanBulletHit(this, bullet)) // ���˫���ܷ������
        {
            bullet.TakeDamage(this);
        }
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public virtual void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {
        OnAllyTriggerExit(collision);
    }

    /// <summary>
    /// �Ƿ�����ж�
    /// </summary>
    /// <returns></returns>
    public virtual bool IsOutOfBound()
    {
        return !isBoss && (GetColumnIndex() > MapController.xColumn + 2 || GetColumnIndex() <= -2 || GetRowIndex() <= -1 || GetRowIndex() >= 7);
    }

    /// <summary>
    /// ���ݹ����ٶ������¹����������ٶ�
    /// </summary>
    protected void UpdateAttackAnimationSpeed()
    {
        AnimatorStateRecorder a = animatorController.GetAnimatorStateRecorder(AttackClipName);
        if (a != null)
        {
            float time = a.aniTime; // һ������һ�ι��������Ĳ���ʱ�䣨֡��
            float interval = 1 / NumericBox.AttackSpeed.Value * 60;  // ���������֡��
            float speed = Mathf.Max(1, time / interval); // ���㶯��ʵ�ʲ����ٶ�
            animatorController.SetSpeed(AttackClipName, speed);
        }
    }

    /// <summary>
    /// ״̬���
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animatorController.Play(IdleClipName, true);
    }

    public override void OnMoveStateEnter()
    {
        animatorController.Play(MoveClipName, true);
    }

    public override void OnAttackStateEnter()
    {
        // ÿ�ι���ʱ����ø��ݹ���������һ�²����ٶȣ�Ȼ��ı䲥���ٶ�
        UpdateAttackAnimationSpeed();
        animatorController.Play(AttackClipName);
    }

    public override void OnDieStateEnter()
    {
        animatorController.Play(DieClipName);
        animatorController.SetNoPlayOtherClip(true); // �����г���������
    }

    public override void OnAttackStateContinue()
    {
        // ÿ�ι���ʱ����ø��ݹ���������һ�²����ٶȣ�Ȼ��ı䲥���ٶ�
        UpdateAttackAnimationSpeed();
    }

    public override void OnAttackStateExit()
    {
        // �ָ������ٶ�
        // ResumeAnimationSpeed();
    }

    public override void OnAttackStateInterrupt()
    {
        // �ָ������ٶ�
        // ResumeAnimationSpeed();
    }

    public override void DuringDeath()
    {
        // ��ȡDie�Ķ�����Ϣ��ʹ�û���ʱ���붯����ʾ������ͬ��
        AnimatorStateRecorder r = animatorController.GetCurrentAnimatorStateRecorder();
        if (r==null || r.IsFinishOnce())
        {
            DeathEvent();
        }
    }

    private BoolModifier boolModifier = new BoolModifier(true);

    public override void OnBurnStateEnter()
    {
        // װ���ջٲ���
        spriteRenderer.material = GameManager.Instance.GetMaterial("Dissolve2");
        // ��ֹ���Ŷ���
        PauseCurrentAnimatorState(boolModifier);
        animatorController.SetNoPlayOtherClip(true);
    }

    public override void DuringBurn(float _Threshold)
    {
        spriteRenderer.material.SetFloat("_Threshold", _Threshold);
        // ����1�Ϳ��Ի�����
        if (_Threshold >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            DeathEvent();
        }
    }

    /// <summary>
    /// ˤ������˲��
    /// </summary>
    public override void OnDropStateEnter()
    {
        // ��ֹ���Ŷ���
        PauseCurrentAnimatorState(boolModifier);
        animatorController.SetNoPlayOtherClip(true);
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
    /// ������ͬ������˵���Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), typeAndShapeValue, arrayIndex);
    }

    public override bool TryGetSpriteRenternerSorting(out string name, out int order)
    {
        if (spriteRenderer == null)
            return base.TryGetSpriteRenternerSorting(out name, out order);
        name = spriteRenderer.sortingLayerName;
        order = spriteRenderer.sortingOrder;
        return true;
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
    /// �����ƶ�����
    /// </summary>
    public void SetMoveRoate(Vector2 v2)
    {
        moveRotate = v2;
    }

    /// <summary>
    /// ������ͼ��������
    /// </summary>
    public override void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteTrans.localPosition = vector2;
    }

    /// <summary>
    /// ��ȡ��ͼ�����������
    /// </summary>
    /// <returns></returns>
    public override Vector2 GetSpriteLocalPosition()
    {
        return spriteTrans.localPosition;
    }

    public override int GetColumnIndex()
    {
        return currentXIndex;
    }

    public override int GetRowIndex()
    {
        return currentYIndex;
    }

    /// <summary>
    /// �ɷ�ѡ��ΪĿ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return mBoxCollider2D.enabled && IsAlive() && base.CanBeSelectedAsTarget(otherUnit);
    }

    /// <summary>
    /// Ŀ���Ƿ���BOSS��λ
    /// </summary>
    /// <returns></returns>
    public bool IsBoss()
    {
        return isBoss;
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

    /// <summary>
    /// �ܷ���ʹ����
    /// </summary>
    /// <returns></returns>
    public virtual bool CanDrivenAway()
    {
        return canDrivenAway;
    }

    /// <summary>
    /// ����ʹ��������
    /// </summary>
    public void DrivenAway()
    {
        if (GetTask("DrivenAway") != null)
            return;

        // ���������¸��Σ��Ȩ�أ�Ȼ��ȡΣ��Ȩ��С�Ļ���
        int currentRowIndex = GetRowIndex();
        int currentColumnIndex = GetColumnIndex();
        int startIndex = Mathf.Max(0, currentRowIndex - 1);
        int endIndex = Mathf.Min(6, currentRowIndex + 1);
        List<int> rowIndexList = new List<int>();
        // �ⲽ��ȡ��Ȩ����С�ļ�������
        int min = int.MaxValue;
        for (int i = startIndex; i <= endIndex; i++)
        {
            int weight;
            BaseGrid grid = GameController.Instance.mMapController.GetGrid(currentColumnIndex, i);
            if(grid != null)
            {
                weight = GetDangerousWeight(grid);
            }
            else
            {
                // �������û����Ч���ӣ���ô��ΪĬ�ϸ������ͼ���
                weight = GridDangerousWeightDict[GridType.Default];
            }
            if(weight < min)
            {
                min = weight;
                rowIndexList.Clear();
                rowIndexList.Add(i);
            }
            else if(weight == min)
            {
                rowIndexList.Add(i);
            }
        }
        // ����һ�����ʱ��Ҫ�����Ƴ���·����Ŀ��ּ�ھ����ܲ�������ͣ���ڱ�·
        if (rowIndexList.Count > 1)
        {
            rowIndexList.Remove(currentRowIndex);
        }
        // Ȼ���ʣ�µ����������ȡһ�����˹�����Ϊ�����ƶ���
        int selectedIndex = Random.Range(0, rowIndexList.Count); // ����ע�⣬��������ʱ���������ֵ
        // ����λ��
        AddUniqueTask("DrivenAway", new StraightMovePresetTask(transform, MapManager.gridHeight / 60 * (currentRowIndex - rowIndexList[selectedIndex]), Vector3.up, 60));
        // AddTask(new StraightMovePresetTask(transform, MapManager.gridHeight / 60 * (currentRowIndex - rowIndexList[selectedIndex]), Vector3.up, 60));
        // GameController.Instance.AddTasker(new StraightMovePresetTasker(this, MapManager.gridHeight / 60 * (currentRowIndex - rowIndexList[selectedIndex]), Vector3.up, 60));
    }

    /// <summary>
    /// ��ȡ�õ�λ��ĳ���ϵĵ���Σ��Ȩ��
    /// </summary>
    /// <param name="g"></param>
    /// <returns></returns>
    public int GetDangerousWeight(BaseGrid g)
    {
        int maxDangerous = int.MinValue; // ���Σ��Ȩ��
        foreach (var t in g.GetAllGridType())
        {
            if (GridDangerousWeightDict.ContainsKey(t))
            {
                if (GridDangerousWeightDict[t] > maxDangerous)
                    maxDangerous = GridDangerousWeightDict[t];
            }
        }
        return maxDangerous;
    }

    /// <summary>
    /// ����è�ж�ʱ�Ƿ��ܴ���è
    /// </summary>
    /// <returns></returns>
    public virtual bool CanTriggerCat()
    {
        return true;
    }

    /// <summary>
    /// ��Խ��ʧ���ж��ߺ��Ƿ�ᴥ����Ϸʧ���ж�
    /// </summary>
    /// <returns></returns>
    public virtual bool CanTriggerLoseWhenEnterLoseLine()
    {
        return true;
    }

    /// <summary>
    /// �޸ĵ���Ȩֵ��
    /// </summary>
    public virtual void SetGridDangerousWeightDict()
    {

    }

    /// <summary>
    /// ����BOSS��λ��˵�����˾�����ͨ�˺�
    /// </summary>
    /// <param name="value"></param>
    public override void AddRecordDamage(float value)
    {
        if (!isIgnoreRecordDamage && IsBoss())
            OnDamage(value);
        mRecordDamageComponent.AddRecordDamage(value);
    }

    /// <summary>
    /// ���ùֵ����ԣ�����ȫˢ��Ŀ���ս���������ݣ�
    /// </summary>
    /// <param name="attr"></param>
    public void SetAttribute(MouseManager.MouseAttribute attr)
    {
        // Ѫ��
        NumericBox.Hp.SetBase(attr.hp);
        mCurrentHp = mMaxHp;
        // ������
        NumericBox.Attack.SetBase(attr.attack);
        // �����ٶ��빥�����
        NumericBox.AttackSpeed.SetBase(attr.attackSpeed);
        NumericBox.MoveSpeed.SetBase(TransManager.TranToVelocity(attr.moveSpeed));
        attackPercent = attr.attackPercent; // �����������Ž��ȵ�attackPercent����ʱ����������Ĺ���

        if (attr.burnDefence != 0)
            NumericBox.BurnRate.AddModifier(new FloatModifier(1-attr.burnDefence));
        if(attr.aoeDefence != 0)
            NumericBox.AoeRate.AddModifier(new FloatModifier(1 - attr.aoeDefence));
        mHertRateList.Clear();
        foreach (var item in attr.mHertRateList)
        {
            mHertRateList.Add(item);
        }
    }
}
