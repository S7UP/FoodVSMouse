using System;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ����Ͷ��
/// </summary>
public class EggPitcher : FoodUnit
{
    private static RuntimeAnimatorController BulletRuntimeAnimatorController;

    private float mainDamageRate; // ��ҪĿ���˺�����
    private Vector2 targetPosition;

    public override void MInit()
    {
        base.MInit();
        if (BulletRuntimeAnimatorController == null)
            BulletRuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Food/37/" + mShape + "/Bullet");
        // ����תְ�����ȷ����ҪĿ���뷶Χ�˺����˺�����
        switch (mShape)
        {
            case 1:
                mainDamageRate = 2.26f;
                break;
            case 2:
                mainDamageRate = 2.26f;
                break;
            default:
                mainDamageRate = 1.6f;
                break;
        }

        targetPosition = Vector2.zero;
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
        int startIndex = Mathf.Max(0, GetRowIndex() - 1);
        int endIndex = Mathf.Min(6, GetRowIndex() + 1);

        for (int i = startIndex; i <= endIndex; i++)
        {
            BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, i);
            if (targetUnit != null)
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
            if(mShape == 2)
            {
                CustomizationTask task = new CustomizationTask();
                task.AddTimeTaskFunc(15);
                task.AddOnExitAction(delegate {
                    ExecuteDamage();
                });
                taskController.AddTask(task);
            }

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
        GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // ѡ��Ŀ��
        BaseUnit target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());

        if(target != null)
        {
            CreateBullet(transform.position, mCurrentAttack, target);
        }
        else
        {
            if(GetRowIndex() == 0)
            {
                target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex() + 1);
                if (target != null)
                    CreateBullet(transform.position, mCurrentAttack, target);
                else
                    CreateBullet(transform.position, mCurrentAttack, MapManager.GetGridLocalPosition(8, GetRowIndex()));
            }else if(GetRowIndex() == 6)
            {
                target = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex() - 1);
                if (target != null)
                    CreateBullet(transform.position, mCurrentAttack, target);
                else
                    CreateBullet(transform.position, mCurrentAttack, MapManager.GetGridLocalPosition(8.5f, GetRowIndex()));
            }
            else
            {
                BaseUnit target_down = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex() + 1);
                BaseUnit target_up = PitcherManager.FindTargetByPitcher(this, transform.position.x - MapManager.gridWidth / 2, GetRowIndex() - 1);
                float min_x = MapManager.GetColumnX(8.5f);
                if(target_down != null && target_down.transform.position.x < min_x)
                {
                    min_x = target_down.transform.position.x;
                }
                if(target_up != null && target_up.transform.position.x < min_x)
                {
                    min_x = target_up.transform.position.x;
                }
                CreateBullet(transform.position, mCurrentAttack, new Vector2(min_x, MapManager.GetRowY(GetRowIndex())));
            }
        }
        
    }

    /// <summary>
    /// ����һ�������ӵ�
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BulletRuntimeAnimatorController, this, 0);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ
        b.SetHitSoundEffect("Eggimpact"+GameManager.Instance.rand.Next(0, 2));

        // ������к��Ч��
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                CreateDamageArea(b, b.transform.position, ori_dmg);
            };
        }

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
                        CreateBullet(b.transform.position, ori_dmg, next_target);
                    }
                    else
                    {
                        // ԭ�����ѣ���ͬ���ᴥ����ΧЧ��
                        hitEnemyAction(b, null);
                    }
                }
            });

            // ����������˶�
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        }
        else
        {
            // Ŀ���ǵз���ֱ����Ӽ���
            b.AddHitAction(hitEnemyAction);

            // ȷ���ò���������������˶�
            if (target != null)
                PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
            else
                PitcherManager.AddDefaultFlyTask(b, startPosition, targetPosition, true, false);
        }
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// ����һ�������ӵ�
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, Vector2 endPos)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, BulletRuntimeAnimatorController, this, 0);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ
        b.SetHitSoundEffect("Eggimpact" + GameManager.Instance.rand.Next(0, 2));

        b.AddHitAction((b, u) =>
        {
            CreateDamageArea(b, b.transform.position, ori_dmg);
        });
        PitcherManager.AddDefaultFlyTask(b, startPosition, endPos, true, false);
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// AOE�˺�
    /// </summary>
    private void CreateDamageArea(BaseBullet b, Vector2 pos, float ori_dmg)
    {
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.SetInstantaneous();
            r.AddEnemyEnterConditionFunc((u) => {
                return UnitManager.CanBeSelectedAsTarget(this, u);
            });
            r.AddBeforeDestoryAction(delegate {
                if (r.mouseUnitList.Count > 0)
                {
                    float dmg = (mainDamageRate / r.mouseUnitList.Count) * ori_dmg;
                    foreach (var u in r.mouseUnitList.ToArray())
                    {
                        // ƽ̯����
                        new DamageAction(CombatAction.ActionType.CauseDamage, this, u, dmg).ApplyAction();
                    }
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }

        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, new Vector2(MapManager.gridWidth, 2.5f * MapManager.gridHeight), "ItemCollideEnemy");
            r.isAffectMouse = true;
            r.SetAffectHeight(0);
            r.SetInstantaneous();
            r.AddEnemyEnterConditionFunc((u) => {
                return UnitManager.CanBeSelectedAsTarget(this, u);
            });
            r.AddBeforeDestoryAction(delegate {
                if (r.mouseUnitList.Count > 0)
                {
                    foreach (var u in r.mouseUnitList.ToArray())
                    {
                        // Ⱥ�˲���
                        DamageAction d = new DamageAction(CombatAction.ActionType.CauseDamage, this, u, 0.4f * ori_dmg);
                        d.AddDamageType(DamageAction.DamageType.AOE);
                        d.ApplyAction();
                    }
                }
            });
            GameController.Instance.AddAreaEffectExecution(r);
        }
    }
}
