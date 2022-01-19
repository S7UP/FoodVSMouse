using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour, IGameControllerMember
{
    // ���ⲿ��ֵ������
    public bool isGameObjectValid;

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

    public virtual void Awake()
    {
        isGameObjectValid = true;
    }

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
        SetActionState(new BaseActionState());
    }

    // �л�����״̬
    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
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

    public virtual void MUpdate()
    {
        // �������ݸ���
        if (mAttackCDLeft > 0)
        {
            mAttackCDLeft--;
        }
        // ��λ����״̬��״̬�����������ƶ���������������������
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
}
