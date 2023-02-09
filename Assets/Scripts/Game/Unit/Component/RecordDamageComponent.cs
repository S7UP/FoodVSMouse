/// <summary>
/// 伤害记录组件（内伤）
/// </summary>
public class RecordDamageComponent
{
    private const float TransRate = 1.0f; // 原伤害对消耗内伤 的 转化率
    private BaseUnit master;
    public float recordDamage; // 伤害记录值
    
    public RecordDamageComponent(BaseUnit master)
    {
        this.master = master;
    }

    /// <summary>
    /// 初始化，每次实例化时都要做
    /// </summary>
    public void Initilize()
    {
        recordDamage = 0;
    }

    /// <summary>
    /// 添加内伤
    /// </summary>
    public void AddRecordDamage(float value)
    {
        recordDamage += value;
    }

    /// <summary>
    /// 触发一次内伤
    /// </summary>
    public void TriggerRecordDamage(float dmg)
    {
        dmg = TransRate * dmg;
        if (recordDamage >= dmg)
        {
            recordDamage -= dmg; // 降低记录值
            new DamageAction(CombatAction.ActionType.RecordDamage, null, master, dmg).ApplyAction();
        }
        else if(recordDamage > 0)
        {
            new DamageAction(CombatAction.ActionType.RecordDamage, null, master, recordDamage).ApplyAction();
            recordDamage = 0;
        }
        
    }

}
