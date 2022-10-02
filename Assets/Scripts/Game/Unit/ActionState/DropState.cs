using UnityEngine;
/// <summary>
/// 摔落状态（死亡）
/// </summary>
public class DropState : BaseActionState
{
    int totalTime = 120; // 动画持续时间（帧），数字为秒
    int currentTime = 0;

    public DropState(BaseUnit baseUnit) : base(baseUnit)
    {
    }

    public override void OnEnter()
    {
        mBaseUnit.OnDropStateEnter();
    }

    public override void OnUpdate()
    {
        currentTime++;
        mBaseUnit.OnDropState((float)currentTime / totalTime);
    }

    public override void OnExit()
    {
        mBaseUnit.OnDropStateExit();
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
