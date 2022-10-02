/// <summary>
/// 美食在格子上的类型
/// </summary>
public enum FoodInGridType
{
    Default, // 默认的一类，不重叠
    Bomb, // 炸弹类
    WaterVehicle, // 水载具类，盘子
    LavaVehicle, // 空载类，棉花糖
    Shield, // 屏障类，如瓜皮
    NoAttach, // 非依附型，指判定不绑定在格子上，不占位也不参与格子-美食相关的判定
    Defence, // 防御型，面包
}
