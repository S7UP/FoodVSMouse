using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �����񷤣��գ�
/// </summary>
public class Map_FennelBambooRaft : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // ���һ��
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 1; j++)
                CreateAndAddGrid(j, i);
        // ���
        for (int i = 0; i < 5; i++)
        {
            for (int j = 1; j < 5; j++)
                CreateAndAddGrid(j, i);
        }
        // �Ұ�
        for (int i = 2; i < 7; i++)
        {
            for (int j = 5; j < 9; j++)
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
        // ���
        for (int i = 0; i < 5; i++)
        {
            for (int j = 1; j < 5; j++)
                listArray[0].Add(GetGrid(j, i));
        }
        // �Ұ�
        listArray[1] = new List<BaseGrid>();
        for (int i = 2; i < 7; i++)
        {
            for (int j = 5; j < 9; j++)
                listArray[1].Add(GetGrid(j, i));
        }

        // ���ĵ�����
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 1.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.down
        };

        // �յ�����
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 1.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.up
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
                        moveTime = 360,
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 360,
                        strandedTime = 450
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + "1"),
                new Vector2(),
                false, false);
            gridGroupList.Add(gridGroup);
            gridGroup.transform.SetParent(transform);

            Transform trans = GameController.Instance.mMapController.transform;
            // ���ý�����ļ����
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position, new Vector2(3.5f * MapManager.gridWidth, 4.5f * MapManager.gridHeight), "ItemCollideEnemy");
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

                // ��������
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

            // �������鱻��Ϊ������
            {
                RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(gridGroup.transform.position, new Vector2(3.5f * MapManager.gridWidth, 4.5f * MapManager.gridHeight), new S7P.Numeric.FloatModifier(0));
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

    /// <summary>
    /// �����ӹ�
    /// </summary>
    public override void OtherProcessing()
    {
        Transform trans = GameController.Instance.mMapController.transform;
        // ���� ���������һ�е�ˮ��
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetWaterArea(MapManager.GetGridLocalPosition(4.25f, 3), new Vector2(7.25f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "WaterArea";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ������һ�г�����
        {
            RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(MapManager.GetGridLocalPosition(0, 3), new Vector2(1 * MapManager.gridWidth, 7 * MapManager.gridHeight), new S7P.Numeric.FloatModifier(0));
            r.name = "WaterVehicle(left)";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
