using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using S7P.Numeric;
/// <summary>
/// 负责控制音乐的播放与停止，以及游戏中各种音效的播放
/// </summary>
public class AudioSourceController
{
    /// <summary>
    /// 存放当前BGM播放对象的信息
    /// </summary>
    private class BGMObject
    {
        public MusicInfo musicInfo;
        public AudioClip audioClip;
        public MultiplyFloatModifierCollector VolumeRate = new MultiplyFloatModifierCollector();
    }

    private AudioSource[] MusicAudioSource = new AudioSource[2]; // 背景音乐播放器（一般只播一首，但可以实现两首音乐的渐入渐出切换）
    private BGMObject[] BGMObjArray = new BGMObject[2];
    private Dictionary<string, BGMObject> BGMObjDict = new Dictionary<string, BGMObject>();
    private float[] musicVolumeRate = new float[2] { 1, 0 }; // 管理背景音乐的播放音量倍率（与传入的音频剪辑预设有关）

    private AudioSource SeAudioSource; // 音效播放器
    private AudioSource currentAudioSource; // 当前正在播放的背景音乐播放器（主播放器）
    private int currentAudioSourceIndex = 0; // 当前音乐播放器数组下标

    private bool playBGMusic = true;
    private bool isStopAllMusic = true;
    private bool isPauseAllMusic = false;

    // 构造函数
    public AudioSourceController()
    {
        // 创建时动态为GameManager添加组件
        MusicAudioSource[0] = GameManager.Instance.gameObject.AddComponent<AudioSource>();
        MusicAudioSource[0].loop = true;
        MusicAudioSource[1] = GameManager.Instance.gameObject.AddComponent<AudioSource>();
        MusicAudioSource[1].loop = true;
        SeAudioSource = GameManager.Instance.gameObject.AddComponent<AudioSource>();

        currentAudioSourceIndex = 0;
        currentAudioSource = MusicAudioSource[currentAudioSourceIndex]; // 默认主播放器
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
    /// 以引用名播放BGM
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

        // 读取与播放
        {
            BGMObject obj = BGMObjDict[refenceName];
            PlayBGMusic(obj);
        }
    }

    /// <summary>
    /// 从头开始播放BGM
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

        // 有在播，且正在播的与下一个要播的不是同一首
        // 切换到另一个音乐播放器播放音乐
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
    /// 从循环点开始播放BGM
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
    /// 播放音效
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
    /// 播放音效
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
    /// 停止播放全部音乐
    /// </summary>
    public void StopAllMusic()
    {
        isStopAllMusic = true;
        MusicAudioSource[0].Stop();
        MusicAudioSource[1].Stop();
    }

    /// <summary>
    /// 暂停全部音乐
    /// </summary>
    public void PauseAllMusic()
    {
        isPauseAllMusic = true;
        MusicAudioSource[0].Pause();
        MusicAudioSource[1].Pause();
    }

    /// <summary>
    /// 恢复播放全部音乐
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
    /// 重放当前音乐剪辑
    /// </summary>
    public void ReplayCurrentClip()
    {
        PlayBGMusic(BGMObjArray[currentAudioSourceIndex]);
    }

    /// <summary>
    /// 关闭BGM
    /// </summary>
    public void CloseBGMusic()
    {
        MusicAudioSource[0].Stop();
        MusicAudioSource[1].Stop();
    }

    /// <summary>
    /// 打开BGM
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
    /// 开关音效
    /// </summary>
    public void CloseOrOpenEffectMusic()
    {
        GameManager.Instance.configManager.mConfig.isPlaySE = !GameManager.Instance.configManager.mConfig.isPlaySE;
    }

    /// <summary>
    /// 每帧更新方法，请把它挂在GameManager上
    /// </summary>
    public void Update()
    {
        ConfigManager.Config config = GameManager.Instance.configManager.mConfig;

        // 查看一下当前播放BGM是否超过循环断点
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

        // 渐入渐出效果更新
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

        // 调整音量
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


        // 调整音效音量
        SeAudioSource.volume = config.SEVolume;
    }
}
