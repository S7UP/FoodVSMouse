/// <summary>
/// 鼠国列车车头
/// </summary>
public class RatTrainHead : RatTrainComponent
{
    /// <summary>
    /// 转化为出现态时事件
    /// </summary>
    public override void OnTurnToAppear()
    {
        // 通知宿主对象将新的路径信息加入车厢管理器，以便分配给后续车厢
        GetMaster().AddNextRouteToManager();
    }

    /// <summary>
    /// 当抵达最后一个路径点时（这部分由车头重写）
    /// </summary>
    public override void OnOutOfBound()
    {
        // 当还有剩下路径时，则发生传送效果，否则通知BOSS本体表明已抵达目的地
        if (GetRouteQueue().Count > 0)
            base.OnOutOfBound();
        else
        {
            GetMaster().MarkMoveToDestination();
            GetMaster().SetActionState(new IdleState(GetMaster()));
        }
            
    }
}
