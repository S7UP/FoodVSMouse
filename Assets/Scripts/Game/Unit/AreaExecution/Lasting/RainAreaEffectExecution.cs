using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 下雨区域
/// </summary>
public class RainAreaEffectExecution : RetangleAreaEffectExecution
{
    private Animator animator;
    private SpriteRenderer Bg_SpriteRender;

    private FloatModifier attackSpeedModifier = new FloatModifier(20);
    private FloatModifier moveSpeedModifier = new FloatModifier(20);
    private FloatModifier attackModifier = new FloatModifier(-50); // 生产降低50%产能

    private int mushroomInterval = 600; // 蘑菇生成的时间间隔
    private int mushroomTimeLeft; // 下一个蘑菇生成剩余时间

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        Bg_SpriteRender = transform.Find("Background").GetComponent<SpriteRenderer>();
    }

    public override void MInit()
    {
        mushroomInterval = 600; // 蘑菇生成的时间间隔
        mushroomTimeLeft = mushroomInterval; // 下一个蘑菇生成剩余时间
        base.MInit();
    }

    public override void MUpdate()
    {
        base.MUpdate();

        if (mushroomTimeLeft > 0)
        {
            mushroomTimeLeft--;
        }
        else
        {
            TryCreateMushroom();
        }
    }

    /// <summary>
    /// 尝试生成发光霉菌
    /// </summary>
    private void TryCreateMushroom()
    {
        // 先寻找符合条件的格子
        List<BaseGrid> g_List = new List<BaseGrid>();
        float max = float.MinValue;
        bool hasInfectable = false; // 能否发现可作为感染目标的美食
        foreach (var g in gridList)
        {
            // 如果不能感染当前格子则直接跳过
            if (!LuminescentMold.CanInfectGrid(g))
                continue;

            List<FoodUnit> list = new List<FoodUnit>();
            if (g.GetFoodByTag(FoodInGridType.Default) != null)
                list.Add(g.GetFoodByTag(FoodInGridType.Default));
            if (g.GetFoodByTag(FoodInGridType.Shield) != null)
                list.Add(g.GetFoodByTag(FoodInGridType.Shield));

            
            // 找血量最高的可被感染的美食，如果有多个就添加多个，标记上它们的格子
            foreach (var item in list)
            {
                if (LuminescentMold.CanInfectUnit(item))
                {
                    // 当第一次发现可被感染的美食目标时，清空表中所有格子
                    if (!hasInfectable)
                    {
                        hasInfectable = true;
                        g_List.Clear();
                    }

                    if (item.GetCurrentHp() > max)
                    {
                        g_List.Clear();
                        g_List.Add(g);
                        max = item.GetCurrentHp();
                    }else if(item.GetCurrentHp() == max)
                    {
                        if (!g_List.Contains(g))
                            g_List.Add(g);
                    }
                }
            }

            // 如果未发现可作为感染目标的美食，但是该格子符合感染条件，则把该格子加入候选
            if (!hasInfectable)
                g_List.Add(g);
        }

        // 如果还是没有符合条件的格子，那么直接跳过，等待下一帧
        if (g_List.Count <= 0)
            return;

        // 在符合条件的格子中随机取一个，感染该格子
        List<BaseGrid> randList = GridManager.GetRandomUnitList(g_List, 1);
        LuminescentMold.InfectGrid(randList[0]);

        mushroomTimeLeft = mushroomInterval;
    }

    public override void OnEnemyEnter(MouseUnit unit)
    {
        Debug.Log("EnemyHasEnter!");
        unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
        unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
        // 如果有能在雨中滑行的老鼠则触发其滑行效果
        if(unit.mType == (int)MouseNameTypeMap.WonderLandNormalMouse)
        {
            WonderLandNormalMouse m = (WonderLandNormalMouse)unit;
            m.SetInRainArea();
        }
        base.OnEnemyEnter(unit);
    }

    public override void OnEnemyExit(MouseUnit unit)
    {
        Debug.Log("EnemyHasExit!");
        unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
        // 如果有能在雨中滑行的老鼠则取消其滑行效果
        if (unit.mType == (int)MouseNameTypeMap.WonderLandNormalMouse)
        {
            WonderLandNormalMouse m = (WonderLandNormalMouse)unit;
            m.SetOutOfRainArea();
        }
        base.OnEnemyExit(unit);
    }

    public override void OnFoodEnter(FoodUnit unit)
    {
        Debug.Log("FoodHasEnter!");
        unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
        unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
        if (FoodManager.IsProductionType(unit))
        {
            unit.NumericBox.Attack.AddFinalPctAddModifier(attackModifier);
        }
        base.OnFoodEnter(unit);
    }

    public override void OnFoodExit(FoodUnit unit)
    {
        Debug.Log("FoodHasExit!");
        unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
        if (FoodManager.IsProductionType(unit))
        {
            unit.NumericBox.Attack.RemoveFinalPctAddModifier(attackModifier);
        }
        base.OnFoodExit(unit);
    }

    public override void OnCharacterEnter(CharacterUnit unit)
    {
        Debug.Log("CharacterHasEnter!");
        unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
        unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
        base.OnCharacterEnter(unit);
    }

    public override void OnCharacterExit(CharacterUnit unit)
    {
        Debug.Log("CharacterHasExit!");
        unit.NumericBox.MoveSpeed.RemovePctAddModifier(moveSpeedModifier);
        unit.NumericBox.AttackSpeed.RemovePctAddModifier(attackSpeedModifier);
        base.OnCharacterExit(unit);
    }

    public override void OnGridEnter(BaseGrid unit)
    {
        Debug.Log("GridHasEnter!");
        base.OnGridEnter(unit);
    }

    public override void OnGridExit(BaseGrid unit)
    {
        Debug.Log("GridHasExit!");
        base.OnGridExit(unit);
    }

    public static RainAreaEffectExecution GetInstance(int type)
    {
        RainAreaEffectExecution e = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "AreaEffect/RainAreaEffect").GetComponent<RainAreaEffectExecution>();
        e.MInit();
        e.Init(0, 3, 3, 0, 0, true, true);
        e.SetBoxCollider2D(Vector2.zero, new Vector2(0.95f * 3 * MapManager.gridWidth, 0.95f* 3 * MapManager.gridHeight));
        e.isAffectCharacter = true;
        e.isAffectGrid = true;
        e.SetAffectHeight(0);
        return e;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "AreaEffect/RainAreaEffect", gameObject);
    }
}
