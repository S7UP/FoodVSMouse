/// <summary>
/// Ĩ��ׯ԰(��)
/// </summary>
public class Map_MatchaEstate : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 3-i; j <= 3+i; j++)
                CreateAndAddGrid(i, j);
        for (int i = 3; i < 6; i++)
            for (int j = 0; j < 7; j++)
                CreateAndAddGrid(i, j);
        for (int i = 6; i < 9; i++)
            for (int j = 3 - (8-i); j <= 3 + (8-i); j++)
                CreateAndAddGrid(i, j);

        //�ĸ���
        CreateAndAddGrid(0, 0);
        CreateAndAddGrid(8, 0);
        CreateAndAddGrid(8, 6);
        CreateAndAddGrid(0, 6);
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
