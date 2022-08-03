using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��Ϸ�ڵĻ�����ͼ����
/// </summary>
public class BaseMap : MonoBehaviour, IGameControllerMember
{
    // ȫ���ĸ��Ӷ���
    [HideInInspector]
    public List<BaseGrid> mGridList = new List<BaseGrid>();
    // ������
    [HideInInspector]
    public List<GridGroup> gridGroupList = new List<GridGroup>();
    public LayerMask mask; // �򿪵�9�㣬��9��Ϊ���ӣ�

    // ��ͼ
    [HideInInspector]
    public Vector3 mapCenter;
    [HideInInspector]
    public float mapWidth;
    [HideInInspector]
    public float mapHeight;
    // ����
    [HideInInspector]
    public float gridWidth;
    [HideInInspector]
    public float gridHeight;
    // ����
    public int yRow = 7;
    public int xColumn = 9;

    /////////////////////////////////////////////////////////�˲��ַ�����������д����һ������ʵ��ֻ��Ҫ��д�ⲿ�ַ���������/////////////////////////////////////
    /// <summary>
    /// ��������
    /// </summary>
    public virtual void CreateGridList()
    {

    }

    /// <summary>
    /// ����������
    /// </summary>
    public virtual void CreateGridGoupList()
    {

    }

    /// <summary>
    /// �Ը��ӽ��мӹ�
    /// </summary>
    public virtual void ProcessingGridList()
    {

    }

    /// <summary>
    /// �Ը�����ӹ�
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
    /// �������и��Ӷ�������������
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
        // ���ӹ���
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

        // �������¼�
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
        // ���ӹ���
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
    /// ����һ�����Ӳ������ͼ
    /// </summary>
    public void CreateAndAddGrid(int xIndex, int yIndex)
    {
        BaseGrid grid = BaseGrid.GetInstance();
        grid.MInit();
        grid.transform.SetParent(transform);
        grid.InitGrid(xIndex, yIndex);
        mGridList.Add(grid);
    }

    ////////////////////////////////////////////������˽�з���//////////////////////////////////////////////////////
    // �����ͼ���ӿ��
    private void CalculateSize()
    {
        mapCenter = new Vector3(MapManager.CenterX, MapManager.CenterY, 0);
        gridWidth = MapManager.gridWidth;
        gridHeight = MapManager.gridHeight;
        mapWidth = gridWidth * xColumn;
        mapHeight = gridHeight * yRow;
    }
}
