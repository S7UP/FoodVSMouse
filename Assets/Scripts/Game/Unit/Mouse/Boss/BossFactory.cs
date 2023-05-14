using System.Collections.Generic;
using System;
/// <summary>
/// 在战斗场景中建造老鼠的工厂
/// </summary>
public class BossFactory : IGameControllerMember
{
    private static BossFactory _Instance;
    public static BossFactory Instance { get { if (_Instance == null) _Instance = new BossFactory(); return _Instance; } }

    private List<Action<BossUnit>> ProcessActionList = new List<Action<BossUnit>>();

    private BossFactory()
    {

    }

    public void MInit()
    {
        ProcessActionList.Clear();
    }

    public void MUpdate()
    {

    }

    public void MPause()
    {

    }

    public void MPauseUpdate()
    {

    }

    public void MResume()
    {

    }

    public void MDestory()
    {

    }

    public BossUnit GetBoss(int type, int shape, float hp)
    {
        BossUnit boss = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Boss/" + type + "/" + shape).GetComponent<BossUnit>();
        boss.mType = type;
        boss.mShape = shape;
        boss.MInit();
        boss.SetMaxHpAndCurrentHp(hp);
        boss.LoadSeedDict(); // 读取BOSS的种子表
        Process(boss);
        return boss;
    }

    private void Process(BossUnit u)
    {
        foreach (var action in ProcessActionList)
            action(u);
    }

    public void AddProcessAction(Action<BossUnit> action)
    {
        ProcessActionList.Add(action);
    }

    public void RemoveProcessAction(Action<BossUnit> action)
    {
        ProcessActionList.Remove(action);
    }
}
