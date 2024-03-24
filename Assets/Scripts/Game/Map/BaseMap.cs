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

    private float symY = 0; // y������ͬ��Ŀ��y���������
    private Dictionary<BaseUnit, List<GridGroup>> unit_gridGroupMapDict = new Dictionary<BaseUnit, List<GridGroup>>(); // ��λ-��<�Ӵ�>�ĸ��ӵ�ӳ���ֵ�
    //private class GridGroupInfo
    //{
    //    public List<BaseUnit> unitList = new List<BaseUnit>();
    //    public List<BaseUnit> canUpdateYList = new List<BaseUnit>();
    //    public float deltaY = 0;
    //}
    //private Dictionary<GridGroup, GridGroupInfo> groupInfoDict = new Dictionary<GridGroup, GridGroupInfo>();

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

    /// <summary>
    /// �����ӹ�
    /// </summary>
    public virtual void OtherProcessing()
    {

    }

    /// <summary>
    /// ÿ֡���½����󸽴������¼�
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
        // groupInfoDict.Clear();

        CalculateSize();

        CreateGridList();
        CreateGridGoupList();

        ProcessingGridList();
        ProcessingGridGroupList();

        OtherProcessing();

        // ������󣬶����и�������Ӽ���
        {
            foreach (var group in gridGroupList)
            {
                //GridGroupInfo info = new GridGroupInfo();
                //groupInfoDict.Add(group, info);

                // ��������
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
                // �����¼�
                group.AddOnUnitEnterAction((u) => {
                    if (!unit_gridGroupMapDict.ContainsKey(u))
                        unit_gridGroupMapDict.Add(u, new List<GridGroup>());
                    unit_gridGroupMapDict[u].Add(group);
                });
                // �˳��¼�
                group.AddOnUnitExitAction((u) => {
                    if (unit_gridGroupMapDict.ContainsKey(u))
                        unit_gridGroupMapDict[u].Remove(group);
                    
                    //info.unitList.Remove(u);
                    //info.canUpdateYList.Remove(u);
                });
            }
        }
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
            item.MUpdate();

        // �������¼�
        foreach (var item in gridGroupList)
            item.MUpdate();

        // ���µ�λ-������ӳ���ϵ
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var keyValuePair in unit_gridGroupMapDict)
        {
            BaseUnit u = keyValuePair.Key;
            List<GridGroup> groupList = keyValuePair.Value;
            if (!u.IsAlive() || groupList.Count <= 0)
                delList.Add(u);
            else
            {
                // ȡ���ȽӴ��ĸ����飨Ҳ���ǵ�һλ����Ϊ<λ��>�ж�����ִ�ж�Ӧ��ͬ��x�����߼�
                GridGroup group = groupList[0];

                //if(groupInfoDict.ContainsKey(group) && !groupInfoDict[group].unitList.Contains(u) && !groupInfoDict[group].canUpdateYList.Contains(u))
                //    groupInfoDict[group].unitList.Add(u);
                u.transform.position += group.GetDeltaPos().x * Vector3.right;
                // �����������������Ŀ������ĸ��ӣ�Ȼ����ͼ��y������ͬ��y����
                //BaseGrid g = null;
                //float min = float.MaxValue;
                //foreach (var _g in group.GetGridList())
                //{
                //    float dist = (u.transform.position - _g.transform.position).magnitude;
                //    if(dist < min)
                //    {
                //        g = _g;
                //        min = dist;
                //    }
                //}
                //if (g != null)
                //{
                //    float deltaY = g.transform.position.y - u.transform.position.y;
                //    float sign = Mathf.Sign(deltaY);
                //    u.transform.position += new Vector3(0, sign * Mathf.Min(symY, Mathf.Abs(deltaY)));
                //}
            }
        }
        foreach (var u in delList)
        {
            unit_gridGroupMapDict.Remove(u);
        }


        //// ���¸�����-��λ��yͬ����ϵ
        //foreach (var keyValuePair in groupInfoDict)
        //{
        //    GridGroup group = keyValuePair.Key;
        //    GridGroupInfo info = keyValuePair.Value;

        //    foreach (var u in info.unitList)
        //        u.transform.position += group.GetDeltaPos().x * Vector3.right;

        //    foreach (var u in info.canUpdateYList)
        //        u.transform.position += group.GetDeltaPos().y * Vector3.up;

        //    info.deltaY += group.GetDeltaPos().y;
        //    if(Mathf.Abs(info.deltaY) >= MapManager.gridHeight || group.GetDeltaPos().y == 0)
        //    {
        //        info.deltaY -= Mathf.Sign(info.deltaY)*MapManager.gridHeight;
        //        foreach (var u in info.unitList)
        //        {
        //            u.transform.position += info.deltaY * Vector3.up;
        //            info.canUpdateYList.Add(u);
        //        }
        //        info.unitList.Clear();
        //        info.deltaY = 0;
        //    }
        //}

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
    /// �������и��Ӷ�������������
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
