/// <summary>
/// �޵е��ϰ�
/// </summary>
public class InvincibilityBarrier :��BaseBarrier 
{
    private int leftTime; // ����ʱ��

    public override void MInit()
    {
        leftTime = -1;
        base.MInit();
        // ����޵еı�ǩ
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
    }

    /// <summary>
    /// ���ô��ʱ��
    /// </summary>
    public void SetLeftTime(int time)
    {
        leftTime = time;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // ʣ��ʱ�����0����ոö������һ��ʼ������ó�-1�������Զ����ʧ
        leftTime--;
        if (leftTime == 0)
        {
            ExecuteDeath();
        }
    }
}
