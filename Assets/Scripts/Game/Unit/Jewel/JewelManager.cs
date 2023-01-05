using System.Collections.Generic;
/// <summary>
/// ��ʯ����������װ��̬������
/// </summary>
public class JewelManager
{
    private static Dictionary<JewelNameTypeMap, string> JewelNameDict = new Dictionary<JewelNameTypeMap, string>() 
    {
        { JewelNameTypeMap.Ice, "������ʯ"},
        { JewelNameTypeMap.Sacred, "��ʥ��ʯ"},
        { JewelNameTypeMap.Bombing, "��ը��ʯ"},
        { JewelNameTypeMap.Laser, "���ⱦʯ"},
        { JewelNameTypeMap.CatEyes, "è�۱�ʯ"},
    };

    /// <summary>
    /// ��ȡȫ����ʯ�������ֵ�
    /// </summary>
    /// <returns></returns>
    public static Dictionary<JewelNameTypeMap, string> GetJewelNameDict()
    {
        return JewelNameDict;
    }
}
