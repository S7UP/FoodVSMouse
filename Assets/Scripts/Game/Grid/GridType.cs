/// <summary>
/// 地图格子种类
/// </summary>
public enum GridType
{
    None = 0, // 空，等于无格子，不允许放置卡片
    Default = 1, // 普通格子
    Water = 2, // 水地形格子
    Lava = 3, // 岩浆地形格子
}
