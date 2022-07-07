/// <summary>
/// 单位转化管理器
/// </summary>
public class TransManager
{
    /// <summary>
    /// 预设 每6秒走完1格为1个单位的标准移动速度 传入参数为标准移动速度，传出参数为每帧走的unity坐标单位移动速度
    /// </summary>
    /// <param name="standardVelocity">标准移动速度</param>
    /// <returns></returns>
    public static float TranToVelocity(float standardVelocity)
    {
        return standardVelocity * MapManager.gridWidth / ConfigManager.fps / 6;
    }

    /// <summary>
    /// 将unity坐标单位移动速度转为标准移动速度
    /// </summary>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public static float TranToStandardVelocity(float velocity)
    {
        return velocity * 6 * ConfigManager.fps / MapManager.gridWidth;
    }
}
