using UnityEngine;
/// <summary>
/// ��ଵ�(ҹ)
/// </summary>
public class Map_CurryIsland2 : ChapterMap
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
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 9; j++)
                GetGrid(j, i).AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
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
        {
            //ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
            //GameController.Instance.AddAreaEffectExecution(e);
        }


        // �������
        // ½��
        for (int i = 6; i > 0; i -= 4)
        {
            for (int j = 0; j <= 5; j+=5)
            {
                // 2*2����
                for (int k = 0; k < 2; k++)
                {
                    for (int l = 0; l < 2; l++)
                    {
                        FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(i+k), MapManager.GetRowY(j+l)));
                        e.SetOpen();
                        GameController.Instance.AddAreaEffectExecution(e);
                    }
                }
            }
        }
        // ˮ��
        for (int i = 0; i < 7; i += 4)
        {
            // 2*3����
            for (int j = 0; j < 2; j ++)
            {
                for (int k = 2; k < 5; k++)
                {
                    FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(i + j), MapManager.GetRowY(k)));
                    e.SetOpen();
                    GameController.Instance.AddAreaEffectExecution(e);
                }
            }
        }
    }
}
