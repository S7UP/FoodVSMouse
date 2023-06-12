using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ������Ͷ��
/// </summary>
public class TofuPitcher : FoodUnit
{
    private static RuntimeAnimatorController[] GreenBulletRuntimeAnimatorControllerArray;
    private static RuntimeAnimatorController[] RedBulletRuntimeAnimatorControllerArray;
    private static RuntimeAnimatorController PoisonEffectRuntimeAnimatorController;
    private static RuntimeAnimatorController PoisonAreaEffectRuntimeAnimatorController;
    private static string DebuffName = "�������ж�";

    private Vector2 targetPosition;
    private int GreenBulletAttackCount; // Ͷ������������Ҫ�Ĺ�������
    private int greenLeft; // Ͷ��������ǰ����Ҫ�Ĺ�������
    private float GreenDamageRate; // ���������˺�����
    private float poisonDamageRate; // ÿ���ж��˺�����
    private int PoisonTime; // �ж�ʱ��

    public override void Awake()
    {
        base.Awake();
        if (GreenBulletRuntimeAnimatorControllerArray == null)
        {
            GreenBulletRuntimeAnimatorControllerArray = new RuntimeAnimatorController[3];
            RedBulletRuntimeAnimatorControllerArray = new RuntimeAnimatorController[3];
            for (int i = 0; i < 3; i++)
            {
                GreenBulletRuntimeAnimatorControllerArray[i] = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + i + "/GreenBullet");
                RedBulletRuntimeAnimatorControllerArray[i] = GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + i + "/RedBullet");
            }
            PoisonEffectRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/Poison");
            PoisonAreaEffectRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/36/PoisonEffect");
        }
    }

    public override void MInit()
    {
        base.MInit();
        // ����תְ�����ȷ������
        switch (mShape)
        {
            case 1:
                GreenBulletAttackCount = 2;
                PoisonTime = 360;
                greenLeft = 0;
                break;
            case 2:
                GreenBulletAttackCount = 1;
                PoisonTime = 360;
                greenLeft = 0;
                break;
            default:
                GreenBulletAttackCount = 2;
                PoisonTime = 240;
                greenLeft = GreenBulletAttackCount;
                break;
        }
        
        GreenDamageRate = 1.0f;
        poisonDamageRate = 0.5f;
        targetPosition = Vector2.zero;
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
        if (greenLeft > 0)
            animatorController.Play("Attack0");
        else
            animatorController.Play("Attack1");
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
        BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());
        if (targetUnit != null)
        {
            targetPosition = targetUnit.transform.position;
        }
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
        GameManager.Instance.audioSourceManager.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // ѡ��Ŀ��
        BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x, GetRowIndex());


        if (greenLeft > 0)
        {
            greenLeft--;
            CreateRedBullet(transform.position, target, mCurrentAttack);
        }
        else
        {
            greenLeft = GreenBulletAttackCount;
            CreateGreenBullet(transform.position, target, mCurrentAttack);
        }

    }

    /// <summary>
    /// Ͷ���춹��
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateRedBullet(Vector2 startPosition, BaseUnit target, float ori_dmg)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, RedBulletRuntimeAnimatorControllerArray[mShape], this, ori_dmg);
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
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);

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
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, GreenBulletRuntimeAnimatorControllerArray[mShape], this, GreenDamageRate * ori_dmg);
        b.SetHitSoundEffect("Splat" + GameManager.Instance.rand.Next(0, 3));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ

        // ȷ���ò���������������˶�
        if (target != null)
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        else
            PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);


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
                        CreateGreenBullet(b.transform.position, next_target, ori_dmg);
                    }
                    else
                    {
                        // ԭ�����ѣ������ǻᴥ����ΧЧ��
                        CreateDebuffArea(b.transform.position, ori_dmg);
                    }
                }
            });
        }
        else
        {
            // ��ӻ��к���¼�
            b.AddHitAction((b, u) => {
                if (u == null || !u.IsAlive())
                    CreateDebuffArea(b.transform.position, ori_dmg);
                else
                    CreateDebuffArea(u.transform.position, ori_dmg);
            });
        }

        GameController.Instance.AddBullet(b);
        return b;
    }


    /// <summary>
    /// Ϊһ����Χ�ڵĵ��˸����ж�
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="ori_dmg"></param>
    private void CreateDebuffArea(Vector2 pos, float ori_dmg)
    {
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.isAffectMouse = true;
        r.SetAffectHeight(0);
        r.SetInstantaneous();

        r.SetOnEnemyEnterAction((u) => {
            TofuTask t;
            if (u.GetTask(DebuffName) == null)
            {
                t = new TofuTask(u);
                u.AddUniqueTask(DebuffName, t);
            }
            else
            {
                t = u.GetTask(DebuffName) as TofuTask;
            }
            t.AddBuff(poisonDamageRate * ori_dmg, PoisonTime);
        });

        GameController.Instance.AddAreaEffectExecution(r);

        // ��ӷ�Χ��Ч
        {
            BaseEffect e = BaseEffect.CreateInstance(PoisonAreaEffectRuntimeAnimatorController, null, "Appear", "Disappear", false);
            e.SetSpriteRendererSorting("Unit", LayerManager.CalculateSortingLayer(LayerManager.UnitType.Enemy, 7, 0, UnityEngine.Random.Range(0, 100)));
            e.transform.position = pos;
            GameController.Instance.AddEffect(e);
        }
    }

    /// <summary>
    /// ������BUFF����
    /// </summary>
    private class TofuTask : ITask
    {
        private class Recorder
        {
            public float dmg; // ����
            public int timeLeft;

            public Recorder(float dmg, int timeLeft)
            {
                this.dmg = dmg;
                this.timeLeft = timeLeft;
            }
        }

        // ÿ�����˺�ϵ��
        private const int interval = 30;
        private int triggerDamageTimeLeft; // �����˺�ʣ��ʱ��
        private List<Recorder> rList = new List<Recorder>();
        private BaseUnit unit;

        public TofuTask(BaseUnit unit)
        {
            this.unit = unit;
            Initial();
        }

        private void Initial()
        {
            rList.Clear();
            triggerDamageTimeLeft = interval;
        }

        public void OnEnter()
        {
            // ����ж���Ч
            BaseEffect e = BaseEffect.CreateInstance(PoisonEffectRuntimeAnimatorController, "Appear", "Idle", "Disappear", true);
            string name;
            int order;
            if (unit.TryGetSpriteRenternerSorting(out name, out order))
            {
                e.SetSpriteRendererSorting(name, order + 4);
            }
            GameController.Instance.AddEffect(e);
            unit.mEffectController.AddEffectToDict(DebuffName, e, 0.3f*Vector2.up);
        }

        public void OnUpdate()
        {
            triggerDamageTimeLeft--;
            if (triggerDamageTimeLeft <= 0)
            {
                triggerDamageTimeLeft += interval;
                float totalDamage = 0;
                List<Recorder> delList = new List<Recorder>();
                foreach (var r in rList)
                {
                    totalDamage += r.dmg;
                    r.timeLeft--;
                    if (r.timeLeft <= 0)
                        delList.Add(r);
                }
                foreach (var r in delList)
                    rList.Remove(r);
                DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, null, unit, totalDamage);
                d.AddDamageType(DamageAction.DamageType.AOE);
                d.ApplyAction();
            }
            else
            {
                List<Recorder> delList = new List<Recorder>();
                foreach (var r in rList)
                {
                    r.timeLeft--;
                    if (r.timeLeft <= 0)
                        delList.Add(r);
                }
                foreach (var r in delList)
                    rList.Remove(r);
            }
               
        }

        public bool IsMeetingExitCondition()
        {
            return rList.Count <= 0;
        }

        public void OnExit()
        {
            // �Ƴ��ж���Ч
            unit.mEffectController.RemoveEffectFromDict(DebuffName);
        }

        // ����һ��Ч��
        public void AddBuff(float dmg, int timeLeft)
        {
            rList.Add(new Recorder(dmg, timeLeft));
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
