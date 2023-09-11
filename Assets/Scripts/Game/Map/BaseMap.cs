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

    private float symY = 0; // y方向上同步目标y坐标的速率
    private Dictionary<BaseUnit, List<GridGroup>> unit_gridGroupMapDict = new Dictionary<BaseUnit, List<GridGroup>>(); // 单位-所<接触>的格子的映射字典

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

    /// <summary>
    /// 其他加工
    /// </summary>
    public virtual void OtherProcessing()
    {

    }

    /// <summary>
    /// 每帧更新结束后附带更新事件
    /// </summary>
    public virtual void AfterUpdate()
    {

    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    public void MInit()
    {
        symY = MapManager.gridHeight/240;
        mGridList.Clear();
        gridGroupList.Clear();
        unit_gridGroupMapDict.Clear();

        CalculateSize();

        CreateGridList();
        CreateGridGoupList();

        ProcessingGridList();
        ProcessingGridGroupList();

        OtherProcessing();

        // 放在最后，对所有格子组添加监听
        {
            foreach (var group in gridGroupList)
            {
                // 进入条件
                group.AddOnUnitEnterCondiFunc((u) => {
                    if (u.GetHeight() != 0)
                        return false;
                    if(u is MouseUnit)
                    {
                        MouseUnit m = u as MouseUnit;
                        return !m.IsBoss();
                    }
                    return false;
                });
                // 进入事件
                group.AddOnUnitEnterAction((u) => {
                    if (!unit_gridGroupMapDict.ContainsKey(u))
                        unit_gridGroupMapDict.Add(u, new List<GridGroup>());
                    unit_gridGroupMapDict[u].Add(group);
                });
                // 退出事件
                group.AddOnUnitExitAction((u) => {
                    if (unit_gridGroupMapDict.ContainsKey(u))
                        unit_gridGroupMapDict[u].Remove(group);
                });

                // 为所有格子添加一个范围检测
                foreach (var g in group.GetGridList())
                {
                    RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(g.transform.position, new Vector2(0.75f*MapManager.gridWidth, 0.5f*MapManager.gridHeight), "ItemCollideEnemy");
                    r.isAffectMouse = true;
                    r.SetAffectHeight(0);
                    r.SetOnEnemyEnterAction((u) => {
                        group.TryEnter(u);
                    });
                    r.SetOnEnemyExitAction((u) => {
                        group.TryExit(u);
                    });
                    GameController.Instance.AddAreaEffectExecution(r);

                    // 跟随任务
                    CustomizationTask t = new CustomizationTask();
                    t.AddTaskFunc(delegate {
                        r.transform.position = g.transform.position;
                        return !g.IsAlive();
                    });
                    t.AddOnExitAction(delegate {
                        r.MDestory();
                    });
                    r.taskController.AddTask(t);
                }
            }
        }
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
            item.MUpdate();

        // 格子组事件
        foreach (var item in gridGroupList)
            item.MUpdate();

        // 更新单位-格子组映射关系
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var keyValuePair in unit_gridGroupMapDict)
        {
            BaseUnit u = keyValuePair.Key;
            List<GridGroup> groupList = keyValuePair.Value;
            if (!u.IsAlive() || groupList.Count <= 0)
                delList.Add(u);
            else
            {
                // 取最先接触的格子组（也就是第一位）作为<位于>判定，并执行对应的同步逻辑
                GridGroup group = groupList[0];
                u.transform.position += (Vector3)group.GetDeltaPos();
                // 在这个格子组里找离目标最近的格子，然后试图在y方向上同步y坐标
                BaseGrid g = null;
                float min = float.MaxValue;
                foreach (var _g in group.GetGridList())
                {
                    float dist = (u.transform.position - _g.transform.position).magnitude;
                    if(dist < min)
                    {
                        g = _g;
                        min = dist;
                    }
                }
                if (g != null)
                {
                    float deltaY = g.transform.position.y - u.transform.position.y;
                    float sign = Mathf.Sign(deltaY);
                    u.transform.position += new Vector3(0, sign * Mathf.Min(symY, Mathf.Abs(deltaY)));
                }
            }
        }
        foreach (var u in delList)
        {
            unit_gridGroupMapDict.Remove(u);
        }

        AfterUpdate();
    }

    public void MPause()
    {
        foreach (var item in mGridList)
        {
            item.MPause();
        }
        foreach (var item in gridGroupList)
        {
            item.MPause();
        }
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
        foreach (var item in mGridList)
        {
            item.MResume();
        }
        foreach (var item in gridGroupList)
        {
            item.MResume();
        }
    }

    public void MDestory()
    {
        
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
        foreach (var group in gridGroupList)
        {
            group.MDestory();
        }
        gridGroupList.Clear();
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
