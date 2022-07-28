using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ����ʺ磨�գ�
/// </summary>
public class Map_LilacRainbow : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // 5*7
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[7];

        // 7�а��
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
            for (int j = 0; j < 5; j++)
            {
                listArray[i].Add(GetGrid(j, i));
            }
        }

        for (int k = 0; k < 7; k++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // �������ĵ�����
            gridGroup.transform.position = mapCenter + 2f * MapManager.gridWidth * Vector3.left + (3-k)*MapManager.gridHeight * Vector3.up;
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
                        targetPosition = mapCenter + 2f * MapManager.gridWidth * Vector3.left + (3-k)*MapManager.gridHeight * Vector3.up,
                        moveTime = 720,
                        strandedTime = 720
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 2f * MapManager.gridWidth * Vector3.right + (3-k)*MapManager.gridHeight * Vector3.up,
                        moveTime = 720,
                        strandedTime = 720
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + (k + 1)),
                new Vector2(),
                false, false);
            gridGroup.SetCurrentMovementPercent((float)k / 6);
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
