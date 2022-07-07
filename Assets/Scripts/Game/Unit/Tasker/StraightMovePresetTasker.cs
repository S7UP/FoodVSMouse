using UnityEngine;

/// <summary>
/// 匀变直线移动的预设任务
/// </summary>
public class StraightMovePresetTasker: PresetTasker
{
    private BaseBullet master;
    private float velocity;
    private float acc;
    private Vector3 rot;
    private int currentTimer;
    private int totalTimer;

    /// <summary>
    /// 已知初速，末速和运动时间的匀变速直线运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTasker(BaseBullet master, float v0, float v1, Vector3 rot, int t)
    {
        this.master = master;
        velocity = v0;
        acc = (v1 - v0) / (t-1);
        totalTimer = t;
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    /// <summary>
    /// 已知初速，末速和位移的匀变速直线运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="rot"></param>
    /// <param name="s"></param>
    public StraightMovePresetTasker(BaseBullet master, float v0, float v1, Vector3 rot, float s)
    {
        this.master = master;
        totalTimer = Mathf.CeilToInt(Mathf.Abs(2f * s / (v0 + v1)));
        velocity = v0;
        acc = (v1 - v0) / (totalTimer - 1);
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    /// <summary>
    /// 已知速度、时间的匀速直线运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="v"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTasker(BaseBullet master, float v, Vector3 rot, int t)
    {
        this.master = master;
        velocity = v;
        acc = 0;
        totalTimer = t;
        this.rot = rot;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    /// <summary>
    /// 已知初位置、末位置还有时间的匀减速至0的运动
    /// </summary>
    /// <param name="master"></param>
    /// <param name="s"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTasker(BaseBullet master, Vector3 pos, int t)
    {
        this.master = master;
        float v = (pos - master.transform.position).magnitude/t; // 平均速度
        velocity = 2*v;
        acc = -velocity / (t - 1);
        totalTimer = t;
        rot = (pos - master.transform.position).normalized;

        // 填写事件预设
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    private void Update()
    {
        master.transform.position += rot * velocity;
        velocity += acc;
        currentTimer++;
    }

    private bool EndCond()
    {
        return (currentTimer >= totalTimer || master.isDeathState);
    }
}
