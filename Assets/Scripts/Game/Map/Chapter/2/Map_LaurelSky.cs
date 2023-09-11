using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 月桂天空（日）
/// </summary>
public class Map_LaurelSky : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 中三线
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
        // 上板
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
        }
        // 下板
        for (int i = 5; i < 7; i++)
        {
            for (int j = 4; j < 9; j++)
                CreateAndAddGrid(j, i);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[2];
        listArray[0] = new List<BaseGrid>();
        // 上板
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
                listArray[0].Add(GetGrid(j, i));
        }
        // 下板
        listArray[1] = new List<BaseGrid>();
        for (int i = 5; i < 7; i++)
        {
            for (int j = 4; j < 9; j++)
                listArray[1].Add(GetGrid(j, i));
        }

        // 中心点坐标
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.down
        };

        // 终点坐标
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.down
        };

        for (int k = 0; k < 2; k++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // 设置中心点坐标
            gridGroup.transform.position = centerList[k];
            // 加入对应的格子
            foreach (var grid in listArray[k])
            {
                gridGroup.Add(grid);
            }
            // 设置并启用移动版块
            gridGroup.StartMovement(
                new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = centerList[k],
                        moveTime = 720,
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 720,
                        strandedTime = 450
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + "1"),
                new Vector2(),
                false, false);
            gridGroupList.Add(gridGroup);
            gridGroup.transform.SetParent(transform);
        }
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

            // 上板
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 5; j++)
                    createVehicleAreaAction(GetGrid(j, i));
            }
            // 下板
            for (int i = 5; i < 7; i++)
            {
                for (int j = 4; j < 9; j++)
                    createVehicleAreaAction(GetGrid(j, i));
            }
        }

        // 添加云层
        for (int i = 2; i < 5; i++)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 8);
        }

        // 添加风域
        {
            WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(8.6f, 3, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(3)));
            WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 0, 360, 120, 1440); // 等待时间、速度变化时间、匀速时间
            GameController.Instance.AddAreaEffectExecution(e);
        }
    }
}
