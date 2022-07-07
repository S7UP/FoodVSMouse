using System.Collections.Generic;

using UnityEngine;

public class MapController : MonoBehaviour, IGameControllerMember
{
    private static MapController _instance;
    public Transform masterTrans;
    public Transform GridGroupTrans;
    public LayerMask mask = 1 << 9; // 打开第9层，第9层为格子！

    // 引用
    public static MapController Instance { get => _instance; } //+ 自身单例

    /// <summary>
    /// 地图信息
    /// </summary>
    public struct MapInfo
    {
        public GridType[,] gridList;
    }

    private bool drawLine = true;
    // 地图有关属性
    // 地图
    private Vector3 mapCenter;
    private float mapWidth; //
    private float mapHeight;
    // 格子
    public float gridWidth;
    public float gridHeight;

    // 全部的格子对象
    public List<BaseGrid> mGridList = new List<BaseGrid>();
    // 格子组
    public List<GridGroup> gridGroupList = new List<GridGroup>();

    // 行列
    public const int yRow = 7;
    public const int xColumn = 9;

    private void Awake()
    {
        _instance = this;
        masterTrans = transform.Find("GridList").transform;
        GridGroupTrans = transform.Find("GridGroupList").transform;
    }


    // 计算地图格子宽高
    private void CalculateSize()
    {
        mapCenter = new Vector3(MapManager.CenterX, MapManager.CenterY, 0);
        gridWidth = MapManager.gridWidth;
        gridHeight = MapManager.gridHeight;
        mapWidth = gridWidth*xColumn;
        mapHeight = gridHeight*yRow;
    }


    // 画格子用于辅助设计
    // OnDrawGizmos()方法每当鼠标进入、点击Scene视图时就会调用一次
    private void OnDrawGizmos()
    {
        if (drawLine)
        {
            CalculateSize();
            Gizmos.color = Color.green;

            // 画行
            for (int y = 0; y <= yRow; y++)
            {
                Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                Vector3 endPos = mapCenter + new Vector3(mapWidth / 2, -mapHeight / 2 + y * gridHeight);
                Gizmos.DrawLine(startPos, endPos);
            }
            // 画列
            for (int x = 0; x <= xColumn; x++)
            {
                Vector3 startPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, mapHeight / 2);
                Vector3 endPos = mapCenter + new Vector3(-mapWidth / 2 + gridWidth * x, -mapHeight / 2);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
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

    /// <summary>
    /// 生成场地格子
    /// </summary>
    public void MInit()
    {
        mGridList.Clear();
        gridGroupList.Clear();

        CalculateSize();
        // 创建地图格子
        //CreateDefaultGridList();
        CreateTestGridList();
        // 创建格子组
        CreateTestGridGroupList();
    }

    public void MUpdate()
    {
        // 格子管理
        Ray r = GameController.Instance.mCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(r.origin.x, r.origin.y), Vector2.zero, Mathf.Infinity, mask);

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

    /// <summary>
    /// 暂停时依旧允许选取格子
    /// </summary>
    public void MPauseUpdate()
    {
        // 格子管理
        Ray r = GameController.Instance.mCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(new Vector2(r.origin.x, r.origin.y), Vector2.zero, Mathf.Infinity, mask);

        if (hit.collider != null)
        {
            if (hit.collider.tag == "Grid")
            {
                GameController.Instance.overGrid = hit.collider.GetComponent<BaseGrid>();
            }
        }
    }

    public void MPause()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 使用默认的方法创建地图格子
    /// </summary>
    public void CreateDefaultGridList()
    {
        for (int i = 0; i < MapController.yRow; i++)
        {
            for (int j = 0; j < MapController.xColumn; j++)
            {
                BaseGrid grid = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/Grid").GetComponent<BaseGrid>();
                grid.MInit();
                grid.transform.SetParent(masterTrans);
                grid.InitGrid(j, i);
                mGridList.Add(grid);
            }
        }
    }

    /// <summary>
    /// 测试创建格子
    /// </summary>
    public void CreateTestGridList()
    {
        for (int i = 1; i < 6; i++)
        {
            for (int j = 2; j < 7; j++)
            {
                BaseGrid grid = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/Grid").GetComponent<BaseGrid>();
                grid.MInit();
                grid.transform.SetParent(masterTrans);
                grid.InitGrid(j, i);
                mGridList.Add(grid);
            }
        }
    }

    /// <summary>
    /// 测试创建格子组
    /// </summary>
    public void CreateTestGridGroupList()
    {
        Vector2[,] v2List = new Vector2[,] 
        {
            { new Vector2(2, 1), new Vector2(3, 1), new Vector2(4, 1), new Vector2(5, 1), new Vector2(5, 2), new Vector2(5, 3)},
            { new Vector2(6, 1), new Vector2(6, 2), new Vector2(6, 3), new Vector2(6, 4), new Vector2(5, 4), new Vector2(4, 4)},
            { new Vector2(6, 5), new Vector2(5, 5), new Vector2(4, 5), new Vector2(3, 5), new Vector2(3, 4), new Vector2(3, 3)},
            { new Vector2(2, 5), new Vector2(2, 4), new Vector2(2, 3), new Vector2(2, 2), new Vector2(3, 2), new Vector2(4, 2)}
        };

        Vector2[] endPointList = new Vector2[]
        {
            mapCenter + new Vector3(0, gridHeight, 0),
            mapCenter + new Vector3(2*gridWidth, 0, 0),
            mapCenter + new Vector3(0, -gridHeight, 0),
            mapCenter + new Vector3(-2*gridWidth, 0, 0)
        };

        Vector2[] offsetList = new Vector2[]
        {
            new Vector2(-gridWidth/2, gridHeight),
            new Vector2(gridWidth, gridHeight/2),
            new Vector2(gridWidth/2, -gridHeight),
            new Vector2(-gridWidth, -gridHeight/2)
        };

        for (int i = 0; i < 4; i++)
        {
            MovementGridGroup gridGroup = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/MovementGridGroup").GetComponent<MovementGridGroup>();
            // 设置中心点坐标
            gridGroup.transform.position = mapCenter;
            // 加入对应的格子
            for (int j = 0; j < 6; j++)
            {
                gridGroup.Add(GetGrid(((int)v2List[i, j].x), ((int)v2List[i, j].y)));
            }
            // 设置并启用移动版块
            bool filp = false;
            if (i > 0 && i < 3)
            {
                filp = true;
            }
            gridGroup.StartMovement(
                new List<MovementGridGroup.PointInfo>() { 
                    new MovementGridGroup.PointInfo()
                    { 
                        targetPosition = mapCenter, 
                        moveTime = 360, 
                        strandedTime = 960 
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endPointList[i],
                        moveTime = 360,
                        strandedTime = 960
                    }
                }, 
                GameManager.Instance.GetSprite("Map/1/"+(2-(i%2))),
                offsetList[i],
                filp, filp);
            gridGroupList.Add(gridGroup);
            gridGroup.transform.SetParent(GridGroupTrans);
        }

    }

    public List<BaseGrid> GetGridList()
    {
        return mGridList;
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
}
