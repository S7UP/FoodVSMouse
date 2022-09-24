using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorGo : MonoBehaviour, IGameControllerMember
{
    public SpriteRenderer spriteRenderer;
    public Animator animator;

    public void MInit()
    {
        spriteRenderer.sprite = null;
        spriteRenderer.color = Color.white;
        spriteRenderer.flipX = false;
        spriteRenderer.flipY = false;
        spriteRenderer.sortingLayerID = 0;
        spriteRenderer.sortingOrder = 0;

        animator.runtimeAnimatorController = null;
    }

    public void SetSpriteAndSorting(Sprite sprite, int layer, int order)
    {
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingLayerID = layer;
        spriteRenderer.sortingOrder = order;
    }

    public void SetRuntimeAnimatorController(RuntimeAnimatorController runtimeAnimatorController)
    {
        animator.runtimeAnimatorController = runtimeAnimatorController;
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

    public static AnimatorGo GetInstance()
    {
        AnimatorGo a = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "General/AnimatorGo").GetComponent<AnimatorGo>();
        a.MInit();
        return a;
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "General/AnimatorGo", gameObject);
    }
}
