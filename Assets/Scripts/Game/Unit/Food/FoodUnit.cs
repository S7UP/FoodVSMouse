using System.Collections.Generic;

using UnityEngine;

public class FoodUnit : BaseUnit
{
    // ��ʳ��λ������
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public float[] valueList; // ÿ����Ӧ��ֵ
        public FoodType foodType;
    }

    // Awake��ȡ�����
    protected Animator animator;
    protected Animator rankAnimator;
    protected FoodUnit.Attribute attr;
    private SpriteRenderer spriteRenderer1;
    private SpriteRenderer spriteRenderer2;
    public BoxCollider2D mBoxCollider2D;
    public Transform spriteTrans;

    // ����
    public FoodType mFoodType; // ��ʳְҵ����
    public BaseCardBuilder mBuilder; // ��������Ľ�����
    public int mLevel; //�Ǽ�
    public int typeAndShapeToLayer = 0; // ��������ֶ�ͼ��ļ�Ȩ�ȼ�

    private BaseGrid mGrid; // ��Ƭ���ڵĸ��ӣ�����)
    private List<BaseGrid> mGridList; // ��Ƭ���ڵĸ��ӣ���񿨣�
    public bool isUseSingleGrid; // �Ƿ�ֻռһ��


    public override void Awake()
    {
        base.Awake();
        spriteTrans = transform.Find("Ani_Food");
        animator = spriteTrans.GetComponent<Animator>();
        rankAnimator = transform.Find("Ani_Rank").gameObject.GetComponent<Animator>();
        spriteRenderer1 = spriteTrans.GetComponent<SpriteRenderer>();
        spriteRenderer2 = transform.Find("Ani_Rank").gameObject.GetComponent<SpriteRenderer>();
        mBoxCollider2D = transform.GetComponent<BoxCollider2D>();
    }

    // ��λ������ػ���ʱ����
    public override void OnDisable()
    {
        base.OnDisable();
        mGrid = null; // ��Ƭ���ڵĸ��ӣ�����)
        mGridList = null; // ��Ƭ���ڵĸ��ӣ���񿨣�
        isUseSingleGrid = false; // �Ƿ�ֻռһ��

    }

    // ÿ�ζ��󱻴���ʱҪ���ĳ�ʼ������
    public override void MInit()
    {
        base.MInit();
        // ������������animator
        animatorController.ChangeAnimator(animator);
        mBuilder = null;
        attr = GameController.Instance.GetFoodAttribute();
        mFoodType = attr.foodType;

        mGridList = new List<BaseGrid>();
        isUseSingleGrid = true;

        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/"+mType+"/"+mShape);
        SetActionState(new IdleState(this));

        SetLevel(0);
        // ��������
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        spriteRenderer2.enabled = true; // �����Ǽ�����
        // װ���������ܻ�����
        spriteRenderer1.material = GameManager.Instance.GetMaterial("Hit");
    }

    /// <summary>
    /// �����ж�����
    /// </summary>
    public override void SetCollider2DParam()
    {
        mBoxCollider2D.offset = new Vector2(0, 0);
        mBoxCollider2D.size = new Vector2(0.49f * MapManager.gridWidth, 0.49f * MapManager.gridHeight);
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Food;
    }

    public void SetLevel(int level)
    {
        mLevel = level;
        UpdateAttributeByLevel();
        if (mLevel > 3)
        {
            rankAnimator.gameObject.SetActive(true);
            rankAnimator.Play(mLevel.ToString()); // �Ȳ����Ǽ���ͼ�궯��
        }else
            rankAnimator.gameObject.SetActive(false);
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public virtual void UpdateAttributeByLevel()
    {

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
                GeneralAttackSkillAbility s = new GeneralAttackSkillAbility(this, item);
                skillAbilityManager.AddSkillAbility(s);
                s.FullCurrentEnergy(); // ��������Ҳ����ֱ�ӹ����ˣ����ǵȵ�һ�ֹ���CD
            }
        }
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
        mAttackFlag = true;
        SetActionState(new IdleState(this));
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
    /// ���ڶ��Ƭ��ʹ�����
    /// </summary>
    /// <returns></returns>
    public List<BaseGrid> GetGridList()
    {
        return mGridList;
    }

    /// <summary>
    /// ���뵥λ������ײʱ�ܷ��赲���ж�
    /// Ĭ��������������ͬ��ʱ�����赲����
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanBlock(BaseUnit unit)
    {
        if(unit is MouseUnit)
        {
            return GetRowIndex() == unit.GetRowIndex() && IsAlive();
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
        return IsAlive();
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
        animatorController.Play("Attack", false);
    }

    //public override void OnAttackStateExit()
    //{
    //    // ���������󲥷��ٶȸĻ���
    //    animator.speed = 1;
    //}

    public override void OnAttackStateContinue()
    {
        // ÿ�ι���ʱ����ø��ݹ���������һ�²����ٶȣ�Ȼ��ı䲥���ٶ�
        UpdateAttackAnimationSpeed();
        // animator.Play("Attack", -1, currentStateTimer/ConfigManager.fps);
    }

    public override void OnDieStateEnter()
    {
        // ������ʳ��˵û�����������Ļ���ֱ�ӻ��ն�����У�����Ϸ������־���ֱ����ʧ�����ն��������duringDeath��һ֡ȥ��

        // �ӵ�ǰ�������Ƴ�����
        RemoveFromGrid();
    }

    private BoolModifier boolModifier = new BoolModifier(true);
    public override void OnBurnStateEnter()
    {
        // �ӵ�ǰ�������Ƴ�����
        RemoveFromGrid();
        // װ���ջٲ���
        spriteRenderer1.material = GameManager.Instance.GetMaterial("Dissolve2");
        // �����Ǽ���Ч
        spriteRenderer2.enabled = false;
        // ��ֹ���Ŷ���
        PauseCurrentAnimatorState(boolModifier);
    }

    public override void DuringBurn(float _Threshold)
    {
        spriteRenderer1.material.SetFloat("_Threshold", _Threshold);
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
        // �ӵ�ǰ�������Ƴ�����
        RemoveFromGrid();
        // �����Ǽ���Ч
        spriteRenderer2.enabled = false;
        // ��ֹ���Ŷ���
        PauseCurrentAnimatorState(boolModifier);
    }

    /// <summary>
    /// ˤ����������
    /// </summary>
    public override void OnDropState(float r)
    {
        SetAlpha(1 - r);
        spriteRenderer1.transform.localPosition = spriteRenderer1.transform.localPosition + 0.25f*MapManager.gridHeight*r*Vector3.down;
        spriteRenderer1.transform.localScale = Vector3.one * (1-r);
        // ����1�Ϳ��Ի�����
        if (r >= 1.0)
        {
            ResumeCurrentAnimatorState(boolModifier);
            SetAlpha(1);
            spriteRenderer1.transform.localPosition = Vector3.zero;
            spriteRenderer1.transform.localScale = Vector3.one;
            DeathEvent();
        }
    }

    /// <summary>
    /// ���ݹ����ٶ������¹����������ٶ�
    /// </summary>
    protected virtual void UpdateAttackAnimationSpeed()
    {
        UpdateAnimationSpeedByAttackSpeed("Attack");
    }

    protected void UpdateAnimationSpeedByAttackSpeed(string attackClipName)
    {
        AnimatorStateRecorder a = animatorController.GetAnimatorStateRecorder(attackClipName);
        float time = a.aniTime; // һ������һ�ι��������Ĳ���ʱ�䣨֡��
        float interval = 1 / NumericBox.AttackSpeed.Value * 60;  // ���������֡��
        float speed = Mathf.Max(1, time / interval); // ���㶯��ʵ�ʲ����ٶ�
        animatorController.SetSpeed(attackClipName, speed);
    }

    public static void SaveNewFoodInfo()
    {
        FoodUnit.Attribute attr = new FoodUnit.Attribute()
        {
            baseAttrbute = new BaseUnit.Attribute()
            {
                name = "�ս��߾Ƽ�", // ��λ�ľ�������
                type = 7, // ��λ���ڵķ���
                shape = 2, // ��λ�ڵ�ǰ����ı��ֱ��

                baseHP = 50, // ����Ѫ��
                baseAttack = 0, // ��������
                baseAttackSpeed = 1.05, // ���������ٶ�
                attackPercent = 0.5,
                baseDefense = 0,
                baseMoveSpeed = 0,
                baseRange = 9,
                baseHeight = 0, // �����߶�
            },
            valueList = new float[] {10, 12, 14, 15, 18, 20, 22, 26, 32, 40, 55, 70, 85, 100, 115, 130, 145 },
            foodType = FoodType.Shooter
        };

        //Debug.Log("��ʼ�浵��ʳ��Ϣ��");
        JsonManager.Save(attr, "Food/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        //Debug.Log("��ʳ��Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// ������Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer1.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), typeAndShapeToLayer, 2*arrayIndex);
        spriteRenderer2.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), typeAndShapeToLayer, 2*arrayIndex+1);
        UpdateSpecialRenderLayer();
    }

    /// <summary>
    /// ������ʵ�֣�����������������Ĳ���
    /// </summary>
    public virtual void UpdateSpecialRenderLayer()
    {

    }


    public override void MUpdate()
    {
        base.MUpdate();
        // �����ܻ���˸״̬
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer1.material.SetFloat("_FlashRate", 0.5f * hitBox.GetPercent());
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
            spriteRenderer1.material.SetFloat("_IsSlow", 1);
        }
        else
        {
            spriteRenderer1.material.SetFloat("_IsSlow", 0);
        }
    }

    /// <summary>
    /// �������Ƴ�������
    /// </summary>
    public virtual void RemoveFromGrid()
    {
        mGrid.RemoveFoodUnit(this);
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
        spriteRenderer1.material.SetFloat("_Alpha", a);
    }

    /// <summary>
    /// ��ȡ���±�
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        // �����ڸ����򷵻ض�Ӧ���ӵ��±�
        if (GetGrid()!=null)
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
        rankAnimator.transform.localPosition = -0.15f*Vector2.up + vector2;
    }

    /// <summary>
    /// ִ�иõ�λ�����¼�
    /// </summary>
    public override void ExecuteRecycle()
    {
        // ���Ŀ�����佨���������߽��������������̣�������Ĭ�ϻ�������
        if (mBuilder != null)
            mBuilder.Destructe(this);
        else
            base.ExecuteRecycle();
    }

    /// <summary>
    /// ��ȡ�佨����
    /// </summary>
    public BaseCardBuilder GetCardBuilder()
    {
        return mBuilder;
    }

    /// <summary>
    /// �ɷ�ѡ��ΪĿ��
    /// </summary>
    /// <returns></returns>
    public override bool CanBeSelectedAsTarget(BaseUnit otherUnit)
    {
        return mBoxCollider2D.enabled && IsAlive() && base.CanBeSelectedAsTarget(otherUnit);
    }

    public override void MPause()
    {
        base.MPause();
        // ��ͣ�Ǽ�����
        rankAnimator.speed = 0;
    }

    public override void MResume()
    {
        base.MResume();
        // ȡ����ͣ��������
        rankAnimator.speed = 1;
    }

    /// <summary>
    /// ��ȡ��ͼ����
    /// </summary>
    public override Sprite GetSpirte()
    {
        return spriteRenderer1.sprite;
    }

    /// <summary>
    /// ��ȡSpriterRenderer
    /// </summary>
    /// <returns></returns>
    public override SpriteRenderer GetSpriteRenderer()
    {
        return spriteRenderer1;
    }

    public FoodInGridType GetFoodInGridType()
    {
        return BaseCardBuilder.GetFoodInGridType(mType);
    }
}