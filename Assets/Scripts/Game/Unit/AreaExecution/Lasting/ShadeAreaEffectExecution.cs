
using UnityEngine;
/// <summary>
/// �������
/// </summary>
public class ShadeAreaEffectExecution : RetangleAreaEffectExecution
{
    private const string TaskName = "ShadeTask"; // ר���������� 

    public override void Awake()
    {
        base.Awake();
    }

    public override void MInit()
    {

        base.MInit();
    }

    public override void MUpdate()
    {
        base.MUpdate();
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
        ShadeTask t = null;
        if (unit.GetTask(TaskName) == null)
        {
            t = new ShadeTask(type, unit);
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as ShadeTask;
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
            ShadeTask t = unit.GetTask(TaskName) as ShadeTask;
            t.DecCount();
        }
    }

    public static ShadeAreaEffectExecution GetInstance(int col, int row, Vector2 position)
    {
        ShadeAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/ShadeAreaEffect").GetComponent<ShadeAreaEffectExecution>();
        e.MInit();
        e.Init(0, col, row, 0, 0, true, true);
        e.SetBoxCollider2D(Vector2.zero, new Vector2(0.95f * col * MapManager.gridWidth, 0.95f* row * MapManager.gridHeight));
        e.isAffectCharacter = true;
        e.transform.position = position;
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/ShadeAreaEffect", gameObject);
    }


    // �������
    private class ShadeTask : ITask
    {
        private static FloatModifier attackSpeedModifier = new FloatModifier(20);  // ���е�λ����20%����
        private static FloatModifier foodDamageRate = new FloatModifier(1.5f); // ��ʳ��������
        private static FloatModifier mouseDamageRate = new FloatModifier(0.8f); // �������

        private int count; // ����������
        private BaseUnit unit;
        private UnitType type;
        private bool lastBlock; // ���������λ����һ֡�Ƿ����赲״̬

        public ShadeTask(UnitType type, BaseUnit unit)
        {
            this.type = type;
            this.unit = unit;
        }

        public void OnEnter()
        {
            count = 1;
            if (type == UnitType.Food || type == UnitType.Character)
            {
                unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
                unit.NumericBox.DamageRate.AddModifier(foodDamageRate);
            }
            else if(type == UnitType.Mouse)
            {
                unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
                unit.NumericBox.DamageRate.AddModifier(mouseDamageRate);
            }
        }

        public void OnUpdate()
        {
            if(type == UnitType.Mouse)
            {
                MouseUnit m = unit as MouseUnit;
                bool isBlock = m.IsBlock();
                if (lastBlock != isBlock)
                {
                    // �������赲ʱ����ʱ�Ƴ��˺�����Ч��
                    if (isBlock)
                    {
                        unit.NumericBox.DamageRate.RemoveModifier(mouseDamageRate);
                    }
                    else
                    {
                        unit.NumericBox.DamageRate.RemoveModifier(mouseDamageRate);
                        unit.NumericBox.DamageRate.AddModifier(mouseDamageRate);
                    }
                }
                lastBlock = isBlock;
            }
            
        }

        public bool IsMeetingExitCondition()
        {
            return count == 0;
        }

        public void OnExit()
        {
            if (type == UnitType.Food || type == UnitType.Character)
            {
                unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
                unit.NumericBox.DamageRate.RemoveModifier(foodDamageRate);
            }
            else if (type == UnitType.Mouse)
            {
                unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
                unit.NumericBox.DamageRate.RemoveModifier(mouseDamageRate);
            }
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
