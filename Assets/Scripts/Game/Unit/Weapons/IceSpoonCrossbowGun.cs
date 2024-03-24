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
            needEnergy = 60,
            startEnergy = 60,
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
        int startIndex = Mathf.Max(0, GetRowIndex() - 1);
        int endIndex = Mathf.Min(6, GetRowIndex() + 1);

        for (int i = startIndex; i <= endIndex; i++)
        {
            BaseUnit targetUnit = PitcherManager.FindTargetByPitcher(master, master.transform.position.x - MapManager.gridWidth / 2, i);
            if (targetUnit != null)
                return true;
        }
        return false;

        //// ��������
        //List<BaseUnit> list = new List<BaseUnit>();
        //// ɸѡ���߶�Ϊ0�Ŀ�ѡȡ��λ
        //foreach (var item in GameController.Instance.GetSpecificRowEnemyList(GetRowIndex()))
        //{
        //    if (item.GetHeight() == 0 && UnitManager.CanBeSelectedAsTarget(master, item))
        //        list.Add(item);
        //}
        //if (list.Count > 0)
        //{
        //    bool flag = false;
        //    foreach (var item in list)
        //    {
        //        if(item.transform.position.x > master.transform.position.x)
        //        {
        //            flag = true;
        //            target = item;
        //            SearchTargetPosition();
        //            break;
        //        }
        //    }
        //    return flag;
        //}
        //return false;
    }

    /// <summary>
    /// ִ�о���Ĺ�����λ���˺��ж�Ϊ��֮��
    /// </summary>
    public override void ExecuteDamage()
    {
        //GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        //// ѡ��Ŀ��
        //BaseUnit target = PitcherManager.FindTargetByPitcher(master, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());
        //CreateBullet(transform.position, target);
        GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // ѡ��Ŀ��
        BaseUnit target = PitcherManager.FindTargetByPitcher(master, master.transform.position.x - MapManager.gridWidth / 2, master.GetRowIndex());

        if (target != null)
        {
            CreateBullet(master.transform.position, target);
        }
        else
        {
            if (GetRowIndex() == 0)
            {
                target = PitcherManager.FindTargetByPitcher(master, master.transform.position.x - MapManager.gridWidth / 2, master.GetRowIndex() + 1);
                if (target != null)
                    CreateBullet(master.transform.position, target);
                else
                    CreateBullet(master.transform.position, MapManager.GetGridLocalPosition(8, master.GetRowIndex()));
            }
            else if (GetRowIndex() == 6)
            {
                target = PitcherManager.FindTargetByPitcher(master, master.transform.position.x - MapManager.gridWidth / 2, master.GetRowIndex() - 1);
                if (target != null)
                    CreateBullet(master.transform.position, target);
                else
                    CreateBullet(master.transform.position, MapManager.GetGridLocalPosition(8.5f, master.GetRowIndex()));
            }
            else
            {
                BaseUnit target_down = PitcherManager.FindTargetByPitcher(master, master.transform.position.x - MapManager.gridWidth / 2, master.GetRowIndex() + 1);
                BaseUnit target_up = PitcherManager.FindTargetByPitcher(master, master.transform.position.x - MapManager.gridWidth / 2, master.GetRowIndex() - 1);
                float min_x = MapManager.GetColumnX(8.5f);
                if (target_down != null && target_down.transform.position.x < min_x)
                {
                    min_x = target_down.transform.position.x;
                }
                if (target_up != null && target_up.transform.position.x < min_x)
                {
                    min_x = target_up.transform.position.x;
                }
                CreateBullet(master.transform.position, new Vector2(min_x, MapManager.GetRowY(master.GetRowIndex())));
            }
        }
    }

    /// <summary>
    /// ����һ�������ӵ�
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, Vector2 endPos)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, IceEggBullet_RuntimeAnimatorController, master, 0);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ
        b.SetHitSoundEffect("Eggimpact" + GameManager.Instance.rand.Next(0, 2));

        b.AddHitAction((b, u) =>
        {
            Vector2 pos = b.transform.position; // ��Χ��������Ч������λ��
            // ����һ����Χ��������Ч��
            CreateIceSlowDownArea(pos);
        });
        PitcherManager.AddDefaultFlyTask(b, startPosition, endPos, true, false);
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// ����һ�������ӵ�
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, IceEggBullet_RuntimeAnimatorController, master, 0);
        b.SetHitSoundEffect("Eggimpact" + GameManager.Instance.rand.Next(0, 2));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // ��������ɾ
        // ���׻���Ч��
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                Vector2 pos = b.transform.position; // ��Χ��������Ч������λ��
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
                        CreateBullet(b.transform.position, next_target);
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
            MouseUnit m = u as MouseUnit;
            if (!m.IsBoss())
            {
                EnvironmentFacade.AddIceDebuff(u, 12);
                new DamageAction(CombatAction.ActionType.CauseDamage, master, u, 0.005f*u.mCurrentHp).ApplyAction();
            }
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
