/// <summary>
/// ��ͨ����
/// </summary>
public class NormalMouse : MouseUnit
{
    public override void MInit()
    {
        base.MInit();
        // ��е���ܵ�ˮʴ�˺�����
        if(mShape >= 6 && mShape <= 8)
        {
            Environment.WaterTask.AddUnitWaterRate(this, new S7P.Numeric.FloatModifier(2.0f));
        }
    }
}
