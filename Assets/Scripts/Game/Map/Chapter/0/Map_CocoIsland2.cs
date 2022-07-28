/// <summary>
/// �ɿɵ�(ҹ)
/// </summary>
public class Map_CocoIsland2 : ChapterMap
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
        for (int i = 0; i < 2; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).ChangeMainGridState(new WaterGridState(GetGrid(j, i)));
        for (int i = 5; i < 7; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).ChangeMainGridState(new WaterGridState(GetGrid(j, i)));
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }
}
