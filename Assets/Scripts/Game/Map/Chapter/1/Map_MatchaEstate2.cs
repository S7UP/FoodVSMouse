using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// Ĩ��ׯ԰(ҹ)
/// </summary>
public class Map_MatchaEstate2 : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // �ո��
        List<Vector2> emptyList = new List<Vector2>();
        emptyList.Add(new Vector2(0, 0));
        emptyList.Add(new Vector2(0, 1));
        emptyList.Add(new Vector2(0, 5));
        emptyList.Add(new Vector2(0, 6));
        emptyList.Add(new Vector2(2, 1));
        emptyList.Add(new Vector2(2, 2));
        emptyList.Add(new Vector2(2, 5));
        emptyList.Add(new Vector2(4, 3));
        emptyList.Add(new Vector2(4, 5));
        emptyList.Add(new Vector2(6, 1));
        emptyList.Add(new Vector2(6, 2));
        emptyList.Add(new Vector2(6, 5));
        emptyList.Add(new Vector2(8, 0));
        emptyList.Add(new Vector2(8, 1));
        emptyList.Add(new Vector2(8, 5));
        emptyList.Add(new Vector2(8, 6));
        int index = 0;
        Vector2 v2 = emptyList[index];
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 7; j++)
            {
                if(v2.x == i && v2.y == j)
                {
                    index++;
                    if(index < emptyList.Count)
                        v2 = emptyList[index];
                }
                else
                {
                    CreateAndAddGrid(i, j);
                }
            }
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

    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
