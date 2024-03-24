using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ������Ͷ��
/// </summary>
public class TofuPitcher : FoodUnit
{
    private static RuntimeAnimatorController GreenBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController RedBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController PoisonEffectRuntimeAnimatorController;
    private static RuntimeAnimatorController PoisonAreaEffectRuntimeAnimatorController;
    private const string DebuffName = "�������ж�";

    private float poisonDamageRate; // ÿ���ж��˺�����
    private int PoisonTime; // �ж�ʱ��

    private int chargeTime; // ����һ����Ҫ��ʱ��
    private int chargeCount; // ��ǰ������
    private const int maxChargeCount = 2; // ��������
    private int chargeTimeLeft; // ʣ�����ʱ��

    public override void Awake()
    {
        base.Awake();
        if (PoisonEffectRuntimeAnimatorController == null)
        {
            PoisonEffectRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/Poison");
            PoisonAreaEffectRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/PoisonEffect");
        }
    }

    public override void MInit()
    {
        base.MInit();

        if(GreenBulletRuntimeAnimatorController == null)
        {
            GreenBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + mShape + "/GreenBullet");
            RedBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + mShape + "/RedBullet");
        }
        // ����תְ�����ȷ������
        switch (mShape)
        {
            case 1:
                poisonDamageRate = 0.5f;
                PoisonTime = 720;
                chargeTime = 180;
                break;
            case 2:
                poisonDamageRate = 1f;
                PoisonTime = 360;
                chargeTime = 102;
                break;
            default:
                poisonDamageRate = 0.5f;
                PoisonTime = 720;
                chargeTime = 180;
                break;
        }

        chargeCount = 1;
        chargeTimeLeft = chargeTime;
    }

    public override void MUpdate()
    {
        // ���ܻ���
        if(chargeCount < maxChargeCount)
        {
            chargeTimeLeft--;
            if(chargeTimeLeft <= 0)
            {
                chargeTimeLeft += chargeTime;
                chargeCount++;
            }
        }

        base.MUpdate();
    }

    public override void MDestory()
    {
        base.MDestory();
    }

    /// <summary>
    /// ���ݵȼ���͵ȼ������¶�Ӧ����
    /// </summary>
    public override void UpdateAttributeByLevel()
    {
        NumericBox.Attack.SetBase((float)(attr.baseAttrbute.baseAttack + attr.valueList[mLevel]));
    }

    public override void OnAttackStateEnter()
    {
        animatorController.Play("Attack0");
    }

    protected override void UpdateAttackAnimationSpeed()
    {
        UpdateAnimationSpeedByAttackSpeed("Attack0");
        UpdateAnimationSpeedByAttackSpeed("Attack1");
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    protected override bool IsHasTarget()
    {
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());
        return targetUnit != null;
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
        int rowIndex = GetRowIndex();
        GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // ѡ��춹��Ŀ��
        {
            BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, rowIndex);
            CreateRedBullet(transform.position, target, mCurrentAttack);
        }

        // ѡ�������Ŀ��
        {
            BaseUnit target;
            if (chargeCount > 0 && TryFindGreenBulletTarget(out target))
            {
                chargeCount--;
                CreateGreenBullet(transform.position, target, mCurrentAttack);
            }
        }

    }

    /// <summary>
    /// Ͷ���춹��
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateRedBullet(Vector2 startPosition, BaseUnit target, float ori_dmg)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, RedBulletRuntimeAnimatorController, this, ori_dmg);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ

        // �����ж�Ŀ�����ѷ����ǵз�������û�У�������Щ������ƶ��߼�
        if (target != null && target is FoodUnit && target.mType == (int)FoodNameTypeMap.CherryPudding)
        {
            // Ŀ�����ѷ�����
            CherryPuddingFoodUnit pudding = target.GetComponent<CherryPuddingFoodUnit>();

            // ���Ŀ�겼��������
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                return !pudding.IsAlive();
            });
            t.AddOnExitAction(delegate
            {
                pudding = null; // ���Ŀ�겼��������ȡ��������
            });
            b.AddTask(t);

            // ����������ض�������
            b.AddHitAction((b, u) => {
                // �ض���
                if (pudding != null)
                {
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if (next_target != null)
                    {
                        b.isnUseHitEffect = true; // ��ǰ�ӵ������û��ж�����ֱ����ʧ
                        CreateRedBullet(b.transform.position, next_target, ori_dmg);
                    }
                }
            });
        }

        // ȷ���ò���������������˶�
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, new Vector2(MapManager.GetColumnX(8.5f), startPosition.y), true, false);

        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// Ͷ��������
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private BaseBullet CreateGreenBullet(Vector2 startPosition, BaseUnit target, float ori_dmg)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, GreenBulletRuntimeAnimatorController, this, 0);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ

        // ȷ���ò���������������˶�
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, new Vector2(MapManager.GetColumnX(8.5f), startPosition.y), true, false);

        // ��ӻ��к���¼�
        b.AddHitAction((b, u) => {
            if (u != null && u.IsAlive())
            {
                TofuTask t;
                if (u.GetTask(DebuffName) == null)
                {
                    t = new TofuTask(u);
                    u.taskController.AddUniqueTask(DebuffName, t);
                }
                else
                {
                    t = u.GetTask(DebuffName) as TofuTask;
                }
                t.AddBuff(poisonDamageRate * ori_dmg, PoisonTime);
            }
        });
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// ����ȥ�ҳ������Ĺ���Ŀ��
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryFindGreenBulletTarget(out BaseUnit target)
    {
        float minX = transform.position.x - MapManager.gridWidth / 2;
        // ��������
        List<BaseUnit> list = new List<BaseUnit>();
        List<BaseUnit> debuffList = new List<BaseUnit>(); // �ж��ĵ�λ
        // ɸѡ�����ʵĵ�λ
        float maxHp = float.MinValue;
        foreach (var u in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()).ToArray())
        {
            bool canSelect = UnitManager.CanBeSelectedAsTarget(this, u);

            // �߶�Ϊ0���ɱ�ѡΪ����Ŀ�꣬�ڷ�Χ�ڣ��Ҳ��ж�
            if (u.GetHeight() == 0 && canSelect && u.transform.position.x >= minX)
            {
                if (u.GetTask(DebuffName) != null)
                {
                    debuffList.Add(u);
                    continue;
                }

                // ��Ѫ����
                if (u.mCurrentHp > maxHp)
                {
                    maxHp = u.mCurrentHp;
                    list.Clear();
                    list.Add(u);
                }
                else if (u.mCurrentHp == maxHp)
                    list.Add(u);
            }
        }

        // ��������ж���λ
        if (list.Count == 0 && debuffList.Count > 0)
        {
            foreach (var u in debuffList)
            {
                // ��Ѫ����
                if (u.mCurrentHp > maxHp)
                {
                    maxHp = u.mCurrentHp;
                    list.Clear();
                    list.Add(u);
                }
                else if (u.mCurrentHp == maxHp)
                    list.Add(u);
            }
        }

        target = null;
        // ȥ��������С�ĵ�λ
        if (list.Count > 0)
        {
            foreach (var u in list)
            {
                if (target == null || u.transform.position.x < target.transform.position.x)
                {
                    target = u;
                }
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// ������BUFF����
    /// </summary>
    private class TofuTask : ITask
    {
        // ÿ�����˺�ϵ��
        private const int interval = 15;
        private int triggerDamageTimeLeft; // �����˺�ʣ��ʱ��
        private float dmg; // ÿ���˺�
        private int timeLeft; // ʣ��ʱ��
        private BaseUnit unit;

        public TofuTask(BaseUnit unit)
        {
            this.unit = unit;
            Initial();
        }

        private void Initial()
        {
            triggerDamageTimeLeft = interval;
        }

        public void OnEnter()
        {
            // ����ж���Ч
            BaseEffect e = BaseEffect.CreateInstance(PoisonAreaEffectRuntimeAnimatorController, "Appear", "Idle", "Disappear", true);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 2);
            }
            GameController.Instance.AddEffect(e);
            unit.mEffectController.AddEffectToDict(DebuffName, e, 0.35f * Vector2.up + 0.15f * Vector2.left);
        }

        public void OnUpdate()
        {
            timeLeft--;
            triggerDamageTimeLeft--;
            if (triggerDamageTimeLeft <= 0)
            {
                triggerDamageTimeLeft += interval;
                new DamageAction(CombatAction.ActionType.CauseDamage, null, unit, dmg).ApplyAction();
            }
        }

        public bool IsMeetingExitCondition()
        {
            return timeLeft <= 0;
        }

        public void OnExit()
        {
            // �Ƴ��ж���Ч
            unit.mEffectController.RemoveEffectFromDict(DebuffName);
        }

        // ����һ��Ч��
        public void AddBuff(float dmg, int timeLeft)
        {
            this.dmg = dmg / (60f/interval);
            this.timeLeft = timeLeft;
        }

        public void ShutDown()
        {
            
        }

        public bool IsClearWhenDie()
        {
            return true;
        }
    }
}
