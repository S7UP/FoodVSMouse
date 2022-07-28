/// <summary>
/// 任务接口（挂在对象上的）
/// </summary>
public interface ITask
{
    public void OnEnter();
    public void OnUpdate();
    public bool IsMeetingExitCondition();
    public void OnExit();
}
