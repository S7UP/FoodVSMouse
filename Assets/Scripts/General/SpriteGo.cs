using UnityEngine;
/// <summary>
/// 仅仅是带着一个SpriteRenderer的GameObject
/// </summary>
public class SpriteGo : MonoBehaviour, IGameControllerMember
{
    public SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void MInit()
    {
        spriteRenderer.sprite = null;
        spriteRenderer.color = Color.white;
        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;
        spriteRenderer.sortingLayerID = 0;
        spriteRenderer.sortingOrder = 0;
        spriteRenderer.material = null;
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
        ExecuteRecycle();
    }

    public void SetSpriteAndSorting(Sprite sprite)
    {
        spriteRenderer.sprite = sprite;
    }

    public static SpriteGo GetInstance()
    {
        SpriteGo a = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "General/SpriteGo").GetComponent<SpriteGo>();
        a.MInit();
        return a;
    }

    private void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "General/SpriteGo", gameObject);
    }
}
