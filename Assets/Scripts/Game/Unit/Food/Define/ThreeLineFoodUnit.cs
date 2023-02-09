
using UnityEngine;
/// <summary>
/// 三线酒架具体实现
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
        // 根据转职情况来确定三条线各发几发子弹
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
    /// 根据等级表和等级来更新对应数据
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    /// <summary>
    /// 判断是否有有效的攻击目标
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
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 发现目标即可
        return IsHasTarget();
    }

    /// <summary>
    /// 进入普通攻击动画状态
    /// </summary>
    public override void BeforeGeneralAttack()
    {
        // 切换为攻击动画贴图
        SetActionState(new AttackState(this));
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        // 伤害判定帧应当执行判定
        if (IsDamageJudgment())
        {
            mAttackFlag = false;
            ExecuteDamage();
        }
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        return animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce();
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        SetActionState(new IdleState(this));
    }

    /// <summary>
    /// 是否为伤害判定时刻（近战攻击为打出实际伤害，远程攻击为确定发射弹体）
    /// </summary>
    /// <returns></returns>
    public override bool IsDamageJudgment()
    {
        return (animatorController.GetCurrentAnimatorStateRecorder().GetNormalizedTime() >= attackPercent && mAttackFlag);
    }

    /// <summary>
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        int rowIndex = GetRowIndex(); // 获取当前行

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
                    // 添加一个纵向位移的任务
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridHeight / 30, 0, Vector3.up * 0, MapManager.gridHeight));
                    // 横向位移
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridWidth / 30 * (j + 0.5f), 0, Vector3.right, 60));
                }
                else
                {
                    // 添加一个纵向位移的任务
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridHeight / 30, 0, Vector3.up * i, MapManager.gridHeight));
                    // 横向位移
                    GameController.Instance.AddTasker(new StraightMovePresetTasker(b, MapManager.gridWidth / 30 * j, 0, Vector3.right, 60));
                }
            }
        }
    }
}
