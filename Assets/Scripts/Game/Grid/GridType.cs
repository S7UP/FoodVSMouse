/// <summary>
/// 地图格子种类
/// </summary>
public enum GridType
{
    Default = -1, // 默认，即什么地形也没有时为这个
    NotBuilt = 0, // 不允许放置卡片
    Water = 1, // 水地形格子
    Lava = 2, // 岩浆地形格子
    Sky = 3, // 高空格子
    Teleport = 4, // 传送格子
}
