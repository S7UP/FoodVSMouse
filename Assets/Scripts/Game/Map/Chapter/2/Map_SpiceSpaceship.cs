using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 香料飞船
/// </summary>
public class Map_SpiceSpaceship : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 中三路
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 6; j++)
                CreateAndAddGrid(j, i);
        // 两侧
        for (int i = 0; i < 7; i++)
        {
            if (i >= 2 && i <= 4)
                continue;
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
        // 设置中心点坐标
        gridGroup.transform.position = mapCenter + 1.5f * MapManager.gridWidth * Vector3.left;
        // 加入对应的格子
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 6; j++)
                gridGroup.Add(GetGrid(j, i));
        // 设置并启用移动版块
        gridGroup.StartMovement(
            new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.left,
                        moveTime = 540,
                        strandedTime = 420
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.right,
                        moveTime = 540,
                        strandedTime = 420
                    }
            },
            GameManager.Instance.GetSprite(GetSpritePath() + "1"),
            new Vector2(),
            false, false);
        gridGroupList.Add(gridGroup);
        gridGroup.transform.SetParent(transform);
    }

    /// <summary>
    /// 对格子进行加工
    /// </summary>
    public override void ProcessingGridList()
    {

    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
