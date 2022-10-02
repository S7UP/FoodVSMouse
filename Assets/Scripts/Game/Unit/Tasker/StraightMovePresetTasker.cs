using UnityEngine;

/// <summary>
/// 匀变直线移动的预设任务
/// </summary>
public class StraightMovePresetTasker: PresetTasker
{
    private BaseBullet bullet_master;
    private BaseUnit unit_master;
    
    private float velocity;
    private float acc;
    private Vector3 rot;
    private int currentTimer;
    private int totalTimer;

    /// <summary>
    /// 已知初速，末速和运动时间的匀变速直线运动
    /// </summary>
    /// <param name="bullet_master"></param>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTasker(BaseBullet bullet_master, float v0, float v1, Vector3 rot, int t)
    {
        this.bullet_master = bullet_master;
        velocity = v0;
        acc = (v1 - v0) / (t-1);
        totalTimer = t;
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = BulletUpdate;
        EndCondition = BulletEndCond;
        EndEvent = null;
    }

    public StraightMovePresetTasker(BaseUnit unit_master, float v0, float v1, Vector3 rot, int t)
    {
        this.unit_master = unit_master;
        velocity = v0;
        acc = (v1 - v0) / (t - 1);
        totalTimer = t;
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = UnitUpdate;
        EndCondition = UnitEndCond;
        EndEvent = null;
    }

    /// <summary>
    /// 已知初速，末速和位移的匀变速直线运动
    /// </summary>
    /// <param name="bullet_master"></param>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="rot"></param>
    /// <param name="s"></param>
    public StraightMovePresetTasker(BaseBullet bullet_master, float v0, float v1, Vector3 rot, float s)
    {
        this.bullet_master = bullet_master;
        totalTimer = Mathf.CeilToInt(Mathf.Abs(2f * s / (v0 + v1)));
        velocity = v0;
        acc = (v1 - v0) / (totalTimer - 1);
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = BulletUpdate;
        EndCondition = BulletEndCond;
        EndEvent = null;
    }

    public StraightMovePresetTasker(BaseUnit unit_master, float v0, float v1, Vector3 rot, float s)
    {
        this.unit_master = unit_master;
        totalTimer = Mathf.CeilToInt(Mathf.Abs(2f * s / (v0 + v1)));
        velocity = v0;
        acc = (v1 - v0) / (totalTimer - 1);
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = UnitUpdate;
        EndCondition = UnitEndCond;
        EndEvent = null;
    }

    /// <summary>
    /// 已知速度、时间的匀速直线运动
    /// </summary>
    /// <param name="bullet_master"></param>
    /// <param name="v"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTasker(BaseBullet bullet_master, float v, Vector3 rot, int t)
    {
        this.bullet_master = bullet_master;
        velocity = v;
        acc = 0;
        totalTimer = t;
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = BulletUpdate;
        EndCondition = BulletEndCond;
        EndEvent = null;
    }

    public StraightMovePresetTasker(BaseUnit unit_master, float v, Vector3 rot, int t)
    {
        this.unit_master = unit_master;
        velocity = v;
        acc = 0;
        totalTimer = t;
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = UnitUpdate;
        EndCondition = UnitEndCond;
        EndEvent = null;
    }

    /// <summary>
    /// 已知初位置、末位置还有时间的匀减速至0的运动
    /// </summary>
    /// <param name="bullet_master"></param>
    /// <param name="s"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTasker(BaseBullet bullet_master, Vector3 pos, int t)
    {
        this.bullet_master = bullet_master;
        float v = (pos - bullet_master.transform.position).magnitude/t; // 平均速度
        velocity = 2*v;
        acc = -velocity / (t - 1);
        totalTimer = t;
        rot = (pos - bullet_master.transform.position).normalized;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = BulletUpdate;
        EndCondition = BulletEndCond;
        EndEvent = null;
    }

    public StraightMovePresetTasker(BaseUnit unit_master, Vector3 pos, int t)
    {
        this.unit_master = unit_master;
        float v = (pos - bullet_master.transform.position).magnitude / t; // 平均速度
        velocity = 2 * v;
        acc = -velocity / (t - 1);
        totalTimer = t;
        rot = (pos - bullet_master.transform.position).normalized;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = UnitUpdate;
        EndCondition = UnitEndCond;
        EndEvent = null;
    }

    private void UnitUpdate()
    {
        // unit_master.transform.position += rot * velocity;
        unit_master.SetPosition(unit_master.GetPosition() + rot * velocity);
        velocity += acc;
        currentTimer++;
    }

    private void BulletUpdate()
    {
        bullet_master.transform.position += rot * velocity;
        velocity += acc;
        currentTimer++;
    }

    private bool UnitEndCond()
    {
        return (currentTimer >= totalTimer || unit_master.isDeathState);
    }

    private bool BulletEndCond()
    {
        return (currentTimer >= totalTimer || bullet_master.isDeathState);
    }
}
