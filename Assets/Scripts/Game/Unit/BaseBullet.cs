using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.UI.CanvasScaler;

public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // �����ⲿ����
    public BaseUnit mMasterBaseUnit; // �����������˵�λ

    // ����
    public Animator animator;


    // ��������
    public float mVelocity;
    public Vector2 mRotate;
    public float mDamage;

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬

    public virtual void Awake()
    {
        MInit();
        
        // ����ʱ�����ߵ�
        animator = transform.GetChild(0).GetComponent<Animator>();
        // animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/7/bullet");
    }

    // �ӵ���Ŀ������˺���TakeDamage�ĵ���ʱ���ǵжԵ�λ����������ӵ���Ȼ�������������ӵ����˺��߼�
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        baseUnit.OnDamage(mDamage);
        SetActionState(new BulletHitActionState(this));
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
        SetActionState(new BulletFlyActionState(this));
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
        // TODO ����ʱ��Ϊtrue��֮���������������Ϊfalse
        return true;
    }
}



public class BulletFlyActionState : IBaseActionState
{
    private BaseBullet mBaseBullet;

    public BulletFlyActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // ������ʱ
    public virtual void OnEnter()
    {
        Animator ani = mBaseBullet.transform.GetChild(0).gameObject.GetComponent<Animator>();
        ani.Play("Fly");
    }

    // ���˳�ʱ
    public virtual void OnExit()
    {

    }

    // ʵ�ֶ���״̬
    public virtual void OnUpdate()
    {
        mBaseBullet.transform.position += (Vector3)mBaseBullet.mRotate* mBaseBullet.mVelocity * 0.05f;
    }
}


public class BulletHitActionState : IBaseActionState
{
    private BaseBullet mBaseBullet;
    private bool isFirstFrame; // �Ƿ�Ϊ�л�ʱ�ĵ�һ֡ 

    public BulletHitActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // ������ʱ
    public virtual void OnEnter()
    {
        isFirstFrame = true;
        Animator ani = mBaseBullet.transform.GetChild(0).gameObject.GetComponent<Animator>();
        ani.Play("Hit");
    }

    // ���˳�ʱ
    public virtual void OnExit()
    {

    }

    // ʵ�ֶ���״̬
    public virtual void OnUpdate()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (isFirstFrame)
        {
            isFirstFrame = !isFirstFrame;
            return;
        }

        AnimatorStateInfo info = mBaseBullet.animator.GetCurrentAnimatorStateInfo(0);
        // Debug.Log(info.normalizedTime);
        if (info.normalizedTime >= 1.0f)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/Pre_Bullet", mBaseBullet.gameObject);
            mBaseBullet.SetActionState(new BulletDefaultActionState(mBaseBullet));
        }
    }
}

public class BulletDefaultActionState : IBaseActionState
{
    private BaseBullet mBaseBullet;

    public BulletDefaultActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // ������ʱ
    public virtual void OnEnter()
    {

    }

    // ���˳�ʱ
    public virtual void OnExit()
    {

    }

    // ʵ�ֶ���״̬
    public virtual void OnUpdate()
    {

    }
}