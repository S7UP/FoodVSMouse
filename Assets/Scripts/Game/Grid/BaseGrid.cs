using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGrid : MonoBehaviour
{
    public GridIndex gridIndex;

    public BaseGridState mMainGridState; // ��Ҫ����״̬
    public List<BaseGridState> mOtherGridStateList; // ��������״̬���������ʱ�ģ�
    public bool canBuild; // �Ƿ���Խ���
    public List<BaseUnit> mUnitList; // λ�ڸ����ϵ���ʳ��λ


    //��������
    [System.Serializable]
    public struct GridIndex
    {
        public int xIndex;
        public int yIndex;
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
        unit.SetPosition(MapManager.GetGridPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x* MapManager.gridWidth, pos.y*MapManager.gridHeight));
    }
}
