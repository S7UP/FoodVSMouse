/// <summary>
/// ���ɺ�̲(ҹ)
/// </summary>
public class Map_MintBeach2 : ChapterMap
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
        // �������ϰ���
        BaseBarrier b = GameController.Instance.CreateItem(2, 0, (int)ItemInGridType.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
        b = GameController.Instance.CreateItem(1, 3, (int)ItemInGridType.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
        b = GameController.Instance.CreateItem(8, 3, (int)ItemInGridType.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
        b = GameController.Instance.CreateItem(5, 6, (int)ItemInGridType.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
