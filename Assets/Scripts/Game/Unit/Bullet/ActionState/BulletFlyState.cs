public class BulletFlyState : BaseBulletActionState
{
    public BulletFlyState(BaseBullet baseBullet):base(baseBullet)
    {
    }

    // 当进入时
    public override void OnEnter()
    {
        mBaseBullet.OnFlyStateEnter();
    }

    // 实现动作状态
    public override void OnUpdate()
    {
        mBaseBullet.OnFlyState();
    }

    // 当退出时
    public override void OnExit()
    {
        mBaseBullet.OnFlyStateExit();
    }
}
