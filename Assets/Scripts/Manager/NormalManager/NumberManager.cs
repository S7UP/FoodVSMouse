/// <summary>
/// 用来存储游戏常用的静态数值
/// </summary>
public class NumberManager
{
    public static float[] enemyHpRateByDifficult = new float[] { 0.4f, 0.6f, 0.8f, 1.0f };

    /// <summary>
    /// 获取当前难度下敌方血量补正
    /// </summary>
    /// <returns></returns>
    public static float GetCurrentEnemyHpRate()
    {
        return enemyHpRateByDifficult[PlayerData.GetInstance().GetDifficult()];
    }
}
