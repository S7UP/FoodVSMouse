using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��Ҷ�ոۣ��գ�
/// </summary>
public class Map_FragrantLeafAirport : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // 1357·
        for (int i = 0; i < 9; i++)
        {
            CreateAndAddGrid(i, 0);
            CreateAndAddGrid(i, 2);
            CreateAndAddGrid(i, 4);
            CreateAndAddGrid(i, 6);
        }
        // 2 6ͬ��
        for (int i = 0; i < 4; i++)
        {
            CreateAndAddGrid(i, 1);
            CreateAndAddGrid(i, 5);
        }
        // 4��2 6����
        for (int i = 5; i < 9; i++)
        {
            CreateAndAddGrid(i, 3);
        }
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[3];
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
        }
        
        // 2 6ͬ��
        for (int i = 0; i < 4; i++)
        {
            listArray[0].Add(GetGrid(i, 1));
            listArray[2].Add(GetGrid(i, 5));
        }
        // 4��2 6����
        for (int i = 5; i < 9; i++)
        {
            listArray[1].Add(GetGrid(i, 3));
        }

        // ���ĵ�����
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.down,
        };

        // �յ�����
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.down,
        };

        for (int k = 0; k < 3; k++)
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
                        moveTime = 900,
                        strandedTime = 450
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 900,
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
        for (int i = 0; i < 8; i++)
            for (int j = 0; j < 7; j+=2)
                GetGrid(i, j).AddGridType(GridType.Sky, BaseGridType.GetInstance(GridType.Sky, 0));
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {
        {
            // ����Ʋ�
            for (int i = 0; i < 7; i += 2)
            {
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(3.5f), MapManager.GetRowY(i)), 10);
            }

            // ��ӷ���
            for (int i = 0; i < 7; i += 2)
            {
                WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(8.6f, 1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)));
                WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 1, 450, 120, 780); // �ȴ�ʱ�䡢�ٶȱ仯ʱ�䡢����ʱ��
                GameController.Instance.AddAreaEffectExecution(e);
            }
        }
    }
}
