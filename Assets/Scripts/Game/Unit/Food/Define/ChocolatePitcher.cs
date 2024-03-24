using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// �ɿ���Ͷ��
/// </summary>
public class ChocolatePitcher : FoodUnit
{
    private static RuntimeAnimatorController BigBulletRuntimeAnimatorController;
    private static RuntimeAnimatorController SmallBulletRuntimeAnimatorController;
    private static Sprite Debuff_Spr;
    private const string DebuffName = "ճ��";

    private const int DecMoveSpeedTime = 720; // �ɿ�����ļ���ʱ��

    private const int chargeTime = 180; // ����һ����Ҫ��ʱ��
    private const int maxChargeCount = 2; // ��������

    private int chargeCount; // ��ǰ������
    private int chargeTimeLeft; // ʣ�����ʱ��
    private float moveSpeedRate; // ������debuff�����ٱ���

    public override void Awake()
    {
        base.Awake();
        if (BigBulletRuntimeAnimatorController == null)
        {
            BigBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/BigBullet");
            SmallBulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/35/SmallBullet");
            Debuff_Spr = GameManager.Instance.GetSprite("Food/35/Debuff");
        }
    }

    public override void MInit()
    {
        base.MInit();
        chargeCount = 1;
        chargeTimeLeft = chargeTime;
        switch (mShape)
        {
            case 1:
                moveSpeedRate = 0.33f;
                break;
            case 2:
                moveSpeedRate = 0.22f;
                break;
            default:
                moveSpeedRate = 0.33f;
                break;
        }
    }

    public override void MUpdate()
    {
        // ���ܻ���
        if (chargeCount < maxChargeCount)
        {
            chargeTimeLeft--;
            if (chargeTimeLeft <= 0)
            {
                chargeTimeLeft += chargeTime;
                chargeCount++;
            }
        }

        base.MUpdate();
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
        // ѡ���ɿ�����Ŀ��
        {
            BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, rowIndex);
            CreateSmallBullet(transform.position, target);
        }

        // ѡ���ɿ�����Ŀ��
        {
            BaseUnit target;
            if (chargeCount > 0 && TryFindBigBulletTarget(out target))
            {
                chargeCount--;
                CreateBigBullet(transform.position, target);
            }
        }
    }

    /// <summary>
    /// Ͷ���ɿ�����
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateSmallBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, SmallBulletRuntimeAnimatorController, this, mCurrentAttack);
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
                        CreateSmallBullet(b.transform.position, next_target);
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
    /// Ͷ���ɿ�����
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private BaseBullet CreateBigBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BigBulletRuntimeAnimatorController, this, 0);
        b.SetHitSoundEffect("Butter");
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
                DebuffTask t;
                if (u.GetTask(DebuffName) == null)
                {
                    t = new DebuffTask(u);
                    u.taskController.AddUniqueTask(DebuffName, t);
                }
                else
                {
                    t = u.GetTask(DebuffName) as DebuffTask;
                }
                t.AddBuff(DecMoveSpeedTime, moveSpeedRate);
            }
        });

        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// ����ȥ���ɿ�����Ĺ���Ŀ��
    /// </summary>
    /// <param name="target"></param>
    /// <returns></returns>
    public bool TryFindBigBulletTarget(out BaseUnit target)
    {
        // ��������
        List<BaseUnit> list = new List<BaseUnit>();
        List<BaseUnit> debuffList = new List<BaseUnit>(); // ճ��ĵ�λ
        // ɸѡ�����ʵĵ�λ
        float minX = transform.position.x - MapManager.gridWidth / 2;
        foreach (var u in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()).ToArray())
        {
            bool canSelect = UnitManager.CanBeSelectedAsTarget(this, u);
            // �߶�Ϊ0���ɱ�ѡΪ����Ŀ�꣬�ڷ�Χ�ڣ��Ҳ�ճ�����߼���
            if (u.GetHeight() == 0 && canSelect && u.transform.position.x >= minX)
            {
                if (u.GetTask(DebuffName) != null || u.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
                {
                    debuffList.Add(u);
                }
                else
                {
                    list.Add(u);
                }
            }
        }

        // �������ճ��λ
        if (list.Count == 0 && debuffList.Count > 0)
        {
            list = debuffList;
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
    /// ճ��Debuff����
    /// </summary>
    private class DebuffTask : ITask
    {
        private int timeLeft; // ʣ��ʱ��
        // private FloatModifier decMoveSpeedMod = new FloatModifier(0);
        private BaseUnit unit;

        public DebuffTask(BaseUnit unit)
        {
            this.unit = unit;
            // decMoveSpeedMod.Value = 0;
            Initial();
        }

        private void Initial()
        {

        }

        public void OnEnter()
        {
            // ���ճ����Ч
            BaseEffect e = BaseEffect.CreateInstance(Debuff_Spr);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 1);
            }
            GameController.Instance.AddEffect(e);
            unit.mEffectController.AddEffectToDict(DebuffName, e, 0.35f * Vector2.up + 0.15f * Vector2.left);    
        }

        public void OnUpdate()
        {
            timeLeft--;
        }

        public bool IsMeetingExitCondition()
        {
            return timeLeft <= 0;
        }

        public void OnExit()
        {
            // �Ƴ�ճ����Ч
            unit.mEffectController.RemoveEffectFromDict(DebuffName);

            // unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(decMoveSpeedMod);
        }

        // ����һ��Ч��
        public void AddBuff(int timeLeft, float move_speed_rate)
        {
            // �����Ӽ��ٵĵ�λ��Ч
            if (unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreSlowDown))
                timeLeft = 1;
            //move_speed_rate = 0.5f || = 0.33f;
            this.timeLeft = timeLeft;
            //unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(decMoveSpeedMod);
            //decMoveSpeedMod.Value = -(1 - move_speed_rate)*100;
            //unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(decMoveSpeedMod);
            unit.AddStatusAbility(new SlowStatusAbility(-(1-move_speed_rate)*100, unit, timeLeft));
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
