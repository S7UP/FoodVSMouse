using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// �����Ϣ�������
/// </summary>
public class PlayerInfoPanel : BasePanel
{
    private const string Suit_path = "Character/";
    private const string Weapons_path = "Weapons/";
    private const string Jewel_path = "Jewel/";
    private const string Sprite_path = "UI/PlayerInfoPanel/";

    private RectTransform RectTrans_PlayerUI;
    private RectTransform RectTrans_Expbar1;
    private Text Tex_ExpValue;
    private Text Tex_Level;
    private Text Tex_PlayerName;
    private Text Tex_PlayTime; // �ؿ���ʱ��
    private Image Img_PlayerImage;
    private EventTrigger EventTrigger_PlayerImage;
    private Image Img_MainWeapons;
    private Button Btn_Jewel1;
    private Button Btn_Jewel2;
    private Button Btn_Jewel3;
    private Image Img_Jewel1;
    private Image Img_Jewel2;
    private Image Img_Jewel3;
    private EventTrigger EventTrigger_Weapons;
    private EventTrigger EventTrigger_Jewel1;
    private EventTrigger EventTrigger_Jewel2;
    private EventTrigger EventTrigger_Jewel3;
    private Button Btn_HidePlayerUI;
    private Button Btn_LegendChallenge;

    private Button Btn_WorldMap;
    private Text Tex_WorldMap_Position;
    private Text Tex_WorldMap_Lock;

    private Transform Trans_ModeSelectList;
    private Button Btn_WarriorChallenge;
    private Button Btn_Spurline;
    private Button Btn_MagicTower;
    private Button Btn_SpeedRun;
    private Button Btn_Unused;
    private Button Btn_LocalStage;

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

    private bool isHidePlayerUI = true; // �Ƿ��������UI

    protected override void Awake()
    {
        base.Awake();
        RectTrans_PlayerUI = transform.Find("PlayerUI").GetComponent<RectTransform>();
        RectTrans_Expbar1 = RectTrans_PlayerUI.Find("Img_Expbar1").GetComponent<RectTransform>();
        Tex_ExpValue = RectTrans_PlayerUI.Find("Tex_Exp").Find("Text").GetComponent<Text>();
        Tex_Level = RectTrans_PlayerUI.Find("Tex_Level").GetComponent<Text>();
        Tex_PlayerName = RectTrans_PlayerUI.Find("Tex_PlayerName").GetComponent<Text>();
        Tex_PlayTime = RectTrans_PlayerUI.Find("Tex_TotalTime").Find("Text").GetComponent<Text>();
        Img_PlayerImage = RectTrans_PlayerUI.Find("Btn_PlayerImage").Find("Image").GetComponent<Image>();
        EventTrigger_PlayerImage = RectTrans_PlayerUI.Find("Btn_PlayerImage").GetComponent<EventTrigger>();
        Img_MainWeapons = RectTrans_PlayerUI.Find("Btn_MainWeapons").Find("Image").GetComponent<Image>();
        Btn_Jewel1 = RectTrans_PlayerUI.Find("Btn_Jewel1").GetComponent<Button>();
        Btn_Jewel2 = RectTrans_PlayerUI.Find("Btn_Jewel2").GetComponent<Button>();
        Btn_Jewel3 = RectTrans_PlayerUI.Find("Btn_Jewel3").GetComponent<Button>();
        Img_Jewel1 = Btn_Jewel1.transform.Find("Image").GetComponent<Image>();
        Img_Jewel2 = Btn_Jewel2.transform.Find("Image").GetComponent<Image>();
        Img_Jewel3 = Btn_Jewel3.transform.Find("Image").GetComponent<Image>();
        EventTrigger_Weapons = RectTrans_PlayerUI.Find("Btn_MainWeapons").GetComponent<EventTrigger>();
        EventTrigger_Jewel1 = RectTrans_PlayerUI.Find("Btn_Jewel1").GetComponent<EventTrigger>();
        EventTrigger_Jewel2 = RectTrans_PlayerUI.Find("Btn_Jewel2").GetComponent<EventTrigger>();
        EventTrigger_Jewel3 = RectTrans_PlayerUI.Find("Btn_Jewel3").GetComponent<EventTrigger>();
        Btn_HidePlayerUI = RectTrans_PlayerUI.Find("Rect").Find("Button").GetComponent<Button>();

        Btn_WorldMap = transform.Find("WorldMap").GetComponent<Button>();
        Tex_WorldMap_Position = Btn_WorldMap.transform.Find("Text").GetComponent<Text>();
        Tex_WorldMap_Lock = Btn_WorldMap.transform.Find("Tex_Lock").GetComponent<Text>();

        Trans_ModeSelectList = transform.Find("ModeSelectList");
        Btn_WarriorChallenge = Trans_ModeSelectList.Find("Btn_WarriorChallenge").GetComponent<Button>();
        Btn_Spurline = Trans_ModeSelectList.Find("Btn_Spurline").GetComponent<Button>();
        Btn_MagicTower = Trans_ModeSelectList.Find("Btn_MagicTowerCake").GetComponent<Button>();
        Btn_LegendChallenge = Trans_ModeSelectList.Find("Btn_LegendChallenge").GetComponent<Button>();
        Btn_LocalStage = Trans_ModeSelectList.Find("Btn_LocalStage").GetComponent<Button>();
        Btn_SpeedRun = Trans_ModeSelectList.Find("Btn_SpeedRun").GetComponent<Button>();
        Btn_Unused = Trans_ModeSelectList.Find("Btn_Unused").GetComponent<Button>();

        SelectWeaponsUI = transform.Find("SelectWeaponsUI").gameObject;
        Tran_WeaponsDisplay = SelectWeaponsUI.transform.Find("Img_center").Find("Emp_Weapons").Find("Scr").Find("Viewport").Find("Content");

        SelectJewelUI = transform.Find("SelectJewelUI").gameObject;
        Tran_JewelDisplay = SelectJewelUI.transform.Find("Img_center").Find("Emp_Jewel").Find("Scr").Find("Viewport").Find("Content");

        SelectSuitUI = transform.Find("SelectSuitUI").gameObject;
        Tran_CharacterDisplay = SelectSuitUI.transform.Find("Img_center").Find("Emp_Suit").Find("Scr").Find("Viewport").Find("Content");

        AddEventTriggerOnAwake();
    }

    /// <summary>
    /// �������ƶ�������Ԫ���ϻḡ�����ֵ�Ч��
    /// </summary>
    private void AddEventTriggerOnAwake()
    {
        // ��ʯ
        EventTrigger[] trList = new EventTrigger[3] { EventTrigger_Jewel1, EventTrigger_Jewel2, EventTrigger_Jewel3 };
        for (int i = 0; i < trList.Length; i++)
        {
            EventTrigger trigger = trList[i];
            int index = i;
            EventTrigger.TriggerEvent tr1 = new EventTrigger.TriggerEvent();
            tr1.AddListener(delegate {
                int jewel = PlayerData.GetInstance().GetJewel(index);
                if (jewel > -1)
                    OnPointerEnterIcon(trigger.GetComponent<RectTransform>(), JewelManager.GetName(jewel) + "\n" + JewelManager.GetInfo(jewel));
            });
            trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = tr1 });
            EventTrigger.TriggerEvent tr2 = new EventTrigger.TriggerEvent();
            tr2.AddListener(delegate {
                int jewel = PlayerData.GetInstance().GetJewel(index);
                if (jewel > -1)
                    OnPointerExitIcon();
            });
            trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit, callback = tr2 });
        }
        // ����
        {
            EventTrigger trigger = EventTrigger_Weapons;
            EventTrigger.TriggerEvent tr1 = new EventTrigger.TriggerEvent();
            tr1.AddListener(delegate {
                int weapons = PlayerData.GetInstance().GetWeapons();
                if (weapons > -1)
                    OnPointerEnterIcon(trigger.GetComponent<RectTransform>(), WeaponsManager.GetName(weapons) + "\n(���ͼ����Ը�������)" + "\n���������" + WeaponsManager.GetInterval(weapons) + "\n" + WeaponsManager.GetInfo(weapons));
            });
            trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = tr1 });
            EventTrigger.TriggerEvent tr2 = new EventTrigger.TriggerEvent();
            tr2.AddListener(delegate {
                int weapons = PlayerData.GetInstance().GetWeapons();
                if (weapons > -1)
                    OnPointerExitIcon();
            });
            trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit, callback = tr2 });
        }
        // ����ͷ��
        {
            EventTrigger trigger = EventTrigger_PlayerImage;
            EventTrigger.TriggerEvent tr1 = new EventTrigger.TriggerEvent();
            tr1.AddListener(delegate {
                int weapons = PlayerData.GetInstance().GetWeapons();
                if (weapons > -1)
                    OnPointerEnterIcon(trigger.GetComponent<RectTransform>(), "����л���ҽ�ɫ���");
            });
            trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter, callback = tr1 });
            EventTrigger.TriggerEvent tr2 = new EventTrigger.TriggerEvent();
            tr2.AddListener(delegate {
                int weapons = PlayerData.GetInstance().GetWeapons();
                if (weapons > -1)
                    OnPointerExitIcon();
            });
            trigger.triggers.Add(new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit, callback = tr2 });
        }
    }

    private void Initial()
    {
        ClearWeaponsDisplayList();
        FillWeaponsDisplayList();
        ClearJewelDisplayList();
        FillJewelDisplayList();
        ClearCharacterDisplayList();
        FillCharacterDisplayList();
        // ��ʼ��ʾ���UI���
        ShowPlayerUI();
        // ��Ҫ�����ε�UI
        SelectWeaponsUI.SetActive(false);
        SelectJewelUI.SetActive(false);
        SelectSuitUI.SetActive(false);

        TryUnLockUI();
    }

    /// <summary>
    /// ��⡢�����Խ����ó����Ĳ���UI��ͬʱ�������ǵ���ʾ
    /// </summary>
    private void TryUnLockUI()
    {
        bool isDeveloperMode = ConfigManager.IsDeveloperMode();

        // �����ͼ
        {
            Image img = Btn_WorldMap.GetComponent<Image>();
            if (isDeveloperMode)
            {
                img.sprite = GameManager.Instance.GetSprite(Sprite_path+"WorldMap");
                img.color = new Color(1, 1, 1, 1);
                Btn_WorldMap.interactable = true;
                Tex_WorldMap_Lock.gameObject.SetActive(false);
            }
            else
            {
                img.sprite = GameManager.Instance.GetSprite(Sprite_path + "WorldMap_Lock");
                img.color = new Color(0.5f, 0.5f, 0.5f, 1);
                Btn_WorldMap.interactable = false;
                Tex_WorldMap_Lock.gameObject.SetActive(true);
            }
        }
        // ����ʯ
        {
            Button[] jewelArray = new Button[3] { Btn_Jewel1, Btn_Jewel2, Btn_Jewel3 };
            Image[] imgArray = new Image[3] { Img_Jewel1, Img_Jewel2, Img_Jewel3 };
            for (int i = 0; i < jewelArray.Length; i++)
            {
                Button btn = jewelArray[i];
                GameObject go_lock = btn.transform.Find("Lock").gameObject;
                Text tex_condition = go_lock.transform.Find("Text").GetComponent<Text>();
                string id = "Jewel" + (i+1);
                if(isDeveloperMode || (OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
                {
                    btn.interactable = true;
                    go_lock.SetActive(false);
                }
                else
                {
                    btn.interactable = false;
                    imgArray[i].sprite = GameManager.Instance.GetSprite("Jewel/locked");
                    go_lock.SetActive(true);
                    tex_condition.text = OtherUnlockManager.GetUnlockLevel(id)+"������";
                }
            }
        }
        // ��ʿ��ս
        {
            Image img = Btn_WarriorChallenge.GetComponent<Image>();
            Text tex = Btn_WarriorChallenge.transform.Find("Text").GetComponent<Text>();
            string id = "WarriorChallenge";
            if(isDeveloperMode || (OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
            {
                Btn_WarriorChallenge.interactable = true;
                img.color = new Color(1, 1, 1, 1);
                tex.gameObject.SetActive(false);
            }
            else
            {
                Btn_WarriorChallenge.interactable = false;
                img.color = new Color(0.5f, 0.5f, 0.5f, 1);
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "������";
            }
        }
        // ֧��
        {
            Image img = Btn_Spurline.GetComponent<Image>();
            Text tex = Btn_Spurline.transform.Find("Text").GetComponent<Text>();
            string id = "Spurline";
            if (isDeveloperMode || (OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
            {
                Btn_Spurline.interactable = true;
                img.color = new Color(1, 1, 1, 1);
                tex.gameObject.SetActive(false);
            }
            else
            {
                Btn_Spurline.interactable = false;
                img.color = new Color(0.5f, 0.5f, 0.5f, 1);
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "������";
            }
        }
        // ħ��
        {
            Image img = Btn_MagicTower.GetComponent<Image>();
            Text tex = Btn_MagicTower.transform.Find("Text").GetComponent<Text>();
            string id = "MagicTower";
            if (isDeveloperMode || (OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
            {
                Btn_MagicTower.interactable = true;
                img.color = new Color(1, 1, 1, 1);
                tex.gameObject.SetActive(false);
            }
            else
            {
                Btn_MagicTower.interactable = false;
                img.color = new Color(0.5f, 0.5f, 0.5f, 1);
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "������";
            }
        }
        // ��˵��ս
        {
            Image img = Btn_LegendChallenge.GetComponent<Image>();
            Text tex = Btn_LegendChallenge.transform.Find("Text").GetComponent<Text>();
            string id = "LegendChallenge";
            if (isDeveloperMode || (OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
            {
                Btn_LegendChallenge.interactable = true;
                img.color = new Color(1, 1, 1, 1);
                tex.gameObject.SetActive(false);
            }
            else
            {
                Btn_LegendChallenge.interactable = false;
                img.color = new Color(0.5f, 0.5f, 0.5f, 1);
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "������";
            }
        }
        // ��ͨ��ս
        {
            Image img = Btn_SpeedRun.GetComponent<Image>();
            Text tex = Btn_SpeedRun.transform.Find("Text").GetComponent<Text>();
            string id = "SpeedRun";
            if (isDeveloperMode || (OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
            {
                Btn_SpeedRun.interactable = true;
                img.color = new Color(1, 1, 1, 1);
                tex.gameObject.SetActive(false);
            }
            else
            {
                Btn_SpeedRun.interactable = false;
                img.color = new Color(0.5f, 0.5f, 0.5f, 1);
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "������";
            }
        }

        // ��������
        {
            Image img = Btn_Unused.GetComponent<Image>();
            Text tex = Btn_Unused.transform.Find("Text").GetComponent<Text>();
            string id = "Unused";
            if (isDeveloperMode || (OtherUnlockManager.IsUnlock(id) || OtherUnlockManager.TryUnlock(id)))
            {
                Btn_Unused.interactable = true;
                img.color = new Color(1, 1, 1, 1);
                tex.gameObject.SetActive(false);
            }
            else
            {
                Btn_Unused.interactable = false;
                img.color = new Color(0.5f, 0.5f, 0.5f, 1);
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "������";
            }
        }
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
        UpdatePlayerInfo();
        // ��װ
        UpdateSelectedCharacterDisplay();
        // ����
        PlayerData data = PlayerData.GetInstance();
        Img_MainWeapons.sprite = GameManager.Instance.GetSprite(Weapons_path + data.GetWeapons() + "/icon");
        UpdateSelectedWeaponsDisplay(data.GetWeapons());
        // ��ʯ
        UpdateSelectedJewelDisplay();
    }

    /// <summary>
    /// �������������Ϣ
    /// </summary>
    public void UpdatePlayerInfo()
    {
        PlayerData data = PlayerData.GetInstance();
        Tex_PlayerName.text = data.name;
        Tex_Level.text = data.GetLevel().ToString();
        // ͳ������ʱ��
        {
            float real_time = data.playTime;
            int hour = Mathf.FloorToInt(real_time / 3600);
            real_time = real_time % 3600;
            int min = Mathf.FloorToInt(real_time / 60);
            real_time = real_time % 60;
            int sec = Mathf.FloorToInt(real_time);
            string hh = (hour >= 10 ? hour.ToString() : "0" + hour.ToString());
            string mm = (min >= 10 ? min.ToString() : "0" + min.ToString());
            string ss = (sec >= 10 ? sec.ToString() : "0" + sec.ToString());
            Tex_PlayTime.text = hh + " : " + mm + " : " + ss;
        }
        
        if (data.IsMaxLevel())
        {
            Tex_ExpValue.text = data.currentExp.ToString("#0") + "/--";
            RectTrans_Expbar1.localScale = new Vector2(1, 1);
        }
        else
        {
            Tex_ExpValue.text = data.currentExp.ToString("#0") + "/" + data.GetNextLevelExp();
            RectTrans_Expbar1.localScale = new Vector2(Mathf.Min(1, Mathf.Max(0, data.currentExp/ data.GetNextLevelExp())), 1);
        }
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
        List<WeaponsNameTypeMap> list = WeaponsManager.GetWeaponsList();
        foreach (var item in list)
        {
            WeaponsDisplay w = WeaponsDisplay.GetInstance();
            w.Initial();
            w.SetValues(this, (int)item, 0);
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
        List<JewelNameTypeMap> list = JewelManager.GetJewelList();
        foreach (var type in list)
        {
            JewelDisplay j = JewelDisplay.GetInstance();
            j.Initial();
            j.SetValues(this, (int)type);
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
        Img_MainWeapons.sprite = GameManager.Instance.GetSprite(Weapons_path + type + "/icon");

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
        for (int i = 0; i < 3; i++)
        {
            if(data.GetJewel(i) == type)
            {
                data.SetJewel(i, -1);
                flag = true;
                break;
            }
        }
        if (!flag)
            data.SetJewel(currentSelecteJewelIndex, type);
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
        for (int i = 0; i < 3; i++)
        {
            if (data.GetJewel(i) > -1)
            {
                jewelDisplayList[data.GetJewel(i)].SetSelected();
            }
        }

        // �������Ͻ���ұ�ʯUI����
        bool isDeveloperMode = ConfigManager.IsDeveloperMode();
        Image[] imgList = new Image[] { Img_Jewel1, Img_Jewel2, Img_Jewel3 };
        for (int i = 0; i < imgList.Length; i++)
        {
            Image img = imgList[i];
            string id = "Jewel" + (i+1);
            if (data.GetJewel(i) == -1)
            {
                if (isDeveloperMode || OtherUnlockManager.IsUnlock(id))
                {
                    img.sprite = null;
                    img.color = new Color(1, 1, 1, 0.01f);
                }
                else
                {
                    img.sprite = GameManager.Instance.GetSprite("Jewel/locked");
                }
            }
            else
            {
                img.sprite = GameManager.Instance.GetSprite(Jewel_path + data.GetJewel(i));
                img.color = new Color(1, 1, 1, 1);
            }
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
        BigChapterPanel.BigChapterPanel panel = mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel] as BigChapterPanel.BigChapterPanel;
        panel.EnterPanel();
        panel.SetBigChapter("Mainline");
    }

    /// <summary>
    /// ����ʿ�ؿ���ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickWarriorChallenge()
    {
        BigChapterPanel.BigChapterPanel panel = mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel] as BigChapterPanel.BigChapterPanel;
        panel.EnterPanel();
        panel.SetBigChapter("WarriorChallenge");
    }

    /// <summary>
    /// ��֧�߹ؿ���ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickSpurline()
    {
        BigChapterPanel.BigChapterPanel panel = mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel] as BigChapterPanel.BigChapterPanel;
        panel.EnterPanel();
        panel.SetBigChapter("Spurline");
    }

    /// <summary>
    /// ��ħ����ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickMagicTower()
    {
        BigChapterPanel.BigChapterPanel panel = mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel] as BigChapterPanel.BigChapterPanel;
        panel.EnterPanel();
        panel.SetBigChapter("MagicTower");
    }

    /// <summary>
    /// ����˵��ս��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickLegendChallenge()
    {
        BigChapterPanel.BigChapterPanel panel = mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel] as BigChapterPanel.BigChapterPanel;
        panel.EnterPanel();
        panel.SetBigChapter("LegendChallenge");
    }


    /// <summary>
    /// ��ʵ���Ұ�ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickLocalStage()
    {
        LocalStagePanel.LocalStagePanel panel = mUIFacade.currentScenePanelDict[StringManager.LocalStagePanel] as LocalStagePanel.LocalStagePanel;
        panel.EnterPanel();
    }

    /// <summary>
    /// ����ͨ��ս��ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickSpeedRun()
    {
        BigChapterPanel.BigChapterPanel panel = mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel] as BigChapterPanel.BigChapterPanel;
        panel.EnterPanel();
        panel.SetBigChapter("SpeedRun");
    }

    /// <summary>
    /// ������������ť�����ʱ���ɱ༭����ֵ����ť��
    /// </summary>
    public void OnClickUnused()
    {
        BigChapterPanel.BigChapterPanel panel = mUIFacade.currentScenePanelDict[StringManager.BigChapterPanel] as BigChapterPanel.BigChapterPanel;
        panel.EnterPanel();
        panel.SetBigChapter("Unused");
    }

    /// <summary>
    /// �������ͼ�����ʱ
    /// </summary>
    public void OnClickWorldMap()
    {
        GameManager.Instance.EnterSelectScene();
    }

    /// <summary>
    /// ���������UI��ť�����ʱ
    /// </summary>
    public void OnClickHidePlayerUI()
    {
        if (isHidePlayerUI)
            ShowPlayerUI();
        else
            HidePlayerUI();
    }

    private void HidePlayerUI()
    {
        if (!isHidePlayerUI)
        {
            isHidePlayerUI = true;
            RectTrans_PlayerUI.anchoredPosition = new Vector2(-300, 0);
            Btn_HidePlayerUI.transform.localScale = new Vector2(-1, 1);
        }
    }

    private void ShowPlayerUI()
    {
        if (isHidePlayerUI)
        {
            isHidePlayerUI = false;
            RectTrans_PlayerUI.anchoredPosition = new Vector2(0, 0);
            Btn_HidePlayerUI.transform.localScale = new Vector2(1, 1);
        }
    }

    /// <summary>
    /// ���������ͼ�걻��껬������ʱ����ʾ������Ϣ
    /// </summary>
    public void OnPointerEnterIcon(RectTransform rect, string text)
    {
        TextArea.Instance.SetText(text);
        TextArea.Instance.SetLocalPosition(rect.transform, new Vector2(rect.sizeDelta.x / 2, 0), new Vector2(1, -1));
    }

    /// <summary>
    /// ���������ͼ�걻��껬���Ƴ�ʱ��ȡ��������Ϣ�����ⲿ��ӣ�
    /// </summary>
    public void OnPointerExitIcon()
    {
        TextArea.ExecuteRecycle();
    }

    /// <summary>
    /// �������˵���ť����������ⲿ��ӣ�
    /// </summary>
    public void OnClickReturnToMain()
    {
        GameManager.Instance.EnterMainScene();
    }
}
