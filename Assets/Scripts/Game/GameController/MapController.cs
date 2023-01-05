using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��ͼ������
/// </summary>
public class MapController : MonoBehaviour, IGameControllerMember
{
    // ����
    public const int yRow = 7;
    public const int xColumn = 9;

    public float gridWidth { get { return currentMap.gridWidth; } }
    public float gridHeight { get { return currentMap.gridHeight; } }

    public BaseMap currentMap; // ��ǰ��ͼ

    /// <summary>
    /// �������и��Ӷ�������������
    /// </summary>
    public void RecycleAllGridAndGroup()
    {
        if(currentMap!=null)
            currentMap.RecycleAllGridAndGroup();
    }

    /// <summary>
    /// ���ɳ��ظ���
    /// </summary>
    public void MInit()
    {
        if (currentMap == null)
        {
            BaseStage.StageInfo v = GameManager.Instance.playerData.GetCurrentStageInfo();
            currentMap = ChapterMap.GetInstance(v.chapterIndex, v.sceneIndex);
            currentMap.MInit();
        }
        else
        {
            currentMap.MInit();
        }
    }

    public void MUpdate()
    {
        currentMap.MUpdate();
    }

    /// <summary>
    /// ��ͣʱ��������ѡȡ����
    /// </summary>
    public void MPauseUpdate()
    {
        currentMap.MPauseUpdate();
    }

    public void MPause()
    {
        currentMap.MPause();
    }

    public void MResume()
    {
        currentMap.MResume();
    }

    public void MDestory()
    {
        currentMap.MDestory();
    }

    public List<BaseGrid> GetGridList()
    {
        return currentMap.GetGridList();
    }

    public BaseGrid GetGrid(int xIndex, int yIndex)
    {
        return currentMap.GetGrid(xIndex, yIndex);
    }

    //private void Awake()
    //{
    //    //_instance = this;
    //    //masterTrans = transform.Find("GridList").transform;
    //    //GridGroupTrans = transform.Find("GridGroupList").transform;
    //}


    //// �����ͼ���ӿ��
    //private void CalculateSize()
    //{
    //    mapCenter = new Vector3(MapManager.CenterX, MapManager.CenterY, 0);
    //    gridWidth = MapManager.gridWidth;
    //    gridHeight = MapManager.gridHeight;
    //    mapWidth = gridWidth*xColumn;
    //    mapHeight = gridHeight*yRow;
    //}


    //// ���������ڸ������
    //// OnDrawGizmos()����ÿ�������롢���Scene��ͼʱ�ͻ����һ��
    //private void OnDrawGizmos()
    //{
    //    if (drawLine)
    //    {
    //        CalculateSize();
    //        Gizmos.color = Color.green;

    //        // ����
    //        for (int y = 0; y <= yRow; y++)
    //        {
    //            Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2, -mapHeight / 2 + y * gridHeight);
    //            Vector3 endPos = mapCenter + new Vector3(mapWidth / 2, -mapHeight / 2 + y * gridHeight);
    //            Gizmos.DrawLine(startPos, endPos);
    //        }
    //        // ����
    //        for (int x = 0; x <= xColumn; x++)
    //        {
    //            Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, mapHeight / 2);
    //            Vector3 endPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, -mapHeight / 2);
    //            Gizmos.DrawLine(startPos, endPos);
    //        }
    //    }
    //}
}
