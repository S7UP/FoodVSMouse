
using UnityEngine;
using Environment;
/// <summary>
/// �ҽ�����
/// </summary>
public class LavaAreaEffectExecution : RetangleAreaEffectExecution
{
    private const string TaskName = "LavaTask"; // ר���������� 
    private SpriteRenderer spriteRenderer;
    private bool isOpen;
    private bool isDisappear;
    private float size;

    public override void Awake()
    {
        base.Awake();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void MInit()
    {
        isOpen = false;
        isDisappear = false;
        size = 0;
        spriteRenderer.transform.localScale = Vector2.zero;
        base.MInit();
        AddFoodEnterConditionFunc((u) => {
            return isOpen && size >= 1f && u.GetHeight()<=0;
        });
        AddEnemyEnterConditionFunc((m) => {
            // ��BOSS��λ��Ч
            if (m.IsBoss())
                return false;
            return isOpen && size >= 1f && m.GetHeight() <= 0;
        });
        AddCharacterEnterConditionFunc((u) => {
            return isOpen && size >= 1f && u.GetHeight() <= 0;
        });
    }

    public override void MUpdate()
    {
        if (isOpen)
        {
            size = Mathf.Min(size + 0.02f, 1f);
        }
        else
        {
            size = Mathf.Max(size - 0.02f, 0f);
            if(size == 0 && isDisappear)
            {
                MDestory();
                return;
            }
        }
        spriteRenderer.transform.localScale = new Vector2(size, size);
        base.MUpdate();
    }

    /// <summary>
    /// ����
    /// </summary>
    public void SetOpen()
    {
        isOpen = true;
    }

    /// <summary>
    /// �رգ�
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

    /// <summary>
    /// ʹ�Լ���ʧ����С�𽥽��ͣ���0ʱ�����Լ�
    /// </summary>
    public void SetDisappear()
    {
        SetClose();
        isDisappear = true;
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
        LavaTask t;
        if (unit.GetTask(TaskName) == null)
        {
            t = new LavaTask(unit);
            unit.AddUniqueTask(TaskName, t);
        }
        else
        {
            t = unit.GetTask(TaskName) as LavaTask;
            t.AddCount();
        }
    }

    public void OnUnitExit(BaseUnit unit)
    {
        if (unit.GetTask(TaskName) != null)
        {
            LavaTask t = unit.GetTask(TaskName) as LavaTask;
            t.DecCount();
        }
        else
        {
            Debug.LogWarning("Ϊʲô�ж�������û���ҽ�������ҽ���");
        }
    }

    public static LavaAreaEffectExecution GetInstance(Vector2 position)
    {
        LavaAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/LavaAreaEffect").GetComponent<LavaAreaEffectExecution>();
        e.MInit();
        e.Init(0, 1, 1, 0, 0, true, true);
        e.SetBoxCollider2D(Vector2.zero, new Vector2(0.51f * MapManager.gridWidth, 0.51f * MapManager.gridHeight));
        e.isAffectCharacter = true;
        e.transform.position = position;
        e.SetCollisionLayer("BothCollide");
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/LavaAreaEffect", gameObject);
    }
}
