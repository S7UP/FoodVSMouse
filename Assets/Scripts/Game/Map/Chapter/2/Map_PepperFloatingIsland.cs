using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �����������գ�
/// </summary>
public class Map_PepperFloatingIsland : ChapterMap
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
            mapCenter + 3 * MapManager.gridWidth * Vector3.left + 2.5f * MapManager.gridHeight * Vector3.down,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2 * MapManager.gridHeight * Vector3.down,
            mapCenter + 3 * MapManager.gridWidth * Vector3.right + 2.5f * MapManager.gridHeight * Vector3.up,
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
                        moveTime = 540,
                        strandedTime = 900
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 540,
                        strandedTime = 900
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + (k+1) +""),
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
}
