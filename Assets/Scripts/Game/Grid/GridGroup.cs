using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 格子组，控制多个格子绑定行动逻辑
/// </summary>
public class GridGroup : MonoBehaviour, IGameControllerMember
{
    public List<BaseGrid> gridList;

    public virtual void Awake()
    {

    }

    public virtual void MInit()
    {
        
    }

    public virtual void MUpdate()
    {
        
    }

    /// <summary>
    /// 往自身添加格子
    /// </summary>
    public void Add(BaseGrid grid)
    {
        gridList.Add(grid);
        grid.transform.SetParent(transform);
    }

    /// <summary>
    /// 转移特定格子到另一个组上
    /// </summary>
    /// <param name="grid"></param>
    public void TransferGridToOtherGroup(BaseGrid grid, GridGroup otherGroup)
    {
        gridList.Remove(grid);
        otherGroup.Add(grid);
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="position"></param>
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
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

    public void MPauseUpdate()
    {
        
    }

    public virtual void ExecuteRecycle()
    {

    }
}
