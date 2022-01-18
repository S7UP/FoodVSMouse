using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class FoodUnit : BaseUnit
{
    public FoodUnit(GameObject gameObject):base(gameObject)
    {

    }

    public override void Init()
    {
        base.Init();
        SetActionState(new FoodIdleState(this));
        mGameObject.transform.GetChild(0).gameObject.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animator/AnimatorController/Food/0/2");
    }
}

/// <summary>
/// ����ʱ��ʽ�ð����ߵ�״̬����д����
/// </summary>
public class FoodAttackState : BaseActionState
{
    private FoodUnit mFoodUnit;
    private Animator animator;
    private bool canDamage;  // �Ƿ��ܳ��˺��ж��ı�־
    private bool isFirstFrame; // �Ƿ�Ϊ�л�ʱ�ĵ�һ֡ 

    public FoodAttackState(FoodUnit foodUnit)
    {
        mFoodUnit = foodUnit;
        animator = mFoodUnit.mGameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        canDamage = true;
    }

    public override void OnEnter()
    {
        animator.Play("Attack");
    }

    public override void OnUpdate()
    {
        // �л�ʱ�ĵ�һֱ֡�Ӳ�ִ��update()����Ϊ������info.normalizedTime��ֵ��ͣ������һ��״̬���߼�������⣡
        if (isFirstFrame)
        {
            isFirstFrame = !isFirstFrame;
            return;
        }
        // ��ȡAttack�Ķ�����Ϣ��ʹ���˺��ж��붯����ʾ������ͬ��
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        // ���normalizedTime��С�����Խ��Ʊ�ʾһ���������Ž��ȵİٷֱȣ���λ������Ա�ʾ��ѭ���Ĵ�����
        // Debug.Log(info.normalizedTime);
        int c = Mathf.FloorToInt(info.normalizedTime);
        // Debug.Log("��λ��Ϊ��"+c);

        // ���������ڵ�һ��ʱ�����˺��ж�
        float percent = info.normalizedTime - c;
        if (percent >= 0.65 && canDamage)
        {
            Debug.Log("��Ƭ�����ˣ�");
            canDamage = false;
        }else if (info.normalizedTime >= 1.0f)
        {
            mFoodUnit.SetActionState(new FoodIdleState(mFoodUnit));
        }
    }
}


public class FoodIdleState : BaseActionState
{
    private FoodUnit mFoodUnit;
    private Animator animator;

    public FoodIdleState(FoodUnit foodUnit)
    {
        mFoodUnit = foodUnit;
        animator = mFoodUnit.mGameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
    }

    public override void OnEnter()
    {
        animator.Play("Idle");
    }

    public override void OnUpdate()
    {
        // ������ڷ��ֵ��� �� ����������Ϊ��ʱ���ܷ��𹥻�
        if (true && mFoodUnit.mAttackCDLeft <= 0)
        {
            mFoodUnit.SetActionState(new FoodAttackState(mFoodUnit));
            mFoodUnit.mAttackCDLeft += mFoodUnit.mAttackCD;
        }
    }
}