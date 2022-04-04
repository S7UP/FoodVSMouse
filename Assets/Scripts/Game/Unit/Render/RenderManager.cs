using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��λ������Ⱦ������
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
    /// ��һ��Animator��ӽ��������й���
    /// </summary>
    /// <param name="aniName"></param>
    /// <param name="animator"></param>
    public void AddAnimator(string aniName, Animator animator)
    {
        animatorControllerDict.Add(aniName, new AnimatorController(animator));
    }

    /// <summary>
    /// ��һ��ͼƬ������Ⱦ����ӽ��������й���
    /// </summary>
    /// <param name="sprName"></param>
    /// <param name="spriteRenderer"></param>
    public void AddSpriteRenderer(string sprName, SpriteRenderer spriteRenderer)
    {
        spriteRendererManagerDict.Add(sprName, new SpriteRendererManager(spriteRenderer));
    }
}
