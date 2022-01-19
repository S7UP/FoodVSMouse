using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.GraphicsBuffer;

public class FoodUnit : BaseUnit
{
    public override void Awake()
    {
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        SetActionState(new FoodIdleState(this));
        gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>().runtimeAnimatorController = (RuntimeAnimatorController)Resources.Load("Animator/AnimatorController/Food/7/2");
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
        animator = mFoodUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
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
            // Debug.Log("��Ƭ�����ˣ�");
            for (int i = -1; i <= 1; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    GameController.Instance.CreateBullet(mFoodUnit.transform.position + Vector3.right*0.5f*j + Vector3.up*0.7f*i);
                }
            }
            
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
        animator = mFoodUnit.gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
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