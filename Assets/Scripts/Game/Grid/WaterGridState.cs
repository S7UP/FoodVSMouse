/// <summary>
/// 水地形状态
/// </summary>
public class WaterGridState : BaseGridState
{
    public WaterGridState(BaseGrid grid):base(grid)
    { 

    }

    public override void OnEnter()
    {
        // 切换瞬间记得给所有美食和老鼠施加对应进入效果
        foreach (var item in mGrid.GetFoodUnitList())
        {
            OnUnitEnter(item);
        }
        foreach (var item in mGrid.GetMouseUnitList())
        {
            OnUnitEnter(item);
        }
    }

    public override void OnExit()
    {
        // 移除瞬间记得给所有美食和老鼠取消对应进入效果
        foreach (var item in mGrid.GetFoodUnitList())
        {
            OnUnitExit(item);
        }
        foreach (var item in mGrid.GetMouseUnitList())
        {
            OnUnitExit(item);
        }
    }

    public override void OnUpdate()
    {
        // 不需要，因为持续效果已经在buff中生效了
    }

    // 当有单位进入地形时施加给单位的效果
    public override void OnUnitEnter(BaseUnit baseUnit)
    {
        baseUnit.AddUniqueStatusAbility(StringManager.WaterGridState, new WaterStatusAbility(baseUnit));
    }

    // 当有单位处于地形时持续给单位的效果
    public override void OnUnitStay(BaseUnit baseUnit)
    {
        // 不需要，因为持续效果已经在buff中生效了
    }

    // 当有单位离开地形时施加给单位的效果
    public override void OnUnitExit(BaseUnit baseUnit)
    {
        baseUnit.RemoveUniqueStatusAbility(StringManager.WaterGridState);
    }
}
