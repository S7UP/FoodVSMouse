using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ���Ϸɴ�
/// </summary>
public class Map_SpiceSpaceship : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // ����·
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 6; j++)
                CreateAndAddGrid(j, i);
        // ����
        for (int i = 0; i < 7; i++)
        {
            if (i >= 2 && i <= 4)
                continue;
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
        }
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
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 6; j++)
                gridGroup.Add(GetGrid(j, i));
        // ���ò������ƶ����
        gridGroup.StartMovement(
            new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.left,
                        moveTime = 540,
                        strandedTime = 420
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.right,
                        moveTime = 540,
                        strandedTime = 420
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

    }
}
