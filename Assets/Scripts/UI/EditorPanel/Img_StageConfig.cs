using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 编辑器中的关卡配置
/// </summary>
public class Img_StageConfig : MonoBehaviour
{
    private EditorPanel mEditorPanel;

    private InputField Inp_StartBGM;
    private InputField Inp_StartEnergy;
    private Toggle Tog_TimeLimit;
    private InputField Inp_TotalSeconds;
    private Toggle Tog_CardCountLimit;
    private InputField Inp_CardCount;
    private Toggle Tog_Card;
    private Toggle Tog_StartCard;
    private Button Btn_ConfigCard;
    private Button Btn_ConfigStartCard;
    private Button Btn_Background;
    private Button Btn_Illustrate;
    private Button Btn_AdditionalNotes;

    private void Awake()
    {
        Inp_StartBGM = transform.Find("Emp_ConfigList").Find("Emp_StartBGM").Find("InputField").GetComponent<InputField>();
        Inp_StartBGM.onEndEdit.AddListener(delegate { OnStartBGMInputFieldChange(); });
        Inp_StartEnergy = transform.Find("Emp_ConfigList").Find("Emp_InitialEnergy").Find("InputField").GetComponent<InputField>();
        Inp_StartEnergy.onEndEdit.AddListener(delegate { OnStartEnergyInputFieldChange(); });
        Tog_TimeLimit = transform.Find("Emp_ConfigList").Find("Tog_TimeLimit").GetComponent<Toggle>();
        Tog_TimeLimit.onValueChanged.AddListener(delegate { OnTimeLimitToggleChange(); });
        Inp_TotalSeconds = transform.Find("Emp_ConfigList").Find("Tog_TimeLimit").Find("InputField").GetComponent<InputField>();
        Inp_TotalSeconds.onEndEdit.AddListener(delegate { OnTimeLimitInputFieldChange(); });
        Inp_TotalSeconds.interactable = false;

        Tog_CardCountLimit = transform.Find("Emp_ConfigList").Find("Tog_CardCountLimit").GetComponent<Toggle>();
        Tog_CardCountLimit.onValueChanged.AddListener(delegate { OnCardCountLimitToggleChange(); });
        Inp_CardCount = transform.Find("Emp_ConfigList").Find("Tog_CardCountLimit").Find("InputField").GetComponent<InputField>();
        Inp_CardCount.onEndEdit.AddListener(delegate { OnCardCountLimitInputFieldChange(); });
        Inp_CardCount.interactable = false;

        Tog_Card = transform.Find("Emp_ConfigList").Find("Tog_Card").GetComponent<Toggle>();
        Tog_Card.onValueChanged.AddListener(delegate { OnCardLimitToggleChange(); });
        Tog_StartCard = transform.Find("Emp_ConfigList").Find("Tog_StartCard").GetComponent<Toggle>();
        Tog_StartCard.onValueChanged.AddListener(delegate { OnStartCardToggleChange(); });

        Btn_ConfigCard = transform.Find("Emp_ConfigList").Find("Tog_Card").Find("Button").GetComponent<Button>();
        Btn_ConfigCard.onClick.AddListener(delegate { OnCardConfigButtonClick(); });
        Btn_ConfigCard.interactable = false;

        Btn_ConfigStartCard = Tog_StartCard.transform.Find("Button").GetComponent<Button>();
        Btn_ConfigStartCard.onClick.AddListener(delegate { OnStartCardConfigButtonClick(); });
        Btn_ConfigStartCard.interactable = false;

        Btn_Background = transform.Find("Emp_ConfigList").Find("Emp_Background").Find("Button").GetComponent<Button>();
        Btn_Background.onClick.AddListener(delegate { mEditorPanel.ShowEditTextArea(0); });
        Btn_Illustrate = transform.Find("Emp_ConfigList").Find("Emp_Illustrate").Find("Button").GetComponent<Button>();
        Btn_Illustrate.onClick.AddListener(delegate { mEditorPanel.ShowEditTextArea(1); });
        Btn_AdditionalNotes = transform.Find("Emp_ConfigList").Find("Emp_AdditionalNotes").Find("Button").GetComponent<Button>();
        Btn_AdditionalNotes.onClick.AddListener(delegate { mEditorPanel.ShowEditTextArea(2); });
    }

    public void Initial()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();

        Inp_StartBGM.text = info.startBGMRefence + "";

        Inp_StartEnergy.text = info.startCost + "";

        Tog_TimeLimit.isOn = info.isEnableTimeLimit;
        Inp_TotalSeconds.interactable = Tog_TimeLimit.isOn;
        Inp_TotalSeconds.text = info.totalSeconds+"";

        Tog_CardCountLimit.isOn = info.isEnableCardCount;
        Inp_CardCount.interactable = Tog_CardCountLimit.isOn;
        Inp_CardCount.text = info.cardCount + "";

        Tog_Card.isOn = info.isEnableCardLimit;
        Btn_ConfigCard.interactable = Tog_Card.isOn;

        Tog_StartCard.isOn = info.isEnableStartCard;
        Btn_ConfigStartCard.interactable = Tog_StartCard.isOn;
    }

    public void SetEditorPanel(EditorPanel panel)
    {
        mEditorPanel = panel;
    }

    /// <summary>
    /// 当输入初始BGM引用时
    /// </summary>
    private void OnStartBGMInputFieldChange()
    {
        if (Inp_StartBGM.text == null || Inp_StartBGM.text.Equals(""))
        {
            Inp_StartBGM.text = "";
            return;
        }
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.startBGMRefence = Inp_StartBGM.text;
    }

    /// <summary>
    /// 当输入初始能量值时
    /// </summary>
    private void OnStartEnergyInputFieldChange()
    {
        if(Inp_StartEnergy.text==null || Inp_StartEnergy.text.Equals(""))
        {
            Inp_StartEnergy.text = ""+0;
            return;
        }
        int startEnergy = int.Parse(Inp_StartEnergy.text);
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.startCost = startEnergy;
    }

    /// <summary>
    /// 当启用时间限制选框发生改变时
    /// </summary>
    private void OnTimeLimitToggleChange()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.isEnableTimeLimit = Tog_TimeLimit.isOn;
        Inp_TotalSeconds.interactable = Tog_TimeLimit.isOn;
    }

    /// <summary>
    /// 当时间限制输入框更改时
    /// </summary>
    private void OnTimeLimitInputFieldChange()
    {
        if (Inp_TotalSeconds.text == null || Inp_TotalSeconds.text.Equals(""))
        {
            Inp_TotalSeconds.text = "" + 0;
            return;
        }
        int seconds = int.Parse(Inp_TotalSeconds.text);
        if (seconds < 0)
        {
            Inp_TotalSeconds.text = "" + 0;
            return;
        }
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.totalSeconds = seconds;
    }


    /// <summary>
    /// 当启用卡片数量限制选框发生改变时
    /// </summary>
    private void OnCardCountLimitToggleChange()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.isEnableCardCount = Tog_CardCountLimit.isOn;
        Inp_CardCount.interactable = Tog_CardCountLimit.isOn;
    }

    /// <summary>
    /// 当卡片数量限制输入框更改时
    /// </summary>
    private void OnCardCountLimitInputFieldChange()
    {
        if (Inp_CardCount.text == null || Inp_CardCount.text.Equals(""))
        {
            Inp_CardCount.text = "" + 18;
            return;
        }
        int c = int.Parse(Inp_CardCount.text);
        if (c < 0 || c>18)
        {
            Inp_CardCount.text = "" + 18;
            return;
        }
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.cardCount = c;
    }

    /// <summary>
    /// 当启用卡片限制选框发生改变时
    /// </summary>
    private void OnCardLimitToggleChange()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.isEnableCardLimit = Tog_Card.isOn;
        Btn_ConfigCard.interactable = Tog_Card.isOn;
    }

    /// <summary>
    /// 当配置卡片按钮被点击时
    /// </summary>
    private void OnCardConfigButtonClick()
    {
        mEditorPanel.SetConfigCardUIEnable(true);
    }

    /// <summary>
    /// 当启用初始卡片选框发生改变时
    /// </summary>
    private void OnStartCardToggleChange()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.isEnableStartCard = Tog_StartCard.isOn;
        Btn_ConfigStartCard.interactable = Tog_StartCard.isOn;
    }

    /// <summary>
    /// 当配置初始卡片按钮被点击时
    /// </summary>
    private void OnStartCardConfigButtonClick()
    {
        mEditorPanel.SetStartCardEditorUIEnable(true);
    }
}
