namespace EditorPanel
{

    using UnityEngine;
    using UnityEngine.UI;
    /// <summary>
    /// �༭���еĹؿ�����
    /// </summary>
    public class Img_StageConfig : MonoBehaviour
    {
        private EditorPanel mEditorPanel;

        private InputField Inp_StartBGM;
        private InputField Inp_StartEnergy;
        private InputField Inp_PauseCount;
        private Toggle Tog_TimeLimit;
        private InputField Inp_TotalSeconds;
        private Toggle Tog_CardCountLimit;
        private InputField Inp_CardCount;
        private Toggle Tog_Card;
        private Toggle Tog_StartCard;
        private Toggle Tog_Jewel;
        private InputField Inp_JewelCount;
        private Button Btn_ConfigCard;
        private Button Btn_ConfigStartCard;
        private Button Btn_Background;
        private Button Btn_Illustrate;
        private Button Btn_AdditionalNotes;
        private Button Btn_Param;
        private InputField Inp_Difficulty;

        private void Awake()
        {
            Inp_StartBGM = transform.Find("Emp_ConfigList").Find("Emp_StartBGM").Find("InputField").GetComponent<InputField>();
            Inp_StartBGM.onEndEdit.AddListener(delegate { OnStartBGMInputFieldChange(); });
            Inp_StartEnergy = transform.Find("Emp_ConfigList").Find("Emp_InitialEnergy").Find("InputField").GetComponent<InputField>();
            Inp_StartEnergy.onEndEdit.AddListener(delegate { OnStartEnergyInputFieldChange(); });
            Inp_PauseCount = transform.Find("Emp_ConfigList").Find("Emp_PauseCount").Find("InputField").GetComponent<InputField>();
            Inp_PauseCount.onEndEdit.AddListener(delegate { OnPauseCountInputFieldChange(); });
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

            Tog_Jewel = transform.Find("Emp_ConfigList").Find("Tog_Jewel").GetComponent<Toggle>();
            Tog_Jewel.onValueChanged.AddListener(delegate {
                BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
                info.isEnableJewelCount = Tog_Jewel.isOn;
                Inp_JewelCount.interactable = Tog_Jewel.isOn;
            });
            Inp_JewelCount = Tog_Jewel.transform.Find("InputField").GetComponent<InputField>();
            Inp_JewelCount.onEndEdit.AddListener(delegate
            {
                if (Inp_JewelCount.text == null || Inp_JewelCount.text.Equals(""))
                {
                    Inp_JewelCount.text = "" + 3;
                    return;
                }
                int c = int.Parse(Inp_JewelCount.text);
                if (c < 0 || c > 3)
                {
                    Inp_JewelCount.text = "" + 3;
                    return;
                }
                BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
                info.jewelCount = c;
            });
            Inp_JewelCount.interactable = false;

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

            Btn_Param = transform.Find("Emp_ConfigList").Find("Emp_Param").Find("Button").GetComponent<Button>();
            Btn_Param.onClick.AddListener(delegate { mEditorPanel.ShowDefineParamUI(mEditorPanel.GetCurrentStageInfo().ParamArrayDict); });

            Inp_Difficulty = transform.Find("Emp_ConfigList").Find("Emp_Difficulty").Find("InputField").GetComponent<InputField>();
            Inp_Difficulty.onEndEdit.AddListener(delegate { OnDifficultyInputFieldChange();  });
        }

        public void Initial()
        {
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();

            Inp_StartBGM.text = info.startBGMRefence + "";

            Inp_StartEnergy.text = info.startCost + "";
            Inp_PauseCount.text = info.pauseCount + "";

            Tog_TimeLimit.isOn = info.isEnableTimeLimit;
            Inp_TotalSeconds.interactable = Tog_TimeLimit.isOn;
            Inp_TotalSeconds.text = info.totalSeconds + "";

            Tog_CardCountLimit.isOn = info.isEnableCardCount;
            Inp_CardCount.interactable = Tog_CardCountLimit.isOn;
            Inp_CardCount.text = info.cardCount + "";

            Tog_Card.isOn = info.isEnableCardLimit;
            Btn_ConfigCard.interactable = Tog_Card.isOn;

            Tog_StartCard.isOn = info.isEnableStartCard;
            Btn_ConfigStartCard.interactable = Tog_StartCard.isOn;

            Tog_Jewel.isOn = info.isEnableJewelCount;
            Inp_JewelCount.interactable = Tog_Jewel.isOn;
            Inp_JewelCount.text = info.jewelCount + "";

            Inp_Difficulty.text = info.difficulty.ToString();
        }

        public void SetEditorPanel(EditorPanel panel)
        {
            mEditorPanel = panel;
        }

        /// <summary>
        /// �������ʼBGM����ʱ
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
        /// �������ʼ����ֵʱ
        /// </summary>
        private void OnStartEnergyInputFieldChange()
        {
            if (Inp_StartEnergy.text == null || Inp_StartEnergy.text.Equals(""))
            {
                Inp_StartEnergy.text = "" + 0;
                return;
            }
            int startEnergy = int.Parse(Inp_StartEnergy.text);
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.startCost = startEnergy;
        }

        /// <summary>
        /// ����ͣ�����ı�ʱ
        /// </summary>
        private void OnPauseCountInputFieldChange()
        {
            if (Inp_PauseCount.text == null || Inp_PauseCount.text.Equals(""))
            {
                Inp_PauseCount.text = "" + -1;
                return;
            }
            int pauseCount = int.Parse(Inp_PauseCount.text);
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.pauseCount = pauseCount;
        }

        /// <summary>
        /// ������ʱ������ѡ�����ı�ʱ
        /// </summary>
        private void OnTimeLimitToggleChange()
        {
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.isEnableTimeLimit = Tog_TimeLimit.isOn;
            Inp_TotalSeconds.interactable = Tog_TimeLimit.isOn;
        }

        /// <summary>
        /// ��ʱ��������������ʱ
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
        /// �����ÿ�Ƭ��������ѡ�����ı�ʱ
        /// </summary>
        private void OnCardCountLimitToggleChange()
        {
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.isEnableCardCount = Tog_CardCountLimit.isOn;
            Inp_CardCount.interactable = Tog_CardCountLimit.isOn;
        }

        /// <summary>
        /// ����Ƭ����������������ʱ
        /// </summary>
        private void OnCardCountLimitInputFieldChange()
        {
            if (Inp_CardCount.text == null || Inp_CardCount.text.Equals(""))
            {
                Inp_CardCount.text = "" + 18;
                return;
            }
            int c = int.Parse(Inp_CardCount.text);
            if (c < 0 || c > 18)
            {
                Inp_CardCount.text = "" + 18;
                return;
            }
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.cardCount = c;
        }

        /// <summary>
        /// �����ÿ�Ƭ����ѡ�����ı�ʱ
        /// </summary>
        private void OnCardLimitToggleChange()
        {
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.isEnableCardLimit = Tog_Card.isOn;
            Btn_ConfigCard.interactable = Tog_Card.isOn;
        }

        /// <summary>
        /// �����ÿ�Ƭ��ť�����ʱ
        /// </summary>
        private void OnCardConfigButtonClick()
        {
            mEditorPanel.SetConfigCardUIEnable(true);
        }

        /// <summary>
        /// �����ó�ʼ��Ƭѡ�����ı�ʱ
        /// </summary>
        private void OnStartCardToggleChange()
        {
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.isEnableStartCard = Tog_StartCard.isOn;
            Btn_ConfigStartCard.interactable = Tog_StartCard.isOn;
        }

        /// <summary>
        /// �����ó�ʼ��Ƭ��ť�����ʱ
        /// </summary>
        private void OnStartCardConfigButtonClick()
        {
            mEditorPanel.SetStartCardEditorUIEnable(true);
        }

        /// <summary>
        /// ���Ѷȵ�λ�ı�ʱ
        /// </summary>
        private void OnDifficultyInputFieldChange()
        {
            if (Inp_Difficulty.text == null || Inp_Difficulty.text.Equals(""))
            {
                Inp_Difficulty.text = "" + 0;
                return;
            }
            int diff = int.Parse(Inp_Difficulty.text);
            BaseStage.StageInfo info = mEditorPanel.GetCurrentStageInfo();
            info.difficulty = diff;
        }
    }

}