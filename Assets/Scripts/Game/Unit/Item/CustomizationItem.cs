using UnityEngine;
/// <summary>
/// 自定义道具
/// </summary>
public class CustomizationItem : BaseItem
{
    public override void MInit()
    {
        base.MInit();
        SetTag("Untagged");
        SetLayer("Default");
    }

    public void SetTag(string tag)
    {
        this.tag = tag;
    }

    public void SetLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public static CustomizationItem GetInstance(Vector2 pos, RuntimeAnimatorController runtimeAnimatorController)
    {
        CustomizationItem item = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Item/CustomizationItem").GetComponent<CustomizationItem>();
        item.MInit();
        item.transform.position = pos;
        item.animator.runtimeAnimatorController = runtimeAnimatorController;
        return item;
    }

    protected override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Item/CustomizationItem", gameObject);
    }
}
