using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
using Environment;
/// <summary>
/// ������ǹ
/// </summary>
public class IceSpoonCrossbowGun : BaseWeapons
{
    private static RuntimeAnimatorController IceEggBullet_RuntimeAnimatorController;
    
    private BaseUnit target;
    private Vector3 targetPosition;

    public override void Awake()
    {
        if (IceEggBullet_RuntimeAnimatorController == null)
            IceEggBullet_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/7/0");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        //targetPosition = null;
    }

    /// <summary>
    /// ���ؼ��ܣ��˴���������ͨ���������弼�ܼ���ʵ��������������д
    /// </summary>
    public override void LoadSkillAbility()
    {
        //List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(UnitType.Weapons, mType, mShape);
        // ��ͨ����
        weaponsGeneralAttackSkillAbility = new WeaponsGeneralAttackSkillAbility(this, new SkillAbility.SkillAbilityInfo()
        {
            name = "��ͨ����",
            needEnergy = 180,
            startEnergy = 180,
            energyRegeneration = 1.0f,
            skillType = SkillAbility.Type.GeneralAttack,
            isExclusive = true,
            canActiveInDeathState = false,
            priority = 0
        });
        skillAbilityManager.AddSkillAbility(weaponsGeneralAttackSkillAbility);
    }

    /// <summary>
    /// �ж��Ƿ�����Ч�Ĺ���Ŀ��
    /// </summary>
    /// <returns></returns>
    public override bool IsHasTarget()
    {
        // ��������
        List<BaseUnit> list = new List<BaseUnit>();
        // ɸѡ���߶�Ϊ0�Ŀ�ѡȡ��λ
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        {
            if (item.GetHeight() == 0 && UnitManager.CanBeSelectedAsTarget(master, item))
                list.Add(item);
        }
        if (list.Count > 0)
        {
            bool flag = false;
            foreach (var item in list)
            {
                if(item.transform.position.x > master.transform.position.x)
                {
                    flag = true;
                    target = item;
                    SearchTargetPosition();
                    break;
                }
            }
            return flag;
        }
        return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // ѡ��Ŀ��
        BaseUnit target = PitcherManager.FindTargetByPitcher(master, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());
        CreateBullet(transform.position, 4*master.mCurrentAttack, target);
    }

    /// <summary>
    /// ����һ�������ӵ�
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, float ori_dmg, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, IceEggBullet_RuntimeAnimatorController, master, ori_dmg);
        b.SetHitSoundEffect("Eggimpact" + GameManager.Instance.rand.Next(0, 2));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ
        // ���׻���Ч��
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                Vector2 pos; // ��Χ��������Ч������λ��
                if (u != null && u.IsValid())
                {
                    // �����λ���ڣ����ڵ�λλ�ñ�ը�����ҵ�λ�ܵ�3%�������ֵ�빥�����нϴ����˺�����BOSS��Ϊ������
                    float dmg = Mathf.Max(0.03f * u.mMaxHp, b.GetDamage());
                    if (u is MouseUnit)
                    {
                        MouseUnit m = u as MouseUnit;
                        if (m.IsBoss())
                        {
                            dmg = b.GetDamage();
                        }
                    }
                    new DamageAction(CombatAction.ActionType.CauseDamage, master, u, dmg).ApplyAction();
                   pos = u.transform.position;
                }
                else
                {
                    // ����λ�ڸ��������ı�ը
                    pos = MapManager.GetGridLocalPosition(b.GetColumnIndex(), b.GetRowIndex());
                }
                // ����һ����Χ��������Ч��
                CreateIceSlowDownArea(pos);
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

    private RetangleAreaEffectExecution CreateIceSlowDownArea(Vector2 pos)
    {
        // ԭ�ز���һ������Ч��
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.SetInstantaneous();
        r.isAffectMouse = true;
        r.SetAffectHeight(0); // ֻ�е��浥λ��Ӱ��
        r.SetOnEnemyEnterAction((u) => {
            // u.AddStatusAbility(new FrozenSlowStatusAbility(-35, u, 120));
            EnvironmentFacade.AddIceDebuff(u, 35);
        });
        GameController.Instance.AddAreaEffectExecution(r);
        return r;
    }

    /// <summary>
    /// Ѱ��Ŀ��
    /// </summary>
    public void SearchTargetPosition()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        // ɸѡ���߶�Ϊ0�Ŀ�ѡȡ��λ
        foreach (var item in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        {
            if (item.GetHeight() == 0 && UnitManager.CanBeSelectedAsTarget(master, item))
                list.Add(item);
        }
        if (list.Count <= 0)
            return;
        targetPosition = target.transform.position;
        foreach (var item in list)
        {
            if(item.transform.position.x < targetPosition.x && item.transform.position.x > master.transform.position.x)
            {
                targetPosition = item.transform.position;
                target = item;
            }
        }
    }
}
