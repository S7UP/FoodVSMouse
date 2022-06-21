using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �������ƶ���Ԥ������
/// </summary>
public class ParabolaMovePresetTasker: PresetTasker
{
    public Vector3 firstPosition; // ��ʼ��
    public Vector3 targetPosition; // Ŀ���
    private BaseUnit targerUnit;
    private int totalTimer; // ��ʼ�㵽Ŀ�����ʱ
    private int currentTimer; // ��ǰ��ʱ
    private float acc; // ���ٶ�
    private Vector2 rotationY; // y����
    private float velocityX; // ˮƽ�����ٶ�
    private float velocityY; // ��ֱ������ٶ�
    private float sY; // y�����ϵ�λ�� 

    /// <summary>
    /// ���췽��
    /// </summary>
    public ParabolaMovePresetTasker(BaseUnit baseUnit, float standardVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool filpY)
    {
        targerUnit = baseUnit;
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

        // ��д�¼�Ԥ��
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    private void Update()
    {
        sY += velocityY;
        velocityY += acc;
        Vector2 vx = Vector2.Lerp(firstPosition, targetPosition, (float)currentTimer / totalTimer);
        // ����Ϊ�ж������ƶ�
        targerUnit.SetPosition(vx);
        // ����Ϊ��ͼ��������ƶ��������ж����겻�䣩
        targerUnit.SetSpriteLocalPosition(rotationY * sY);
        currentTimer++;
    }

    private bool EndCond()
    {
        return currentTimer > totalTimer;
    }
}
