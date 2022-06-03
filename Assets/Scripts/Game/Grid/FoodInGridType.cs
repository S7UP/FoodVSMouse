using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 美食在格子上的类型
/// </summary>
public enum FoodInGridType
{
    Default, // 默认的一类，不重叠
    WaterVehicle, // 水载具类，盘子
    FloatVehicel, // 空载类，棉花糖
    Shield, // 屏障类，如瓜皮
    NoAttach, // 非依附型，指判定不绑定在格子上，不占位也不参与格子-美食相关的判定 如咖啡粉，冰淇淋
}
