using UnityEngine;

/// <summary>
/// ����Ani�Ĺ���
/// </summary>
public class AnimatorManager
{
    /// <summary>
    /// ��ȡ��ǰ�������Ž���(��������Ϊ��ѭ��������С������Ϊ��ǰѭ���ڲ��Űٷֱ�)
    /// </summary>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static float GetNormalizedTime(Animator animator)
    {
        return animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
    }

    /// <summary>
    /// ��ȡ����ÿ֡�ĳ���ʱ��
    /// </summary>
    /// <returns></returns>
    public static float GetLengthPerFrame(Animator animator)
    {
        return 1/ animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate;
    }

    /// <summary>
    /// ��ȡ��ǰ����Ƭ����
    /// </summary>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static string GetCurrentClipName(Animator animator)
    {
        return animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
    }

    /// <summary>
    /// ����һ��Animator��ǰ����Clip����֡��
    /// </summary>
    /// <returns></returns>
    public static int GetTotalFrame(Animator animator)
    {
        float frameRate = animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate; // ֡Ƶ��ÿ���ж���֡��
        float length = animator.GetCurrentAnimatorClipInfo(0)[0].clip.length; // ����ʱ�����룩
        return Mathf.CeilToInt(frameRate*length);
    }

    /// <summary>
    /// ����һ��Animator��ǰ����Clip���ŵڼ�֡��Ĭ��ʱ��Ϊ0ʱ��ʼΪ��1֡��
    /// ��ˣ���ǰ֡����֡������Ϊ0ʱ���������������꣬��������Ƕ������ŵ����һ֡�����Ҫ�ж�����������Ӧ��������Ϊ1ʱ��
    /// ����һ��13֡�Ķ������������֡Ϊ13�������ڲ��ŵ�13֡����Ҫ�жϲ�����һ�̵�ʱ��Ӧ���������ǰ֡Ϊ13+1=14
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentFrame(Animator animator)
    {
        return 1 + Mathf.FloorToInt(GetNormalizedTime(animator) * animator.GetCurrentAnimatorClipInfo(0)[0].clip.length * animator.GetCurrentAnimatorClipInfo(0)[0].clip.frameRate);
    }

    /// <summary>
    /// ��ȡĳ������Ƭ�ε�ʱ��
    /// </summary>
    /// <param name="animator"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static float GetClipTime(Animator animator, string name)
    {
        RuntimeAnimatorController c = animator.runtimeAnimatorController;
        for (int i = 0; i < c.animationClips.Length; i++)
        {
            AnimationClip animationClip = c.animationClips[i];
            if (animationClip.name.Equals(name))
            {
                return animationClip.length;
            }
        }
        Debug.Log("δ��animator�﷢����Ϊ:"+name+"�Ķ���Ƭ��");
        return -1;
    }

    public static void SetClipSpeed(Animator animator, string name, float speed)
    {
        RuntimeAnimatorController c = animator.runtimeAnimatorController;
        for (int i = 0; i < c.animationClips.Length; i++)
        { 
            AnimationClip animationClip = c.animationClips[i];
            if (animationClip.name.Equals(name))
            {
                animator.speed = speed;
                break;
            }
        }
    }
}
