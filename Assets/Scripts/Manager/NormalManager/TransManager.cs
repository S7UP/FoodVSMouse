using UnityEngine;
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

    /// <summary>
    /// �����������ļнǣ�ȡֵ��Χ0~360)
    /// </summary>
    /// <param name="from_"></param>
    /// <param name="to_"></param>
    /// <returns></returns>
    public static float Angle_360(Vector3 from_, Vector3 to_)
    {
        Vector3 v3 = Vector3.Cross(from_, to_); //����ж�������

        if (v3.z > 0)
            return Vector3.Angle(from_, to_);
        else
            return 360 - Vector3.Angle(from_, to_);
    }


    /// <summary>
    /// �����������ļнǣ�ȡֵ��Χ0~2PI)
    /// </summary>
    /// <param name="from_"></param>
    /// <param name="to_"></param>
    /// <returns></returns>
    public static float Angle_2PI(Vector3 from_, Vector3 to_)
    {
        return Angle_360(from_, to_)*Mathf.PI/180;
    }

    /// <summary>
    /// 2D��Ϸ��������ֵתͼƬ��������ֵ
    /// </summary>
    /// <returns></returns>
    public static float WorldToTex(float f)
    {
        return f * 100;
    }

    /// <summary>
    /// ͼƬ��������ֵת2D��Ϸ��������ֵ
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    public static float TexToWorld(float f)
    {
        return f / 100;
    }
}
