using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �����飬���ƶ�����Ӱ��ж��߼�
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
