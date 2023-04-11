using System;
using System.Collections.Generic;

using S7P.Numeric;

using UnityEngine;
/// <summary>
/// ���ͻ�����
/// </summary>
public class TeleportGridType : BaseGridType
{
    private const string NoAffectKey = "NoAffectByTeleportGridType"; // ���ᱻ���͵ı�־
    private static BoolModifier NoAffectMod = new BoolModifier(true);
    private const string TpTaskKey = "TeleportGridType_TpTask"; // ������Ψһ������
    private const string WindCaveTaskKey = "TeleportGridType_WindCaveTask"; // ����ʳΨһ������

    private static RuntimeAnimatorController Tp_AnimatorController;
    private static List<MouseNameTypeMap> NoEffectMouseTypeList = new List<MouseNameTypeMap>()
    { 
        MouseNameTypeMap.GhostMouse
    };

    public bool isOpen = false; // �Ƿ�Ϊ����״̬

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
    /// �Ƿ������������
    /// </summary>
    /// <returns></returns>
    public override bool IsMeetingEnterCondition(BaseUnit unit)
    {
        // BOSS��λ���ӵ���Ч��
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
            // ֻ����߶�Ϊ0 �� �����ڲ�����Ч�ĵ�λ����
            return unit.GetHeight() == 0 && !NoEffectMouseTypeList.Contains((MouseNameTypeMap)m.mType);
        }else if(unit is FoodUnit || unit is CharacterUnit)
        {
            // ��ʳ�����ﵥλֱ�ӽ���
            return true;
        }
        return false;
    }

    /// <summary>
    /// ���е�λ�������ʱʩ�Ӹ���λ��Ч��
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
    /// ���е�λ���ڵ���ʱ��������λ��Ч��
    /// </summary>
    /// <param name="unit"></param>
    public override void OnUnitStay(BaseUnit unit)
    {
        if (unit is MouseUnit)
        {
            if (isOpen && !IsTpFlying(unit) && !UnitManager.IsFlying(unit))
            {
                // ��
                ExecuteTp(unit);
            }
        }else if (unit is FoodUnit || unit is CharacterUnit)
        {

        }
    }

    /// <summary>
    /// ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
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
    /// ִ��һ�δ���
    /// </summary>
    private void ExecuteTp(BaseUnit m)
    {
        float moveDistance = MapManager.gridWidth * 3.5f;
        // ���һ�����������
        CustomizationTask t = TaskManager.AddParabolaTask(m, TransManager.TranToVelocity(12), moveDistance/2, m.transform.position, m.transform.position + (Vector3)m.moveRotate * moveDistance, false);
        // �ҽ�ֹ�ƶ�
        t.AddOnEnterAction(delegate {
            m.DisableMove(true);
        });
        t.AddOnExitAction(delegate {
            m.DisableMove(false);
            m.AddNoCountUniqueStatusAbility(StringManager.Stun, new StunStatusAbility(m, 360, false)); // Ŀ������غ���ѣ����
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
                // �ӹرյ���
                animatorController.Play("TP", true);
                foreach (var unit in unitList)
                {
                    if (unit is FoodUnit || unit is CharacterUnit)
                        AddTaskCountToAlly(unit);
                }
            }
            else
            {
                // �Ӵ򿪵��ر�
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
    /// �綴BUFF����
    /// </summary>
    private class WindCaveTask : ITask
    {
        // ����
        private FloatModifier attackMod = new FloatModifier(50);
        private FloatModifier attackSpeedMod = new FloatModifier(50);
        private FloatModifier yMod = new FloatModifier(0);
        private float yPos = 0;
        private float ayPos = 0;
        private const int totalTime = 30;
        private int currentTime = 0;
        private Func<BaseUnit, BaseUnit, bool> noBeSelectedAsTarget = delegate { return false; };


        private int count; // ����ĵ�����
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

        // �Զ��巽��
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
