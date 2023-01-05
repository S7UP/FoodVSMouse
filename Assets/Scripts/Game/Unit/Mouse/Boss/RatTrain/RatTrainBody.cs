using UnityEngine;
/// <summary>
/// 鼠国列车车厢
/// </summary>
public class RatTrainBody : RatTrainComponent
{
    private static RuntimeAnimatorController Body_RuntimeAnimatorController;

    public override void Awake()
    {
        if (Body_RuntimeAnimatorController == null)
            Body_RuntimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Boss/19/Body");
        base.Awake();
    }

    public override void MInit()
    {
        base.MInit();
        animator.runtimeAnimatorController = Body_RuntimeAnimatorController;
    }

    public static RatTrainBody GetInstance(int layer)
    {
        RatTrainBody obj = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Boss/19/Body").GetComponent<RatTrainBody>();
        obj.MInit();
        obj.bodyLayerIndex = layer;
        return obj;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Boss/19/Body", gameObject);
    }
}
