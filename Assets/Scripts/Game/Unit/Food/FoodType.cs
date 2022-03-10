using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 美食单位的分类（更像是职业系统）
/// </summary>
public enum FoodType
{
    Producer, // 生产类（产生火苗等资源）
    Shooter, // 射手类（直线攻击型）
    Aoe, // 群伤类（在攻击范围内所有敌对单位受到等额或差额伤害）
    Pitcher, // 投手类（投掷型攻击）
    Tracker, // 追踪类
    Bomb, // 炸弹类
    Support, // 辅助类（功能型如冰淇淋）
    Vehicle, // 载具类（于特殊地形承载卡片的卡片）
    Protect, // 防御类（需要单独占用一个格子的防御型卡片）
    Barrier // 屏障类（可以与非屏障类共存于一个格子的防御型卡片）
}
