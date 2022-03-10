using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGrid : MonoBehaviour, IGameControllerMember
{
    public GridIndex gridIndex;

    public BaseGridState mMainGridState; // ��Ҫ����״̬
    public List<BaseGridState> mOtherGridStateList; // ��������״̬���������ʱ�ģ�

    protected List<FoodUnit> mFoodUnitList; // λ�ڸ����ϵ���ʳ��λ��


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

    }

    public void AddGridState(BaseGridState state)
    {

    }

    public void RemoveGridState(int index)
    {

    }

    // ��һ����λ�����������ڸ����ϣ��ڶ�������Ϊ��λ�������������ɵ�λ�ڸø��ӵ��ĸ��ߣ������� pos = Vector2.right������������λ���������Ϊ�ø��ӵ��ұ���
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        unit.SetPosition(MapManager.GetGridLocalPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x* MapManager.gridWidth, pos.y*MapManager.gridHeight)/2);
    }

    private void OnMouseDown()
    {
        Debug.Log("aaaa");
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


    // �˳�ʱȡ�����
    private void OnMouseExit()
    {
        GameController.Instance.overGrid = null;
    }

    public void MInit()
    {
        
    }

    public void MUpdate()
    {
        
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
