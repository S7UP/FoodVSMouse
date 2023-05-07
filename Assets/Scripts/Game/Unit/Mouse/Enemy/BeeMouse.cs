using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 大蜜蜂鼠
/// </summary>
public class BeeMouse : MouseUnit
{
    private bool isUseRangedAttack; // 是否使用远程攻击
    private Transform Ani_MouseTrans;
    private GeneralAttackSkillAbility generalAttackSkillAbility; // 平A技能
    private static RuntimeAnimatorController Bullet_AnimatorController; // 蜜蜂鼠子弹

    public override void Awake()
    {
        if (Bullet_AnimatorController == null)
            Bullet_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/8/0");
        base.Awake();
        Ani_MouseTrans = transform.Find("Ani_Mouse");
    }

    /// <summary>
    /// 加载技能，加载普通攻击与技能
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // 普通攻击
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
            generalAttackSkillAbility.ClearCurrentEnergy(); // 这个怪的平A初始技力应当为0，否则一进来就远程攻击有点太那个了
        }
    }

    public override void MInit()
    {
        base.MInit();
        isUseRangedAttack = false;
    }

    public override void OnAttackStateEnter()
    {
        if (IsHasTarget())
        {
            isUseRangedAttack = false;
            // 被阻挡时使用高频电钻攻击
            animatorController.Play("Attack0", true);
        }
        else
        {
            isUseRangedAttack = true;
            // 不被阻挡时使用远程攻击
            animatorController.Play("Attack1");
        }
    }

    /// <summary>
    /// 是否满足普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // 这里改为CD转好了就满足
        return true;
    }

    /// <summary>
    /// 普通攻击期间
    /// </summary>
    public override void OnGeneralAttack()
    {
        if (isUseRangedAttack)
        {
            base.OnGeneralAttack();
        }
        else
        {
            if (currentStateTimer <= 0)
            {
                return;
            }
            // 近战攻击直接帧伤
            if (IsHasTarget())
            {
                TakeDamage(GetCurrentTarget());
            }
        }
    }

    /// <summary>
    /// 停滞态时如果被阻挡了立即回满平A技力（马上开始近战攻击）
    /// </summary>
    public override void OnIdleState()
    {
        if (IsHasTarget())
        {
            generalAttackSkillAbility.FullCurrentEnergy();
        }
        base.OnIdleState();
    }

    /// <summary>
    /// 移动态时如果被阻挡了立即回满平A技力（马上开始近战攻击）
    /// </summary>
    public override void OnMoveState()
    {
        if (IsHasTarget())
        {
            generalAttackSkillAbility.FullCurrentEnergy();
        }
        base.OnMoveState();
    }

    /// <summary>
    /// 执行远程攻击
    /// </summary>
    public override void ExecuteDamage()
    {
        Vector3 spawnPoint = GetBulletSpawnPoint();
        for (int i = -1; i <= 1; i++)
        {
            EnemyBullet b = EnemyBullet.GetInstance(Bullet_AnimatorController, this, mCurrentAttack);
            b.isAffectFood = true;
            b.isAffectCharacter = true;
            b.SetStandardVelocity(6.0f);
            b.SetRotate(Vector2.left);
            b.transform.position = transform.position + 1.0f*Vector3.up;
            // 添加一个纵向位移的任务
            BaseTask t = new StraightMovePresetTask(b.transform, new Vector3(b.transform.position.x, transform.position.y + MapManager.gridHeight * i, b.transform.position.z), 60);
            t.AddOnExitAction(delegate { b.UpdateRenderLayer(0); });
            b.AddTask(t);
            // GameController.Instance.AddTasker(new StraightMovePresetTasker(b, new Vector3(b.transform.position.x, transform.position.y + MapManager.gridHeight * i, b.transform.position.z), 60)).AddOtherEndEvent(delegate { b.UpdateRenderLayer(0); });
            GameController.Instance.AddBullet(b);
        }
    }

    /// <summary>
    /// 退出普通攻击的条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetEndGeneralAttackCondition()
    {
        if (isUseRangedAttack)
        {
            return base.IsMeetEndGeneralAttackCondition();
        }
        else
        {
            // 近战攻击不把目标钻死或者强制脱离了射程不会停的
            return !IsHasTarget();
        }
    }

    /// <summary>
    /// 退出普通攻击后要做的事
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        UpdateBlockState(); // 更新阻挡状态
        // 如果是以近战攻击结尾的，则立即恢复满技力，这样马上能出下一发远程攻击
        if (!isUseRangedAttack)
        {
            generalAttackSkillAbility.FullCurrentEnergy();
        }
        SetActionState(new MoveState(this));
    }

    /// <summary>
    /// 获取子弹发射源
    /// </summary>
    /// <returns></returns>
    private Vector3 GetBulletSpawnPoint()
    {
        Vector3 scale = Ani_MouseTrans.transform.localScale;
        float r = 1.45f*MapManager.gridHeight;
        float angle = transform.localEulerAngles.z + 90;
        return transform.position + new Vector3( r*Mathf.Cos(angle*Mathf.PI/180) * scale.x, r*Mathf.Sin(angle * Mathf.PI / 180) * scale.y, 0);
    }
}
