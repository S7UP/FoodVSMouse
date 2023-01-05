using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 设置面板
/// </summary>
public class ConfigPanel : MonoBehaviour, IBasePanel
{
    private Toggle Toggle_BGM;
    private Toggle Toggle_SoundEffect;
    private Toggle Toggle_QuickCardRelease;

    private Slider Slider_BGM;
    private Slider Slider_SoundEffect;

    private Button Btn_ReturnToMain;
    private Button Btn_RestoreTheDefaultSettings;



    public void Awake()
    {
        Toggle_BGM = transform.Find("Img_BGM").Find("Toggle").GetComponent<Toggle>();
        Slider_BGM = transform.Find("Img_BGM").Find("Slider").GetComponent<Slider>();
        Toggle_SoundEffect = transform.Find("Img_SoundEffect").Find("Toggle").GetComponent<Toggle>();
        Slider_SoundEffect = transform.Find("Img_SoundEffect").Find("Slider").GetComponent<Slider>();
        Toggle_QuickCardRelease = transform.Find("Img_QuickCardRelease").Find("Toggle").GetComponent<Toggle>();
        Btn_ReturnToMain = transform.Find("Btn_ReturnToMain").GetComponent<Button>();
        Btn_RestoreTheDefaultSettings = transform.Find("Btn_RestoreTheDefaultSettings").GetComponent<Button>();
    }


    public void InitPanel()
    {
        Refresh();
        // 返回主菜单
        Btn_ReturnToMain.onClick.RemoveAllListeners();
        Btn_ReturnToMain.onClick.AddListener(delegate {
            MainSceneState main = GameManager.Instance.uiManager.mUIFacade.currentSceneState as MainSceneState;
            main.EnterMainPanel();
        });
        // 恢复默认设置
        Btn_RestoreTheDefaultSettings.onClick.RemoveAllListeners();
        Btn_RestoreTheDefaultSettings.onClick.AddListener(delegate {
            GameManager.Instance.configManager.RestoreTheDefaultSettings();
            Refresh();
        });
    }

    public void EnterPanel()
    {
        gameObject.SetActive(true);
    }

    public void UpdatePanel()
    {

    }

    public void ExitPanel()
    {
        GameManager.Instance.configManager.Save();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 刷新
    /// </summary>
    private void Refresh()
    {
        ConfigManager.Config config = GameManager.Instance.configManager.mConfig;
        // BGM开关
        Toggle_BGM.onValueChanged.RemoveAllListeners();
        Toggle_BGM.isOn = config.isPlayBGM;
        Toggle_BGM.onValueChanged.AddListener(delegate { 
            config.isPlayBGM = Toggle_BGM.isOn;
            if (config.isPlayBGM)
            {
                GameManager.Instance.audioSourceManager.ReplayCurrentClip();
            }
            else
            {
                GameManager.Instance.audioSourceManager.StopAllMusic();
            }
        });
        // 音效开关
        Toggle_SoundEffect.onValueChanged.RemoveAllListeners();
        Toggle_SoundEffect.isOn = config.isPlaySE;
        Toggle_SoundEffect.onValueChanged.AddListener(delegate { config.isPlaySE = Toggle_SoundEffect.isOn; });
        // BGM大小滑块
        Slider_BGM.onValueChanged.RemoveAllListeners();
        Slider_BGM.minValue = 0f;
        Slider_BGM.maxValue = 1.0f;
        Slider_BGM.value = config.BGMVolume;
        Slider_BGM.onValueChanged.AddListener(delegate { 
            config.BGMVolume = Slider_BGM.value;
        });
        // 音效大小滑块
        Slider_SoundEffect.onValueChanged.RemoveAllListeners();
        Slider_SoundEffect.minValue = 0f;
        Slider_SoundEffect.maxValue = 1.0f;
        Slider_SoundEffect.value = config.SEVolume;
        Slider_SoundEffect.onValueChanged.AddListener(delegate { config.SEVolume = Slider_SoundEffect.value; });
        // 快速放卡
        Toggle_QuickCardRelease.onValueChanged.RemoveAllListeners();
        Toggle_QuickCardRelease.isOn = config.isEnableQuickReleaseCard;
        Toggle_QuickCardRelease.onValueChanged.AddListener(delegate { config.isEnableQuickReleaseCard = Toggle_QuickCardRelease.isOn; });
    }
}
