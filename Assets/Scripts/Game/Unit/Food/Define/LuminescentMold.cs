using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
/// <summary>
/// ����ù��
/// </summary>
public class LuminescentMold : FoodUnit
{
    private static BoolModifier InfectModifier = new BoolModifier(true);

    /// <summary>
    /// ���ؼ���
    /// </summary>
    private static Action<CombatAction> ImmediatelyDeath = (action)=> {
        if(action is DamageAction)
        {
            DamageAction dmgAction = action as DamageAction;
            // ��1���ڵĹ��������10%�������ֵ������Դ�˺�,��ʩ��1.5s����ѣЧ��
            if (dmgAction.Creator != null 
            && (dmgAction.Creator.transform.position - dmgAction.Target.transform.position).magnitude <= MapManager.gridWidth
            && !(dmgAction.Creator is MouseUnit && dmgAction.Creator.mType == (int)MouseNameTypeMap.WonderLandMole)) // �澳�����������Ч��
            {
                new DamageAction(CombatAction.ActionType.CauseDamage, null, dmgAction.Creator, 0.10f*dmgAction.Creator.mMaxHp).ApplyAction();
                dmgAction.Creator.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(dmgAction.Creator, 90, true));
            }
            // ��������
            dmgAction.Target.ExecuteDeath();
        }
    };

    /// <summary>
    /// ���طǻҽ�����ǰ�¼�����ɢ��
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
        // �����Ⱦù������Ҫ��Ϊ�˻���䱻��Ч����
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
    /// �Ƿ���Ը�Ⱦù��
    /// ��ʳ��������ͨ���߻������͵ģ����Ҳ�����<������Ⱦ>Ҳ������<������Ⱦ>
    /// ��Ȼ������������ù����Ц��
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
    /// ʹĳ����λ��Ⱦù��
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
    /// ʹĳ����λ���ù����Ⱦ
    /// </summary>
    /// <returns>�Ƿ����ɹ�</returns>
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
        // ���Ŀ�����ù����ֱ��ȥ�����ҷ����𣩣������Ƴ������Ч
        if (unit.mType == (int)FoodNameTypeMap.LuminescentMold)
        {
            unit.ExecuteBurn();
        }else
            EffectManager.RemoveMushroomEffectFromUnit(unit);
        return flag;
    }

    /// <summary>
    /// �ܷ��Ⱦ����
    /// </summary>
    /// <returns></returns>
    public static bool CanInfectGrid(BaseGrid g)
    {
        // ������Ģ�����������ĸ���
        if (g.IsContainGridType(GridType.NotBuilt)
            || g.IsContainGridType(GridType.Water)
            || g.IsContainGridType(GridType.Lava)
            || (g.IsContainTag(FoodInGridType.Shield) && g.GetFoodByTag(FoodInGridType.Shield).mType == (int)FoodNameTypeMap.PokerShield)) // �˿˻������ڵĸ�ֱ�Ӳ��ܱ���Ⱦ
        {
            return false;
        }
        else
            return true;
    }

    /// <summary>
    /// ��Ⱦ����
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
        // ���û�пɸ�Ⱦ����ʳ���Ǿ���ԭ������һ��Ģ��
        if (!flag)
            GameController.Instance.CreateFoodUnit(FoodNameTypeMap.LuminescentMold, 0, 0, g);
    }
}
