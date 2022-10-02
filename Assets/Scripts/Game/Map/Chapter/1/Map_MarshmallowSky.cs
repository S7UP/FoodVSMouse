
using UnityEngine;
/// <summary>
/// �޻�����գ��գ�
/// </summary>
public class Map_MarshmallowSky : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 9; i++)
            for (int j = 0; j < 7; j++)
                CreateAndAddGrid(i, j);
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

    
    public override void OtherProcessing()
    {
        {
            // ����Ʋ�
            for (int i = 0; i < 7; i++)
            {
                Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 8);
            }

            // ��ӷ���
            WindAreaEffectExecution e = WindAreaEffectExecution.GetInstance(9, 7, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(3)));
            WindAreaEffectExecution.SetClassicalWindAreaEffectMode(e, 0, 420, 120, 360, true); // �ȴ�ʱ�䡢�ٶȱ仯ʱ�䡢����ʱ��
            GameController.Instance.AddAreaEffectExecution(e);
        }
    }
}
