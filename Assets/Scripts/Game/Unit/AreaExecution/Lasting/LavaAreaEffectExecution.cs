
using UnityEngine;
using Environment;
/// <summary>
/// 岩浆区域
/// </summary>
public class LavaAreaEffectExecution : RetangleAreaEffectExecution
{
    private const string TaskName = "LavaTask"; // 专属的任务名 
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
            // 对BOSS单位无效
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
    /// 启用
    /// </summary>
    public void SetOpen()
    {
        isOpen = true;
    }

    /// <summary>
    /// 关闭（
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
    /// 使自己消失：大小逐渐降低，到0时回收自己
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
            Debug.LogWarning("为什么有东西可以没带岩浆任务出岩浆？");
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
