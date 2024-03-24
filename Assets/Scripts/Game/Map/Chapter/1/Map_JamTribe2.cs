using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��������(ҹ)
/// </summary>
public class Map_JamTribe2 : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {

    }

    /// <summary>
    /// �Ը��ӽ��мӹ�
    /// </summary>
    public override void ProcessingGridList()
    {
        // �����ҽ�
        List<Vector2> list = new List<Vector2>()
        {
            new Vector2(5, 0), new Vector2(6, 0),
            new Vector2(6, 1),
            new Vector2(5, 2), new Vector2(6, 2),
            new Vector2(4, 3), new Vector2(5, 3),
            new Vector2(4, 4), new Vector2(5, 4),
            new Vector2(5, 5),
            new Vector2(4, 6), new Vector2(5, 6),
        };
        foreach (var v in list)
        {
            GetGrid((int)v.x, (int)v.y).AddGridType(GridType.Lava, BaseGridType.GetInstance(GridType.Lava, 0));
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
        // Ϊȫͼ��Ӻ�ҹBUFF
        //ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
        //GameController.Instance.AddAreaEffectExecution(e);
    }
}
