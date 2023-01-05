using UnityEngine;
/// <summary>
/// �޻�����գ�ҹ��
/// </summary>
public class Map_MarshmallowSky2 : ChapterMap
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
        for (int i = 2; i < 7; i++)
            for (int j = 0; j < 7; j++)
                GetGrid(i, j).AddGridType(GridType.Sky, BaseGridType.GetInstance(GridType.Sky, 0));
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
        {
            // Ϊȫͼ��Ӻ�ҹBUFF
            ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
            GameController.Instance.AddAreaEffectExecution(e);
        }

        {
            // ����Ʋ�
            for (int i = 0; i < 7; i++)
            {
                Item_Cloud.GetCloudGroup(1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 8);
            }

            // ��ӷ���
            WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(8.6f, 7, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(3)));
            WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 0, 360, 120, 360, true); // �ȴ�ʱ�䡢�ٶȱ仯ʱ�䡢����ʱ��
            GameController.Instance.AddAreaEffectExecution(e);
        }
    }
}
