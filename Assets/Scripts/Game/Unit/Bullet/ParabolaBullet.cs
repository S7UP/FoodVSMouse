using UnityEngine;

/// <summary>
/// �������ƶ�ʽ���ӵ�
/// </summary>
public class ParabolaBullet : BaseBullet
{
    // �������ӵ����ʹ�ø���
    public Rigidbody2D r2D;

    public Vector3 firstPosition; // ��ʼ��
    public Vector3 targetPosition; // Ŀ���
    private int totalTimer; // ��ʼ�㵽Ŀ�����ʱ
    private int currentTimer; // ��ǰ��ʱ
    private float g; // ���ٶ�
    private float velocityVertical; // ��ֱ������ٶ�
    private int currentRow; // ��ǰ������
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


    // ע���ڴ˴���mVelocity��Ϊˮƽ�ٶ�
    /// <summary>
    /// ���ñ�Ҫ�����ԣ������޷�����ִ��
    /// </summary>
    /// <param name="standardVelocity">��׼ˮƽ�����ٶ�</param>
    /// <param name="isLeftPosition">ˮƽ�����Ƿ�Ϊ����</param>
    /// <param name="height">�����߶�����Ը߶�</param>
    /// <param name="firstPosition">��ʼλ��</param>
    /// <param name="targetPosition">Ŀ��λ��</param>
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
        // ����ó��������ٶȣ����£�
        g = 8*height / (totalTimer * totalTimer);
        // Ϊ��ֱ����ĳ��ٶȸ�ֵ
        velocityVertical = g * totalTimer / 2;
    }

    public override void OnFlyState()
    {
        // ��ʱ���Ա�
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
    /// ��ȡˮƽ������ٶ�
    /// </summary>
    /// <returns></returns>
    public float GetHorizontalVelocity()
    {
        return mVelocity;
    }

    /// <summary>
    /// ��ȡ��ֱ������ٶ�
    /// </summary>
    public float GetVerticalVelocity()
    {
        return velocityVertical;
    }

    /// <summary>
    /// ������ײʱ
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
