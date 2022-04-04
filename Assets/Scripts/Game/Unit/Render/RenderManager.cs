using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 单位外形渲染管理者
/// </summary>
public class RenderManager
{
    public Dictionary<string, AnimatorController> animatorControllerDict = new Dictionary<string, AnimatorController>();
    public Dictionary<string, SpriteRendererManager> spriteRendererManagerDict = new Dictionary<string, SpriteRendererManager>();

    public void Initialize()
    {
        animatorControllerDict.Clear();
        spriteRendererManagerDict.Clear();
    }

    /// <summary>
    /// 把一个Animator添加进管理器中管理
    /// </summary>
    /// <param name="aniName"></param>
    /// <param name="animator"></param>
    public void AddAnimator(string aniName, Animator animator)
    {
        animatorControllerDict.Add(aniName, new AnimatorController(animator));
    }

    /// <summary>
    /// 把一个图片精灵渲染器添加进管理器中管理
    /// </summary>
    /// <param name="sprName"></param>
    /// <param name="spriteRenderer"></param>
    public void AddSpriteRenderer(string sprName, SpriteRenderer spriteRenderer)
    {
        spriteRendererManagerDict.Add(sprName, new SpriteRendererManager(spriteRenderer));
    }
}
