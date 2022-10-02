using UnityEngine;

public class SpriteGo : MonoBehaviour, IGameControllerMember
{
    public SpriteRenderer spriteRenderer;

    public void MInit()
    {
        spriteRenderer.sprite = null;
        spriteRenderer.color = Color.white;
        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;
        spriteRenderer.sortingLayerID = 0;
        spriteRenderer.sortingOrder = 0;
    }

    public void SetSpriteAndSorting(Sprite sprite, int layer, int order)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerID = layer;
        spriteRenderer.sortingOrder = order;
    }

    public void MUpdate()
    {
        
    }

    public void MPause()
    {
        
    }

    public void MPauseUpdate()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        
    }

    public static SpriteGo GetInstance()
    {
        SpriteGo a = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "General/SpriteGo").GetComponent<SpriteGo>();
        a.MInit();
        return a;
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "General/SpriteGo", gameObject);
    }
}
