using UnityEngine;
// 玩家的管理，负责保存以及加载各种玩家以及游戏的信息
public class PlayerManager
{
    public const int version = 1; // 当前游戏玩家应该的存档版本号，每次启动游戏时检测玩家的存档版本号，若低于该值，则启用玩家存档更新功能 
    public const int maxLevel = 15; // 当前最大等级
    
    
}
