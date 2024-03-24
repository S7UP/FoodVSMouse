using System;
using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �����飬���ƶ�����Ӱ��ж��߼�
/// </summary>
public class GridGroup : MonoBehaviour, IGameControllerMember
{
    public List<BaseGrid> gridList;
    private Vector2 lastPos; // ��֡��λ��
    private Vector2 deltaPos; // ����֡��ȵ�λ��

    private Dictionary<BaseUnit, int> unitDict = new Dictionary<BaseUnit, int>();
    private List<Func<BaseUnit, bool>> OnUnitEnterCondiFuncList = new List<Func<BaseUnit, bool>>();
    private List<Action<BaseUnit>> OnUnitEnterActionList = new List<Action<BaseUnit>>();
    private List<Action<BaseUnit>> OnUnitExitActionList = new List<Action<BaseUnit>>();

    public virtual void Awake()
    {

    }

    public virtual void MInit()
    {
        unitDict.Clear();
        OnUnitEnterActionList.Clear();
        OnUnitEnterCondiFuncList.Clear();
        OnUnitExitActionList.Clear();
        lastPos = transform.position;
        deltaPos = Vector2.zero;
    }

    public virtual void MUpdate()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var keyValuePair in unitDict)
        {
            BaseUnit u = keyValuePair.Key;
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            unitDict.Remove(u);
        }

        deltaPos = (Vector2)transform.position - lastPos;
        lastPos = transform.position;
    }

    public void MPause()
    {
        
    }

    public void MResume()
    {
       
    }

    public void MDestory()
    {
        ExecuteRecycle();
    }

    public void MPauseUpdate()
    {
        
    }



    #region ���������ķ���
    /// <summary>
    /// ��������Ӹ���
    /// </summary>
    public void Add(BaseGrid grid)
    {
        gridList.Add(grid);
        grid.transform.SetParent(transform);
    }

    /// <summary>
    /// ת���ض����ӵ���һ������
    /// </summary>
    /// <param name="grid"></param>
    public void TransferGridToOtherGroup(BaseGrid grid, GridGroup otherGroup)
    {
        gridList.Remove(grid);
        otherGroup.Add(grid);
    }

    /// <summary>
    /// ����λ��
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    /// <summary>
    /// ��ȡ����֡��ȵ�λ��
    /// </summary>
    public Vector2 GetDeltaPos()
    {
        return deltaPos;
    }

    public List<BaseGrid> GetGridList()
    {
        return gridList;
    }

    public void TryEnter(BaseUnit u)
    {
        foreach (var func in OnUnitEnterCondiFuncList)
        {
            if (!func(u))
                return;
        }
        if (!unitDict.ContainsKey(u))
        {
            unitDict.Add(u, 1);
            foreach (var action in OnUnitEnterActionList)
                action(u);
        }
        else
        {
            unitDict[u] += 1; // ������1
        }
    }

    public void TryExit(BaseUnit u)
    {
        if (unitDict.ContainsKey(u))
        {
            unitDict[u] -= 1; // ������1
            // ��0��Ҫ�Ƴ����˳���
            if(unitDict[u] <= 0)
            {
                unitDict.Remove(u);
                foreach (var action in OnUnitExitActionList)
                    action(u);
            }        
        }
    }

    public bool IsAlive()
    {
        return isActiveAndEnabled;
    }
    #endregion

    #region �¼�����
    public void AddOnUnitEnterCondiFunc(Func<BaseUnit, bool> func)
    {
        OnUnitEnterCondiFuncList.Add(func);
    }

    public void RemoveOnUnitEnterCondiFunc(Func<BaseUnit, bool> func)
    {
        OnUnitEnterCondiFuncList.Remove(func);
    }

    public void AddOnUnitEnterAction(Action<BaseUnit> action)
    {
        OnUnitEnterActionList.Add(action);
    }

    public void RemoveOnUnitEnterAction(Action<BaseUnit> action)
    {
        OnUnitEnterActionList.Remove(action);
    }

    public void AddOnUnitExitAction(Action<BaseUnit> action)
    {
        OnUnitExitActionList.Add(action);
    }

    public void RemoveOnUnitExitAction(Action<BaseUnit> action)
    {
        OnUnitExitActionList.Remove(action);
    }
    #endregion

    #region ˽�з������ܱ����ķ���
    protected virtual void ExecuteRecycle()
    {

    }
    #endregion


}
