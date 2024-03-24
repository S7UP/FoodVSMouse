using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
using Environment;
/// <summary>
/// 冰勺弩枪
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
    /// 加载技能，此处仅加载普通攻击，具体技能加载实现请在子类中重写
    /// </summary>
    public override void LoadSkillAbility()
    {
        //List<SkillAbility.SkillAbilityInfo> infoList = AbilityManager.Instance.GetSkillAbilityInfoList(UnitType.Weapons, mType, mShape);
        // 普通攻击
        weaponsGeneralAttackSkillAbility = new WeaponsGeneralAttackSkillAbility(this, new SkillAbility.SkillAbilityInfo()
        {
            name = "普通攻击",
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
    /// 判断是否有有效的攻击目标
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

        //// 单行索敌
        //List<BaseUnit> list = new List<BaseUnit>();
        //// 筛选出高度为0的可选取单位
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
    /// 执行具体的攻击，位于伤害判定为真之后
    /// </summary>
    public override void ExecuteDamage()
    {
        //GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        //// 选择目标
        //BaseUnit target = PitcherManager.FindTargetByPitcher(master, transform.position.x - MapManager.gridWidth / 2, GetRowIndex());
        //CreateBullet(transform.position, target);
        GameManager.Instance.audioSourceController.PlayEffectMusic("Throw" + GameManager.Instance.rand.Next(0, 2));
        // 选择目标
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
    /// 创建一发弹射子弹
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, Vector2 endPos)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, IceEggBullet_RuntimeAnimatorController, master, 0);
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // 出屏不自删
        b.SetHitSoundEffect("Eggimpact" + GameManager.Instance.rand.Next(0, 2));

        b.AddHitAction((b, u) =>
        {
            Vector2 pos = b.transform.position; // 范围冰冻减速效果中心位置
            // 创建一个范围冰冻减速效果
            CreateIceSlowDownArea(pos);
        });
        PitcherManager.AddDefaultFlyTask(b, startPosition, endPos, true, false);
        GameController.Instance.AddBullet(b);
        return b;
    }

    /// <summary>
    /// 创建一发弹射子弹
    /// </summary>
    /// <param name="startPosition"></param>
    /// <param name="ori_dmg"></param>
    /// <param name="target"></param>
    private BaseBullet CreateBullet(Vector2 startPosition, BaseUnit target)
    {
        AllyBullet b = AllyBullet.GetInstance(BulletStyle.Throwing, IceEggBullet_RuntimeAnimatorController, master, 0);
        b.SetHitSoundEffect("Eggimpact" + GameManager.Instance.rand.Next(0, 2));
        b.AddSpriteOffsetY(new FloatModifier(0.5f * MapManager.gridHeight));
        b.isnDelOutOfBound = true; // 出屏不自删
        // 冰勺击中效果
        Action<BaseBullet, BaseUnit> hitEnemyAction = null;
        {
            hitEnemyAction = (b, u) =>
            {
                Vector2 pos = b.transform.position; // 范围冰冻减速效果中心位置
                // 创建一个范围冰冻减速效果
                CreateIceSlowDownArea(pos);
            };
        }

        // 这里判断目标是友方还是敌方，还是没有，根据这些情况来制定逻辑
        if (target != null && target is FoodUnit && target.mType == (int)FoodNameTypeMap.CherryPudding)
        {
            // 目标是友方布丁
            CherryPuddingFoodUnit pudding = target.GetComponent<CherryPuddingFoodUnit>();

            // 添加目标布丁存活监听
            CustomizationTask t = new CustomizationTask();
            t.AddTaskFunc(delegate
            {
                return !pudding.IsAlive();
            });
            t.AddOnExitAction(delegate
            {
                pudding = null; // 如果目标布丁不存活，则取消其引用
            });
            b.AddTask(t);

            // 定义与添加重定向任务
            b.AddHitAction((b, u) => {
                // 重定向
                if (pudding != null)
                {
                    BaseUnit next_target = pudding.FindRedirectThrowingObjectTarget(b);
                    if (next_target != null)
                    {
                        b.isnUseHitEffect = true; // 当前子弹不采用击中动画，直接消失
                        CreateBullet(b.transform.position, next_target);
                    }
                    else
                    {
                        // 原地破裂，但同样会触发范围效果
                        hitEnemyAction(b, null);
                    }
                }
            });

            // 添加抛物线运动
            PitcherManager.AddDefaultFlyTask(b, startPosition, target, true, false);
        }
        else
        {
            // 目标是敌方，直接添加即可
            b.AddHitAction(hitEnemyAction);

            // 确定好参数后添加抛物线运动
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
        // 原地产生一个减速效果
        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(pos, 3, 3, "ItemCollideEnemy");
        r.SetInstantaneous();
        r.isAffectMouse = true;
        r.SetAffectHeight(0); // 只有地面单位被影响
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
    /// 寻找目标
    /// </summary>
    public void SearchTargetPosition()
    {
        List<BaseUnit> list = new List<BaseUnit>();
        // 筛选出高度为0的可选取单位
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
