using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ������
/// </summary>
public class Map_MacchiatoHarbor : ChapterMap
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
        List<Vector2> list = new List<Vector2>()
        {
            new Vector2(8, 1), new Vector2(6, 2), new Vector2(8, 3), new Vector2(6, 4), new Vector2(8, 5)
        };

        foreach (var item in list)
        {
            // ���ɷ綴
            BaseWindCave c = (BaseWindCave)GameController.Instance.CreateItem((int)item.x, (int)item.y, (int)ItemNameTypeMap.WindCave, 0);
            c.SetStartTimeAndMaxTime(360, 480);
        }
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
