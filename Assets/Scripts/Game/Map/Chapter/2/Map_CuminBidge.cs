using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��Ȼ���ţ��գ�
/// </summary>
public class Map_CuminBidge : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // 1��7·
        for (int i = 0; i < 9; i++)
        {
            CreateAndAddGrid(i, 0);
            CreateAndAddGrid(i, 6);
        }
        // 2��4��6·
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
        // 3��5·
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
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[5];
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
        }
        // 2 4 6·
        for (int i = 5; i < 9; i++)
        {
            listArray[0].Add(GetGrid(i, 1));
            listArray[2].Add(GetGrid(i, 3));
            listArray[4].Add(GetGrid(i, 5));
        }
        // 3 5·
        for (int i = 2; i < 6; i++)
        {
            listArray[1].Add(GetGrid(i, 2));
            listArray[3].Add(GetGrid(i, 4));
        }

        // ���ĵ�����
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.up,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.left + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.down,
        };

        // �յ�����
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.up,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 1*MapManager.gridHeight * Vector3.down,
            mapCenter + 0.5f * MapManager.gridWidth * Vector3.right + 2*MapManager.gridHeight * Vector3.down,
        };

        // �ƶ�ʱ��
        List<int> moveTimeList = new List<int>() { 360, 540, 360, 540, 360};

        for (int k = 0; k < 5; k++)
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
    /// �Ը��ӽ��мӹ�
    /// </summary>
    public override void ProcessingGridList()
    {
        // 1��7·
        for (int i = 0; i < 9; i++)
        {
            GetGrid(i, 0).ChangeMainGridState(new WaterGridState(GetGrid(i, 0)));
            GetGrid(i, 6).ChangeMainGridState(new WaterGridState(GetGrid(i, 6)));
        }
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
