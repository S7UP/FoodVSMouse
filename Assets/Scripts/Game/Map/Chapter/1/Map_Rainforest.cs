/// <summary>
/// ̿������(��)
/// </summary>
public class Map_Rainforest : ChapterMap
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
        GetGrid(1, 0).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(1, 1).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(6, 1).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(4, 3).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(1, 5).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(6, 5).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
