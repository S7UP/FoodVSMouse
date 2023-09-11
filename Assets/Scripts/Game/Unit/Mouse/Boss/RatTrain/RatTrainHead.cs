using UnityEngine;
/// <summary>
/// 鼠国列车车头
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
