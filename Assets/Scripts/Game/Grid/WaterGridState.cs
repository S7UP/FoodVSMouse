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
        baseUnit.AddUniqueStatusAbility(StringManager.WaterGridState, new WaterStatusAbility(baseUnit));
    }

    // ���е�λ���ڵ���ʱ��������λ��Ч��
    public override void OnUnitStay(BaseUnit baseUnit)
    {
        // ����Ҫ����Ϊ����Ч���Ѿ���buff����Ч��
    }

    // ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
    public override void OnUnitExit(BaseUnit baseUnit)
    {
        baseUnit.RemoveUniqueStatusAbility(StringManager.WaterGridState);
    }
}
