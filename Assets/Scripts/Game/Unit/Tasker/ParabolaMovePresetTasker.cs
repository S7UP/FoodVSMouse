using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 抛物线移动的预设任务
/// </summary>
public class ParabolaMovePresetTasker: PresetTasker
{
    public Vector3 firstPosition; // 初始点
    public Vector3 targetPosition; // 目标点
    private BaseUnit targerUnit;
    private int totalTimer; // 初始点到目标点用时
    private int currentTimer; // 当前用时
    private float acc; // 加速度
    private Vector2 rotationY; // y方向
    private float velocityX; // 水平方向速度
    private float velocityY; // 垂直方向的速度
    private float sY; // y方向上的位移 

    /// <summary>
    /// 构造方法
    /// </summary>
    public ParabolaMovePresetTasker(BaseUnit baseUnit, float standardVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool filpY)
    {
        targerUnit = baseUnit;
        velocityX = TransManager.TranToVelocity(standardVelocity);
        this.firstPosition = firstPosition;
        this.targetPosition = targetPosition;
        // 获取y方向向量
        if (filpY)
            rotationY = Vector2.down;
        else
            rotationY = Vector2.up;
        baseUnit.transform.position = new Vector3(firstPosition.x, targetPosition.y, 0);
        totalTimer = Mathf.CeilToInt(Mathf.Abs(targetPosition.x - baseUnit.transform.position.x) / velocityX);
        // 计算得出加速度
        acc = -8 * height / (totalTimer * totalTimer);
        // 为垂直方向的初速度赋值
        velocityY = -acc * totalTimer / 2;

        // 填写事件预设
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
        // 横向为判定坐标移动
        targerUnit.SetPosition(vx);
        // 纵向为贴图相对坐标移动（纵向判定坐标不变）
        targerUnit.SetSpriteLocalPosition(rotationY * sY);
        currentTimer++;
    }

    private bool EndCond()
    {
        return currentTimer > totalTimer;
    }
}
