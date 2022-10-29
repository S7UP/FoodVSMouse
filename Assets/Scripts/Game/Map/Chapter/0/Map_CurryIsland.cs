using UnityEngine;
/// <summary>
/// ��ଵ����գ�
/// </summary>
public class Map_CurryIsland : ChapterMap
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

    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }


    public override void OtherProcessing()
    {
        // �������
        for (int i = 1; i < 9; i+=2)
        {
            for (int j = 0; j < 7; j++)
            {
                FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(i), MapManager.GetRowY(j)));
                e.SetOpen();
                GameController.Instance.AddAreaEffectExecution(e);
            }
        }
    }
}
