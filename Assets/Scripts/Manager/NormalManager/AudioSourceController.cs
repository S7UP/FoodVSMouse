using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using S7P.Numeric;
/// <summary>
/// ����������ֵĲ�����ֹͣ���Լ���Ϸ�и�����Ч�Ĳ���
/// </summary>
public class AudioSourceController
{
    /// <summary>
    /// ��ŵ�ǰBGM���Ŷ������Ϣ
    /// </summary>
    private class BGMObject
    {
        public MusicInfo musicInfo;
        public AudioClip audioClip;
        public MultiplyFloatModifierCollector VolumeRate = new MultiplyFloatModifierCollector();
    }

    private AudioSource[] MusicAudioSource = new AudioSource[2]; // �������ֲ�������һ��ֻ��һ�ף�������ʵ���������ֵĽ��뽥���л���
    private BGMObject[] BGMObjArray = new BGMObject[2];
    private Dictionary<string, BGMObject> BGMObjDict = new Dictionary<string, BGMObject>();
    private float[] musicVolumeRate = new float[2] { 1, 0 }; // ���������ֵĲ����������ʣ��봫�����Ƶ����Ԥ���йأ�

    private AudioSource SeAudioSource; // ��Ч������
    private AudioSource currentAudioSource; // ��ǰ���ڲ��ŵı������ֲ�����������������
    private int currentAudioSourceIndex = 0; // ��ǰ���ֲ����������±�

    private bool playBGMusic = true;
    private bool isStopAllMusic = true;
    private bool isPauseAllMusic = false;

    // ���캯��
    public AudioSourceController()
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

    public static void UnLoadBGMusic(string refenceName)
    {
        MusicInfo info = MusicManager.GetMusicInfo(refenceName);
        if (info != null)
            GameManager.Instance.UnLoadAudioClip(info.resPath);
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

    /// <summary>
    /// ������������BGM
    /// </summary>
    /// <param name="refenceName"></param>
    public void PlayBGMusic(string refenceName)
    {
        if (refenceName == null)
            return;

        if (!BGMObjDict.ContainsKey(refenceName))
        {
            MusicInfo info = MusicManager.GetMusicInfo(refenceName);
            if (info != null)
            {
                AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
                BGMObject obj = new BGMObject() { audioClip = audioClip, musicInfo = info };
                obj.VolumeRate.AddModifier(new FloatModifier(info.volume));
                BGMObjDict.Add(refenceName, obj);
            }else
            {
                return;
            }
        }

        // ��ȡ�벥��
        {
            BGMObject obj = BGMObjDict[refenceName];
            PlayBGMusic(obj);
        }
    }

    /// <summary>
    /// ��ͷ��ʼ����BGM
    /// </summary>
    /// <param name="obj"></param>
    private void PlayBGMusic(BGMObject obj)
    {
        if (obj.audioClip == null)
            return;

        if (!GameManager.Instance.configManager.mConfig.isPlayBGM)
            return;

        isStopAllMusic = false;
        ResumeAllMusic();

        // ���ڲ��������ڲ�������һ��Ҫ���Ĳ���ͬһ��
        // �л�����һ�����ֲ�������������
        if (currentAudioSource.isPlaying && currentAudioSource.clip != obj.audioClip)
        {
            SetAnotherAudioSourceAsCurrent();
        }

        BGMObjArray[currentAudioSourceIndex] = obj;
        currentAudioSource.clip = obj.audioClip;
        if (playBGMusic)
        {
            currentAudioSource.time = obj.musicInfo.startTime;
            currentAudioSource.Play();
        }
    }

    /// <summary>
    /// ��ѭ���㿪ʼ����BGM
    /// </summary>
    /// <param name="obj"></param>
    public void PlayBGMusicAgain()
    {
        if (!GameManager.Instance.configManager.mConfig.isPlayBGM)
            return;

        isStopAllMusic = false;
        ResumeAllMusic();

        BGMObject beforeObj = BGMObjArray[currentAudioSourceIndex];
        AudioClip beforeClip = currentAudioSource.clip;

        SetAnotherAudioSourceAsCurrent();

        //BGMObject currentObj = BGMObjArray[currentAudioSourceIndex];
        Debug.Log("play again!");
        BGMObjArray[currentAudioSourceIndex] = beforeObj;
        currentAudioSource.clip = beforeClip;
        if (playBGMusic)
        {
            currentAudioSource.time = beforeObj.musicInfo.loopStartTime;
            currentAudioSource.Play();
        }
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="refenceName"></param>
    public void PlayEffectMusic(string refenceName)
    {
        if (!GameManager.Instance.configManager.mConfig.isPlaySE)
            return;
        SoundsInfo info = SoundsManager.GetSoundsInfo(refenceName);
        if(info != null)
        {
            AudioClip audioClip = GameManager.Instance.GetAudioClip(info.resPath);
            PlayEffectMusic(audioClip, info.volume * GameManager.Instance.configManager.mConfig.SEVolume);
        }
    }

    /// <summary>
    /// ������Ч
    /// </summary>
    /// <param name="audioClip"></param>
    /// <param name="volume"></param>
    public void PlayEffectMusic(AudioClip audioClip, float volume)
    {
        if (!GameManager.Instance.configManager.mConfig.isPlaySE)
            return;
        SeAudioSource.PlayOneShot(audioClip, volume);
    }

    /// <summary>
    /// ֹͣ����ȫ������
    /// </summary>
    public void StopAllMusic()
    {
        isStopAllMusic = true;
        MusicAudioSource[0].Stop();
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
        PlayBGMusic(BGMObjArray[currentAudioSourceIndex]);
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
        GameManager.Instance.configManager.mConfig.isPlaySE = !GameManager.Instance.configManager.mConfig.isPlaySE;
    }

    /// <summary>
    /// ÿ֡���·��������������GameManager��
    /// </summary>
    public void Update()
    {
        ConfigManager.Config config = GameManager.Instance.configManager.mConfig;

        // �鿴һ�µ�ǰ����BGM�Ƿ񳬹�ѭ���ϵ�
        {
            BGMObject obj = BGMObjArray[currentAudioSourceIndex];
            AudioSource mainAudioSource = MusicAudioSource[currentAudioSourceIndex];
            if (obj != null)
            {
                float endTime = obj.musicInfo.loopEndTime;
                if(obj.musicInfo.loopEndTime == -1)
                    endTime = obj.audioClip.length;
                if(mainAudioSource.time >= endTime)
                {
                    PlayBGMusicAgain();
                }
            }
        }

        // ���뽥��Ч������
        if (!isStopAllMusic)
        {
            musicVolumeRate[currentAudioSourceIndex] = Mathf.Min(musicVolumeRate[currentAudioSourceIndex] + 1.0f / 60, 1.0f);
            musicVolumeRate[1 - currentAudioSourceIndex] = Mathf.Max(musicVolumeRate[1 - currentAudioSourceIndex] - 1.0f / 60, 0.0f);
        }
        else
        {
            for (int i = 0; i < musicVolumeRate.Length; i++)
            {
                musicVolumeRate[i] = Mathf.Max(musicVolumeRate[1 - currentAudioSourceIndex] - 1.0f / 60, 0.0f);
            }
        }

        // ��������
        MusicAudioSource[currentAudioSourceIndex].volume = config.BGMVolume * musicVolumeRate[currentAudioSourceIndex];
        MusicAudioSource[1-currentAudioSourceIndex].volume = config.BGMVolume * musicVolumeRate[1-currentAudioSourceIndex];
        if(musicVolumeRate[currentAudioSourceIndex] <= 0.0f)
        {
            MusicAudioSource[currentAudioSourceIndex].Stop();
            MusicAudioSource[currentAudioSourceIndex].time = 0;
        }
        if (musicVolumeRate[1-currentAudioSourceIndex] <= 0.0f)
        {
            MusicAudioSource[1-currentAudioSourceIndex].Stop();
            MusicAudioSource[1-currentAudioSourceIndex].time = 0;
        }


        // ������Ч����
        SeAudioSource.volume = config.SEVolume;
    }
}
