using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseGrid : MonoBehaviour
{
    public GridIndex gridIndex;

    public BaseGridState mMainGridState; // 主要地形状态
    public List<BaseGridState> mOtherGridStateList; // 其它地形状态表（大多是临时的）
    public bool canBuild; // 是否可以建造
    public List<BaseUnit> mUnitList; // 位于格子上的美食单位


    //格子索引
    [System.Serializable]
    public struct GridIndex
    {
        public int xIndex;
        public int yIndex;
    }

    // 地形状态操作
    public void ChangeMainGridState(BaseGridState state)
    {

    }

    public void AddGridState(BaseGridState state)
    {

    }

    public void RemoveGridState(int index)
    {

    }

    // 把一个单位的坐标设置在格子上，第二个参数为单位向量，表明生成点位于该格子的哪个边，比如填 pos = Vector2.right，则代表这个单位的坐标会设为该格子的右边上
    public void SetUnitPosition(BaseUnit unit, Vector2 pos)
    {
        unit.SetPosition(MapManager.GetGridPosition(gridIndex.xIndex, gridIndex.yIndex) + new Vector3(pos.x* MapManager.gridWidth, pos.y*MapManager.gridHeight));
    }
}
