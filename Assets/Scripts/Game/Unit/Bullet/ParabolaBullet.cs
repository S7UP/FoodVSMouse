using UnityEngine;

/// <summary>
/// 抛物线移动式的子弹
/// </summary>
public class ParabolaBullet : BaseBullet
{
    // 抛物线子弹最好使用刚体
    public Rigidbody2D r2D;

    public Vector3 firstPosition; // 初始点
    public Vector3 targetPosition; // 目标点
    private int totalTimer; // 初始点到目标点用时
    private int currentTimer; // 当前用时
    private float g; // 加速度
    private float velocityVertical; // 垂直方向的速度
    private int currentRow; // 当前所在列
    private bool canAttackFood;
    private bool canAttackMouse;

    public override void Awake()
    {
        base.Awake();
        r2D = GetComponent<Rigidbody2D>();
    }

    public override void MInit()
    {
        base.MInit();
        currentTimer = 0;
        //CloseCollision();
    }


    // 注：在此处，mVelocity变为水平速度
    /// <summary>
    /// 设置必要的属性，否则无法正常执行
    /// </summary>
    /// <param name="standardVelocity">标准水平方向速度</param>
    /// <param name="isLeftPosition">水平方向是否为左方向</param>
    /// <param name="height">抛物线顶点相对高度</param>
    /// <param name="firstPosition">初始位置</param>
    /// <param name="targetPosition">目标位置</param>
    public void SetAttribute(float standardVelocity, bool isLeftPosition, float height, Vector3 firstPosition, Vector3 targetPosition, int currentRow)
    {
        this.currentRow = currentRow;
        SetStandardVelocity(standardVelocity);
        if (isLeftPosition)
        {
            SetRotate(Vector2.left);
        }
        else
        {
            SetRotate(Vector2.right);
        }
        this.firstPosition = firstPosition;
        this.targetPosition = targetPosition;
        transform.position = new Vector3(firstPosition.x, targetPosition.y, 0);
        totalTimer = Mathf.CeilToInt(Mathf.Abs(targetPosition.x - transform.position.x) / GetHorizontalVelocity());
        // 计算得出重力加速度（向下）
        g = 8*height / (totalTimer * totalTimer);
        // 为垂直方向的初速度赋值
        velocityVertical = g * totalTimer / 2;
    }

    public override void OnFlyState()
    {
        // 超时则自爆
        if(currentTimer > totalTimer)
        {
            TakeDamage(null);
        }
        velocityVertical -= g;
        Vector2 vx = Vector2.Lerp(firstPosition, targetPosition, (float)currentTimer/totalTimer);
        r2D.MovePosition(new Vector2(vx.x, transform.position.y) + Vector2.up * GetVerticalVelocity());
        currentTimer++;
    }

    /// <summary>
    /// 获取水平方向的速度
    /// </summary>
    /// <returns></returns>
    public float GetHorizontalVelocity()
    {
        return mVelocity;
    }

    /// <summary>
    /// 获取垂直方向的速度
    /// </summary>
    public float GetVerticalVelocity()
    {
        return velocityVertical;
    }

    /// <summary>
    /// 接受碰撞时
    /// </summary>
    /// <param name="collision"></param>
    public void OnCollsion(Collider2D collision)
    {
        if (collision.tag.Equals("Food"))
        {
            if (canAttackFood)
            {
                FoodUnit u = collision.GetComponent<FoodUnit>();
                if (u.GetRowIndex() == currentRow && UnitManager.CanBulletHit(u, this))
                {
                    TakeDamage(u);
                }
            }
        }
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollsion(collision);
    }

    public override void OnTriggerStay2D(Collider2D collision)
    {
        OnCollsion(collision);
    }

    public override int GetRowIndex()
    {
        return currentRow;
    }

    public void SetCanAttackFood(bool enable)
    {
        canAttackFood = enable;
    }

    public void SetCanAttackMouse(bool enable)
    {
        canAttackMouse = enable;
    }
}
