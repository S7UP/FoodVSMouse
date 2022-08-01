public class BaseGridState
{
    // 持有该状态的格子
    public BaseGrid mGrid;
    public GridType gridType;

    public BaseGridState(BaseGrid grid)
    {
        mGrid = grid;
        gridType = GridType.Default;
    }

    // 地形切换为该状态瞬间需要做的事
    public virtual void OnEnter()
    {

    }

    // 地形从该状态切换到其他状态后需要做的事
    public virtual void OnExit() 
    {

    }

    // 地形持续效果
    public virtual void OnUpdate() 
    {
        
    }

    // 当有单位进入地形时施加给单位的效果
    public virtual void OnUnitEnter(BaseUnit baseUnit)
    {

    }

    // 当有单位处于地形时持续给单位的效果
    public virtual void OnUnitStay(BaseUnit baseUnit)
    {

    }

    // 当有单位离开地形时施加给单位的效果
    public virtual void OnUnitExit(BaseUnit baseUnit)
    {

    }
}
