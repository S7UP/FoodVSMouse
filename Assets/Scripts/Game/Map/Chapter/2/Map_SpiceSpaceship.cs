using System;
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

    /// <summary>
    /// 其他加工
    /// </summary>
    public override void OtherProcessing()
    {
        Transform trans = GameController.Instance.mMapController.transform;
        // 添加空域
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetSkyArea(MapManager.GetGridLocalPosition(4, 3), new Vector2(5 * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "SkyArea";
            GameController.Instance.AddAreaEffectExecution(r);
        }


        // 创建与板块绑定的承载域
        {
            Action<BaseGrid> createVehicleAreaAction = (g) => {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(g.transform.position, new Vector2(1 * MapManager.gridWidth, 0.5f * MapManager.gridHeight));
                r.name = "SkyVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);

                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate {
                    r.transform.position = g.transform.position;
                    return !g.IsAlive();
                });
                r.taskController.AddTask(t);
            };

            for (int i = 2; i < 5; i++)
                for (int j = 0; j < 6; j++)
                    createVehicleAreaAction(GetGrid(j, i));
        }

        // 为全图添加黑夜BUFF
        {
            ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
            GameController.Instance.AddAreaEffectExecution(e);
        }


        {
            // 添加云层
            for (int i = 0; i <= 1; i++)
            {
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(3.75f), MapManager.GetRowY(i)), 10);
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(3.75f), MapManager.GetRowY(5+i)), 10);
            }

            // 添加风域
            for (int i = 0; i <= 5; i+=5)
            {
                for (int j = 0; j <= 1; j++)
                {
                    WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(8.6f, 1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i + j)));
                    WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 0, 360, 120, 1440); // 等待时间、速度变化时间、匀速时间
                    GameController.Instance.AddAreaEffectExecution(e);
                }
            }
        }
    }
}
