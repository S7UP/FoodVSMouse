/// <summary>
/// ̿������(ҹ)
/// </summary>
public class Map_Rainforest2 : ChapterMap
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
        GetGrid(2, 0).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(5, 2).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(2, 3).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(5, 3).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(1, 5).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        GetGrid(4, 6).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
