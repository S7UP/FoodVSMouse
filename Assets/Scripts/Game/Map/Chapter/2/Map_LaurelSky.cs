using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �¹���գ��գ�
/// </summary>
public class Map_LaurelSky : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // ������
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
        // �ϰ�
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
        }
        // �°�
        for (int i = 5; i < 7; i++)
        {
            for (int j = 4; j < 9; j++)
                CreateAndAddGrid(j, i);
        }
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[2];
        listArray[0] = new List<BaseGrid>();
        // �ϰ�
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
                listArray[0].Add(GetGrid(j, i));
        }
        // �°�
        listArray[1] = new List<BaseGrid>();
        for (int i = 5; i < 7; i++)
        {
            for (int j = 4; j < 9; j++)
                listArray[1].Add(GetGrid(j, i));
        }

        // ���ĵ�����
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.down
        };

        // �յ�����
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.down
        };

        for (int k = 0; k < 2; k++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // �������ĵ�����
            gridGroup.transform.position = centerList[k];
            // �����Ӧ�ĸ���
            foreach (var grid in listArray[k])
            {
                gridGroup.Add(grid);
            }
            // ���ò������ƶ����
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

    }

    public override void OtherProcessing()
    {
        Transform trans = GameController.Instance.mMapController.transform;
        // ��ӿ���
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetSkyArea(MapManager.GetGridLocalPosition(4, 3), new Vector2(5 * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "SkyArea";
            GameController.Instance.AddAreaEffectExecution(r);
        }


        // ��������󶨵ĳ�����
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

            // �ϰ�
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 5; j++)
                    createVehicleAreaAction(GetGrid(j, i));
            }
            // �°�
            for (int i = 5; i < 7; i++)
            {
                for (int j = 4; j < 9; j++)
                    createVehicleAreaAction(GetGrid(j, i));
            }
        }

        // ����Ʋ�
        for (int i = 2; i < 5; i++)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 8);
        }

        // ��ӷ���
        {
            WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(8.6f, 3, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(3)));
            WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 0, 360, 120, 1440); // �ȴ�ʱ�䡢�ٶȱ仯ʱ�䡢����ʱ��
            GameController.Instance.AddAreaEffectExecution(e);
        }
    }
}
