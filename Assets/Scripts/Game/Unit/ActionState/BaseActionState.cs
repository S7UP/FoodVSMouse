using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseActionState
{
    // �״ν������״̬
    public void OnEnter();
    // �˳����״̬
    public void OnExit();
    // ���״̬��֡����
    public void OnUpdate();
    // ���״̬���ж�ʱ���¼������ڻָ��Ŀ����ԣ���ͣ��
    public void OnInterrupt();
    // ���״̬���жϵ��ָ�ʱ���¼�
    public void OnContinue();
}

public class BaseActionState: IBaseActionState
{
    // ���е�Unit����
    protected BaseUnit mBaseUnit;

    public BaseActionState(BaseUnit baseUnit)
    {
        mBaseUnit = baseUnit;
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

    public virtual void OnInterrupt()
    {
        
    }

    public virtual void OnContinue()
    {
        
    }
}