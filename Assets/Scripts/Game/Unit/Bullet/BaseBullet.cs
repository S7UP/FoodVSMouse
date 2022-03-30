using System.Collections;
using System.Collections.Generic;
using System.Drawing;

using UnityEngine;

using static UnityEngine.UI.CanvasScaler;

public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // �����ⲿ����
    public BaseUnit mMasterBaseUnit; // �����������˵�λ

    // ����
    public Animator animator;
    public CircleCollider2D mCircleCollider2D;

    // ��������
    public float mVelocity;
    public Vector2 mRotate;
    public float mHeight;
    public float mDamage;
    public bool isDeathState;

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬

    public virtual void Awake()
    {
        // ����ʱ�����ߵ�
        animator = transform.GetChild(0).GetComponent<Animator>();
        mCircleCollider2D = GetComponent<CircleCollider2D>();
        // animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/7/bullet");
    }

    // �ӵ���Ŀ������˺���TakeDamage�ĵ���ʱ���ǵжԵ�λ����������ӵ���Ȼ�������������ӵ����˺��߼�
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, mDamage).ApplyAction();
        SetActionState(new BulletHitState(this));
    }

    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
        mCurrentActionState.OnEnter();
    }

    /// <summary>
    /// �����ж�
    /// </summary>
    public void SetCollision(bool colli)
    {
        mCircleCollider2D.enabled = colli;
    }

    // ������GameController��
    public virtual void MDestory()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// ÿ�γ�ʼ����Ҫ������
    /// </summary>
    public virtual void MInit()
    {
        mVelocity = 1.0f;
        mRotate = Vector2.right;
        mDamage = 10;
        isDeathState = false;
        SetCollision(true);
        SetActionState(new BulletFlyState(this));
    }

    public void SetDamage(float dmg)
    {
        mDamage = dmg;
    }

    public virtual void MPause()
    {
        throw new System.NotImplementedException();
    }

    public virtual void MResume()
    {
        throw new System.NotImplementedException();
    }

    public virtual void MUpdate()
    {
        // ��λ����״̬��״̬�����������ƶ���������������������
        mCurrentActionState.OnUpdate();
        // �ӵ�������������ɾ
        if (!IsInView())
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/Pre_Bullet", gameObject);
        }
    }

    /// <summary>
    /// �ж��ӵ��ڲ�����Ļ��
    /// </summary>
    public bool IsInView()
    {
        Vector3 worldPos = transform.position;
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos); // ��������תΪ��Ļ���꣬Ȼ���ж��ǲ�����(0,0)-(1,1)֮��
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
            return false;
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// �ж��ӵ��ܷ���е�λ 
    /// </summary>
    /// <returns></returns>
    public virtual bool CanHit(BaseUnit unit)
    {
        return !isDeathState;
    }

    public virtual void OnFlyStateEnter()
    {
        animator.Play("Fly");
    }

    public virtual void OnFlyState()
    {
        transform.position += (Vector3)mRotate * mVelocity * 0.05f;
    }

    public virtual void OnFlyStateExit()
    {
        
    }

    public virtual void OnHitStateEnter()
    {
        animator.Play("Hit");
        SetCollision(false);
        isDeathState = true;
    }

    public virtual void OnHitState()
    {
        // ����������һ�κ�תΪ�ƶ�״̬
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        //if (info.normalizedTime >= 1.0f)
        if(AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        {
            SetActionState(new BaseBulletActionState(this));
        }
    }

    public virtual void OnHitStateExit()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/Pre_Bullet", gameObject);
    }
}



