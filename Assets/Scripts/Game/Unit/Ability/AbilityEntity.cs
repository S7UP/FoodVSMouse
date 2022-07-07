/// <summary>
/// 能力实体，存储着某个单位某个能力的数据和状态
/// </summary>
public abstract class AbilityEntity
{
    public string name; // 名
    public BaseUnit master { get; set; }

    public AbilityEntity()
    {

    }

    public AbilityEntity(BaseUnit pmaster)
    {
        master = pmaster;
    }


    public virtual void Init()
    {

    }

    public virtual BaseUnit GetMaster()
    {
        return master;
    }

    //尝试激活能力
    public void TryActivateAbility()
    {
        if (CanActive())
        {
            ActivateAbility();
        }
    }

    //激活能力
    public virtual void ActivateAbility()
    {

    }

    /// <summary>
    /// 每帧随着主人更新
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// 手动尝试停止能力
    /// </summary>
    public void TryEndActivate()
    {
        EndActivate();
    }

    //结束能力
    public virtual void EndActivate()
    {
        
    }

    /// <summary>
    /// 是否达到了激活的条件
    /// </summary>
    /// <returns></returns>
    public virtual bool CanActive()
    {
        return true;
    }

    //创建能力执行体
    public virtual AbilityExecution CreateAbilityExecution()
    {
        return null;
    }

    //应用能力效果
    public virtual void ApplyAbilityEffect(BaseUnit targetEntity)
    {
        //应用能力效果
    }
}
