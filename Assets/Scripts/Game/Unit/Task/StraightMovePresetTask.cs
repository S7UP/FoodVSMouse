using UnityEngine;

public class StraightMovePresetTask : BaseTask
{
    private Transform trans;
    private float velocity;
    private float acc;
    private Vector3 rot;
    private int currentTimer;
    private int totalTimer;

    /// <summary>
    /// 已知初速，末速和运动时间的匀变速直线运动
    /// </summary>
    /// <param name="trans"></param>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTask(Transform trans, float v0, float v1, Vector3 rot, int t)
    {
        this.trans = trans;
        velocity = v0;
        acc = (v1 - v0) / (t - 1);
        totalTimer = t;
        this.rot = rot;
    }

    /// <summary>
    /// 已知初速，末速和位移的匀变速直线运动
    /// </summary>
    /// <param name="bullet_master"></param>
    /// <param name="v0"></param>
    /// <param name="v1"></param>
    /// <param name="rot"></param>
    /// <param name="s"></param>
    public StraightMovePresetTask(Transform trans, float v0, float v1, Vector3 rot, float s)
    {
        this.trans = trans;
        totalTimer = Mathf.CeilToInt(Mathf.Abs(2f * s / (v0 + v1)));
        velocity = v0;
        acc = (v1 - v0) / (totalTimer - 1);
        this.rot = rot;
    }

    /// <summary>
    /// 已知速度、时间的匀速直线运动
    /// </summary>
    /// <param name="bullet_master"></param>
    /// <param name="v"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTask(Transform trans, float v, Vector3 rot, int t)
    {
        this.trans = trans;
        velocity = v;
        acc = 0;
        totalTimer = t;
        this.rot = rot;
    }

    /// <summary>
    /// 已知初位置、末位置还有时间的匀减速至0的运动
    /// </summary>
    /// <param name="bullet_master"></param>
    /// <param name="s"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTask(Transform trans, Vector3 pos, int t)
    {
        this.trans = trans;
        float v = (pos - trans.position).magnitude / t; // 平均速度
        velocity = 2 * v;
        acc = -velocity / (t - 1);
        totalTimer = t;
        rot = (pos - trans.position).normalized;
    }

    protected override void O_OnUpdate()
    {
        trans.position += rot * velocity;
        velocity += acc;
        currentTimer++;
    }

    protected override bool O_IsMeetingCondition()
    {
        return currentTimer >= totalTimer;
    }
}
