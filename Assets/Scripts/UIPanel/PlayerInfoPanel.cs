using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// �����Ϣ�������
/// </summary>
public class PlayerInfoPanel : BasePanel
{
    private const string Suit_path = "Character/";
    private const string Weapons_path = "Weapons/";
    private const string Jewel_path = "Jewel/";

    private Transform Trans_PlayerUI;
    private RectTransform RectTrans_Expbar1;
    private Text Tex_ExpValue;
    private Text Tex_Level;
    private Text Tex_PlayerName;
    private Image Img_PlayerImage;
    private Image Img_MainWeapons;
    private Image Img_Jewel1;
    private Image Img_Jewel2;
    private Image Img_Jewel3;

    private Text Tex_WorldMap;

    private GameObject SelectWeaponsUI;
    private Transform Tran_WeaponsDisplay;
    private List<WeaponsDisplay> weaponsDisplayList = new List<WeaponsDisplay>();
    private WeaponsDisplay currentSelectedWeaponsDisplay;

    private GameObject SelectJewelUI;
    private Transform Tran_JewelDisplay;
    private List<JewelDisplay> jewelDisplayList = new List<JewelDisplay>();
    private int currentSelecteJewelIndex;

    private GameObject SelectSuitUI;
    private CharacterDisplay currentSelectedCharacterDisplay;
    private Transform Tran_CharacterDisplay;
    private List<CharacterDisplay> characterDisplayList = new List<CharacterDisplay>();

    protected override void Awake()
    {
        base.Awake();
        Trans_PlayerUI = transform.Find("PlayerUI");
        RectTrans_Expbar1 = Trans_PlayerUI.Find("Img_Expbar1").GetComponent<RectTransform>();
        Tex_ExpValue = Trans_PlayerUI.Find("Tex_Exp").Find("Text").GetComponent<Text>();
        Tex_Level = Trans_PlayerUI.Find("Tex_Level").GetComponent<Text>();
        Tex_PlayerName = Trans_PlayerUI.Find("Tex_PlayerName").GetComponent<Text>();
        Img_PlayerImage = Trans_PlayerUI.Find("Btn_PlayerImage").Find("Image").GetComponent<Image>();
        Img_MainWeapons = Trans_PlayerUI.Find("Btn_MainWeapons").Find("Image").GetComponent<Image>();
        Img_Jewel1 = Trans_PlayerUI.Find("Btn_Jewel1").Find("Image").GetComponent<Image>();
        Img_Jewel2 = Trans_PlayerUI.Find("Btn_Jewel2").Find("Image").GetComponent<Image>();
        Img_Jewel3 = Trans_PlayerUI.Find("Btn_Jewel3").Find("Image").GetComponent<Image>();

        Tex_WorldMap = transform.Find("WorldMap").Find("Text").GetComponent<Text>();

        SelectWeaponsUI = transform.Find("SelectWeaponsUI").gameObject;
        Tran_WeaponsDisplay = SelectWeaponsUI.transform.Find("Img_center").Find("Emp_Weapons").Find("Scr").Find("Viewport").Find("Content");

        SelectJewelUI = transform.Find("SelectJewelUI").gameObject;
        Tran_JewelDisplay = SelectJewelUI.transform.Find("Img_center").Find("Emp_Jewel").Find("Scr").Find("Viewport").Find("Content");

        SelectSuitUI = transform.Find("SelectSuitUI").gameObject;
        Tran_CharacterDisplay = SelectSuitUI.transform.Find("Img_center").Find("Emp_Suit").Find("Scr").Find("Viewport").Find("Content");
    }

    private void Initial()
    {
        ClearWeaponsDisplayList();
        FillWeaponsDisplayList();
        ClearJewelDisplayList();
        FillJewelDisplayList();
        ClearCharacterDisplayList();
        FillCharacterDisplayList();

        // ��Ҫ�����ε�UI
        SelectWeaponsUI.SetActive(false);
        SelectJewelUI.SetActive(false);
        SelectSuitUI.SetActive(false);
    }

    public override void EnterPanel()
    {
        base.EnterPanel();
        Initial();
    }

    public override void InitPanel()
    {
        base.InitPanel();
        Initial();
        // ��ȡ�浵��Ϣ������UI
        PlayerData data = PlayerData.GetInstance();
        Tex_PlayerName.text = data.name;
        Tex_Level.text = data.level.ToString();
        Tex_ExpValue.text = data.currentExp.ToString();
        // ��װ
        UpdateSelectedCharacterDisplay();
        // ����
        Img_MainWeapons.sprite = GameManager.Instance.GetSprite(Weapons_path + data.weapons + "/0/icon");
        UpdateSelectedWeaponsDisplay(data.weapons);
        // ��ʯ
        UpdateSelectedJewelDisplay();
    }

    /// <summary>
    /// ���������
    /// </summary>
    private void ClearWeaponsDisplayList()
    {
        foreach (var item in weaponsDisplayList)
        {
            item.ExecuteRecycle();
        }
        weaponsDisplayList.Clear();
    }

    /// <summary>
    /// ���������
    /// </summary>
    private void FillWeaponsDisplayList()
    {
        Dictionary<WeaponsNameTypeMap, string> dict = WeaponsManager.GetWeaponsNameDict();
        foreach (var keyValuePair in dict)
        {
            WeaponsDisplay w = WeaponsDisplay.GetInstance();
            w.Initial();
            w.SetValues(this, (int)keyValuePair.Key, 0);
            w.transform.SetParent(Tran_WeaponsDisplay);
            w.transform.localScale = Vector3.one;
            weaponsDisplayList.Add(w);
        }
    }

    /// <summary>
    /// ��ձ�ʯ��
    /// </summary>
    private void ClearJewelDisplayList()
    {
        foreach (var item in jewelDisplayList)
        {
            item.ExecuteRecycle();
        }
        jewelDisplayList.Clear();
    }

    /// <summary>
    /// ��䱦ʯ��
    /// </summary>
    private void FillJewelDisplayList()
    {
        Dictionary<JewelNameTypeMap, string> dict = JewelManager.GetJewelNameDict();
        foreach (var keyValuePair in dict)
        {
            JewelDisplay j = JewelDisplay.GetInstance();
            j.Initial();
            j.SetValues(this, (int)keyValuePair.Key);
            j.transform.SetParent(Tran_JewelDisplay);
            j.transform.localScale = Vector3.one;
            jewelDisplayList.Add(j);
        }
    }

    /// <summary>
    /// ��ս�ɫ��
    /// </summary>
    private void ClearCharacterDisplayList()
    {
        foreach (var item in characterDisplayList)
        {
            item.ExecuteRecycle();
        }
        characterDisplayList.Clear();
    }

    /// <summary>
    /// ����ɫ��
    /// </summary>
    private void FillCharacterDisplayList()
    {
        Dictionary<CharacterNameShapeMap, string> dict = CharacterManager.GetCharacterNameDict();
        foreach (var keyValuePair in dict)
        {
            CharacterDisplay c = CharacterDisplay.GetInstance();
            c.Initial();
            c.SetValues(this, (int)keyValuePair.Key);
            c.transform.SetParent(Tran_CharacterDisplay);
            c.transform.localScale = Vector3.one;
            characterDisplayList.Add(c);
        }
    }

    /// <summary>
    /// ���鱨���İ�ť��������ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickIntelligenceIsland()
    {
        mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].EnterPanel();
    }

    /// <summary>
    /// ��ĳ������ͼ�걻ѡ��ʱ
    /// </summary>
    public void OnClickWeaponsDisplay(int type)
    {
        // ���������ʾ����
        UpdateSelectedWeaponsDisplay(type);
        // �������Ͻ����UI����
        Img_MainWeapons.sprite = GameManager.Instance.GetSprite(Weapons_path + type + "/0/icon");

        // ���´浵����
        PlayerData data = PlayerData.GetInstance();
        data.SetWeapons(type);
        // ����
        data.Save();
    }

    /// <summary>
    /// �����������ѡ����ʾ
    /// </summary>
    /// <param name="type"></param>
    private void UpdateSelectedWeaponsDisplay(int type)
    {
        currentSelectedWeaponsDisplay = null;
        foreach (var item in weaponsDisplayList)
        {
            item.CancelSelected();
            if (item.type == type)
                currentSelectedWeaponsDisplay = item;
        }
        if (currentSelectedWeaponsDisplay != null)
            currentSelectedWeaponsDisplay.SetSelected();
    }

    /// <summary>
    /// ������ѡ��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickSelectWeaponsUI()
    {
        SelectWeaponsUI.SetActive(true);
    }

    /// <summary>
    /// ���ر�����ѡ��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickExitSelectWeaponsUI()
    {
        SelectWeaponsUI.SetActive(false);
    }

    /// <summary>
    /// ��ĳ����ʯͼ�걻ѡ��ʱ
    /// </summary>
    public void OnClickJewelDisplay(int type)
    {
        // �ظ���⣬�������ʯ�ѱ�ѡ����ȡ��
        // ���´浵����
        PlayerData data = PlayerData.GetInstance();
        bool flag = false;
        for (int i = 0; i < data.jewelArray.Length; i++)
        {
            if(data.jewelArray[i] == type)
            {
                data.jewelArray[i] = -1;
                flag = true;
                break;
            }
        }
        if(!flag)
            data.jewelArray[currentSelecteJewelIndex] = type;
        // ����
        data.Save();
        // ���������ʾ����
        UpdateSelectedJewelDisplay();
    }

    /// <summary>
    /// ���±�ʯ���ѡ����ʾ
    /// </summary>
    /// <param name="type"></param>
    private void UpdateSelectedJewelDisplay()
    {
        foreach (var item in jewelDisplayList)
        {
            item.CancelSelected();
        }
        PlayerData data = PlayerData.GetInstance();
        foreach (var index in data.jewelArray)
        {
            if(index > -1)
            {
                jewelDisplayList[index].SetSelected();
            }
        }

        // �������Ͻ���ұ�ʯUI����
        if (data.jewelArray[0] == -1)
        {
            Img_Jewel1.sprite = null;
            Img_Jewel1.color = new Color(1, 1, 1, 0.01f);
        }
        else
        {
            Img_Jewel1.sprite = GameManager.Instance.GetSprite(Jewel_path + data.jewelArray[0]);
            Img_Jewel1.color = new Color(1, 1, 1, 1);
        }

        if (data.jewelArray[1] == -1)
        {
            Img_Jewel2.sprite = null;
            Img_Jewel2.color = new Color(1, 1, 1, 0.01f);
        }
        else
        {
            Img_Jewel2.sprite = GameManager.Instance.GetSprite(Jewel_path + data.jewelArray[1]);
            Img_Jewel2.color = new Color(1, 1, 1, 1);
        }

        if (data.jewelArray[2] == -1)
        {
            Img_Jewel3.sprite = null;
            Img_Jewel3.color = new Color(1, 1, 1, 0.01f);
        }
        else
        {
            Img_Jewel3.sprite = GameManager.Instance.GetSprite(Jewel_path + data.jewelArray[2]);
            Img_Jewel3.color = new Color(1, 1, 1, 1);
        }
    }

    /// <summary>
    /// ����ʯѡ��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickSelectJewelUI(int selecteIndex)
    {
        SelectJewelUI.SetActive(true);
        currentSelecteJewelIndex = selecteIndex;
    }

    /// <summary>
    /// ���رձ�ʯѡ��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickExitSelectJewelUI()
    {
        SelectJewelUI.SetActive(false);
    }


    /// <summary>
    /// ��ĳ����װͼ�걻ѡ��ʱ
    /// </summary>
    public void OnClickCharacterDisplay(int type)
    {
        // ���´浵����
        PlayerData data = PlayerData.GetInstance();
        data.suit = type;
        // ����
        data.Save();
        // ���������ʾ����
        UpdateSelectedCharacterDisplay();
    }

    /// <summary>
    /// ������װ���ѡ����ʾ
    /// </summary>
    /// <param name="type"></param>
    private void UpdateSelectedCharacterDisplay()
    {
        PlayerData data = PlayerData.GetInstance();
        foreach (var item in characterDisplayList)
        {
            item.CancelSelected();
            if (item.type == data.suit)
                item.SetSelected();
        }
        // �������Ͻ����
        Img_PlayerImage.sprite = GameManager.Instance.GetSprite(Suit_path + data.suit + "/icon");
    }

    /// <summary>
    /// ����װѡ��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickSelectSuitUI()
    {
        SelectSuitUI.SetActive(true);
    }

    /// <summary>
    /// ���ر���װѡ��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickExitSelectSuitUI()
    {
        SelectSuitUI.SetActive(false);
    }

    /// <summary>
    /// �����߹ؿ���ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickMainline()
    {
        mUIFacade.currentScenePanelDict[StringManager.MainlinePanel].EnterPanel();
    }
}
