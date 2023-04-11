using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
/// <summary>
/// 发光霉菌
/// </summary>
public class LuminescentMold : FoodUnit
{
    private static BoolModifier InfectModifier = new BoolModifier(true);

    /// <summary>
    /// 挂载即死
    /// </summary>
    private static Action<CombatAction> ImmediatelyDeath = (action)=> {
        if(action is DamageAction)
        {
            DamageAction dmgAction = action as DamageAction;
            // 对1格内的攻击者造成10%最大生命值的无来源伤害,并施加1.5s的晕眩效果
            if (dmgAction.Creator != null 
            && (dmgAction.Creator.transform.position - dmgAction.Target.transform.position).magnitude <= MapManager.gridWidth
            && !(dmgAction.Creator is MouseUnit && dmgAction.Creator.mType == (int)MouseNameTypeMap.WonderLandMole)) // 奇境鼹鼠免疫这个效果
            {
                new DamageAction(CombatAction.ActionType.CauseDamage, null, dmgAction.Creator, 0.10f*dmgAction.Creator.mMaxHp).ApplyAction();
                dmgAction.Creator.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(dmgAction.Creator, 90, true));
            }
            // 自身死亡
            dmgAction.Target.ExecuteDeath();
        }
    };

    /// <summary>
    /// 挂载非灰烬死亡前事件（扩散）
    /// </summary>
    private static Action<BaseUnit> SpreadDeathEvent = (unit) =>
    {
        int totalTime = 360;
        int timeLeft = totalTime;
        Vector2 startPosition = unit.transform.position;
        List<BaseEffect> effectList = new List<BaseEffect>();
        for (int i = 0; i < 4; i ++)
        {
            effectList.Add(BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Food/32/Fog"), "Appear", "Idle", "Die", true));
            effectList[i].SetSpriteRendererSorting("Grid", 1);
            GameController.Instance.AddEffect(effectList[i]);
        }
        List<Vector2> posList = new List<Vector2>()
        {
            startPosition + new Vector2(-MapManager.gridWidth, 0),
            startPosition + new Vector2(MapManager.gridWidth, 0),
            startPosition + new Vector2(0, -MapManager.gridHeight),
            startPosition + new Vector2(0, MapManager.gridHeight),
        };

        Tasker t = GameController.Instance.AddTasker(
            // Init
            delegate { }, 
            // Update
            delegate 
            {
                float r = 1 - (float)timeLeft / totalTime;
                for (int i = 0; i < 4; i++)
                {
                    effectList[i].transform.position = Vector2.Lerp(startPosition, posList[i], r);
                }
                timeLeft--;
            },
            // ExitCondition
            delegate { return timeLeft <= 0; },
            // Exit
            delegate {
                for (int i = 0; i < 4; i++)
                {
                    effectList[i].ExecuteDeath();

                    BaseGrid g = GameController.Instance.mMapController.GetGrid(MapManager.GetXIndex(posList[i].x), MapManager.GetYIndex(posList[i].y));
                    if (g != null)
                    {
                        if (CanInfectGrid(g))
                            InfectGrid(g);
                    }
                }
            });
    };

    public override void MInit()
    {
        base.MInit();
        SetActionState(new TransitionState(this));
        // 自身感染霉菌（主要是为了获得其被动效果）
        InfectUnit(this);
    }

    public override float OnDamage(float dmg)
    {
        if(dmg > 0)
        {
            ExecuteDeath(); 
        }
        return dmg;
    }

    public override float OnBombBurnDamage(float dmg)
    {
        if(dmg > 0)
            ExecuteBurn();
        return dmg;
    }

    public override void OnTransitionStateEnter()
    {
        animatorController.Play("Appear");
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            SetActionState(new IdleState(this));
    }

    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true);
    }

    public override void OnDieStateEnter()
    {
        animatorController.Play("Die");
    }

    public override void DuringDeath()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
            DeathEvent();
    }

    /// <summary>
    /// 是否可以感染霉菌
    /// 美食必须是普通或者护罩类型的，并且不附带<毒菌感染>也不免疫<毒菌感染>
    /// 当然，它不可以是霉菌（笑）
    /// </summary>
    /// <returns></returns>
    public static bool CanInfectUnit(FoodUnit unit)
    {
        return (unit.GetFoodInGridType().Equals(FoodInGridType.Default) || unit.GetFoodInGridType().Equals(FoodInGridType.Shield))
        && !unit.NumericBox.GetBoolNumericValue(StringManager.BacterialInfection)
        && !unit.NumericBox.GetBoolNumericValue(StringManager.IgnoreBacterialInfection) 
        && unit.mType != (int)FoodNameTypeMap.LuminescentMold;
    }

    /// <summary>
    /// 使某个单位感染霉菌
    /// </summary>
    /// <param name="unit"></param>
    public static void InfectUnit(FoodUnit unit)
    {
        unit.NumericBox.AddDecideModifierToBoolDict(StringManager.BacterialInfection, InfectModifier);
        unit.AddBeforeDeathEvent(SpreadDeathEvent);
        unit.AddActionPointListener(ActionPointType.PostReceiveDamage, ImmediatelyDeath);
        if(unit.mType != (int)FoodNameTypeMap.LuminescentMold)
            EffectManager.AddMushroomEffectToUnit(unit);
    }

    /// <summary>
    /// 使某个单位解除霉菌感染
    /// </summary>
    /// <returns>是否解除成功</returns>
    public static bool CureUnit(FoodUnit unit)
    {
        bool flag = false;
        if (unit.NumericBox.GetBoolNumericValue(StringManager.BacterialInfection))
        {
            unit.NumericBox.RemoveDecideModifierToBoolDict(StringManager.BacterialInfection, InfectModifier);
            unit.RemoveBeforeDeathEvent(SpreadDeathEvent);
            unit.RemoveActionPointListener(ActionPointType.PostReceiveDamage, ImmediatelyDeath);
            flag = true;
        }
        // 如果目标就是霉菌则直接去世（灰飞烟灭），否则移除相关特效
        if (unit.mType == (int)FoodNameTypeMap.LuminescentMold)
        {
            unit.ExecuteBurn();
        }else
            EffectManager.RemoveMushroomEffectFromUnit(unit);
        return flag;
    }

    /// <summary>
    /// 能否感染格子
    /// </summary>
    /// <returns></returns>
    public static bool CanInfectGrid(BaseGrid g)
    {
        // 下面是蘑菇不能生长的格子
        if (g.IsContainGridType(GridType.NotBuilt)
            || g.IsContainGridType(GridType.Water)
            || g.IsContainGridType(GridType.Lava)
            || (g.IsContainTag(FoodInGridType.Shield) && g.GetFoodByTag(FoodInGridType.Shield).mType == (int)FoodNameTypeMap.PokerShield)) // 扑克护罩所在的格直接不能被感染
        {
            return false;
        }
        else
            return true;
    }

    /// <summary>
    /// 感染格子
    /// </summary>
    /// <param name="g"></param>
    public static void InfectGrid(BaseGrid g)
    {
        bool flag = false;
        if (g.IsContainTag(FoodInGridType.Default))
        {
            if(CanInfectUnit(g.GetFoodByTag(FoodInGridType.Default)))
                InfectUnit(g.GetFoodByTag(FoodInGridType.Default));
            flag = true;
        }
        if (g.IsContainTag(FoodInGridType.Shield))
        {
            if(CanInfectUnit(g.GetFoodByTag(FoodInGridType.Shield)))
                InfectUnit(g.GetFoodByTag(FoodInGridType.Shield));
            flag = true;
        }
        // 如果没有可感染的美食，那就在原地生成一个蘑菇
        if (!flag)
            GameController.Instance.CreateFoodUnit(FoodNameTypeMap.LuminescentMold, 0, 0, g);
    }
}
