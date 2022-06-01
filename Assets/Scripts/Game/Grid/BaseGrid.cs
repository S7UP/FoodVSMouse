using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    public GridIndex gridIndex;

    public BaseGridState mMainGridState; // 主要地形状态
    public List<BaseGridState> mOtherGridStateList; // 其它地形状态表（大多是临时的）

    protected List<FoodUnit> mFoodUnitList; // 位于格子上的美食单位表
    protected List<MouseUnit> mMouseUnitList; //　位于格子上的老鼠单位表


    //格子索引
    [System.Serializable]
    public struct GridIndex
    {
        public int xIndex;
        public int yIndex;
        public bool canBuild;
    }

    private void Awake()
    {
        gridIndex = new GridIndex();
        mFoodUnitList = new List<FoodUnit>();
        mMouseUnitList = new List<MouseUnit>();
        mMainGridState = new BaseGridState(this);
        mOtherGridStateList = new List<BaseGridState>();
    }

    /// <summary>
    /// 由格子实例创建后在外部调用这个方法，分配给每个格子独立的坐标
    /// </summary>
    /// <param name="xColumn"></param>
    /// <param name="yRow"></param>
    public void InitGrid(int xColumn, int yRow)
    {
        gridIndex.xIndex = xColumn;
        gridIndex.yIndex = yRow;
        transform.localPosition = MapManager.GetGridLocalPosition(xColumn, yRow);
    }

    // 地形状态操作
    public void ChangeMainGridState(BaseGridState state)
    {
        if (mMainGridState != null)
        {
            mMainGridState.OnExit();
        }
        mMainGridState = state;
        state.OnEnter();
    }

    public void AddGridState(BaseGridState state)
    {
        mOtherGridStateList.Add(state);
        state.OnEnter();
    }

    public void RemoveGridState(BaseGridState state)
    {
        mOtherGridStateList.Remove(state);
        state.OnExit();
    }


    public void OnUnitEnter(BaseUnit unit)
    {
        mMainGridState.OnUnitEnter(unit);
        foreach (var item in mOtherGridStateList)
        {
            item.OnUnitEnter(unit);
        }
    }

    public void OnUnitExit(BaseUnit unit)
    {
        mMainGridState.OnUnitExit(unit);
        foreach (var item in mOtherGridStateList)
        {
            item.OnUnitExit(unit);
        }
    }

    // 把一个单位的坐标设置在格子上，第二个参数为单位向量，表明生成点位于该格子的哪个边，比如填 pos = Vector2.right，则代表这个单位的坐标会设为该格子的右边上
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        //unit.SetPosition(MapManager.GetGridLocalPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x* MapManager.gridWidth, pos.y*MapManager.gridHeight)/2);
        unit.transform.position = MapManager.GetGridLocalPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }

    // 鼠标悬停时标记
    private void OnMouseOver()
    {
        // Debug.Log("当前鼠标悬停在格子上：xIndex= " + gridIndex.xIndex + ", yIndex = " + gridIndex.yIndex) ;
        GameController.Instance.overGrid = this;
    }

    /// <summary>
    /// 将美食单位放置在该格子上
    /// </summary>
    public void SetFoodUnitInGrid(FoodUnit foodUnit)
    {
        mFoodUnitList.Add(foodUnit);
        if (foodUnit.isUseSingleGrid)
        {
            foodUnit.SetGrid(this);
            SetUnitPosition(foodUnit, Vector2.zero); // 坐标也要同步至自身正中心
        }
        else
        {
            foodUnit.GetGridList().Add(this);
            // 多格时坐标该怎么处理？
        }
    }

    /// <summary>
    /// 将老鼠单位放置在该格子上，对于老鼠单位来说，首帧不能操纵其rigibody，因此只能强改其transform
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    public void SetMouseUnitInGrid(MouseUnit unit, Vector2 pos)
    {
        unit.transform.position = MapManager.GetGridLocalPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }

    /// <summary>
    /// 获取本格上的所有美食
    /// </summary>
    public virtual List<FoodUnit> GetFoodUnitList()
    {
        return mFoodUnitList;
    }

    /// <summary>
    /// 获取本格上的所有老鼠
    /// </summary>
    public virtual List<MouseUnit> GetMouseUnitList()
    {
        return mMouseUnitList;
    }

    // 退出时取消标记
    private void OnMouseExit()
    {
        GameController.Instance.overGrid = null;
    }

    /// <summary>
    /// 碰撞事件
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollision(Collider2D collision)
    {
        if (collision.tag.Equals("Food"))
        {
            FoodUnit unit = collision.GetComponent<FoodUnit>();
            if (!unit.isDeathState && unit.GetRowIndex()==gridIndex.yIndex && !mFoodUnitList.Contains(unit))
            {
                Debug.Log("Insert Food Unit!");
                mFoodUnitList.Add(unit);
                OnUnitEnter(unit);
            }
        }else if (collision.tag.Equals("Mouse"))
        {
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            if (!unit.isDeathState && unit.GetRowIndex() == gridIndex.yIndex && !mMouseUnitList.Contains(unit))
            {
                //Debug.Log("Insert Mouse Unit!");
                mMouseUnitList.Add(unit);
                OnUnitEnter(unit);
            }
        }
    }

    // rigibody相关
    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food"))
        {
            Debug.Log("Remove Food Unit!");
            FoodUnit unit = collision.GetComponent<FoodUnit>();
            mFoodUnitList.Remove(unit);
            OnUnitExit(unit);
        }
        else if (collision.tag.Equals("Mouse"))
        {
            //Debug.Log("Remove Mouse Unit!");
            MouseUnit unit = collision.GetComponent<MouseUnit>();
            mMouseUnitList.Remove(unit);
            OnUnitExit(unit);
        }
    }

    public void MInit()
    {
        
    }

    public void MUpdate()
    {
        // 检查单位存活
        List<FoodUnit> f_List = new List<FoodUnit>();
        foreach (var item in mFoodUnitList)
        {
            if (item.isDeathState)
                f_List.Add(item);
        }
        foreach (var item in f_List)
        {
            mFoodUnitList.Remove(item);
        }

        List<MouseUnit> m_List = new List<MouseUnit>();
        foreach (var item in mMouseUnitList)
        {
            if (item.isDeathState)
                m_List.Add(item);
        }
        foreach (var item in m_List)
        {
            mMouseUnitList.Remove(item);
        }

        mMainGridState.OnUpdate();
        foreach (var item in mOtherGridStateList)
        {
            item.OnUpdate();
        }
    }

    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }
}
