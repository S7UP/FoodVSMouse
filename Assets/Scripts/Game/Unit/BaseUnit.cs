using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : IGameControllerMember
{
    // ���ⲿ��ֵ������
    public GameObject mGameObject; // �ýű����������Ϸ����

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

    public BaseUnit(GameObject gameObject)
    {
        mGameObject = gameObject;
    }

    public virtual void Init()
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

    public virtual void Update()
    {
        // �������ݸ���
        if (mAttackCDLeft > 0)
        {
            mAttackCDLeft--;
        }
        // ��λ����״̬��״̬�����������ƶ���������������������
        mCurrentActionState.OnUpdate();
    }

    public virtual void Destory()
    {

    }

    public virtual void Pause()
    {

    }

    public virtual void Resume()
    {

    }
}
