using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ս�����������������Ϸ��λ
/// </summary>
public class BaseUnit : MonoBehaviour, IGameControllerMember, IBaseStateImplementor
{
    // 
    public bool isGameObjectValid; // �Ƿ���
    

    // ����ı���
    public float mBaseHp; //+ ��������ֵ
    public float mMaxHp; //+ �������ֵ
    public float mCurrentHp; //+ ��ǰ����ֵ
    public float mBaseAttack; //+ ����������
    public float mCurrentAttack; //+ ��ǰ������
    public float mBaseAttackSpeed; //+ ���������ٶ�
    public float mCurrentAttackSpeed; //+ ��ǰ�����ٶ�
    public int mAttackCD; //+ ��ǰ�������
    public int mAttackCDLeft; //+ �������������
    public int mHeight; //+ �߶�
    public int mRow; // ��ǰ��������
    public int mColumn; // ��ǰ��������

    public string mName; // ��ǰ��λ����������
    public int mType; // ��ǰ��λ�����ࣨ�粻ͬ�Ŀ�����ͬ������
    public int mShape; // ��ǰ���൥λ����ۣ�ͬһ�ſ���0��1��2ת�������0��1��2ת�����������֣�

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬
    public int currentStateTimer = 0; // ��ǰ״̬�ĳ���ʱ�䣨�л�״̬ʱ�����ã�

    public virtual void Awake()
    {
        isGameObjectValid = true;
        currentStateTimer = -1;
    }

    // �л�����״̬
    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
        currentStateTimer = -1; // ���ü�����������Ϊ-1�Ǳ�֤��һ��ִ�е�״̬Updateʱ����ȡ���ļ�����ֵһ��Ϊ0����ϸʵ�ּ�����MUpdate()����
        mCurrentActionState.OnEnter();
    }

    // �ܵ��˺�
    public virtual void OnDamage(float dmg)
    {
        mCurrentHp -= dmg;
        if(mCurrentHp <= 0)
        {
            BeforeDeath();
        }
    }

    public virtual Vector3 GetPosition()
    {
        return gameObject.transform.position;
    }

    // ����λ��
    public virtual void SetPosition(Vector3 V3)
    {
        gameObject.transform.position = V3;
    }

    // �����������������������ȵģ�
    public virtual void BeforeDeath()
    {
        Debug.Log("BeforeDeath()");
        // �����ض����ؾ�һ�°�
        // ....
        DuringDeath();
    }

    // ������������()
    public virtual void DuringDeath()
    {
        // ��֪��Ҫ��ɶ�ˣ���������ط��϶��Ȳ�����
        Debug.Log("DuringDeath()");
        GameManager.Instance.RecycleUnit(this, "Food/Pre_Food");
        AfterDeath();
    }

    // ����Ÿ�ʲô����û�˾Ȳ�����
    public virtual void AfterDeath()
    {
        Debug.Log("AfterDeath()");
        // ������ҲҪ����Ϊ�����
    }


    // ����Ϊ IGameControllerMember �ӿڷ�����ʵ��
    public virtual void MInit()
    {
        // Ѫ��
        mBaseHp = 100;
        mMaxHp = mBaseHp;
        mCurrentHp = mMaxHp;
        // ������
        mBaseAttack = 10;
        mCurrentAttack = mBaseAttack;
        // �����ٶ��빥�����
        mBaseAttackSpeed = 0.5f;
        mCurrentAttackSpeed = mBaseAttackSpeed;
        mAttackCD = Mathf.FloorToInt(ConfigManager.fps / (mCurrentAttackSpeed));
        mAttackCDLeft = 0;
        // �߶�
        mHeight = 0;

        // ��ʼ����ǰ����״̬
        SetActionState(new BaseActionState(this));
    }

    public virtual void MUpdate()
    {
        // �������ݸ���
        if (mAttackCDLeft > 0)
        {
            mAttackCDLeft--;
        }
        // ��λ����״̬��״̬�����������ƶ���������������������
        currentStateTimer += 1; 
        mCurrentActionState.OnUpdate();
    }

    public virtual void MDestory()
    {

    }

    public virtual void MPause()
    {

    }

    public virtual void MResume()
    {

    }

    // ����Ϊ IBaseStateImplementor �ӿڵķ���ʵ��
    public virtual void OnIdleState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnMoveState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnAttackState()
    {
        throw new System.NotImplementedException();
    }

    public virtual void OnStaticState()
    {
        throw new System.NotImplementedException();
    }

    // ����
}
