using UnityEngine;
using System;
/// <summary>
/// �������ӵ�
/// </summary>
public class TrackingBullets : BaseBullet
{
    private BaseUnit target; // ����Ŀ��
    private bool isSearchEnemy; // �Ƿ��Ե��˵�λ��Ϊ����Ŀ��
    private bool isSearchAlly; // �Ƿ����ѷ���λ��������Ŀ��
    private float currentRotate; // ��ǰ����Ƕȣ��Ƕ��ƣ�
    private Func<BaseUnit, BaseUnit, bool> CompareFunc; // �Ƚϵķ�������һ������Ϊ��ǰtarget���ڶ�������Ϊ�´������Ƚϵ�target

    ////////////////////////////////////////////////////////////////����Ϊ�������÷���/////////////////////////////////////////////
    
    /// <summary>
    /// �����Ƿ��Ե��˵�λ��Ϊ����Ŀ��
    /// </summary>
    public void SetSearchEnemyEnable(bool enable)
    {
        isSearchEnemy = enable;
    }

    /// <summary>
    /// �����Ƿ����ѷ���λ��Ϊ����Ŀ��
    /// </summary>
    public void SetSearchAllyEnable(bool enable)
    {
        isSearchAlly = enable;
    }

    /// <summary>
    /// ��������ʱ�ıȽϷ���
    /// </summary>
    /// <param name="CompareFunc">��һ������Ϊ��ǰtarget���ڶ�������Ϊ�´������Ƚϵ�target</param>
    public void SetCompareFunc(Func<BaseUnit, BaseUnit, bool> CompareFunc)
    {
        this.CompareFunc = CompareFunc;
    }

    ////////////////////////////////////////////////////////////////����Ϊ��д����/////////////////////////////////////////////

    /// <summary>
    /// ��ʼ��
    /// </summary>
    public override void MInit()
    {
        base.MInit();
        target = null;
        isSearchEnemy = false;
        isSearchAlly = false;
        currentRotate = 0;
        CompareFunc = null;
    }

    /// <summary>
    /// ����;�в���ִ������
    /// </summary>
    public override void OnFlyState()
    {
        CheckTargetValid();
        SearchTarget();
        if (target != null)
        {
            // Debug.Log("dangle = " + TransManager.Angle_360(transform.position, target.transform.position));
            // currentRotate = transform.eulerAngles.z + Vector3.Angle(transform.position, target.transform.position) * 0.05f;
            // currentRotate = TransManager.Angle_360(transform.position, target.transform.position);
            // Debug.Log("currentRotate = " + currentRotate);
            // mRotate = new Vector2(Mathf.Cos(currentRotate * Mathf.PI / 180), Mathf.Sin(currentRotate * Mathf.PI / 180));
            mRotate = (target.transform.position - transform.position).normalized;
        }
        base.OnFlyState();
    }

    /// <summary>
    /// ֻ������Ŀ�������ײ
    /// </summary>
    /// <param name="unit"></param>
    /// <returns></returns>
    public override bool CanHit(BaseUnit unit)
    {
        return base.CanHit(unit) && unit == target;
    }

    /// <summary>
    /// �ı䷽��
    /// </summary>
    /// <param name="v"></param>
    public override void SetRotate(Vector2 v)
    {
        base.SetRotate(v);
        currentRotate = 180*Mathf.Acos(mRotate.x)/Mathf.PI;
        Debug.Log("currentRotate = " + currentRotate);
    }


    ////////////////////////////////////////////////////////////////����Ϊ˽�з���/////////////////////////////////////////////


    /// <summary>
    /// ���з���
    /// </summary>
    private void SearchTarget()
    {
        if (CompareFunc == null || target!=null)
            return;

        if(isSearchEnemy)
            foreach (var item in GameController.Instance.GetEachEnemy())
            {
                if(CompareFunc(target, item))
                {
                    target = item;
                }
            }

        if (isSearchAlly)
            foreach (var item in GameController.Instance.GetEachEnemy())
            {
                if (CompareFunc(target, item))
                {
                    target = item;
                }
            }
    }

    /// <summary>
    /// ���target�Ƿ���Ч
    /// </summary>
    private void CheckTargetValid()
    {
        if(target == null || !target.IsAlive() || !target.CanBeSelectedAsTarget() || target.GetHeight() != mHeight)
        {
            target = null;
        }
    }

}
