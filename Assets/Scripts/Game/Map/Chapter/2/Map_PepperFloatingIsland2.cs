using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ����������ҹ��
/// </summary>
public class Map_PepperFloatingIsland2 : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // �м�3*3
        for (int i = 2; i < 5; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                CreateAndAddGrid(j, i);
            }
        }
        // ��
        for (int i = 2; i < 5; i++)
        {
            for (int j = 1; j < 3; j++)
            {
                CreateAndAddGrid(j, i);
            }
        }
        // ��
        for (int i = 5; i < 7; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                CreateAndAddGrid(j, i);
            }
        }
        // ��
        for (int i = 2; i < 5; i++)
        {
            for (int j = 6; j < 8; j++)
            {
                CreateAndAddGrid(j, i);
            }
        }
        // ��
        for (int i = 0; i < 2; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                CreateAndAddGrid(j, i);
            }
        }
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[4];
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
        }
        // ��
        for (int i = 2; i < 5; i++)
        {
            for (int j = 1; j < 3; j++)
            {
                listArray[0].Add(GetGrid(j, i));
            }
        }
        // ��
        for (int i = 5; i < 7; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                listArray[1].Add(GetGrid(j, i));
            }
        }
        // ��
        for (int i = 2; i < 5; i++)
        {
            for (int j = 6; j < 8; j++)
            {
                listArray[2].Add(GetGrid(j, i));
            }
        }
        // ��
        for (int i = 0; i < 2; i++)
        {
            for (int j = 3; j < 6; j++)
            {
                listArray[3].Add(GetGrid(j, i));
            }
        }

        // ���ĵ�����
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left,
            mapCenter + 2.5f * MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 2.5f * MapManager.gridHeight * Vector3.up,
        };

        // �յ�����
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2 * MapManager.gridHeight * Vector3.up,
            mapCenter + 2 * MapManager.gridWidth * Vector3.left + 2.5f * MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2 * MapManager.gridHeight * Vector3.down,
            mapCenter + 2 * MapManager.gridWidth * Vector3.right + 2.5f * MapManager.gridHeight * Vector3.up,
        };

        Vector2[] checkAreaList = new Vector2[]
        {
            // ��
            new Vector2(1.5f, 2.5f),
            // ��
            new Vector2(2.5f, 1.5f),
            // ��
            new Vector2(1.5f, 2.5f),
            // ��
            new Vector2(2.5f, 1.5f),
        };

        for (int k = 0; k < 4; k++)
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
                        strandedTime = 540
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 360,
                        strandedTime = 540
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + (k+1) +""),
                new Vector2(),
                false, false);
            gridGroupList.Add(gridGroup);
            gridGroup.transform.SetParent(transform);

            Transform trans = GameController.Instance.mMapController.transform;
            // ���ý�����ļ����
            {
                Vector3 size = new Vector3(checkAreaList[k].x * MapManager.gridWidth, checkAreaList[k].y * MapManager.gridHeight);
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position, size, "ItemCollideEnemy");
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
        // Ϊȫͼ��Ӻ�ҹBUFF
        //ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
        //GameController.Instance.AddAreaEffectExecution(e);
    }
}
