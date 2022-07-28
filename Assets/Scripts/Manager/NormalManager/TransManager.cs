using UnityEngine;
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

    /// <summary>
    /// 求两个向量的夹角（取值范围0~360)
    /// </summary>
    /// <param name="from_"></param>
    /// <param name="to_"></param>
    /// <returns></returns>
    public static float Angle_360(Vector3 from_, Vector3 to_)
    {
        Vector3 v3 = Vector3.Cross(from_, to_); //叉乘判断正方向

        if (v3.z > 0)
            return Vector3.Angle(from_, to_);
        else
            return 360 - Vector3.Angle(from_, to_);
    }


    /// <summary>
    /// 求两个向量的夹角（取值范围0~2PI)
    /// </summary>
    /// <param name="from_"></param>
    /// <param name="to_"></param>
    /// <returns></returns>
    public static float Angle_2PI(Vector3 from_, Vector3 to_)
    {
        return Angle_360(from_, to_)*Mathf.PI/180;
    }

    /// <summary>
    /// 2D游戏世界坐标值转图片像素坐标值
    /// </summary>
    /// <returns></returns>
    public static float WorldToTex(float f)
    {
        return f * 100;
    }

    /// <summary>
    /// 图片像素坐标值转2D游戏世界坐标值
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static float TexToWorld(float f)
    {
        return f / 100;
    }
}
