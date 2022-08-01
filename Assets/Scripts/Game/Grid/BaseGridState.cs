public class BaseGridState
{
    // ���и�״̬�ĸ���
    public BaseGrid mGrid;
    public GridType gridType;

    public BaseGridState(BaseGrid grid)
    {
        mGrid = grid;
        gridType = GridType.Default;
    }

    // �����л�Ϊ��״̬˲����Ҫ������
    public virtual void OnEnter()
    {

    }

    // ���δӸ�״̬�л�������״̬����Ҫ������
    public virtual void OnExit() 
    {

    }

    // ���γ���Ч��
    public virtual void OnUpdate() 
    {
        
    }

    // ���е�λ�������ʱʩ�Ӹ���λ��Ч��
    public virtual void OnUnitEnter(BaseUnit baseUnit)
    {

    }

    // ���е�λ���ڵ���ʱ��������λ��Ч��
    public virtual void OnUnitStay(BaseUnit baseUnit)
    {

    }

    // ���е�λ�뿪����ʱʩ�Ӹ���λ��Ч��
    public virtual void OnUnitExit(BaseUnit baseUnit)
    {

    }
}
