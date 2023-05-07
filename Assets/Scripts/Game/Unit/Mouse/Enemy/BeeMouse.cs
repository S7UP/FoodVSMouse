using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ���۷���
/// </summary>
public class BeeMouse : MouseUnit
{
    private bool isUseRangedAttack; // �Ƿ�ʹ��Զ�̹���
    private Transform Ani_MouseTrans;
    private GeneralAttackSkillAbility generalAttackSkillAbility; // ƽA����
    private static RuntimeAnimatorController Bullet_AnimatorController; // �۷����ӵ�

    public override void Awake()
    {
        if (Bullet_AnimatorController == null)
            Bullet_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/8/0");
        base.Awake();
        Ani_MouseTrans = transform.Find("Ani_Mouse");
    }

    /// <summary>
    /// ���ؼ��ܣ�������ͨ�����뼼��
    /// </summary>
    public override void LoadSkillAbility()
    {
        List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(mUnitType, mType, mShape);
        // ��ͨ����
        if (infoList.Count > 0)
        {
            generalAttackSkillAbility = new GeneralAttackSkillAbility(this, infoList[0]);
            skillAbilityManager.AddSkillAbility(generalAttackSkillAbility);
            generalAttackSkillAbility.ClearCurrentEnergy(); // ����ֵ�ƽA��ʼ����Ӧ��Ϊ0������һ������Զ�̹����е�̫�Ǹ���
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
            // ���赲ʱʹ�ø�Ƶ���깥��
            animatorController.Play("Attack0", true);
        }
        else
        {
            isUseRangedAttack = true;
            // �����赲ʱʹ��Զ�̹���
            animatorController.Play("Attack1");
        }
    }

    /// <summary>
    /// �Ƿ�������ͨ����������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetGeneralAttackCondition()
    {
        // �����ΪCDת���˾�����
        return true;
    }

    /// <summary>
    /// ��ͨ�����ڼ�
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
            // ��ս����ֱ��֡��
            if (IsHasTarget())
            {
                TakeDamage(GetCurrentTarget());
            }
        }
    }

    /// <summary>
    /// ͣ��̬ʱ������赲����������ƽA���������Ͽ�ʼ��ս������
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
    /// �ƶ�̬ʱ������赲����������ƽA���������Ͽ�ʼ��ս������
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
    /// ִ��Զ�̹���
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
            // ���һ������λ�Ƶ�����
            BaseTask t = new StraightMovePresetTask(b.transform, new Vector3(b.transform.position.x, transform.position.y + MapManager.gridHeight * i, b.transform.position.z), 60);
            t.AddOnExitAction(delegate { b.UpdateRenderLayer(0); });
            b.AddTask(t);
            // GameController.Instance.AddTasker(new StraightMovePresetTasker(b, new Vector3(b.transform.position.x, transform.position.y + MapManager.gridHeight * i, b.transform.position.z), 60)).AddOtherEndEvent(delegate { b.UpdateRenderLayer(0); });
            GameController.Instance.AddBullet(b);
        }
    }

    /// <summary>
    /// �˳���ͨ����������
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
            // ��ս��������Ŀ����������ǿ����������̲���ͣ��
            return !IsHasTarget();
        }
    }

    /// <summary>
    /// �˳���ͨ������Ҫ������
    /// </summary>
    public override void AfterGeneralAttack()
    {
        mAttackFlag = true;
        UpdateBlockState(); // �����赲״̬
        // ������Խ�ս������β�ģ��������ָ������������������ܳ���һ��Զ�̹���
        if (!isUseRangedAttack)
        {
            generalAttackSkillAbility.FullCurrentEnergy();
        }
        SetActionState(new MoveState(this));
    }

    /// <summary>
    /// ��ȡ�ӵ�����Դ
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
