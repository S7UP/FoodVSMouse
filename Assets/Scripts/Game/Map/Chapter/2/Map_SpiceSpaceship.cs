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
        // ����Ʋ�
        for (int i = 1; i < 8; i++)
            for (int j = 0; j < 2; j++)
            {
                GetGrid(i, j).AddGridType(GridType.Sky, BaseGridType.GetInstance(GridType.Sky, 0));
                GetGrid(i, 5 + j).AddGridType(GridType.Sky, BaseGridType.GetInstance(GridType.Sky, 0));
            }
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
        {
            // Ϊȫͼ��Ӻ�ҹBUFF
            ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
            GameController.Instance.AddAreaEffectExecution(e);
        }


        {
            // ����Ʋ�
            for (int i = 0; i <= 1; i++)
            {
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(3.75f), MapManager.GetRowY(i)), 10);
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(3.75f), MapManager.GetRowY(5+i)), 10);
            }

            // ��ӷ���
            for (int i = 0; i <= 5; i+=5)
            {
                for (int j = 0; j <= 1; j++)
                {
                    WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(9, 1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i + j)));
                    WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 1, 300, 120, 540, true); // �ȴ�ʱ�䡢�ٶȱ仯ʱ�䡢����ʱ��
                    GameController.Instance.AddAreaEffectExecution(e);
                }
            }
        }
    }
}
