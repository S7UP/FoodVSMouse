
using UnityEngine;
using System;
/// <summary>
/// ��������
/// </summary>
public class FogAreaEffectExecution : RetangleAreaEffectExecution
{
    private const string TaskName = "FogTask"; // ר���������� 
    private SpriteRenderer spriteRenderer;
    private bool isOpen;
    private float alpha;

    public override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void MInit()
    {
        isOpen = false;
        alpha = 0;
        spriteRenderer.color = new Color(1, 1, 1, alpha);
        base.MInit();

    }

    public override void MUpdate()
    {
        if (isOpen)
        {
            alpha = Mathf.Min(alpha + 0.02f, 1f);
        }
        else
        {
            alpha = Mathf.Max(alpha - 0.02f, 0f);
        }
        spriteRenderer.color = new Color(1, 1, 1, alpha);
        base.MUpdate();
    }

    public override bool IsMeetingCondition(BaseUnit unit)
    {
        // ��BOSS��λ��Ч
        if(unit is MouseUnit)
        {
            MouseUnit m = unit as MouseUnit;
            if (m.IsBoss())
                return false;
        }
        return isOpen && alpha>=1f && base.IsMeetingCondition(unit);
    }

    /// <summary>
    /// ����
    /// </summary>
    public void SetOpen()
    {
        isOpen = true;
    }

    /// <summary>
    /// �رգ�ͬʱ�ͷ����������еĵ�λ��
    /// </summary>
    public void SetClose()
    {
        isOpen = false;
        foreach (var item in foodUnitList)
        {
            OnFoodExit(item);
        }
        foodUnitList.Clear();
        foreach (var item in mouseUnitList)
        {
            OnEnemyExit(item);
        }
        mouseUnitList.Clear();
        foreach (var item in characterList)
        {
            OnCharacterExit(item);
        }
        characterList.Clear();
    }

    public override void OnEnemyEnter(MouseUnit unit)
    {
        OnUnitEnter(UnitType.Mouse, unit);
        base.OnEnemyEnter(unit);
    }

    public override void OnEnemyExit(MouseUnit unit)
    {
        OnUnitExit(unit);
        base.OnEnemyExit(unit);
    }

    public override void OnFoodEnter(FoodUnit unit)
    {
        OnUnitEnter(UnitType.Food, unit);
        base.OnFoodEnter(unit);
    }

    public override void OnFoodExit(FoodUnit unit)
    {
        OnUnitExit(unit);
        base.OnFoodExit(unit);
    }

    public override void OnCharacterEnter(CharacterUnit unit)
    {
        OnUnitEnter(UnitType.Character, unit);
        base.OnCharacterEnter(unit);
    }

    public override void OnCharacterExit(CharacterUnit unit)
    {
        OnUnitExit(unit);
        base.OnCharacterExit(unit);
    }

    public void OnUnitEnter(UnitType type, BaseUnit unit)
    {
        // ��ȡĿ������Ψһ���������
        FogTask t = null;
        if (unit.GetTask(TaskName) == null)
        {
            t = new FogTask(type, unit);
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as FogTask;
            t.AddCount();
        }
    }

    public void OnUnitExit(BaseUnit unit)
    {
        // ��ȡĿ������Ψһ���������
        if (unit.GetTask(TaskName) == null)
        {
            Debug.LogWarning("Ŀ����û��������������¾��˳����������");
        }
        else
        {
            FogTask t = unit.GetTask(TaskName) as FogTask;
            t.DecCount();
        }
    }

    public static FogAreaEffectExecution GetInstance(Vector2 position)
    {
        FogAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/FogAreaEffect").GetComponent<FogAreaEffectExecution>();
        e.MInit();
        e.Init(0, 1, 1, 0, 0, true, true);
        e.SetBoxCollider2D(Vector2.zero, new Vector2(0.51f * MapManager.gridWidth, 0.51f * MapManager.gridHeight));
        e.isAffectCharacter = true;
        e.transform.position = position;
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/FogAreaEffect", gameObject);
    }


    // ��������
    private class FogTask : ITask
    {
        private static FloatModifier mouseMoveSpeedModifier = new FloatModifier(-20); // ���������м���

        // ��ֹ�赲����
        private static Func<BaseUnit, BaseUnit, bool> noBlockFunc = delegate 
        {
            return false;
        };
        // ��ֹ����Ļ��������
        private static Func<BaseUnit, BaseBullet, bool> noHitFunc = delegate
        {
            return false;
        };

        private int count; // �����������
        private BaseUnit unit;
        private UnitType type;

        public FogTask(UnitType type, BaseUnit unit)
        {
            this.type = type;
            this.unit = unit;
        }

        public void OnEnter()
        {
            count = 1;
            if (type == UnitType.Food || type == UnitType.Character)
            {

            }
            else if(type == UnitType.Mouse)
            {
                // ����
                unit.NumericBox.MoveSpeed.AddFinalPctAddModifier(mouseMoveSpeedModifier);
                // ����ȡ��Ŀ����赲״̬
                MouseUnit m = unit as MouseUnit;
                m.SetNoCollideAllyUnit();
            }
            // ʹ��Ŀ��Ȳ��ɱ��赲Ҳ���ɱ���Ļ����
            unit.AddCanBlockFunc(noBlockFunc);
            unit.AddCanHitFunc(noHitFunc);
            // ���������Ч
            BaseEffect e = BaseEffect.CreateInstance(GameManager.Instance.GetRuntimeAnimatorController("Effect/HiddenEffect"), "Appear", "Idle", "Disappear", true);
            e.SetSpriteRendererSorting("Effect", 2);
            GameController.Instance.AddEffect(e);
            unit.AddEffectToDict(EffectType.Hidden, e, new Vector2(0, 0*0.5f * MapManager.gridWidth));
        }

        public void OnUpdate()
        {
            
        }

        public bool IsMeetingExitCondition()
        {
            return count == 0;
        }

        public void OnExit()
        {
            if (type == UnitType.Food || type == UnitType.Character)
            {

            }
            else if (type == UnitType.Mouse)
            {
                unit.NumericBox.MoveSpeed.RemoveFinalPctAddModifier(mouseMoveSpeedModifier);
            }
            // ȡ����������
            unit.RemoveCanBlockFunc(noBlockFunc);
            unit.RemoveCanHitFunc(noHitFunc);
            unit.RemoveEffectFromDict(EffectType.Hidden);
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
