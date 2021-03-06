using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// ?༭???еĹؿ?????
/// </summary>
public class Img_StageConfig : MonoBehaviour
{
    private EditorPanel mEditorPanel;

    private InputField Inp_StartEnergy;
    private Toggle Tog_TimeLimit;
    private InputField Inp_TotalSeconds;
    private Toggle Tog_CardCountLimit;
    private InputField Inp_CardCount;
    private Toggle Tog_Card;
    private Button Btn_ConfigCard;

    private void Awake()
    {
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
        Btn_ConfigCard = transform.Find("Emp_ConfigList").Find("Tog_Card").Find("Button").GetComponent<Button>();
        Btn_ConfigCard.onClick.AddListener(delegate { OnCardConfigButtonClick(); });
        Btn_ConfigCard.interactable = false;
    }

    public void Initial()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        Inp_StartEnergy.text = info.startCost + "";

        Tog_TimeLimit.isOn = info.isEnableTimeLimit;
        Inp_TotalSeconds.interactable = Tog_TimeLimit.isOn;
        Inp_TotalSeconds.text = info.totalSeconds+"";

        Tog_CardCountLimit.isOn = info.isEnableCardCount;
        Inp_CardCount.interactable = Tog_CardCountLimit.isOn;
        Inp_CardCount.text = info.cardCount + "";

        Tog_Card.isOn = info.isEnableCardLimit;
        Btn_ConfigCard.interactable = Tog_Card.isOn;
    }

    public void SetEditorPanel(EditorPanel panel)
    {
        mEditorPanel = panel;
    }

    /// <summary>
    /// ????????ʼ????ֵʱ
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
    /// ??????ʱ??????ѡ???????ı?ʱ
    /// </summary>
    private void OnTimeLimitToggleChange()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.isEnableTimeLimit = Tog_TimeLimit.isOn;
        Inp_TotalSeconds.interactable = Tog_TimeLimit.isOn;
    }

    /// <summary>
    /// ??ʱ????????????????ʱ
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
    /// ?????ÿ?Ƭ????????ѡ???????ı?ʱ
    /// </summary>
    private void OnCardCountLimitToggleChange()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.isEnableCardCount = Tog_CardCountLimit.isOn;
        Inp_CardCount.interactable = Tog_CardCountLimit.isOn;
    }

    /// <summary>
    /// ????Ƭ??????????????????ʱ
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
    /// ?????ÿ?Ƭ????ѡ???????ı?ʱ
    /// </summary>
    private void OnCardLimitToggleChange()
    {
        BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
        info.isEnableCardLimit = Tog_Card.isOn;
        Btn_ConfigCard.interactable = Tog_Card.isOn;
    }

    /// <summary>
    /// ?????ÿ?Ƭ??ť??????ʱ
    /// </summary>
    private void OnCardConfigButtonClick()
    {
        mEditorPanel.SetConfigCardUIEnable(true);
    }
}
