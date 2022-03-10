using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class FoodUnit : BaseUnit
{
    // ��ʳ��λ������
    [System.Serializable]
    new public struct Attribute
    {
        public BaseUnit.Attribute baseAttrbute;
        public FoodType foodType;
    }

    // Awake��ȡ�����
    protected Animator animator;
    protected Animator rankAnimator;

    // ����
    public FoodType mFoodType; // ��ʳְҵ����

    private BaseGrid mGrid; // ��Ƭ���ڵĸ��ӣ�����)
    private List<BaseGrid> mGridList; // ��Ƭ���ڵĸ��ӣ���񿨣�
    public bool isUseSingleGrid; // �Ƿ�ֻռһ��


    public override void Awake()
    {
        base.Awake();
        mPreFabPath = "Food/Pre_Food";
        jsonPath = "Food/";
        animator = gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        rankAnimator = transform.Find("Ani_Rank").gameObject.GetComponent<Animator>();
    }

    // ��λ������ػ���ʱ����
    public override void OnDisable()
    {
        base.OnDisable();
        mFoodType = 0; // ��ʳְҵ����
        mGrid = null; // ��Ƭ���ڵĸ��ӣ�����)
        mGridList = null; // ��Ƭ���ڵĸ��ӣ���񿨣�
        isUseSingleGrid = false; // �Ƿ�ֻռһ��
    }



    // �ж��ܲ��ܿ����ˣ��޶���OnAttackState()����ã�����������д
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

    // ����������ʱ�������ӵ�������ݣ���������Ҫ������д�Դﵽ��̬�ԡ�
    protected virtual void Attack()
    {
        // Debug.Log("��Ƭ�����ˣ�");
        for (int i = -1; i <= 1; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                GameController.Instance.CreateBullet(transform.position + Vector3.right * 0.5f * j + Vector3.up * 0.7f * i);
            }
        }
    }

    // ÿ�ζ��󱻴���ʱҪ���ĳ�ʼ������
    public override void MInit()
    {
        base.MInit();

        FoodUnit.Attribute attr = GameController.Instance.GetFoodAttribute();
        mFoodType = attr.foodType;

        mGridList = new List<BaseGrid>();
        isUseSingleGrid = true;

        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/7/2");
        SetActionState(new IdleState(this));
        
        rankAnimator.Play("12"); // �Ȳ���12�Ǽ���ͼ�궯��
    }

    // �ڴ���״̬ʱÿ֡Ҫ������
    public override void OnIdleState()
    {
        // Ĭ��Ϊ����������Ϊ��ʱ���ܷ��𹥻�
        if (mAttackCDLeft <= 0)
        {
            SetActionState(new AttackState(this));
            mAttackCDLeft += mAttackCD;
        }
    }

    // �ڹ���״̬ʱÿ֡Ҫ������
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
            mAttackFlag = true;
            SetActionState(new IdleState(this));
        }
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
            Debug.Log("��⵽����λ��");
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
        animator.Play("Attack");
    }

    public override void OnDieStateEnter()
    {
        // ������ʳ��˵û�����������Ļ���ֱ�ӻ��ն�����У�����Ϸ������־���ֱ����ʧ
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, mPreFabPath, this.gameObject);
    }
}