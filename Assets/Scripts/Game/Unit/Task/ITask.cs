/// <summary>
/// ����ӿڣ����ڶ����ϵģ�
/// </summary>
public interface ITask
{
    public void OnEnter();
    public void OnUpdate();
    public bool IsMeetingExitCondition();
    public void OnExit();
    public void ShutDown();
    public bool IsClearWhenDie();
}
