/// <summary>
/// �����洢��Ϸ���õľ�̬��ֵ
/// </summary>
public class NumberManager
{
    public static float[] enemyHpRateByDifficult = new float[] { 0.4f, 0.6f, 0.8f, 1.0f };

    /// <summary>
    /// ��ȡ��ǰ�Ѷ��µз�Ѫ������
    /// </summary>
    /// <returns></returns>
    public static float GetCurrentEnemyHpRate()
    {
        return enemyHpRateByDifficult[PlayerData.GetInstance().GetDifficult()];
    }
}
