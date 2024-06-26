using S7P.Numeric;
/// <summary>
/// 无敌的障碍
/// </summary>
public class InvincibilityBarrier :　BaseBarrier 
{
    private int leftTime; // 持续时间

    public override void MInit()
    {
        leftTime = -1;
        base.MInit();
        // 添加无敌的标签
        NumericBox.AddDecideModifierToBoolDict(StringManager.Invincibility, new BoolModifier(true));
    }

    /// <summary>
    /// 设置存活时间
    /// </summary>
    public void SetLeftTime(int time)
    {
        leftTime = time;
    }

    public override void MUpdate()
    {
        base.MUpdate();
        // 剩余时间减到0则回收该对象，因此一开始如果设置成-1则代表永远不消失
        leftTime--;
        if (leftTime == 0)
        {
            ExecuteDeath();
        }
    }
}
