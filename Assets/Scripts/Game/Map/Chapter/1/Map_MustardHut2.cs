/// <summary>
/// ��ĩС��(ҹ)
/// </summary>
public class Map_MustardHut2 : ChapterMap
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
        // ����ˮ����
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
        // �����ϰ�
        BaseBarrier b = GameController.Instance.CreateItem(6, 1, (int)ItemInGridType.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
        b.HideEffect(true);
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
