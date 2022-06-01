using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    public GridIndex gridIndex;

    public BaseGridState mMainGridState; // ��Ҫ����״̬
    public List<BaseGridState> mOtherGridStateList; // ��������״̬���������ʱ�ģ�

    protected List<FoodUnit> mFoodUnitList; // λ�ڸ����ϵ���ʳ��λ��
    protected List<MouseUnit> mMouseUnitList; //��λ�ڸ����ϵ�����λ��


    //��������
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
    /// �ɸ���ʵ�����������ⲿ������������������ÿ�����Ӷ���������
    /// </summary>
    /// <param name="xColumn"></param>
    /// <param name="yRow"></param>
    public void InitGrid(int xColumn, int yRow)
    {
        gridIndex.xIndex = xColumn;
        gridIndex.yIndex = yRow;
        transform.localPosition = MapManager.GetGridLocalPosition(xColumn, yRow);
    }

    // ����״̬����
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

    // ��һ����λ�����������ڸ����ϣ��ڶ�������Ϊ��λ�������������ɵ�λ�ڸø��ӵ��ĸ��ߣ������� pos = Vector2.right������������λ���������Ϊ�ø��ӵ��ұ���
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        //unit.SetPosition(MapManager.GetGridLocalPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x* MapManager.gridWidth, pos.y*MapManager.gridHeight)/2);
        unit.transform.position = MapManager.GetGridLocalPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }

    // �����ͣʱ���
    private void OnMouseOver()
    {
        // Debug.Log("��ǰ�����ͣ�ڸ����ϣ�xIndex= " + gridIndex.xIndex + ", yIndex = " + gridIndex.yIndex) ;
        GameController.Instance.overGrid = this;
    }

    /// <summary>
    /// ����ʳ��λ�����ڸø�����
    /// </summary>
    public void SetFoodUnitInGrid(FoodUnit foodUnit)
    {
        mFoodUnitList.Add(foodUnit);
        if (foodUnit.isUseSingleGrid)
        {
            foodUnit.SetGrid(this);
            SetUnitPosition(foodUnit, Vector2.zero); // ����ҲҪͬ��������������
        }
        else
        {
            foodUnit.GetGridList().Add(this);
            // ���ʱ�������ô����
        }
    }

    /// <summary>
    /// ������λ�����ڸø����ϣ���������λ��˵����֡���ܲ�����rigibody�����ֻ��ǿ����transform
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="pos"></param>
    public void SetMouseUnitInGrid(MouseUnit unit, Vector2 pos)
    {
        unit.transform.position = MapManager.GetGridLocalPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x * MapManager.gridWidth, pos.y * MapManager.gridHeight) / 2;
    }

    /// <summary>
    /// ��ȡ�����ϵ�������ʳ
    /// </summary>
    public virtual List<FoodUnit> GetFoodUnitList()
    {
        return mFoodUnitList;
    }

    /// <summary>
    /// ��ȡ�����ϵ���������
    /// </summary>
    public virtual List<MouseUnit> GetMouseUnitList()
    {
        return mMouseUnitList;
    }

    // �˳�ʱȡ�����
    private void OnMouseExit()
    {
        GameController.Instance.overGrid = null;
    }

    /// <summary>
    /// ��ײ�¼�
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

    // rigibody���
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
        // ��鵥λ���
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
