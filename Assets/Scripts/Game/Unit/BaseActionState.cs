using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseActionState
{
    public void OnEnter();
    public void OnExit();
    public void OnUpdate();
}

public class BaseActionState: IBaseActionState
{
    // ���е�Unit����

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

/// <summary>
/// ����λ�ƶ�״̬
/// </summary>
public class MouseMoveState : BaseActionState
{
    private MouseUnit mMouseUnit;
    private Rigidbody2D rigidbody2D;

    public MouseMoveState(MouseUnit mouseUnit)
    {
        mMouseUnit = mouseUnit;
        rigidbody2D = mouseUnit.mGameObject.GetComponent<Rigidbody2D>();
    }

    public override void OnEnter()
    {
        // ��ȡsprite��obj�����޸Ķ�������
        GameObject go = mMouseUnit.mGameObject.transform.GetChild(0).gameObject;
        Animator ani = go.GetComponent<Animator>();
        ani.Play("Move");
    }

    public override void OnUpdate()
    {
        // �ƶ�����
        // mMouseUnit.mGameObject.transform.localPosition += new Vector3(-mMouseUnit.mCurrentMoveSpeed * 0.005f, 0, 0);
        // ע��һ��Collider2D��Ӧ��ֱ��ʹ��Transform������offset�������ƶ���������Ӧ��ʹ��Rigidbody2D���ƶ�����֮��������õ���õı��ֺ���ȷ����ײ��⡣
        // ��ˣ����������ƶ���Ӧ���������transform,�����������rigibody2D
        rigidbody2D.MovePosition(rigidbody2D.position + Vector2.left * mMouseUnit.mCurrentMoveSpeed * 0.005f);
        // Debug.Log(rigidbody2D.position);
    }
}

/// <summary>
/// ����λ����״̬
/// </summary>
public class MouseAttackState : BaseActionState
{
    // Target���ⲿ����������ʱ�����������������Ϊ�գ�
    public GameObject mTarget;

    private MouseUnit mMouseUnit; // ���д�״̬��MouseUnit����
    private Animator animator;
    private bool canDamage;  // �Ƿ��ܳ��˺��ж��ı�־
    private bool isFirstFrame; // �Ƿ�Ϊ�л�ʱ�ĵ�һ֡ 
    private int attackCount; // �����״̬��ʼ����Attack��������˼���

    public MouseAttackState(MouseUnit mouseUnit)
    {
        mMouseUnit = mouseUnit;
        animator = mMouseUnit.mGameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        attackCount = 0;
        isFirstFrame = true;
        canDamage = true;
    }

    public override void OnEnter()
    {
        // �޸Ķ�������
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
            // TakeDamage()
            if (mTarget != null)
            {
                GameController.Destroy(mTarget);
            }
            Debug.Log("����˺��ж��ˣ���");
            canDamage = false;
        }

        // ��������±�����ʼ�µ�һ�ֹ���ѭ���ˣ��˺��ж�Ӧ���ſ�
        if (attackCount < c)
        {
            Debug.Log("��һ�ֹ�������");
            attackCount++;
            canDamage = true;
            // ���һ�»���û�б��赲
            // IsBlocked()
            if (mTarget == null)
            {
                mMouseUnit.SetActionState(new MouseMoveState(mMouseUnit));
                Debug.Log("�л�Ϊ��·ģʽ");
                // return;
            }
        }
    }
}