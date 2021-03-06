using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 移动版块
/// </summary>
public class MovementGridGroup : GridGroup
{
    /// <summary>
    /// 每个路径点信息
    /// </summary>
    public struct PointInfo
    {
        public Vector3 targetPosition; // 目标位置
        public int moveTime; // 花费时间（帧）
        public int strandedTime; // 滞留时间（帧）
    }

    // 引用
    public SpriteRenderer spriteRenderer;
    // 变量
    public List<PointInfo> positionList = new List<PointInfo>();
    public int currentMoveCount; // 当前已移动过的路径点次数
    public int currentPositionListIndex; // 当前下标 
    public int currentTime; // 当前路径点事件中已花费时间
    public Vector3 startPosition;
    public Vector3 endPosition;
    public bool isPause { get; private set; }

    public override void Awake()
    {
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 开始移动
    /// </summary>
    public void StartMovement(List<PointInfo> positionList, Sprite sprite, Vector2 offset, bool filpX, bool filpY)
    {
        foreach (var item in positionList)
        {
            this.positionList.Add(item);
        }

        if (sprite != null)
        {
            spriteRenderer.sprite = sprite;
        }
        spriteRenderer.flipX = filpX;
        spriteRenderer.flipY = filpY;
        spriteRenderer.transform.localPosition = offset;
        currentPositionListIndex++; // 1为起始
        startPosition = this.positionList[currentPositionListIndex-1].targetPosition; // 0
        endPosition = this.positionList[currentPositionListIndex].targetPosition; // 1
        SetPosition(startPosition);
        isPause = false;
    }

    public override void MInit()
    {
        base.MInit();
        positionList.Clear();
        currentMoveCount = 0;
        currentPositionListIndex = 0;
        currentTime = 0;
        isPause = true; // 默认先暂停
    }

    public override void MUpdate()
    {
        base.MUpdate();
        if (isPause)
            return;
        currentTime++;
        if (currentTime<= positionList[currentPositionListIndex].moveTime)
        {
            // 移动状态
            SetPosition(Vector3.Lerp(startPosition, endPosition, (float)currentTime/ positionList[currentPositionListIndex].moveTime));
        }else if(currentTime <= positionList[currentPositionListIndex].moveTime + positionList[currentPositionListIndex].strandedTime)
        {
            // 停滞状态
        }
        else
        {
            // 切换为下一态
            currentMoveCount++;
            currentPositionListIndex++;
            if (currentPositionListIndex == positionList.Count)
                currentPositionListIndex = 0;
            startPosition = endPosition;
            endPosition = positionList[currentPositionListIndex].targetPosition;
            currentTime = 0;
        }
    }

    /// <summary>
    /// 设置当前移动进程百分比
    /// </summary>
    public void SetCurrentMovementPercent(float percent)
    {
        currentTime = Mathf.FloorToInt(positionList[currentPositionListIndex].moveTime*percent);
        SetPosition(Vector3.Lerp(startPosition, endPosition, (float)currentTime / positionList[currentPositionListIndex].moveTime));
    }

    /// <summary>
    /// 获取一个实例
    /// </summary>
    public static MovementGridGroup GetInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/MovementGridGroup").GetComponent<MovementGridGroup>();
    }

    /// <summary>
    /// 回收自身进对象池
    /// </summary>
    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Grid/MovementGridGroup", this.gameObject);
    }
}
