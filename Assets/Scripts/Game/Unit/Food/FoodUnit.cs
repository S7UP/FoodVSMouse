using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.GraphicsBuffer;

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
    private Material defaultMaterial;

    // ����
    public FoodType mFoodType; // ��ʳְҵ����
    public int mLevel; //�Ǽ�

    private BaseGrid mGrid; // ��Ƭ���ڵĸ��ӣ�����)
    private List<BaseGrid> mGridList; // ��Ƭ���ڵĸ��ӣ���񿨣�
    public bool isUseSingleGrid; // �Ƿ�ֻռһ��


    public override void Awake()
    {
        base.Awake();
        animator = transform.Find("Ani_Food").gameObject.GetComponent<Animator>();
        rankAnimator = transform.Find("Ani_Rank").gameObject.GetComponent<Animator>();
        spriteRenderer1 = transform.Find("Ani_Food").gameObject.GetComponent<SpriteRenderer>();
        spriteRenderer2 = transform.Find("Ani_Rank").gameObject.GetComponent<SpriteRenderer>();
        defaultMaterial = spriteRenderer1.material;
    }

    // ��λ������ػ���ʱ����
    public override void OnDisable()
    {
        base.OnDisable();
        mFoodType = 0; // ��ʳְҵ����
        mGrid = null; // ��Ƭ���ڵĸ��ӣ�����)
        mGridList = null; // ��Ƭ���ڵĸ��ӣ���񿨣�
        isUseSingleGrid = false; // �Ƿ�ֻռһ��
        spriteRenderer1.material = defaultMaterial; // ������
    }

    // ÿ�ζ��󱻴���ʱҪ���ĳ�ʼ������
    public override void MInit()
    {
        base.MInit();
        
        attr = GameController.Instance.GetFoodAttribute();
        mFoodType = attr.foodType;

        mGridList = new List<BaseGrid>();
        isUseSingleGrid = true;

        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/"+mType+"/"+mShape);
        SetActionState(new IdleState(this));

        SetLevel(6);

        spriteRenderer2.enabled = true; // �����Ǽ�����
        AnimatorContinue(); // �ָ����Ŷ���
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Food;
    }

    public void SetLevel(int level)
    {
        mLevel = level;
        UpdateAttributeByLevel();
        rankAnimator.Play(mLevel.ToString()); // �Ȳ����Ǽ���ͼ�궯��
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
                skillAbilityManager.AddSkillAbility(new GeneralAttackSkillAbility(this, item));
            }
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
    public BaseGrid GetGrid()
    {
        return mGrid;
    }

    public void SetGrid(BaseGrid grid)
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
        // animator.Play("Attack", -1, currentStateTimer/ConfigManager.fps);
    }

    public override void OnDieStateEnter()
    {
        // ������ʳ��˵û�����������Ļ���ֱ�ӻ��ն�����У�����Ϸ������־���ֱ����ʧ�����ն��������duringDeath��һ֡ȥ��
    }

    public override void OnBurnStateEnter()
    {
        // װ���ջٲ���
        spriteRenderer1.material = GameManager.Instance.GetMaterial("Dissolve2");
        // �����Ǽ���Ч
        spriteRenderer2.enabled = false;
        // ��ֹ���Ŷ���
        AnimatorStop();
    }

    public override void DuringBurn(float _Threshold)
    {
        spriteRenderer1.material.SetFloat("_Threshold", _Threshold);
        // ����1�Ϳ��Ի�����
        if (_Threshold >= 1.0)
        {
            DeathEvent();
        }
    }

    /// <summary>
    /// ���ݹ����ٶ������¹����������ٶ�
    /// </summary>
    private void UpdateAttackAnimationSpeed()
    {
        float time = AnimatorManager.GetClipTime(animator, "Attack"); // 1������£�һ�ι�����Ĭ��ʱ�� ��
        float interval = 1/NumericBox.AttackSpeed.Value; // �������  ��
        float rate = Mathf.Max(1, time / interval);
        AnimatorManager.SetClipSpeed(animator, "Attack", rate);
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

        Debug.Log("��ʼ�浵��ʳ��Ϣ��");
        JsonManager.Save(attr, "Food/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        Debug.Log("��ʳ��Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// ������Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer1.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), 0, 2*arrayIndex);
        spriteRenderer2.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Ally, GetRowIndex(), 0, 2*arrayIndex+1);
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
        mGrid.RemoveFoodUnit(BaseCardBuilder.GetFoodInGridType(mType));
    }

}