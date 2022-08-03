using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 游戏内的基础地图对象
/// </summary>
public class BaseMap : MonoBehaviour, IGameControllerMember
{
    // 全部的格子对象
    [HideInInspector]
    public List<BaseGrid> mGridList = new List<BaseGrid>();
    // 格子组
    [HideInInspector]
    public List<GridGroup> gridGroupList = new List<GridGroup>();
    public LayerMask mask; // 打开第9层，第9层为格子！

    // 地图
    [HideInInspector]
    public Vector3 mapCenter;
    [HideInInspector]
    public float mapWidth;
    [HideInInspector]
    public float mapHeight;
    // 格子
    [HideInInspector]
    public float gridWidth;
    [HideInInspector]
    public float gridHeight;
    // 行列
    public int yRow = 7;
    public int xColumn = 9;

    /////////////////////////////////////////////////////////此部分方法由子类重写，且一般子类实现只需要重写这部分方法就行了/////////////////////////////////////
    /// <summary>
    /// 创建格子
    /// </summary>
    public virtual void CreateGridList()
    {

    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public virtual void CreateGridGoupList()
    {

    }

    /// <summary>
    /// 对格子进行加工
    /// </summary>
    public virtual void ProcessingGridList()
    {

    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public virtual void ProcessingGridGroupList()
    {

    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void MInit()
    {
        mGridList.Clear();
        gridGroupList.Clear();

        CalculateSize();

        CreateGridList();
        CreateGridGoupList();

        ProcessingGridList();
        ProcessingGridGroupList();
    }

    /// <summary>
    /// 回收所有格子对象与格子组对象
    /// </summary>
    public void RecycleAllGridAndGroup()
    {
        foreach (var item in mGridList)
        {
            item.ExecuteRecycle();
        }
        mGridList.Clear();
        foreach (var item in gridGroupList)
        {
            item.ExecuteRecycle();
        }
        gridGroupList.Clear();
    }

    public void MUpdate()
    {
        // 格子管理
        Ray r = GameController.Instance.mCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(r.origin.x, r.origin.y), Vector2.zero, Mathf.Infinity, mask);
        GameController.Instance.overGrid = null;
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Grid")
            {
                GameController.Instance.overGrid = hit.collider.GetComponent<BaseGrid>();
            }
        }

        foreach (var item in mGridList)
        {
            item.MUpdate();
        }

        // 格子组事件
        foreach (var item in gridGroupList)
        {
            item.MUpdate();
        }
    }

    public void MPause()
    {
        
    }

    public void MPauseUpdate()
    {
        // 格子管理
        Ray r = GameController.Instance.mCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(r.origin.x, r.origin.y), Vector2.zero, Mathf.Infinity, mask);
        GameController.Instance.overGrid = null;
        if (hit.collider != null)
        {
            if (hit.collider.tag == "Grid")
            {
                GameController.Instance.overGrid = hit.collider.GetComponent<BaseGrid>();
            }
        }
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        
    }

    public BaseGrid GetGrid(int xIndex, int yIndex)
    {
        foreach (var item in mGridList)
        {
            if (item.GetColumnIndex() == xIndex && item.GetRowIndex() == yIndex)
                return item;
        }
        return null;
    }

    public List<BaseGrid> GetGridList()
    {
        return mGridList;
    }

    /// <summary>
    /// 生成一个格子并加入地图
    /// </summary>
    public void CreateAndAddGrid(int xIndex, int yIndex)
    {
        BaseGrid grid = BaseGrid.GetInstance();
        grid.MInit();
        grid.transform.SetParent(transform);
        grid.InitGrid(xIndex, yIndex);
        mGridList.Add(grid);
    }

    ////////////////////////////////////////////以下是私有方法//////////////////////////////////////////////////////
    // 计算地图格子宽高
    private void CalculateSize()
    {
        mapCenter = new Vector3(MapManager.CenterX, MapManager.CenterY, 0);
        gridWidth = MapManager.gridWidth;
        gridHeight = MapManager.gridHeight;
        mapWidth = gridWidth * xColumn;
        mapHeight = gridHeight * yRow;
    }
}
