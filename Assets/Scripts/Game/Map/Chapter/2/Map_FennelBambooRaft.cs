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
