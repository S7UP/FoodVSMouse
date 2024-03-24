using S7P.Numeric;
/// <summary>
/// 房东鼠
/// </summary>
public class DoorMouse : MouseUnit
{
    public override void MInit()
    {
        base.MInit();
        // 免疫冰冻效果
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
    }
}
