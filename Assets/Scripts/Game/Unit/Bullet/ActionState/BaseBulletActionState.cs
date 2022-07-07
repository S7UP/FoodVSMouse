public class BaseBulletActionState : IBaseActionState
{
    public BaseBullet mBaseBullet;

    public BaseBulletActionState(BaseBullet baseBullet)
    {
        mBaseBullet = baseBullet;
    }

    // ������ʱ
    public virtual void OnEnter()
    {

    }

    // ���˳�ʱ
    public virtual void OnExit()
    {

    }

    // ʵ�ֶ���״̬
    public virtual void OnUpdate()
    {

    }
    public void OnInterrupt()
    {
        
    }
    public void OnContinue()
    {
        
    }
}
