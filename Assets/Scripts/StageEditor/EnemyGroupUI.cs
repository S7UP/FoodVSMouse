namespace EditorPanel
{

    using System.Collections.Generic;

    using UnityEngine;
    using UnityEngine.UI;

    public class EnemyGroupUI : MonoBehaviour
    {
        // ����
        public BaseEnemyGroup mBaseEnemyGroup;

        // ����
        private EditorPanel mEditorPanel;
        private GameObject Emp_MaskList;
        private Image Img_Mouse;
        private Image[] Img_MaskList;
        private Text Tex_Type;
        private Text Tex_Shape;
        private Dropdown Dro_StartIndex;
        private Dropdown Dro_Number;
        private Dropdown Dro_ApartIndex;
        private Button Btn_Del;
        private Button Btn_EnemyType;
        private RectTransform rectTransform;
        private Transform Emp_Info_Trans;
        private GameObject Emp_Hp;
        private InputField Inf_Hp;
        private InputField Inf_HpRate;
        private InputField Inf_AttackRate;
        private InputField Inf_MoveSpeedRate;
        private InputField Inf_Defence;
        private Text Tex_HpRate;
        private Text Tex_AttackRate;
        private Text Tex_MoveSpeedRate;
        private Text Tex_Defence;
        private Button Btn_Define;

        private void Awake()
        {
            // ���ó�ʼ��
            Emp_MaskList = transform.Find("Img_Mouse").Find("Emp_MaskList").gameObject;
            Img_Mouse = transform.Find("Img_Mouse").GetComponent<Image>();
            Emp_Info_Trans = transform.Find("Img_Mouse").Find("Emp_Info");
            Img_MaskList = new Image[Emp_MaskList.transform.childCount];
            for (int i = 0; i < Emp_MaskList.transform.childCount; i++)
            {
                Img_MaskList[i] = Emp_MaskList.transform.GetChild(i).GetComponent<Image>();
                Img_MaskList[i].enabled = false; // Ĭ�϶������ص�
            }
            Tex_Type = Emp_Info_Trans.Find("Tex_Type").GetComponent<Text>();
            Tex_Shape = Emp_Info_Trans.Find("Tex_Shape").GetComponent<Text>();
            Dro_StartIndex = Emp_Info_Trans.Find("Emp_StartIndex").Find("Dropdown").GetComponent<Dropdown>();
            Dro_Number = Emp_Info_Trans.Find("Emp_Number").Find("Dropdown").GetComponent<Dropdown>();
            Dro_ApartIndex = Emp_Info_Trans.Find("Emp_ApartIndex").Find("Dropdown").GetComponent<Dropdown>();
            rectTransform = transform.GetComponent<RectTransform>();

            Btn_Del = Emp_Info_Trans.Find("Btn_Del").GetComponent<Button>();
            Btn_EnemyType = transform.Find("Img_Mouse").GetComponent<Button>();

            Emp_Hp = Emp_Info_Trans.Find("Emp_Hp").gameObject;
            Inf_Hp = Emp_Hp.transform.Find("InputField").GetComponent<InputField>();

            Inf_HpRate = Emp_Info_Trans.Find("Emp_HpRate").Find("InputField").GetComponent<InputField>();
            Inf_AttackRate = Emp_Info_Trans.Find("Emp_AttackRate").Find("InputField").GetComponent<InputField>();
            Inf_MoveSpeedRate = Emp_Info_Trans.Find("Emp_MoveSpeedRate").Find("InputField").GetComponent<InputField>();
            Inf_Defence = Emp_Info_Trans.Find("Emp_Defence").Find("InputField").GetComponent<InputField>();

            Tex_HpRate = Inf_HpRate.transform.Find("Text").GetComponent<Text>();
            Tex_AttackRate = Inf_AttackRate.transform.Find("Text").GetComponent<Text>();
            Tex_MoveSpeedRate = Inf_MoveSpeedRate.transform.Find("Text").GetComponent<Text>();
            Tex_Defence = Inf_Defence.transform.Find("Text").GetComponent<Text>();

            Btn_Define = Emp_Info_Trans.Find("Btn_Define").GetComponent<Button>();
        }

        public void Initial()
        {
            Dro_StartIndex.gameObject.SetActive(true);
            Dro_Number.gameObject.SetActive(true);
            Dro_ApartIndex.gameObject.SetActive(true);

            Dro_StartIndex.onValueChanged.RemoveAllListeners();
            Dro_Number.onValueChanged.RemoveAllListeners();
            Dro_ApartIndex.onValueChanged.RemoveAllListeners();
            // �������ʼ��
            Dro_StartIndex.ClearOptions();
            Dro_Number.ClearOptions();
            Dro_ApartIndex.ClearOptions();
            // ����
            Dro_StartIndex.onValueChanged.RemoveAllListeners();
            List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
            for (int i = 0; i < 7; i++)
            {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = (i + 1).ToString();
                dataList.Add(data);
            }
            Dro_StartIndex.AddOptions(dataList);
            Dro_StartIndex.value = 0;
            Dro_StartIndex.onValueChanged.AddListener(delegate
            {
                Dro_StartIndexOnValueChanged();
            });
            // ����
            Dro_Number.onValueChanged.RemoveAllListeners();
            List<Dropdown.OptionData> dataList2 = new List<Dropdown.OptionData>();
            for (int i = 0; i <= 7; i++)
            {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = (i).ToString();
                dataList2.Add(data);
            }
            Dro_Number.AddOptions(dataList2);
            Dro_Number.value = 1;
            Dro_Number.onValueChanged.AddListener(delegate
            {
                Dro_NumberOnValueChanged();
            });
            // HP
            Inf_Hp.onEndEdit.RemoveAllListeners();
            Inf_Hp.onEndEdit.AddListener(delegate
            {
                float value = int.Parse(Inf_Hp.text);
                if (value <= 0)
                    value = 100;
                Inf_HpOnValueChanged(value);
            });
            // ��·��
            Dro_ApartIndex.onValueChanged.RemoveAllListeners();
            List<Dropdown.OptionData> dataList3 = new List<Dropdown.OptionData>();
            for (int i = 0; i < mEditorPanel.GetCurrentStageInfo().apartList.Count; i++)
            {
                Dropdown.OptionData data = new Dropdown.OptionData();
                data.text = (i + 1).ToString();
                dataList3.Add(data);
            }
            Dro_ApartIndex.AddOptions(dataList3);
            Dro_ApartIndex.value = 0;
            Dro_ApartIndex.onValueChanged.AddListener(delegate
            {
                Dro_ApartIndexOnValueChanged();
            });


            // HpRate
            Inf_HpRate.onEndEdit.RemoveAllListeners();
            Inf_HpRate.onEndEdit.AddListener(delegate {
                Inf_HpRateOnEndEdit(float.Parse(Inf_HpRate.text));
            });
            // AttackRate
            Inf_AttackRate.onEndEdit.RemoveAllListeners();
            Inf_AttackRate.onEndEdit.AddListener(delegate {
                Inf_AttackRateOnEndEdit(float.Parse(Inf_AttackRate.text));
            });
            // MoveSpeedRate
            Inf_MoveSpeedRate.onEndEdit.RemoveAllListeners();
            Inf_MoveSpeedRate.onEndEdit.AddListener(delegate {
                Inf_MoveSpeedRateOnEndEdit(float.Parse(Inf_MoveSpeedRate.text));
            });
            // Defence
            Inf_Defence.onEndEdit.RemoveAllListeners();
            Inf_Defence.onEndEdit.AddListener(delegate {
                Inf_DefenceOnEndEdit(float.Parse(Inf_Defence.text));
            });

            // �Զ������
            Btn_Define.onClick.RemoveAllListeners();
            Btn_Define.onClick.AddListener(delegate {
                mEditorPanel.ShowDefineParamUI(mBaseEnemyGroup.GetEnemyAttribute().ParamDict);
            });
        }

        /// <summary>
        /// ��ȡɾ����ť����
        /// </summary>
        /// <returns></returns>
        public Button GetDelButton()
        {
            return Btn_Del;
        }

        /// <summary>
        /// ��ȡ�����������ఴť����
        /// </summary>
        /// <returns></returns>
        public Button GetChangeEnemyTypeButton()
        {
            return Btn_EnemyType;
        }

        /// <summary>
        /// ��ʼ��������ֵ�����ı���¼�
        /// </summary>
        private void Dro_StartIndexOnValueChanged()
        {
            mBaseEnemyGroup.mStartIndex = Dro_StartIndex.value;
            DisplayUpdate();
        }

        /// <summary>
        /// ����������ֵ�����ı���¼�
        /// </summary>
        private void Dro_NumberOnValueChanged()
        {
            mBaseEnemyGroup.mCount = Dro_Number.value;
            DisplayUpdate();
        }

        /// <summary>
        /// Ѫ�������ֵ�����ı���¼�
        /// </summary>
        private void Inf_HpOnValueChanged(float hp)
        {
            mBaseEnemyGroup.mHp = hp;
            DisplayUpdate();
        }

        /// <summary>
        /// ��·��������ֵ�����ı��
        /// </summary>
        private void Dro_ApartIndexOnValueChanged()
        {
            mBaseEnemyGroup.mApartIndex = Dro_ApartIndex.value;
            DisplayUpdate();
        }

        /// <summary>
        /// �������������ֵ�����ı���¼�
        /// </summary>
        private void Inf_HpRateOnEndEdit(float hpRate)
        {
            if (hpRate <= 0)
            {
                hpRate = 1;
                Inf_HpRate.text = hpRate.ToString();
            }
            mBaseEnemyGroup.GetEnemyAttribute().HpRate = hpRate;
            DisplayUpdate();
            if (hpRate == 1)
                Tex_HpRate.color = Color.black;
            else
                Tex_HpRate.color = Color.red;
        }

        /// <summary>
        /// �������������ֵ�����ı���¼�
        /// </summary>
        private void Inf_AttackRateOnEndEdit(float attackRate)
        {
            if (attackRate <= 0)
            {
                attackRate = 1;
                Inf_AttackRate.text = attackRate.ToString();
            }
            mBaseEnemyGroup.GetEnemyAttribute().AttackRate = attackRate;
            DisplayUpdate();
            if (attackRate == 1)
                Tex_AttackRate.color = Color.black;
            else
                Tex_AttackRate.color = Color.red;
        }

        /// <summary>
        /// ���ٱ��������ֵ�����ı���¼�
        /// </summary>
        private void Inf_MoveSpeedRateOnEndEdit(float moveSpeedRate)
        {
            if (moveSpeedRate <= 0)
            {
                moveSpeedRate = 1;
                Inf_MoveSpeedRate.text = moveSpeedRate.ToString();
            }
            mBaseEnemyGroup.GetEnemyAttribute().MoveSpeedRate = moveSpeedRate;
            DisplayUpdate();
            if (moveSpeedRate == 1)
                Tex_MoveSpeedRate.color = Color.black;
            else
                Tex_MoveSpeedRate.color = Color.red;
        }

        /// <summary>
        /// ����ֵ�����ֵ�����ı���¼�
        /// </summary>
        private void Inf_DefenceOnEndEdit(float defence)
        {
            if (defence < 0)
            {
                defence = 0;
                Inf_Defence.text = defence.ToString();
            }
            else if (defence > 1)
            {
                defence = 1;
                Inf_Defence.text = defence.ToString();
            }
            mBaseEnemyGroup.GetEnemyAttribute().Defence = defence;
            DisplayUpdate();
            if (defence == 0)
                Tex_Defence.color = Color.black;
            else
                Tex_Defence.color = Color.red;
        }



        /// <summary>
        /// ���ⲿ���ð󶨵�EnemyGroup
        /// </summary>
        /// <param name="baseEnemyGroup"></param>
        public void SetEnemyGroup(BaseEnemyGroup baseEnemyGroup)
        {
            mBaseEnemyGroup = baseEnemyGroup;
            ChangeMouseInfo(baseEnemyGroup.mEnemyInfo);
            DisplayUpdate();
            // ������ѡ�����
            Dro_StartIndex.value = mBaseEnemyGroup.mStartIndex;
            if (mEditorPanel.GetCurrentRoundInfo().isBossRound)
            {
                // �����BOSS�֣���ȡ��������ʾ��ͬ����BOSSֻ����1ֻ��
                Dro_Number.gameObject.SetActive(false);
                // ��ʾѪ������
                Inf_Hp.gameObject.SetActive(true);
                Inf_Hp.text = mBaseEnemyGroup.mHp.ToString();
            }
            else
            {
                Dro_Number.gameObject.SetActive(true);
                // ����Ѫ������
                Inf_Hp.gameObject.SetActive(false);

                Dro_Number.value = mBaseEnemyGroup.mCount;
            }
            // ����о�ˢ����Խ���ˣ���ô������Ϊ0
            if (mBaseEnemyGroup.mApartIndex > Dro_ApartIndex.options.Count - 1)
            {
                mBaseEnemyGroup.mApartIndex = 0;
            }
            Dro_ApartIndex.value = mBaseEnemyGroup.mApartIndex;

            // ���˵�����
            Inf_HpRate.text = mBaseEnemyGroup.GetEnemyAttribute().HpRate.ToString();
            Inf_AttackRate.text = mBaseEnemyGroup.GetEnemyAttribute().AttackRate.ToString();
            Inf_MoveSpeedRate.text = mBaseEnemyGroup.GetEnemyAttribute().MoveSpeedRate.ToString();
            Inf_Defence.text = mBaseEnemyGroup.GetEnemyAttribute().Defence.ToString();
            Inf_HpRateOnEndEdit(mBaseEnemyGroup.GetEnemyAttribute().HpRate);
            Inf_AttackRateOnEndEdit(mBaseEnemyGroup.GetEnemyAttribute().AttackRate);
            Inf_MoveSpeedRateOnEndEdit(mBaseEnemyGroup.GetEnemyAttribute().MoveSpeedRate);
            Inf_DefenceOnEndEdit(mBaseEnemyGroup.GetEnemyAttribute().Defence);
        }

        public void ChangeMouseInfo(BaseEnemyGroup.EnemyInfo enemyInfo)
        {
            mBaseEnemyGroup.mEnemyInfo = enemyInfo;
            // �޸���ͼ
            if (mEditorPanel.GetCurrentRoundInfo().isBossRound)
                Img_Mouse.sprite = GameManager.Instance.GetSprite("Boss/" + enemyInfo.type + "/" + enemyInfo.shape + "/display");
            else
                Img_Mouse.sprite = GameManager.Instance.GetSprite("Mouse/" + enemyInfo.type + "/" + enemyInfo.shape + "/display");
            Img_Mouse.SetNativeSize();
            // ����rect
            float w = Img_Mouse.GetComponent<RectTransform>().rect.width / 2 + Emp_MaskList.GetComponent<RectTransform>().rect.width + Emp_Info_Trans.GetComponent<RectTransform>().rect.width / 2;
            rectTransform.localPosition = new Vector3(w, rectTransform.localPosition.y, rectTransform.localPosition.z);
            rectTransform.sizeDelta = new Vector2(w, rectTransform.sizeDelta.y);
        }



        // ����ʾ����һ�θ���
        public void DisplayUpdate()
        {
            // �����������ʾ����
            Tex_Type.text = "�����ţ�" + mBaseEnemyGroup.mEnemyInfo.type;
            Tex_Shape.text = "���ֱ�ţ�" + mBaseEnemyGroup.mEnemyInfo.shape;

            // ���������з���
            for (int i = 0; i < Img_MaskList.Length; i++)
            {
                Img_MaskList[i].enabled = false;
            }

            // �������޸ķ�����ɫ

            // ���������ʼ���Լ����������غ���ʾ����
            int startIndex = mBaseEnemyGroup.mStartIndex;
            int rowCount = 7; // ��ǰ��·�������
            for (int i = 0; i < mBaseEnemyGroup.mCount; i++)
            {
                int index = (startIndex + i) % rowCount;
                Img_MaskList[index].enabled = true;
            }


        }

        public void SetEditorPanel(EditorPanel panel)
        {
            mEditorPanel = panel;
        }

        /// <summary>
        /// ��������ػ���ʱ��ִ�г�ʼ������
        /// </summary>
        private void OnDisable()
        {
            mBaseEnemyGroup = null;
        }

    }

}