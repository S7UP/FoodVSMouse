using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 幻境花园
/// </summary>
public class Map_BrineGarden2 : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 6; j++)
                CreateAndAddGrid(j, i);
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
        foreach (var item in GetGridList())
        {
            gridGroup.Add(item);
        }
        // 设置并启用移动版块
        gridGroup.StartMovement(
            new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.left,
                        moveTime = 540,
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.right,
                        moveTime = 540,
                        strandedTime = 450
                    }
            },
            GameManager.Instance.GetSprite(GetSpritePath() + "1"),
            new Vector2(),
            false, false);
        gridGroupList.Add(gridGroup);
        gridGroup.transform.SetParent(transform);

        Transform trans = GameController.Instance.mMapController.transform;
        // 设置进入板块的检测域
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position, new Vector2(5.5f * MapManager.gridWidth, 6.5f * MapManager.gridHeight), "ItemCollideEnemy");
            r.name = "GridGroupEnterCheckArea";
            r.transform.SetParent(trans);
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.SetOnEnemyEnterAction((u) => {
                gridGroup.TryEnter(u);
            });
            r.SetOnEnemyExitAction((u) => {
                gridGroup.TryExit(u);
            });
            GameController.Instance.AddAreaEffectExecution(r);

            // 跟随任务
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate {
                r.transform.position = gridGroup.transform.position;
                return !gridGroup.IsAlive();
            });
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(t);
        }

        // 整个大板块被视为承载域
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(gridGroup.transform.position, new Vector2(5.5f * MapManager.gridWidth, 6.5f * MapManager.gridHeight), new S7P.Numeric.FloatModifier(0));
            r.name = "WaterVehicle(gridGroup)";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);

            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                r.transform.position = gridGroup.transform.position;
                return !gridGroup.IsAlive();
            });
            r.taskController.AddTask(t);
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
        Transform trans = GameController.Instance.mMapController.transform;
        // 创建 从左一到右一列的水域
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetWaterArea(MapManager.GetGridLocalPosition(4f, 3), new Vector2(7.75f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "WaterArea";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
