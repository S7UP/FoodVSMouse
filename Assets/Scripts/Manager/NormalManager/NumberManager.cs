/// <summary>
/// 用来存储游戏常用的静态数值
/// </summary>
public class NumberManager
{
    private static float[] attackRateByDiff = new float[] { 0.5f, 0.7f, 0.87f, 1.00f };
    private static float[] attackSpeedRateByDiff = new float[] { 0.5f, 0.7f, 0.87f, 1.00f };
    private static float[] moveSpeedByDiff = new float[] { 0.7f, 0.8f, 0.9f, 1.00f };
    private static float[] skillSpeedRateByDiff = new float[] { 0.5f, 0.6f, 0.75f, 1.00f };


    public static float GetEnemyAttackRate()
    {
        return attackRateByDiff[PlayerData.GetInstance().GetDifficult()];
    }

    public static float GetEnemyAttackSpeedRate()
    {
        return attackSpeedRateByDiff[PlayerData.GetInstance().GetDifficult()];
    }

    public static float GetEnemyMoveSpeedRate()
    {
        return moveSpeedByDiff[PlayerData.GetInstance().GetDifficult()];
    }

    public static float GetEnemySkillSpeedRate()
    {
        return skillSpeedRateByDiff[PlayerData.GetInstance().GetDifficult()];
    }
}
