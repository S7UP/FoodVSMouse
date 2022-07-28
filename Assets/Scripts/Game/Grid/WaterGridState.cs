/// <summary>
/// ˮ����״̬
/// </summary>
public class WaterGridState : BaseGridState
{
    public WaterGridState(BaseGrid grid):base(grid)
    { 

    }

    public override void OnEnter()
    {
        // �л�˲��ǵø�������ʳ������ʩ�Ӷ�Ӧ����Ч��
        foreach (var item in mGrid.GetFoodUnitList())
        {
            OnUnitEnter(item);
        }
        foreach (var item in mGrid.GetMouseUnitList())
        {
            OnUnitEnter(item);
        }
    }

    public override void OnExit()
    {
        // �Ƴ�˲��ǵø�������ʳ������ȡ����Ӧ����Ч��
        foreach (var item in mGrid.GetFoodUnitList())
        {
            OnUnitExit(item);
        }
        foreach (var item in mGrid.GetMouseUnitList())
        {
            OnUnitExit(item);
        }
    }

    public override void OnUpdate()
    {
        // ����Ҫ����Ϊ����Ч���Ѿ���buff����Ч��
    }

    // ���е�λ�������ʱʩ�Ӹ���λ��Ч��
    public override void OnUnitEnter(BaseUnit baseUnit)
    {
        if (baseUnit.mHeight <= 0)
        {
            float descendGridCount;
            if (baseUnit is FoodUnit)
                descendGridCount = 0f;
            else if (baseUnit is MouseUnit)
                descendGridCount = 0.4f;
            else if (baseUnit is BaseItem)
                descendGridCount = 0f;
            else if (baseUnit is CharacterUnit)
                descendGridCount = 0f;
            else
                descendGridCount = 0;
            baseUnit.AddUniqueStatusAbility(StringManager.WaterGridState, new WaterStatusAbility(baseUnit, descendGridCount));
        }
    }

    // ���е�λ���ڵ���ʱ��������λ��Ч��
    public override void OnUnitStay(BaseUnit baseUnit)
    {

    }

    // ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
    public override void OnUnitExit(BaseUnit baseUnit)
    {
        baseUnit.RemoveUniqueStatusAbility(StringManager.WaterGridState);
    }
}
