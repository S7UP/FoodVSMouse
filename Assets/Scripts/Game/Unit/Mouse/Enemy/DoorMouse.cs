using S7P.Numeric;
/// <summary>
/// ������
/// </summary>
public class DoorMouse : MouseUnit
{
    public override void MInit()
    {
        base.MInit();
        // ���߱���Ч��
        NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreFrozen, new BoolModifier(true));
    }
}
