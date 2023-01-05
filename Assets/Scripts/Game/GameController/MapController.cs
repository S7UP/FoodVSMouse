using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 地图管理器
/// </summary>
public class MapController : MonoBehaviour, IGameControllerMember
{
    // 行列
    public const int yRow = 7;
    public const int xColumn = 9;

    public float gridWidth { get { return currentMap.gridWidth; } }
    public float gridHeight { get { return currentMap.gridHeight; } }

    public BaseMap currentMap; // 当前地图

    /// <summary>
    /// 回收所有格子对象与格子组对象
    /// </summary>
    public void RecycleAllGridAndGroup()
    {
        if(currentMap!=null)
            currentMap.RecycleAllGridAndGroup();
    }

    /// <summary>
    /// 生成场地格子
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
    /// 暂停时依旧允许选取格子
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


    //// 计算地图格子宽高
    //private void CalculateSize()
    //{
    //    mapCenter = new Vector3(MapManager.CenterX, MapManager.CenterY, 0);
    //    gridWidth = MapManager.gridWidth;
    //    gridHeight = MapManager.gridHeight;
    //    mapWidth = gridWidth*xColumn;
    //    mapHeight = gridHeight*yRow;
    //}


    //// 画格子用于辅助设计
    //// OnDrawGizmos()方法每当鼠标进入、点击Scene视图时就会调用一次
    //private void OnDrawGizmos()
    //{
    //    if (drawLine)
    //    {
    //        CalculateSize();
    //        Gizmos.color = Color.green;

    //        // 画行
    //        for (int y = 0; y <= yRow; y++)
    //        {
    //            Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2, -mapHeight / 2 + y * gridHeight);
    //            Vector3 endPos = mapCenter + new Vector3(mapWidth / 2, -mapHeight / 2 + y * gridHeight);
    //            Gizmos.DrawLine(startPos, endPos);
    //        }
    //        // 画列
    //        for (int x = 0; x <= xColumn; x++)
    //        {
    //            Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, mapHeight / 2);
    //            Vector3 endPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, -mapHeight / 2);
    //            Gizmos.DrawLine(startPos, endPos);
    //        }
    //    }
    //}
}
