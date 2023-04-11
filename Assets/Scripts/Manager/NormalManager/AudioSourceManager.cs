using UnityEngine;
using System.Collections.Generic;
using System.Collections;
/// <summary>
/// ����������ֵĲ�����ֹͣ���Լ���Ϸ�и�����Ч�Ĳ���
/// </summary>
public class AudioSourceManager
{
    private List<AudioClip> audioClipList = new List<AudioClip>();
    private AudioSource[] MusicAudioSource = new AudioSource[2]; // �������ֲ�������һ��ֻ��һ�ף�������ʵ���������ֵĽ��뽥���л���
    private float[] musicVolume = new float[2] { 1, 0 }; // ���������ֵ�ǰ��Բ�������
    private float[] musicVolumeRate = new float[2] { 1, 1 }; // ��������Ч�Ĳ����������ʣ��봫�����Ƶ����Ԥ���йأ�
    private AudioSource SeAudioSource; // ��Ч������
    private AudioSource currentAudioSource; // ��ǰ���ڲ��ŵı������ֲ�����������������
    private Dictionary<string, float> startTimeDict = new Dictionary<string, float>(); // ��¼���ּ�����ʼ��
    private int currentAudioSourceIndex = 0; // ��ǰ���ֲ����������±�

    private bool playEffectMusic = true;
    private bool playBGMusic = true;
    private bool isStopAllMusic = true;
    private bool isPauseAllMusic = false;

    private AudioClip currentAudioClip; // ��ǰ�������ּ���

    // ���캯��
    public AudioSourceManager()
    {
        // ����ʱ��̬ΪGameManager������
        MusicAudioSource[0] = GameManager.Instance.gameObject.AddComponent<AudioSource>();
        MusicAudioSource[0].loop = true;
        MusicAudioSource[1] = GameManager.Instance.gameObject.AddComponent<AudioSource>();
        MusicAudioSource[1].loop = true;
        SeAudioSource = GameManager.Instance.gameObject.AddComponent<AudioSource>();

        currentAudioSourceIndex = 0;
        currentAudioSource = MusicAudioSource[currentAudioSourceIndex]; // Ĭ����������
    }

    public void SetAnotherAudioSourceAsCurrent()
    {
        currentAudioSourceIndex++;
        currentAudioSourceIndex = currentAudioSourceIndex % MusicAudioSource.Length;
        currentAudioSource = MusicAudioSource[currentAudioSourceIndex];
    }

    public static void LoadBGMusic(string refenceName)
    {
        MusicInfo info = MusicManager.GetMusicInfo(refenceName);
        if (info != null)
        {
            AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
        }
    }

    public static IEnumerator AsyncLoadBGMusic(string refenceName)
    {
        MusicInfo info = MusicManager.GetMusicInfo(refenceName);
        if (info != null)
        {
            yield return GameManager.Instance.StartCoroutine(GameManager.Instance.AsyncGetAudioClip(info.resPath));
        }
        else
        {
            yield return null;
        }
    }

    public static IEnumerator AsyncLoadEffectMusic(string refenceName)
    {
        SoundsInfo info = SoundsManager.GetSoundsInfo(refenceName);
        if (info != null)
        {
            yield return GameManager.Instance.StartCoroutine(GameManager.Instance.AsyncGetAudioClip(info.resPath));
        }
        else
        {
            yield return null;
        }
    }

    public void PlayBGMusic(string refenceName)
    {
        MusicInfo info = MusicManager.GetMusicInfo(refenceName);
        if(info != null)
        {
            AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
            PlayBGMusic(audioClip);
            musicVolumeRate[currentAudioSourceIndex] = info.volume;
        }
    }

    // ���ű�������
    public void PlayBGMusic(AudioClip audioClip)
    {
        if (audioClip == null)
            return;

        currentAudioClip = audioClip;

        if (!GameManager.Instance.configManager.mConfig.isPlayBGM)
            return;

        isStopAllMusic = false;
        ResumeAllMusic();

        if (!audioClipList.Contains(audioClip))
            audioClipList.Add(audioClip);

        // ���ڲ��������ڲ�������һ��Ҫ���Ĳ���ͬһ��
        // �л�����һ�����ֲ�������������
        if (currentAudioSource.isPlaying && currentAudioSource.clip != audioClip)
        {
            SetAnotherAudioSourceAsCurrent();
            // ע���������Ϸ�������ʱcurrentAudioSource���õĶ����ѷ����ı䣬������Բ鿴����ķ���ʵ��
        }

        currentAudioSource.clip = audioClip;
        if (playBGMusic)
        {
            currentAudioSource.time = 0; // ��0��ʼ����
            currentAudioSource.Play();
        }
        currentAudioSource.time = GetLoopStartTime(audioClip.name); // ����ѭ����
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="refenceName"></param>
    public void PlayEffectMusic(string refenceName)
    {
        SoundsInfo info = SoundsManager.GetSoundsInfo(refenceName);
        if(info != null)
        {
            AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
            PlayEffectMusic(audioClip, info.volume * GameManager.Instance.configManager.mConfig.SEVolume);
        }
    }

    // ������Ч
    public void PlayEffectMusic(AudioClip audioClip, float volume)
    {
        if (!playEffectMusic)
            return;
        SeAudioSource.PlayOneShot(audioClip, volume);
    }

    /// <summary>
    /// ֹͣ����ȫ������
    /// </summary>
    public void StopAllMusic()
    {
        isStopAllMusic = true;
        musicVolume[0] = 0;
        MusicAudioSource[0].Stop();
        musicVolume[1] = 0;
        MusicAudioSource[1].Stop();
    }

    /// <summary>
    /// ��ͣȫ������
    /// </summary>
    public void PauseAllMusic()
    {
        isPauseAllMusic = true;
        MusicAudioSource[0].Pause();
        MusicAudioSource[1].Pause();
    }

    /// <summary>
    /// �ָ�����ȫ������
    /// </summary>
    public void ResumeAllMusic()
    {
        isPauseAllMusic = false;
        if (!MusicAudioSource[0].isPlaying)
            MusicAudioSource[0].Play();
        if (!MusicAudioSource[1].isPlaying)
            MusicAudioSource[1].Play();
    }

    /// <summary>
    /// �طŵ�ǰ���ּ���
    /// </summary>
    public void ReplayCurrentClip()
    {
        PlayBGMusic(currentAudioClip);
    }

    /// <summary>
    /// �ر�BGM
    /// </summary>
    public void CloseBGMusic()
    {
        MusicAudioSource[0].Stop();
        MusicAudioSource[1].Stop();
    }

    /// <summary>
    /// ��BGM
    /// </summary>
    public void OpenBGMusic()
    {
        MusicAudioSource[0].Play();
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

    /// <summary>
    /// ������Ч
    /// </summary>
    public void CloseOrOpenEffectMusic()
    {
        playEffectMusic = !playEffectMusic;
    }

    /// <summary>
    /// ��ȡBGM��ѭ����ʼʱ��
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private float GetLoopStartTime(string name)
    {
        if (startTimeDict.ContainsKey(name))
        {
            return startTimeDict[name];
        }
        return 0;
    }

    /// <summary>
    /// ÿ֡���·��������������GameManager��
    /// </summary>
    public void Update()
    {
        ConfigManager.Config config = GameManager.Instance.configManager.mConfig;

        // ���뽥��Ч������
        if (!isStopAllMusic)
        {
            musicVolume[currentAudioSourceIndex] = Mathf.Min(musicVolume[currentAudioSourceIndex] + 1.0f / 120, 1.0f);
            musicVolume[1 - currentAudioSourceIndex] = Mathf.Max(musicVolume[1 - currentAudioSourceIndex] - 1.0f / 120, 0.0f);
        }
        else
        {
            for (int i = 0; i < musicVolume.Length; i++)
            {
                musicVolume[i] = Mathf.Max(musicVolume[1 - currentAudioSourceIndex] - 1.0f / 120, 0.0f);
            }
        }

        // ��������
        MusicAudioSource[currentAudioSourceIndex].volume = musicVolume[currentAudioSourceIndex] * config.BGMVolume * musicVolumeRate[currentAudioSourceIndex];
        MusicAudioSource[1-currentAudioSourceIndex].volume = musicVolume[1-currentAudioSourceIndex] * config.BGMVolume * musicVolumeRate[1-currentAudioSourceIndex];
        // ������Ч����
        SeAudioSource.volume = config.SEVolume;
    }
}
