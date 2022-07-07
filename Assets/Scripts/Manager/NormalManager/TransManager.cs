/// <summary>
/// ��λת��������
/// </summary>
public class TransManager
{
    /// <summary>
    /// Ԥ�� ÿ6������1��Ϊ1����λ�ı�׼�ƶ��ٶ� �������Ϊ��׼�ƶ��ٶȣ���������Ϊÿ֡�ߵ�unity���굥λ�ƶ��ٶ�
    /// </summary>
    /// <param name="standardVelocity">��׼�ƶ��ٶ�</param>
    /// <returns></returns>
    public static float TranToVelocity(float standardVelocity)
    {
        return standardVelocity * MapManager.gridWidth / ConfigManager.fps / 6;
    }

    /// <summary>
    /// ��unity���굥λ�ƶ��ٶ�תΪ��׼�ƶ��ٶ�
    /// </summary>
    /// <param name="velocity"></param>
    /// <returns></returns>
    public static float TranToStandardVelocity(float velocity)
    {
        return velocity * 6 * ConfigManager.fps / MapManager.gridWidth;
    }
}
