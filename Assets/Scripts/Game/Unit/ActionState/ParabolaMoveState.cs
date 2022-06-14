using System.Collections;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 抛物线移动状态，此状态下单位移动行为由状态控制，而非调用单位对应的方法
/// </summary>
public class ParabolaMoveState : BaseActionState
{
    public Vector3 firstPosition; // 初始点
    public Vector3 targetPosition; // 目标点
    private int totalTimer; // 初始点到目标点用时
    private int currentTimer; // 当前用时
    private float acc; // 加速度
    private Vector2 rotationY; // y方向
    private float velocityX; // 水平方向速度
    private float velocityY; // 垂直方向的速度
    private float sY; // y方向上的位移 

    private ParabolaMoveState(BaseUnit baseUnit) : base(baseUnit)
    {

    }

    public ParabolaMoveState(BaseUnit baseUnit, float standardVelocity, float height, Vector3 firstPosition, Vector3 targetPosition, bool filpY) :base(baseUnit)
    {
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
    }

    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnUpdate()
    {
        base.OnUpdate();
        // 超时后转为正常的移动状态
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
