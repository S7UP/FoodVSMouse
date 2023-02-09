
using UnityEngine;
/// <summary>
/// ���߾Ƽܾ���ʵ��
/// </summary>
public class ThreeLineFoodUnit : FoodUnit
{
    private static RuntimeAnimatorController Bullet_RuntimeAnimatorController;
    private int[] countArray;

    public override void Awake()
    {
        if (Bullet_RuntimeAnimatorController == null)
            Bullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/7/Bullet");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        // ����תְ�����ȷ�������߸��������ӵ�
        switch (mShape)
        {
            case 1:
                countArray = new int[] { 1, 2, 1 };
                break;
            case 2:
                countArray = new int[] { 2, 2, 2 };
                break;
            default:
                countArray = new int[] { 1, 1, 1 };
                break;
        }
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        int start = Mathf.Max(0, GetRowIndex() - 1);
        int end = Mathf.Min(GetRowIndex() + 1, MapController.yRow - 1);
        for (int i = start; i <= end; i++)
        {
            if (GameController.Instance.CheckRowCanAttack(this, i))
                return true;
        }
        return false;
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // ����Ŀ�꼴��
        return IsHasTarget();
    }

    /// <summary>
    /// ������ͨ��������״̬
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // �л�Ϊ����������ͼ
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// ��ͨ�����ڼ�
    /// </summary>
    public override void OnGeneralAttack()
    {
        // �˺��ж�֡Ӧ��ִ���ж�
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// �˳���ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// �Ƿ�Ϊ�˺��ж�ʱ�̣���ս����Ϊ���ʵ���˺���Զ�̹���Ϊȷ�����䵯�壩
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        int rowIndex = GetRowIndex(); // ��ȡ��ǰ��

        for (int i = -1; i <= 1; i++)
        {
            for (int j = 0; j < countArray[i + 1]; j++)
            {
                AllyBullet b = AllyBullet.GetInstance(BulletStyle.Normal, Bullet_RuntimeAnimatorController, this, mCurrentAttack);
                b.transform.position = transform.position;
                b.SetSpriteLocalPosition(GetSpriteLocalPosition() + Vector2.up * 0.1f);
                b.SetStandardVelocity(24);
                b.SetRotate(Vector2.right);
                GameController.Instance.AddBullet(b);
                if ((rowIndex == 0 && i==1) || (rowIndex == 6 && i == -1))
                {
                    // ���һ������λ�Ƶ�����
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridHeight / 30, 0, Vector3.up * 0, MapManager.gridHeight));
                    // ����λ��
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridWidth / 30 * (j + 0.5f), 0, Vector3.right, 60));
                }
                else
                {
                    // ���һ������λ�Ƶ�����
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridHeight / 30, 0, Vector3.up * i, MapManager.gridHeight));
                    // ����λ��
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridWidth / 30 * j, 0, Vector3.right, 60));
                }
            }
        }
    }
}
