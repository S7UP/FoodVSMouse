/// <summary>
/// 普通老鼠
/// </summary>
public class NormalMouse : MouseUnit
{
    /// <summary>
    /// 以下几个机械鼠防爆
    /// </summary>
    public override void MInit()
    {
        base.MInit();
        if(mShape>=6 && mShape <= 8)
        {
            NumericBox.AddDecideModifierToBoolDict(StringManager.IgnoreBombInstantKill, new BoolModifier(true));
        }
    }
}
