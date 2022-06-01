using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using static System.Collections.Specialized.BitVector32;
using static UnityEngine.UI.CanvasScaler;

public class MouseUnit : BaseUnit
{
    // ����λ������
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        // public double baseMoveSpeed;
        public double[] hertRateList;
    }

    // Awake��Findһ�ε����
    public Rigidbody2D rigibody2D;
    private SpriteRenderer spriteRenderer;
    protected Animator animator;

    // ��������

    protected List<double> mHertRateList; // �л���ͼʱ�����˱��ʣ���->��)
    public int mHertIndex; // ������ͼ�׶�
    

    // �������
    protected bool isBlock { get; set; } // �Ƿ��赲
    protected BaseUnit mBlockUnit; // �赲��

    /// <summary>
    /// ����ÿ�α�Ͷ��ս��ʱҪ���ĳ�ʼ��������Ҫȷ�����������
    /// </summary>
    public override void MInit()
    {
        base.MInit();
        
        // ��Json�ж�ȡ�������Լ���صĳ�ʼ��
        MouseUnit.Attribute attr = GameController.Instance.GetMouseAttribute();
        mHertRateList = new List<double>();
        foreach (var item in attr.hertRateList)
        {
            mHertRateList.Add(item);
        }
        mHertIndex = 0;

        // ��ʼΪ�ƶ�״̬
        SetActionState(new MoveState(this));
        // ������ͼ
        UpdateRuntimeAnimatorController();

        renderManager.AddSpriteRenderer("mainSprite", spriteRenderer);
        // ��Ӽ����ж�����Ӧ�¼�
        // ���ܵ��˺�����֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveDamage, delegate { UpdateHertMap(); });
        AddActionPointListener(ActionPointType.PostReceiveDamage, FlashWhenHited);
        // �ڽ������ƽ���֮�󣬸���������ͼ״̬
        AddActionPointListener(ActionPointType.PostReceiveCure, delegate { UpdateHertMap(); });
        // װ���������ܻ�����
        spriteRenderer.material = GameManager.Instance.GetMaterial("Hit");
        AnimatorContinue(); // �ָ�����
    }

    public override void SetUnitType()
    {
        mUnitType = UnitType.Mouse;
    }

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
    }

    /// <summary>
    /// ��λ������ػ���ʱ��������Ҫ�����������������ٳ�ʼ����ȥ
    /// </summary>
    public override void OnDisable()
    {
        base.OnDisable();
        // ��������
        mHertRateList = null;

        // �������
        isBlock = false; // �Ƿ��赲
        mBlockUnit = null; // �赲��
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


    // ע��һ��Collider2D��Ӧ��ֱ��ʹ��Transform������offset�������ƶ���������Ӧ��ʹ��Rigidbody2D���ƶ�����֮��������õ���õı��ֺ���ȷ����ײ��⡣
    // ��ˣ����������ƶ���Ӧ���������transform,�����������rigibody2D
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
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
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
        {
            return;
        }
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
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f; // ����������������һ�κ���Ϊ���ܽ���
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        // ����п��Թ�����Ŀ�꣬��ͣ�����ȴ���һ�ι���������ǰ��
        if (IsHasTarget())
            SetActionState(new IdleState(this));
        else
            SetActionState(new MoveState(this));
        UpdateBlockState(); // �����赲״̬
    }

    /// <summary>
    /// �ж�������Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected virtual bool IsHasTarget()
    {
        return (isBlock && mBlockUnit.IsAlive());
    }

    /// <summary>
    /// ��ȡ��ǰ����Ŀ�꣬�����ж�һ��ʹ��
    /// </summary>
    /// <returns></returns>
    protected virtual BaseUnit GetCurrentTarget()
    {
        return mBlockUnit;
    }

    /// <summary>
    /// �����赲��״̬
    /// </summary>
    protected virtual void UpdateBlockState()
    {
        if (mBlockUnit.IsAlive())
            isBlock = true;
        else
            isBlock = false;
    }

    /// <summary>
    /// ����Ⱥ���͵���һ�ֿ���ѡ����Ŀ��Ľӿ�
    /// </summary>
    /// <returns></returns>
    protected virtual List<BaseUnit> GetCurrentTargetList()
    {
        return null;
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public virtual bool IsDamageJudgment()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        return (info.normalizedTime - Mathf.FloorToInt(info.normalizedTime) >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public virtual void ExecuteDamage()
    {
        if (IsHasTarget())
            TakeDamage(GetCurrentTarget());
    }

    // ����Ϊ IBaseStateImplementor �ӿڵķ���ʵ��
    public override void OnIdleState()
    {

    }

    public override void OnMoveState()
    {
        // �ƶ�����
        SetPosition((Vector2)GetPosition() + Vector2.left * mCurrentMoveSpeed * 0.003f);
    }

    public override void OnAttackState()
    {

    }

    /// <summary>
    /// ����λĬ�϶��Ǵ���ͬ����ͬ�߶�ʱ����ʳ�赲
    /// ��ʾ�������������������д���������ǿ����Ϊ�����赲����������
    /// </summary>
    public override bool CanBlock(BaseUnit unit)
    {
        return (unit is FoodUnit && GetRowIndex() == unit.GetRowIndex() && mHeight == unit.mHeight);
    }
    
    /// <summary>
    /// ����λĬ�ϱ�����ͬ�е��ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return (GetRowIndex() == bullet.GetRowIndex() && mHeight == bullet.mHeight);
    }

    /// <summary>
    /// �����˻��߱�����ʱ�����µ�λ��ͼ״̬
    /// </summary>
    protected void UpdateHertMap()
    {
        // Ҫ�����˵Ļ������˰�
        if (isDeathState)
            return;

        // �Ƿ�Ҫ�л���������flag
        bool flag = false;
        // �ָ�����һ��������ͼ���
        while(mHertIndex > 0 && GetHeathPercent() > mHertRateList[mHertIndex - 1])
        {
            mHertIndex --;
            flag = true;
        }
        // ��һ��������ͼ�ļ��
        while(mHertIndex < mHertRateList.Count && GetHeathPercent() <= mHertRateList[mHertIndex])
        {
            mHertIndex ++;
            flag = true;
        }
        // ���л�֪ͨʱ���л�
        if(flag)
            UpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// �Զ�������ͼ
    /// </summary>
    /// <param name="collision"></param>
    public void UpdateRuntimeAnimatorController()
    {
        string name = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
        float time = AnimatorManager.GetNormalizedTime(animator);
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + mType + "/" + mShape + "/" + mHertIndex);
        // ���ֵ�ǰ��������
        animator.Play(name, -1, time);
        OnUpdateRuntimeAnimatorController();
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
            //SpriteRendererManager srm = renderManager.GetSpriteRender("mainSprite");
            //srm.spriteRenderer.material.SetInt("BeAttack",1);
            //spriteRenderer.material.SetFloat("_FlashRate", 1);
            hitBox.OnHit();
        }
    }

    /// <summary>
    /// ����ͼ����ʱҪ�����£�������override
    /// </summary>
    public virtual void OnUpdateRuntimeAnimatorController()
    {
        // demo...
        //switch (mHertIndex)
        //{
        //    case 0:
        //        break;
        //    default:
        //        break;
        //}
    }

    /// <summary>
    /// ��ȷ����ײ����ʳ��λ
    /// </summary>
    public virtual void OnCollideFoodUnit(FoodUnit unit)
    {
        isBlock = true;
        mBlockUnit = unit;
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

        if (collision.tag.Equals("Food"))
        {
            // ��⵽��ʳ��λ��ײ�ˣ�
            FoodUnit food = collision.GetComponent<FoodUnit>();
            if (UnitManager.CanBlock(this, food)) // ���˫���ܷ����赲
            {
                OnCollideFoodUnit(food);
            }
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // ��⵽�ӵ���λ��ײ��
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if (UnitManager.CanBulletHit(this, bullet)) // ���˫���ܷ������
            {
                bullet.TakeDamage(this);
            }
        }
    }

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // ������ͼ������״̬
        // UpdateHertMap();
        // �����ܻ���˸״̬
        if (hitBox.GetPercent() > 0)
        {
            spriteRenderer.material.SetFloat("_FlashRate", 0.5f * hitBox.GetPercent());
        }
    }

    /// <summary>
    /// ״̬���
    /// </summary>
    public override void OnIdleStateEnter()
    {
        animator.Play("Idle");
    }

    public override void OnMoveStateEnter()
    {
        animator.Play("Move");
    }

    public override void OnAttackStateEnter()
    {
        animator.Play("Attack");
    }

    public override void OnDieStateEnter()
    {
        animator.Play("Die");
    }

    public override void DuringDeath()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
        {
            return;
        }
        // ��ȡDie�Ķ�����Ϣ��ʹ�û���ʱ���붯����ʾ������ͬ��
        int currentFrame = AnimatorManager.GetCurrentFrame(animator);
        int totalFrame = AnimatorManager.GetTotalFrame(animator);
        if (currentFrame>totalFrame && currentFrame%totalFrame == 1) // ����������Ϻ����DeathEvent()
        {
            DeathEvent();
        }
    }

    public override void OnBurnStateEnter()
    {
        // װ���ջٲ���
        spriteRenderer.material = GameManager.Instance.GetMaterial("Dissolve2");
        // ��ֹ���Ŷ���
        AnimatorStop();
    }

    public override void DuringBurn(float _Threshold)
    {
        spriteRenderer.material.SetFloat("_Threshold", _Threshold);
        // ����1�Ϳ��Ի�����
        if (_Threshold >= 1.0)
        {
            DeathEvent();
        }
    }

    /// <summary>
    /// �Ƿ���
    /// </summary>
    /// <returns></returns>
    public override bool IsAlive()
    {
        return (!isDeathState && base.IsAlive());
    }


    public static void SaveNewMouseInfo()
    {
        MouseUnit.Attribute attr = new MouseUnit.Attribute()
        {
            baseAttrbute = new BaseUnit.Attribute()
            {
                name = "��ʬħ����", // ��λ�ľ�������
                type = 5, // ��λ���ڵķ���
                shape = 3, // ��λ�ڵ�ǰ����ı��ֱ��

                baseHP = 1700, // ����Ѫ��
                baseAttack = 10, // ��������
                baseAttackSpeed = 1.0, // ���������ٶ�
                attackPercent = 0.6,
                baseDefense = 0,
                baseMoveSpeed = 1.0,
                baseHeight = 0, // �����߶�
            },
            hertRateList = new double[] {  }
        };

        Debug.Log("��ʼ�浵������Ϣ��");
        JsonManager.Save(attr, "Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        Debug.Log("������Ϣ�浵��ɣ�");
    }

    /// <summary>
    /// ������ͬ������˵���Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public override void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, GetRowIndex(), 0, arrayIndex);
    }

    public override void AnimatorStop()
    {
        animator.speed = 0;
    }

    public override void AnimatorContinue()
    {
        animator.speed = 1;
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
}
