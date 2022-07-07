using UnityEngine;

/// <summary>
/// 目标受灰烬效果死亡时，触发时已是duringDeath
/// </summary>
public class BurnState : BaseActionState
{
    int totalTime = ConfigManager.fps * 2; // 动画持续时间（帧），数字为秒
    int currentTime = 0;

    public BurnState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnBurnStateEnter();
    }

    public override void OnUpdate()
    {
        currentTime++;
        mBaseUnit.DuringBurn((float)currentTime/totalTime);
    }

    public override void OnExit()
    {
        // 你觉得离开死亡意味着什么？复活？
        // 不，事实上，永远也无法达到离开死亡的真实！因此在正常流程下不应该调用到这里
        Debug.LogWarning("警告：有对象从死亡状态转换到其他状态了！！！");
    }

    public override void OnInterrupt()
    {

    }

    public override void OnContinue()
    {

    }
}
