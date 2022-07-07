using UnityEngine;

/// <summary>
/// �ȱ�ֱ���ƶ���Ԥ������
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
    /// ��֪���٣�ĩ�ٺ��˶�ʱ����ȱ���ֱ���˶�
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

        // ��д�¼�Ԥ��
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    /// <summary>
    /// ��֪���٣�ĩ�ٺ�λ�Ƶ��ȱ���ֱ���˶�
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

        // ��д�¼�Ԥ��
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    /// <summary>
    /// ��֪�ٶȡ�ʱ�������ֱ���˶�
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

        // ��д�¼�Ԥ��
        InitAction = null;
        UpdateAciton = Update;
        EndCondition = EndCond;
        EndEvent = null;
    }

    /// <summary>
    /// ��֪��λ�á�ĩλ�û���ʱ����ȼ�����0���˶�
    /// </summary>
    /// <param name="master"></param>
    /// <param name="s"></param>
    /// <param name="rot"></param>
    /// <param name="t"></param>
    public StraightMovePresetTasker(BaseBullet master, Vector3 pos, int t)
    {
        this.master = master;
        float v = (pos - master.transform.position).magnitude/t; // ƽ���ٶ�
        velocity = 2*v;
        acc = -velocity / (t - 1);
        totalTimer = t;
        rot = (pos - master.transform.position).normalized;

        // ��д�¼�Ԥ��
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
