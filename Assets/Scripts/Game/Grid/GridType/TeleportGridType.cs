using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// 传送机地形
/// </summary>
public class TeleportGridType : BaseGridType
{
    private const string NoAffectKey = "NoAffectByTeleportGridType"; // 不会被传送的标志
    private static BoolModifier NoAffectMod = new BoolModifier(true);
    private const string TpTaskKey = "TeleportGridType_TpTask"; // 对老鼠唯一任务名
    private const string WindCaveTaskKey = "TeleportGridType_WindCaveTask"; // 对美食唯一任务名

    private static RuntimeAnimatorController Tp_AnimatorController;
    private static List<MouseNameTypeMap> NoEffectMouseTypeList = new List<MouseNameTypeMap>()
    { 
        MouseNameTypeMap.GhostMouse
    };

    public bool isOpen = false; // 是否为开启状态

    public override void Awake()
    {
        if (Tp_AnimatorController == null)
            Tp_AnimatorController = GameManager.Instance.GetRuntimeAnimatorController("GridType/Teleport/0");
        base.Awake();
        animator = GetComponent<Animator>();
    }

    public override void MInit()
    {
        isOpen = false;
        base.MInit();
        animator.runtimeAnimatorController = Tp_AnimatorController;
        animatorController.Play("Idle", true);
    }

    /// <summary>
    /// 是否满足进入条件
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS单位无视地形效果
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
            // 只允许高度为0 且 不属于不可生效的单位表中
            return unit.GetHeight() == 0 && !NoEffectMouseTypeList.Contains((MouseNameTypeMap)m.mType);
        }else if(unit is FoodUnit || unit is CharacterUnit)
        {
            // 美食或人物单位直接进入
            return true;
        }
        return false;
    }

    /// <summary>
    /// 当有单位进入地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitEnter(BaseUnit unit)
    {
        if(unit is FoodUnit || unit is CharacterUnit)
        {
            if (isOpen)
            {
                AddTaskCountToAlly(unit);
            }
        }
    }

    /// <summary>
    /// 当有单位处于地形时持续给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {
        if (unit is MouseUnit)
        {
            if (isOpen && !IsTpFlying(unit) && !UnitManager.IsFlying(unit))
            {
                // 飞
                ExecuteTp(unit);
            }
        }else if (unit is FoodUnit || unit is CharacterUnit)
        {

        }
    }

    /// <summary>
    /// 当有单位离开地形时施加给单位的效果
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitExit(BaseUnit unit)
    {
        DecTaskCountToAlly(unit);
    }

    private void AddTaskCountToAlly(BaseUnit unit)
    {
        WindCaveTask t;
        if (unit.GetTask(WindCaveTaskKey) == null)
        {
            t = new WindCaveTask(unit);
            unit.AddUniqueTask(WindCaveTaskKey, t);
        }
        else
        {
            t = unit.GetTask(WindCaveTaskKey) as WindCaveTask;
            t.AddCount();
        }
    }

    private void DecTaskCountToAlly(BaseUnit unit)
    {
        WindCaveTask t;
        if (unit.GetTask(WindCaveTaskKey) != null)
        {
            t = unit.GetTask(WindCaveTaskKey) as WindCaveTask;
            t.DecCount();
        }
    }

    /// <summary>
    /// 执行一次传送
    /// </summary>
    private void ExecuteTp(BaseUnit m)
    {
        float moveDistance = MapManager.gridWidth * 3.5f;
        // 添加一个弹起的任务
        CustomizationTask t = TaskManager.AddParabolaTask(m, TransManager.TranToVelocity(12), moveDistance/2, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false);
        // 且禁止移动
        t.AddOnEnterAction(delegate {
            m.DisableMove(true);
        });
        t.AddOnExitAction(delegate {
            m.DisableMove(false);
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 360, false)); // 目标在落地后晕眩数秒
            new DamageAction(CombatAction.ActionType.RealDamage, null, m, 0.25f * m.GetCurrentHp()).ApplyAction();
        });
        m.AddUniqueTask(TpTaskKey, t);
    }

    public override void MUpdate()
    {
        base.MUpdate();
    }

    public void SetOpen(bool isOpen)
    {
        if(this.isOpen != isOpen)
        {
            this.isOpen = isOpen;
            if (isOpen)
            {
                // 从关闭到打开
                animatorController.Play("TP", true);
                foreach (var unit in unitList)
                {
                    if (unit is FoodUnit || unit is CharacterUnit)
                        AddTaskCountToAlly(unit);
                }
            }
            else
            {
                // 从打开到关闭
                animatorController.Play("Idle", true);
                foreach (var unit in unitList)
                {
                    if (unit is FoodUnit || unit is CharacterUnit)
                        DecTaskCountToAlly(unit);
                }
            }
        }
    }

    public static void AddNoAffectMod(BaseUnit u)
    {
        u.NumericBox.AddDecideModifierToBoolDict(NoAffectKey, NoAffectMod);
    }

    public static void RemoveNoAffectMod(BaseUnit u)
    {
        u.NumericBox.RemoveDecideModifierToBoolDict(NoAffectKey, NoAffectMod);
    }

    public static bool IsTpFlying(BaseUnit u)
    {
        return u.GetTask(TpTaskKey) != null;
    }

    /// <summary>
    /// 风洞BUFF任务
    /// </summary>
    private class WindCaveTask : ITask
    {
        // 增益
        private FloatModifier attackMod = new FloatModifier(50);
        private FloatModifier attackSpeedMod = new FloatModifier(50);
        private FloatModifier yMod = new FloatModifier(0);
        private float yPos = 0;
        private float ayPos = 0;
        private const int totalTime = 30;
        private int currentTime = 0;
        private Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTarget = delegate { return false; };


        private int count; // 进入的地形数
        private BaseUnit unit;

        public WindCaveTask(BaseUnit unit)
        {
            this.unit = unit;
        }

        public void OnEnter()
        {
            yPos = 0.35f*MapManager.gridHeight;
            ayPos = 0.05f * MapManager.gridHeight;
            count = 1;
            unit.NumericBox.Attack.AddPctAddModifier(attackMod);
            unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedMod);
            unit.AddCanBeSelectedAsTargetFunc(noBeSelectedAsTarget);
        }

        public void OnUpdate()
        {
            if(count <= 0)
            {
                currentTime = Mathf.Max(0, currentTime - 1);
            }
            else
            {
                currentTime = Mathf.Min(currentTime+1, totalTime);
            }

            float rate = (float)currentTime / totalTime;
            yMod.Value = ((yPos + ayPos*Mathf.Sin(6*(float)unit.aliveTime/180*Mathf.PI))*rate*rate);
            unit.RemoveSpriteOffsetY(yMod);
            unit.AddSpriteOffsetY(yMod);
        }

        public bool IsMeetingExitCondition()
        {
            return count <= 0 && currentTime == 0;
        }

        public void OnExit()
        {
            unit.RemoveSpriteOffsetY(yMod);
            unit.NumericBox.Attack.RemovePctAddModifier(attackMod);
            unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedMod);
            unit.RemoveCanBeSelectedAsTargetFunc(noBeSelectedAsTarget);
        }

        // 自定义方法
        public void AddCount()
        {
            count++;
        }

        public void DecCount()
        {
            count--;
        }
    }
}
