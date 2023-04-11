using UnityEngine;
/// <summary>
/// ����г���ͷ
/// </summary>
public class RatTrainHead : RatTrainComponent
{
    private static RuntimeAnimatorController Head_RuntimeAnimatorController;

    public override void Awake()
    {
        if (Head_RuntimeAnimatorController == null)
            Head_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Head");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        animator.runtimeAnimatorController = Head_RuntimeAnimatorController;
    }

    /// <summary>
    /// ת��Ϊ����̬ʱ�¼�
    /// </summary>
    public override void OnTurnToAppear()
    {
        // ֪ͨ���������µ�·����Ϣ���복����������Ա�������������
        GetMaster().AddNextRouteToManager();
    }

    /// <summary>
    /// ���ִ����һ��·����ʱ���ⲿ���ɳ�ͷ��д��
    /// </summary>
    public override void OnOutOfBound()
    {
        // ������ʣ��·��ʱ����������Ч��������֪ͨBOSS��������ѵִ�Ŀ�ĵ�
        if (GetRouteQueue().Count > 0)
            base.OnOutOfBound();
        else
        {
            GetMaster().MarkMoveToDestination();
            GetMaster().SetActionState(new IdleState(GetMaster()));
        }
    }

    public static RatTrainHead GetInstance()
    {
        RatTrainHead obj = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Boss/19/Head").GetComponent<RatTrainHead>();
        obj.MInit();
        obj.bodyLayerIndex = -1;
        return obj;
    }

    protected override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Boss/19/Head", gameObject);
    }
}
