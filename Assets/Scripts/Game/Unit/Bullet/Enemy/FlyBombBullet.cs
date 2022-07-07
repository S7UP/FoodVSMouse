using UnityEngine;

/// <summary>
/// ��ֱ��������+ˮƽ������ը��
/// </summary>
public class FlyBombBullet : BaseBullet
{
    private float acc;
    private float startX;
    private float targetX;
    private float targetY;
    private int currentRowIndex;
    private int currentTime;
    private int totalTime;

    public override void MInit()
    {
        base.MInit();
        acc = 0;
        startX = 0;
        targetX = 0;
        targetY = 0;
        currentRowIndex = -1;
        currentTime = 0;
        totalTime = 0;
    }

    public override void OnFlyState()
    {
        // ʱ�䵽�˾ͻᱬը
        if (currentTime >= totalTime)
        {
            TakeDamage(null);
        }
        // �Ƚ��㱾֡λ�Ƽ���
        base.OnFlyState();
        // �ٽ�����ٶ�
        mVelocity += acc;
        currentTime++;
        // ��������ͬ��
        transform.position = new Vector2(startX + (targetX - startX) * ((float)currentTime/totalTime), transform.position.y);
        // �����ض��߶�Ҳ���Ա�
        //if (transform.position.y < targetY)

    }

    /// <summary>
    /// ��ʼ���ٶ�
    /// </summary>
    /// <param name="v"></param>
    /// <param name="acc"></param>
    public void InitVelocity(float acc, float targetX, float targetY, int currentRowIndex)
    {
        totalTime = Mathf.CeilToInt(Mathf.Sqrt(2 * Mathf.Abs(targetY - transform.position.y) / acc));
        this.acc = acc;
        mVelocity += acc / 2; // �������õ�һ֡��ƽ���ٶȱ���Ϊv0 + 1/2*acc����Ϊλ��������ÿ֡����һ�ε�
        startX = transform.position.x;
        this.targetX = targetX;
        this.targetY = targetY;
        this.currentRowIndex = currentRowIndex;
    }

    /// <summary>
    /// ����Χ���AOE����Ч��
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // ԭ�ز���һ����ըЧ��
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
        BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
        bombEffect.Init(this.mMasterBaseUnit, 900, GetRowIndex(), 1, 1, 0, 0, true, false);
        //if (baseUnit != null && baseUnit.IsValid())
        //{
        //    // �����λ���ڣ����ڵ�λλ�ñ�ը
        //    bombEffect.transform.position = baseUnit.transform.position;
        //}
        //else
        //{
            // ����λ�ڸ��������ı�ը
            bombEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        //}
        GameController.Instance.AddAreaEffectExecution(bombEffect);

        KillThis();
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        return currentRowIndex;
    }
}
