using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��������
/// </summary>
public class RainAreaEffectExecution : RetangleAreaEffectExecution
{
    private Animator animator;
    private SpriteRenderer Bg_SpriteRender;

    private FloatModifier attackSpeedModifier = new FloatModifier(20);
    private FloatModifier moveSpeedModifier = new FloatModifier(20);
    private FloatModifier attackModifier = new FloatModifier(-50); // ��������50%����

    private int mushroomInterval = 600; // Ģ�����ɵ�ʱ����
    private int mushroomTimeLeft; // ��һ��Ģ������ʣ��ʱ��

    public override void Awake()
    {
        base.Awake();
        animator = GetComponent<Animator>();
        Bg_SpriteRender = transform.Find("Background").GetComponent<SpriteRenderer>();
    }

    public override void MInit()
    {
        mushroomInterval = 600; // Ģ�����ɵ�ʱ����
        mushroomTimeLeft = mushroomInterval; // ��һ��Ģ������ʣ��ʱ��
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
    /// �������ɷ���ù��
    /// </summary>
    private void TryCreateMushroom()
    {
        // ��Ѱ�ҷ��������ĸ���
        List<BaseGrid> g_List = new List<BaseGrid>();
        float max = float.MinValue;
        bool hasInfectable = false; // �ܷ��ֿ���Ϊ��ȾĿ�����ʳ
        foreach (var g in gridList)
        {
            // ������ܸ�Ⱦ��ǰ������ֱ������
            if (!LuminescentMold.CanInfectGrid(g))
                continue;

            List<FoodUnit> list = new List<FoodUnit>();
            if (g.GetFoodByTag(FoodInGridType.Default) != null)
                list.Add(g.GetFoodByTag(FoodInGridType.Default));
            if (g.GetFoodByTag(FoodInGridType.Shield) != null)
                list.Add(g.GetFoodByTag(FoodInGridType.Shield));

            
            // ��Ѫ����ߵĿɱ���Ⱦ����ʳ������ж������Ӷ������������ǵĸ���
            foreach (var item in list)
            {
                if (LuminescentMold.CanInfectUnit(item))
                {
                    // ����һ�η��ֿɱ���Ⱦ����ʳĿ��ʱ����ձ������и���
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

            // ���δ���ֿ���Ϊ��ȾĿ�����ʳ�����Ǹø��ӷ��ϸ�Ⱦ��������Ѹø��Ӽ����ѡ
            if (!hasInfectable)
                g_List.Add(g);
        }

        // �������û�з��������ĸ��ӣ���ôֱ���������ȴ���һ֡
        if (g_List.Count <= 0)
            return;

        // �ڷ��������ĸ��������ȡһ������Ⱦ�ø���
        List<BaseGrid> randList = GridManager.GetRandomUnitList(g_List, 1);
        LuminescentMold.InfectGrid(randList[0]);

        mushroomTimeLeft = mushroomInterval;
    }

    public override void OnEnemyEnter(MouseUnit unit)
    {
        Debug.Log("EnemyHasEnter!");
        unit.NumericBox.MoveSpeed.AddPctAddModifier(moveSpeedModifier);
        unit.NumericBox.AttackSpeed.AddPctAddModifier(attackSpeedModifier);
        // ������������л��е������򴥷��们��Ч��
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
        // ������������л��е�������ȡ���们��Ч��
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
