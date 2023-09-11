/// <summary>
/// 普通老鼠
/// </summary>
public class NormalMouse : MouseUnit
{
    public override void MInit()
    {
        base.MInit();
        // 机械鼠受到水蚀伤害翻倍
        if(mShape >= 6 && mShape <= 8)
        {
            Environment.WaterTask.AddUnitWaterRate(this, new S7P.Numeric.FloatModifier(2.0f));
        }
    }
}
