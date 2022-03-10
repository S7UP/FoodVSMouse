using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

using static UnityEngine.UI.CanvasScaler;

public class MouseUnit : BaseUnit
{
    // ����λ������
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public double baseMoveSpeed;
        public double[] hertRateList; 
    }

    // Awake��Findһ�ε����
    public Rigidbody2D rigibody2D;
    protected Animator animator;

    // ��������
    public float mBaseMoveSpeed; // �����ƶ��ٶ�
    public float mCurrentMoveSpeed; // ��ǰ�ƶ��ٶ�
    protected double[] mHertRateList; // �л���ͼʱ�����˱��ʣ���->��)
    public int mHertIndex; // ������ͼ�׶�

    // �������
    protected bool isBlock; // �Ƿ��赲
    protected BaseUnit mBlockUnit; // �赲��

    /// <summary>
    /// ����ÿ�α�Ͷ��ս��ʱҪ���ĳ�ʼ��������Ҫȷ�����������
    /// </summary>
    public override void MInit()
    {
        base.MInit();
        // ��Json�ж�ȡ�������Լ���صĳ�ʼ��
        MouseUnit.Attribute attr = GameController.Instance.GetMouseAttribute();

        mBaseMoveSpeed = (float)attr.baseMoveSpeed; // �ƶ��ٶ�ֵΪ��/�룬Ĭ����Ϊ0.5��/��
        mCurrentMoveSpeed = mBaseMoveSpeed;
        mHertRateList = (double[])attr.hertRateList;
        mHertIndex = 0;

        // �ƶ�״̬test
        SetActionState(new MoveState(this));
        // ������ͼ
        UpdateRuntimeAnimatorController();
    }

    /// <summary>
    /// ֻ�ж��󱻴���ʱ��һ�Σ���Ҫ��������ȡ�����������
    /// </summary>
    public override void Awake()
    {
        base.Awake();
        jsonPath += "Mouse/";
        mPreFabPath = "Mouse/Pre_Mouse";
        // �����ȡ
        rigibody2D = gameObject.GetComponent<Rigidbody2D>();
        animator = gameObject.transform.Find("Ani_Mouse").gameObject.GetComponent<Animator>();
    }

    /// <summary>
    /// ��λ������ػ���ʱ��������Ҫ�����������������ٳ�ʼ����ȥ
    /// </summary>
    public override void OnDisable()
    {
        base.OnDisable();
        // ��������
        mBaseMoveSpeed = 0; // �����ƶ��ٶ�
        mCurrentMoveSpeed = 0; // ��ǰ�ƶ��ٶ�
        mHertRateList = null;

        // �������
        isBlock = false; // �Ƿ��赲
        mBlockUnit = null; // �赲��
    }



    /// <summary>
    /// ���ٴ󲿷�����Ӧ����һ����ֱ�ӹ������ֶ�
    /// </summary>
    /// <param name="unit"></param>
    public void TakeDamage(BaseUnit unit)
    {
        unit.OnDamage(mCurrentAttack);
    }


    // ע��һ��Collider2D��Ӧ��ֱ��ʹ��Transform������offset�������ƶ���������Ӧ��ʹ��Rigidbody2D���ƶ�����֮��������õ���õı��ֺ���ȷ����ײ��⡣
    // ��ˣ����������ƶ���Ӧ���������transform,�����������rigibody2D
    public override void SetPosition(Vector3 V3)
    {
        rigibody2D.MovePosition(V3);
    }

    // �ж��Ƿ��ֹ���Ŀ��
    protected virtual void SearchTarget()
    {
        // ���赲��
        if (isBlock)
        {
            // �赲�����Ƿ���Ч
            if (mBlockUnit.IsValid())
            {
                SetActionState(new AttackState(this));
            }
            else
            {
                isBlock = false;
                SetActionState(new MoveState(this));
            }
        }
    } 

    // �ж��ܲ��ܹ����ˣ��޶���OnAttackState()����ã�����������д
    protected virtual bool CanAttack()
    {
        // ��ȡAttack�Ķ�����Ϣ��ʹ���˺��ж��붯����ʾ������ͬ��
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        // ���normalizedTime��С�����Խ��Ʊ�ʾһ���������Ž��ȵİٷֱȣ���λ������Ա�ʾ��ѭ���Ĵ�����
        int c = Mathf.FloorToInt(info.normalizedTime);

        // ���������ڵ�һ��ʱ�����˺��ж�
        float percent = info.normalizedTime - c;
        if (percent >= attackPercent && mAttackFlag)
        {
            return true;
        }
        return false;
    }

    // ����������ʱ�����ľ��������
    protected virtual void Attack()
    {
        if (mBlockUnit != null && mBlockUnit.IsValid())
        {
            TakeDamage(mBlockUnit);
        }
    }


    // ����Ϊ IBaseStateImplementor �ӿڵķ���ʵ��
    public override void OnIdleState()
    {
        // ��������
        mAttackFlag = true;
        // �����߼�
        SearchTarget();
    }

    public override void OnMoveState()
    {
        // ��������
        mAttackFlag = true;
        // �ƶ�����
        SetPosition((Vector2)GetPosition() + Vector2.left * mCurrentMoveSpeed * 0.005f);
        // �����߼�
        SearchTarget();
    }

    public override void OnAttackState()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (currentStateTimer <= 0)
        {
            return;
        }
        // ��ȡAttack�Ķ�����Ϣ��ʹ���˺��ж��붯����ʾ������ͬ��
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        if (CanAttack())
        {
            Attack();
            mAttackFlag = false;
        }
        else if (info.normalizedTime >= 1.0f) // ��������������һ�κ�תΪ����״̬
        {
            SetActionState(new IdleState(this));
        }
    }

    /// <summary>
    /// ����λĬ�϶��Ǵ���ͬ�б���ʳ�赲
    /// ��ʾ�������������������д���������ǿ����Ϊ�����赲����������
    /// </summary>
    public override bool CanBlock(BaseUnit unit)
    {
        if(unit is FoodUnit)
        {
            Debug.Log("��⵽��ʳ��λ");
            return GetRowIndex() == unit.GetRowIndex();
        }
        return false;
    }
    
    /// <summary>
    /// ����λĬ�ϱ�����ͬ�е��ӵ�����
    /// </summary>
    /// <param name="bullet"></param>
    /// <returns></returns>
    public override bool CanHit(BaseBullet bullet)
    {
        return GetRowIndex() == bullet.GetRowIndex();
    }

    /// <summary>
    /// ÿ֡���Ѫ�������µ�λ��ͼ״̬
    /// </summary>
    protected virtual void UpdateHertMap()
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
        while(mHertIndex < mHertRateList.Length && GetHeathPercent() <= mHertRateList[mHertIndex])
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
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + mType + "/" + mShape + "/" + mHertIndex);
    }

    // rigibody���
    private void OnTriggerEnter2D(Collider2D collision)
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
                isBlock = true;
                mBlockUnit = food;
            }
        }
        else if (collision.tag.Equals("Bullet"))
        {
            // ��⵽�ӵ���λ��ײ��
            BaseBullet bullet = collision.GetComponent<BaseBullet>();
            if(UnitManager.CanBulletHit(this, bullet)) // ���˫���ܷ������
            {
                bullet.TakeDamage(this);
            }
        }
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // ������ͼ������״̬
        UpdateHertMap();
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
        if (currentFrame>totalFrame && currentFrame%totalFrame == 1) // ����������Ϻ����AfterDeath()
        {
            AfterDeath();
        }
    }


    public static void SaveNewMouseInfo()
    {
        MouseUnit.Attribute attr = new MouseUnit.Attribute()
        {
            baseAttrbute = new BaseUnit.Attribute()
            {
                name = "�ƹ�ƽ����", // ��λ�ľ�������
                type = 0, // ��λ���ڵķ���
                shape = 3, // ��λ�ڵ�ǰ����ı��ֱ��

                baseHP = 170, // ����Ѫ��
                baseAttack = 10, // ��������
                baseAttackSpeed = 1.0, // ���������ٶ�
                attackPercent = 0.6,
                baseHeight = 0, // �����߶�
            },
            baseMoveSpeed = 1.0,
            hertRateList = new double[] { 0.5 }
        };

        Debug.Log("��ʼ�浵������Ϣ��");
        JsonManager.Save(attr, "Mouse/" + attr.baseAttrbute.type + "/" + attr.baseAttrbute.shape + "");
        Debug.Log("������Ϣ�浵��ɣ�");
    }
}
