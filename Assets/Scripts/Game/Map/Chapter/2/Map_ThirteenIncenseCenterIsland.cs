using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ʮ�������ĵ�
/// </summary>
public class Map_ThirteenIncenseCenterIsland : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 1; i < 6; i++)
            for (int j = 2; j < 7; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        Vector2[,] v2List = new Vector2[,]
        {
            { new Vector2(2, 1), new Vector2(3, 1), new Vector2(4, 1), new Vector2(5, 1), new Vector2(5, 2), new Vector2(5, 3)},
            { new Vector2(6, 1), new Vector2(6, 2), new Vector2(6, 3), new Vector2(6, 4), new Vector2(5, 4), new Vector2(4, 4)},
            { new Vector2(6, 5), new Vector2(5, 5), new Vector2(4, 5), new Vector2(3, 5), new Vector2(3, 4), new Vector2(3, 3)},
            { new Vector2(2, 5), new Vector2(2, 4), new Vector2(2, 3), new Vector2(2, 2), new Vector2(3, 2), new Vector2(4, 2)}
        };

        Vector2[] endPointList = new Vector2[]
        {
            mapCenter + new Vector3(0, gridHeight, 0),
            mapCenter + new Vector3(2*gridWidth, 0, 0),
            mapCenter + new Vector3(0, -gridHeight, 0),
            mapCenter + new Vector3(-2*gridWidth, 0, 0)
        };

        Vector2[] offsetList = new Vector2[]
        {
            new Vector2(-gridWidth/2, gridHeight),
            new Vector2(gridWidth, gridHeight/2),
            new Vector2(gridWidth/2, -gridHeight),
            new Vector2(-gridWidth, -gridHeight/2)
        };

        for (int i = 0; i < 4; i++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // �������ĵ�����
            gridGroup.transform.position = mapCenter;
            // �����Ӧ�ĸ���
            for (int j = 0; j < 6; j++)
            {
                gridGroup.Add(GetGrid(((int)v2List[i, j].x), ((int)v2List[i, j].y)));
            }
            // ���ò������ƶ����
            bool filp = false;
            if (i > 0 && i < 3)
            {
                filp = true;
            }
            gridGroup.StartMovement(
                new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter,
                        moveTime = 360,
                        strandedTime = 960
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endPointList[i],
                        moveTime = 360,
                        strandedTime = 960
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + (2 - (i % 2))),
                offsetList[i],
                filp, filp);
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
