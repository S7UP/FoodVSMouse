using UnityEngine;

/// <summary>
/// 竖直加速下落+水平匀速型炸弹
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
        // 时间到了就会爆炸
        if (currentTime >= totalTime)
        {
            TakeDamage(null);
        }
        // 先结算本帧位移计算
        base.OnFlyState();
        // 再结算加速度
        mVelocity += acc;
        currentTime++;
        // 横向坐标同步
        transform.position = new Vector2(startX + (targetX - startX) * ((float)currentTime/totalTime), transform.position.y);
        // 低于特定高度也会自爆
        //if (transform.position.y < targetY)

    }

    /// <summary>
    /// 初始化速度
    /// </summary>
    /// <param name="v"></param>
    /// <param name="acc"></param>
    public void InitVelocity(float acc, float targetX, float targetY, int currentRowIndex)
    {
        totalTime = Mathf.CeilToInt(Mathf.Sqrt(2 * Mathf.Abs(targetY - transform.position.y) / acc));
        this.acc = acc;
        mVelocity += acc / 2; // 这里是让第一帧的平均速度保持为v0 + 1/2*acc，因为位移增加是每帧结算一次的
        startX = transform.position.x;
        this.targetX = targetX;
        this.targetY = targetY;
        this.currentRowIndex = currentRowIndex;
    }

    /// <summary>
    /// 对周围造成AOE爆破效果
    /// </summary>
    /// <param name="baseUnit"></param>
    public override void TakeDamage(BaseUnit baseUnit)
    {
        // 原地产生一个爆炸效果
        GameObject instance = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/BombAreaEffect");
        BombAreaEffectExecution bombEffect = instance.GetComponent<BombAreaEffectExecution>();
        bombEffect.Init(this.mMasterBaseUnit, 900, GetRowIndex(), 1, 1, 0, 0, true, false);
        //if (baseUnit != null && baseUnit.IsValid())
        //{
        //    // 如果单位存在，则在单位位置爆炸
        //    bombEffect.transform.position = baseUnit.transform.position;
        //}
        //else
        //{
            // 否则位于格子正中心爆炸
            bombEffect.transform.position = MapManager.GetGridLocalPosition(GetColumnIndex(), GetRowIndex());
        //}
        GameController.Instance.AddAreaEffectExecution(bombEffect);

        KillThis();
    }

    /// <summary>
    /// 获取当前单位所在行下标
    /// </summary>
    /// <returns></returns>
    public override int GetRowIndex()
    {
        return currentRowIndex;
    }
}
