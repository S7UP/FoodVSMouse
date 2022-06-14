using System.Collections;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �������ƶ�״̬����״̬�µ�λ�ƶ���Ϊ��״̬���ƣ����ǵ��õ�λ��Ӧ�ķ���
/// </summary>
public class ParabolaMoveState : BaseActionState
{
    public Vector3 firstPosition; // ��ʼ��
    public Vector3 targetPosition; // Ŀ���
    private int totalTimer; // ��ʼ�㵽Ŀ�����ʱ
    private int currentTimer; // ��ǰ��ʱ
    private float acc; // ���ٶ�
    private Vector2 rotationY; // y����
    private float velocityX; // ˮƽ�����ٶ�
    private float velocityY; // ��ֱ������ٶ�
    private float sY; // y�����ϵ�λ�� 

    private ParabolaMoveState(BaseUnit baseUnit) : base(baseUnit)
    {

    }

    public ParabolaMoveState(BaseUnit baseUnit, float standardVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool filpY) :base(baseUnit)
    {
        velocityX = TransManager.TranToVelocity(standardVelocity);
        this.firstPosition = firstPosition;
        this.targetPosition = targetPosition;
        // ��ȡy��������
        if (filpY)
            rotationY = Vector2.down;
        else
            rotationY = Vector2.up;
        baseUnit.transform.position = new Vector3(firstPosition.x, targetPosition.y, 0);
        totalTimer = Mathf.CeilToInt(Mathf.Abs(targetPosition.x - baseUnit.transform.position.x) / velocityX);
        // ����ó����ٶ�
        acc = -8 * height / (totalTimer * totalTimer);
        // Ϊ��ֱ����ĳ��ٶȸ�ֵ
        velocityY = -acc * totalTimer / 2;
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        // ��ʱ��תΪ�������ƶ�״̬
        if (currentTimer >= totalTimer)
        {
            mBaseUnit.SetActionState(new MoveState(mBaseUnit));
        }
        velocityY += acc;
        sY += velocityY;
        Vector2 vx = Vector2.Lerp(firstPosition, targetPosition, (float)currentTimer / totalTimer);
        mBaseUnit.SetPosition(vx + rotationY * sY);
        currentTimer++;
    }

    public override void OnExit()
    {
        base.OnExit();
    }

    public override void OnInterrupt()
    {
        base.OnInterrupt();
    }

    public override void OnContinue()
    {
        base.OnContinue();
    }
}
