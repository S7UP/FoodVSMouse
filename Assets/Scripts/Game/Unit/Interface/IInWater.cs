/// <summary>
/// 单位下水行为接口
/// </summary>
public interface IInWater
{
    /// <summary>
    /// 下水时事件
    /// </summary>
    public void OnEnterWater();

    /// <summary>
    /// 停留在水中事件
    /// </summary>
    public void OnStayWater();

    /// <summary>
    /// 离开水时事件
    /// </summary>
    public void OnExitWater();
}
