/// <summary>
/// ��ͨ����
/// </summary>
public class NormalMouse : MouseUnit
{
    /// <summary>
    /// ���¼�����е�����
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
