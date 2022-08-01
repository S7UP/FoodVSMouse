using System.Collections.Generic;
/// <summary>
/// 老鼠管理器（静态存储老鼠相关信息）
/// </summary>
public class MouseManager
{
    private static Dictionary<MouseNameTypeMap, string> mouseTypeNameDict = new Dictionary<MouseNameTypeMap, string>() {
        { MouseNameTypeMap.NormalMouse, "基础鼠兵"},
        { MouseNameTypeMap.KangarooMouse, "袋鼠类"},
    //  { MouseNameTypeMap.DoorMouse, "正面防御类"},
        { MouseNameTypeMap.LadderMouse, "梯子类"},
        { MouseNameTypeMap.HealMouse, "加血类"},
        { MouseNameTypeMap.FlyMouse, "基础飞行类"},
        { MouseNameTypeMap.FlyBarrierMouse, "飞行路障鼠"},
        { MouseNameTypeMap.FlySelfDestructMouse, "空中自爆型"},
        { MouseNameTypeMap.AerialBombardmentMouse, "空中投弹型"},
        { MouseNameTypeMap.AirTransportMouse, "空中航母" },
    //{ MouseNameTypeMap.NormalWaterMouse, "基础水军"},
    //{ MouseNameTypeMap.SubmarineMouse, "潜水艇类"},
    //{ MouseNameTypeMap.RowboatMouse, "划艇类"},
    //{ MouseNameTypeMap.PounceMouse, "突袭类"},
        { MouseNameTypeMap.CatapultMouse, "投石车类"},
    //{ MouseNameTypeMap.Mole, "鼹鼠类"},
        { MouseNameTypeMap.PenguinMouse, "企鹅类"},
        { MouseNameTypeMap.ArsonMouse,  "纵火类"},
        { MouseNameTypeMap.PandaMouse, "熊猫类"},
        { MouseNameTypeMap.MagicMirrorMouse, "魔镜类" },
    //{ MouseNameTypeMap.SecondMasterMouse, "二爷类"},
        { MouseNameTypeMap.NinjaMouse, "忍者首领类" },
        { MouseNameTypeMap.NinjaRetinueMouse, "忍者随从类"},
        { MouseNameTypeMap.PandaRetinueMouse, "熊猫随从类"},
    //{ MouseNameTypeMap.ParatrooperMouse, "伞兵类"},
        { MouseNameTypeMap.SnailMouse, "蜗牛车"},
        { MouseNameTypeMap.NonMainstreamMouse, "非主流"},
        { MouseNameTypeMap.BeeMouse, "机械蜂箱"},
        { MouseNameTypeMap.CanMouse, "罐头鼠"}

    };

    private static Dictionary<MouseNameTypeMap, Dictionary<int, string>> mouseShapeNameDict = new Dictionary<MouseNameTypeMap, Dictionary<int, string>>() {
        // 普通老鼠
        { MouseNameTypeMap.NormalMouse, new Dictionary<int, string>(){
            { 0, "平民鼠"},
            { 3, "黄瓜平民鼠"},
            { 6, "机械平民鼠"},
            { 7, "机械球迷鼠"},
            { 8, "机械铁锅鼠"},
            { 9, "滑轮鼠"},
        }},
        // 跳跳鼠
        { MouseNameTypeMap.KangarooMouse, new Dictionary<int, string>(){
            { 0, "跳跳鼠"},
            { 1, "腾空鼠"},
            { 2, "喷气鼠"},
        }},
        // 梯子类
        { MouseNameTypeMap.LadderMouse, new Dictionary<int, string>(){
            { 1, "弹簧鼠"},
        }},
        // 加血类
        { MouseNameTypeMap.HealMouse, new Dictionary<int, string>(){
            { 0, "魔笛鼠"},
            { 1, "祭祀鼠"},
            { 2, "教皇鼠"},
            { 3, "僵尸魔笛鼠"},
        }},
        // 基础飞行类
        { MouseNameTypeMap.FlyMouse, new Dictionary<int, string>(){
            { 0, "滑翔鼠"},
            { 1, "废品飞兵鼠"},
            { 2, "空爆鼠"},
            { 3, "僵尸滑翔鼠"},
            { 4, "僵尸废品飞兵鼠"},
            { 5, "僵尸空爆鼠"},
            { 6, "机械举旗鼠"},
        }},
        // 飞行路障鼠
        { MouseNameTypeMap.FlyBarrierMouse, new Dictionary<int, string>(){
            { 0, "飞行路障鼠"},
        }},
        // 空中自爆型
        { MouseNameTypeMap.FlySelfDestructMouse, new Dictionary<int, string>(){
            { 0, "神风滑翔鼠"},
        }},
        // 空中轰炸型
        { MouseNameTypeMap.AerialBombardmentMouse, new Dictionary<int, string>(){
            { 0, "机械投弹鼠"},
        }},
        // 空中运载型
        { MouseNameTypeMap.AirTransportMouse, new Dictionary<int, string>(){
            { 0, "空中航母"},
        }},
        // 投石车类
        { MouseNameTypeMap.CatapultMouse, new Dictionary<int, string>(){
            { 0, "工程车鼠"},
            { 1, "榛子炮鼠"},
            { 2, "地雷车鼠"},
        }},
        // 企鹅类
        { MouseNameTypeMap.PenguinMouse, new Dictionary<int, string>(){
            { 0, "企鹅鼠"},
            { 1, "僵尸企鹅鼠"},
        }},
        // 纵火类
        { MouseNameTypeMap.ArsonMouse, new Dictionary<int, string>(){
            { 0, "纵火鼠"},
            { 1, "僵尸纵火鼠"},
        }},
        // 熊猫类
        { MouseNameTypeMap.PandaMouse, new Dictionary<int, string>(){
            { 0, "熊猫鼠"},
            { 1, "摔角手鼠"},
            { 2, "特种兵鼠"},
        }},
        // 魔镜类
        { MouseNameTypeMap.MagicMirrorMouse, new Dictionary<int, string>(){
            { 0, "魔镜鼠"},
        }},
        // 忍者首领类
        { MouseNameTypeMap.NinjaMouse, new Dictionary<int, string>(){
            { 0, "忍者鼠首领"},
            { 1, "武士鼠"},
            { 2, "火影忍者鼠"},
        }},
        // 忍者鼠随从类
        { MouseNameTypeMap.NinjaRetinueMouse, new Dictionary<int, string>(){
            { 0, "忍者鼠随从"},
            { 1, "武士鼠随从"},
            { 2, "火影忍者鼠随从"},
        }},
        // 熊猫随从类
        { MouseNameTypeMap.PandaRetinueMouse, new Dictionary<int, string>(){
            { 0, "熊猫鼠随从"},
            { 1, "摔角手鼠随从"},
            { 2, "特种兵鼠随从"},
        }},
        // 蜗牛车鼠
        { MouseNameTypeMap.SnailMouse, new Dictionary<int, string>(){
            { 0, "蜗牛车鼠"},
            { 1, "僵尸蜗牛车鼠"},
        }},
        // 非主流鼠
        { MouseNameTypeMap.NonMainstreamMouse, new Dictionary<int, string>(){
            { 0, "非主流鼠"},
            { 1, "僵尸非主流鼠"},
        }},
        // 机械蜂箱
        { MouseNameTypeMap.BeeMouse, new Dictionary<int, string>(){
            { 0, "机械蜂箱鼠"},
        }},
        // 罐头鼠
        { MouseNameTypeMap.CanMouse, new Dictionary<int, string>(){
            { 0, "罐头鼠"},
            { 1, "僵尸罐头鼠"},
        }},
    };


    /// <summary>
    /// 获取所有老鼠分类的名称字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<MouseNameTypeMap, string> GetMouseTypeNameDict()
    {
        return mouseTypeNameDict;
    }

    /// <summary>
    /// 获取某个老鼠类的所有变种名称字典
    /// </summary>
    /// <returns></returns>
    public static Dictionary<int, string> GetShapeNameDict(MouseNameTypeMap type)
    {
        return mouseShapeNameDict[type];
    }
}
