using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 孜然断桥（夜）
/// </summary>
public class Map_CuminBidge2 : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 1、7路
        for (int i = 0; i < 9; i++)
        {
            CreateAndAddGrid(i, 0);
            CreateAndAddGrid(i, 6);
        }
        // 2、4、6路
        for (int i = 0; i < 3; i++)
        {
            CreateAndAddGrid(i, 1);
            CreateAndAddGrid(i, 3);
            CreateAndAddGrid(i, 5);
        }
        for (int i = 5; i < 9; i++)
        {
            CreateAndAddGrid(i, 1);
            CreateAndAddGrid(i, 3);
            CreateAndAddGrid(i, 5);
        }
        // 3、5路
        for (int i = 0; i < 2; i++)
        {
            CreateAndAddGrid(i, 2);
            CreateAndAddGrid(i, 4);
        }
        for (int i = 2; i < 6; i++)
        {
            CreateAndAddGrid(i, 2);
            CreateAndAddGrid(i, 4);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[5];
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
        }
        // 2 4 6路
        for (int i = 5; i < 9; i++)
        {
            listArray[0].Add(GetGrid(i, 1));
            listArray[2].Add(GetGrid(i, 3));
            listArray[4].Add(GetGrid(i, 5));
        }
        // 3 5路
        for (int i = 2; i < 6; i++)
        {
            listArray[1].Add(GetGrid(i, 2));
            listArray[3].Add(GetGrid(i, 4));
        }

        // 中心点坐标
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.up,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.down,
        };

        // 终点坐标
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.down,
        };

        // 移动时间
        List<int> moveTimeList = new List<int>() { 360, 540, 360, 540, 360};

        for (int k = 0; k < 5; k++)
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
                        moveTime = moveTimeList[k],
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = moveTimeList[k],
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

    /// <summary>
    /// 其他加工
    /// </summary>
    public override void OtherProcessing()
    {
        // 为全图添加黑夜BUFF
        ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
        GameController.Instance.AddAreaEffectExecution(e);

        Transform trans = GameController.Instance.mMapController.transform;
        // 创建 1 7 路 整行水域
        for (int i = 0; i < 2; i++)
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetWaterArea(MapManager.GetGridLocalPosition(4, i * 6), new Vector2(7.75f * MapManager.gridWidth, 1 * MapManager.gridHeight));
            r.name = "WaterArea(" + (i * 6 + 1) + ")";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }
        // 创建 从左四到右一列的水域
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetWaterArea(MapManager.GetGridLocalPosition(5.25f, 3), new Vector2(5.5f * MapManager.gridWidth, 5 * MapManager.gridHeight));
            r.name = "WaterArea(center)";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }
        // 创建 (2,2) (2,4)水域
        {
            Vector2[] v2Array = new Vector2[] { new Vector2(2, 2), new Vector2(2, 4) };
            foreach (var v2 in v2Array)
            {
                RetangleAreaEffectExecution r = Environment.WaterManager.GetWaterArea(MapManager.GetGridLocalPosition(v2.x, v2.y), new Vector2(1 * MapManager.gridWidth, 1 * MapManager.gridHeight));
                r.name = "WaterArea(" + v2.x + "," + v2.y + ")";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }

        // 创建 左一列 到 左二列 承载域
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(MapManager.GetGridLocalPosition(0.5f, 3), new Vector2(2 * MapManager.gridWidth, 5 * MapManager.gridHeight), new S7P.Numeric.FloatModifier(0));
            r.name = "WaterVehicle(left)";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 创建(2,1),(2,3),(2,5)承载域
        {
            Vector2[] v2Array = new Vector2[] { new Vector2(2, 1), new Vector2(2, 3), new Vector2(2, 5) };
            foreach (var v2 in v2Array)
            {
                RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(MapManager.GetGridLocalPosition(v2.x, v2.y), new Vector2(1 * MapManager.gridWidth, 1 * MapManager.gridHeight), new S7P.Numeric.FloatModifier(0));
                r.name = "WaterVehicle(" + v2.x + "," + v2.y + ")";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }

        // 创建与五大板块绑定的承载域
        {
            Action<BaseGrid> createVehicleAreaAction = (g) => {
                RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(g.transform.position, new Vector2(1 * MapManager.gridWidth, 0.5f * MapManager.gridHeight), new S7P.Numeric.FloatModifier(0));
                r.name = "WaterVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);

                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate {
                    r.transform.position = g.transform.position;
                    return !g.IsAlive();
                });
                r.taskController.AddTask(t);
            };

            // 2 4 6路
            for (int i = 5; i < 9; i++)
            {
                createVehicleAreaAction(GetGrid(i, 1));
                createVehicleAreaAction(GetGrid(i, 3));
                createVehicleAreaAction(GetGrid(i, 5));
            }
            // 3 5路
            for (int i = 2; i < 6; i++)
            {
                createVehicleAreaAction(GetGrid(i, 2));
                createVehicleAreaAction(GetGrid(i, 4));
            }
        }
    }
}
