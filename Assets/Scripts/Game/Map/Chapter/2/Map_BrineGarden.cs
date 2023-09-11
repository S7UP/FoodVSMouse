using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ±�ϻ�԰
/// </summary>
public class Map_BrineGarden : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 6; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
        // �������ĵ�����
        gridGroup.transform.position = mapCenter + 1.5f * MapManager.gridWidth * Vector3.left;
        // �����Ӧ�ĸ���
        foreach (var item in GetGridList())
        {
            gridGroup.Add(item);
        }
        // ���ò������ƶ����
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
    }

    /// <summary>
    /// �Ը��ӽ��мӹ�
    /// </summary>
    public override void ProcessingGridList()
    {

    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {
        Transform trans = GameController.Instance.mMapController.transform;
        // ���� ����һ����һ�е�ˮ��
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetWaterArea(MapManager.GetGridLocalPosition(4f, 3), new Vector2(7.75f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "WaterArea";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ���������󶨵ĳ�����
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

            foreach (var g in GetGridList())
                createVehicleAreaAction(g);
        }
    }
}
