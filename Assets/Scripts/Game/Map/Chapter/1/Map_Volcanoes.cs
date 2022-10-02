using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// ѩ����ɽ
/// </summary>
public class Map_Volcanoes : ChapterMap
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
            new Vector2(4, 0), new Vector2(5, 0), new Vector2(8, 0),
            new Vector2(8, 1),
            new Vector2(2, 2), new Vector2(3, 2), new Vector2(7, 2), new Vector2(8, 2),
            new Vector2(2, 4), new Vector2(3, 4),
            new Vector2(3, 5), new Vector2(5, 5), new Vector2(8, 5),
            new Vector2(8, 6),
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
}
