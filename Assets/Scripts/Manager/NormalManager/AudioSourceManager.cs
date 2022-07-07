using UnityEngine;
//����������ֵĲ�����ֹͣ���Լ���Ϸ�и�����Ч�Ĳ���
public class AudioSourceManager
{
    private AudioSource[] audioSource;//0.����BGMusic 2.������Ч��
    private bool playEffectMusic = true;
    private bool playBGMusic = true;

    // ���캯��
    public AudioSourceManager()
    {
        audioSource = GameManager.Instance.GetComponents<AudioSource>();
    }

    // ���ű�������
    public void PlayBGMusic(AudioClip audioClip)
    {
        if (!audioSource[0].isPlaying || audioSource[0].clip != audioClip)
        {
            audioSource[0].clip = audioClip;
            audioSource[0].Play();
        }
    }

    // ������Ч
    public void PlayEffectMusic(AudioClip audioClip)
    {
        if (playEffectMusic)
        {
            audioSource[1].PlayOneShot(audioClip);
        }
    }

    public void CloseBGMusic()
    {
        audioSource[0].Stop();
    }

    public void OpenBGMusic()
    {
        audioSource[0].Play();
    }

    public void CloseOrOpenBGMusic()
    {
        playBGMusic = !playBGMusic;
        if (playBGMusic)
        {
            OpenBGMusic();
        }
        else
        {
            CloseBGMusic();
        }
    }

    public void CloseOrOpenEffectMusic()
    {
        playEffectMusic = !playEffectMusic;
    }
}
