using System.Collections.Generic;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/// <summary>
/// 玩家信息操作面板
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

    private Button Btn_WorldMap;
    private Text Tex_WorldMap_Position;
    private Text Tex_WorldMap_Lock;

    private Transform Trans_ModeSelectList;
    private Button Btn_WarriorChallenge;
    private Button Btn_Spurline;

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

    private bool isHidePlayerUI = true; // 是否隐藏玩家UI

    protected override void Awake()
    {
        base.Awake();
        RectTrans_PlayerUI = transform.Find("PlayerUI").GetComponent<RectTransform>();
        RectTrans_Expbar1 = RectTrans_PlayerUI.Find("Img_Expbar1").GetComponent<RectTransform>();
        Tex_ExpValue = RectTrans_PlayerUI.Find("Tex_Exp").Find("Text").GetComponent<Text>();
        Tex_Level = RectTrans_PlayerUI.Find("Tex_Level").GetComponent<Text>();
        Tex_PlayerName = RectTrans_PlayerUI.Find("Tex_PlayerName").GetComponent<Text>();
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

        SelectWeaponsUI = transform.Find("SelectWeaponsUI").gameObject;
        Tran_WeaponsDisplay = SelectWeaponsUI.transform.Find("Img_center").Find("Emp_Weapons").Find("Scr").Find("Viewport").Find("Content");

        SelectJewelUI = transform.Find("SelectJewelUI").gameObject;
        Tran_JewelDisplay = SelectJewelUI.transform.Find("Img_center").Find("Emp_Jewel").Find("Scr").Find("Viewport").Find("Content");

        SelectSuitUI = transform.Find("SelectSuitUI").gameObject;
        Tran_CharacterDisplay = SelectSuitUI.transform.Find("Img_center").Find("Emp_Suit").Find("Scr").Find("Viewport").Find("Content");

        AddEventTriggerOnAwake();
    }

    /// <summary>
    /// 添加鼠标移动到部分元素上会浮动文字的效果
    /// </summary>
    private void AddEventTriggerOnAwake()
    {
        // 宝石
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
        // 武器
        {
            EventTrigger trigger = EventTrigger_Weapons;
            EventTrigger.TriggerEvent tr1 = new EventTrigger.TriggerEvent();
            tr1.AddListener(delegate {
                int weapons = PlayerData.GetInstance().GetWeapons();
                if (weapons > -1)
                    OnPointerEnterIcon(trigger.GetComponent<RectTransform>(), WeaponsManager.GetName(weapons) + "\n(点击图标可以更换武器)" + "\n攻击间隔：" + WeaponsManager.GetInterval(weapons) + "\n" + WeaponsManager.GetInfo(weapons));
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
        // 人物头像
        {
            EventTrigger trigger = EventTrigger_PlayerImage;
            EventTrigger.TriggerEvent tr1 = new EventTrigger.TriggerEvent();
            tr1.AddListener(delegate {
                int weapons = PlayerData.GetInstance().GetWeapons();
                if (weapons > -1)
                    OnPointerEnterIcon(trigger.GetComponent<RectTransform>(), "点击切换玩家角色外观");
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
        // 初始显示玩家UI面板
        ShowPlayerUI();
        // 需要先屏蔽的UI
        SelectWeaponsUI.SetActive(false);
        SelectJewelUI.SetActive(false);
        SelectSuitUI.SetActive(false);

        TryUnLockUI();
    }

    /// <summary>
    /// 检测、并尝试解锁该场景的部分UI，同时更新它们的显示
    /// </summary>
    private void TryUnLockUI()
    {
        bool isDeveloperMode = ConfigManager.IsDeveloperMode();

        // 世界地图
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
        // 三宝石
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
                    tex_condition.text = OtherUnlockManager.GetUnlockLevel(id)+"级解锁";
                }
            }
        }
        // 勇士挑战
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
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "级解锁";
            }
        }
        // 支线
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
                tex.text = OtherUnlockManager.GetUnlockLevel(id) + "级解锁";
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
        // 读取存档信息并更新UI
        UpdatePlayerInfo();
        // 套装
        UpdateSelectedCharacterDisplay();
        // 武器
        PlayerData data = PlayerData.GetInstance();
        Img_MainWeapons.sprite = GameManager.Instance.GetSprite(Weapons_path + data.GetWeapons() + "/icon");
        UpdateSelectedWeaponsDisplay(data.GetWeapons());
        // 宝石
        UpdateSelectedJewelDisplay();
    }

    /// <summary>
    /// 更新玩家面板的信息
    /// </summary>
    public void UpdatePlayerInfo()
    {
        PlayerData data = PlayerData.GetInstance();
        Tex_PlayerName.text = data.name;
        Tex_Level.text = data.GetLevel().ToString();
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
    /// 清空武器表
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
    /// 填充武器表
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
    /// 清空宝石表
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
    /// 填充宝石表
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
    /// 清空角色表
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
    /// 填充角色表
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
    /// 当情报岛的按钮被点击后（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickIntelligenceIsland()
    {
        mUIFacade.currentScenePanelDict[StringManager.EncyclopediaPanel].EnterPanel();
    }

    /// <summary>
    /// 当某个武器图标被选中时
    /// </summary>
    public void OnClickWeaponsDisplay(int type)
    {
        // 更新面板显示内容
        UpdateSelectedWeaponsDisplay(type);
        // 更新左上角玩家UI内容
        Img_MainWeapons.sprite = GameManager.Instance.GetSprite(Weapons_path + type + "/icon");

        // 更新存档内容
        PlayerData data = PlayerData.GetInstance();
        data.SetWeapons(type);
        // 保存
        data.Save();
    }

    /// <summary>
    /// 更新武器面板选中显示
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
    /// 当武器选择按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickSelectWeaponsUI()
    {
        SelectWeaponsUI.SetActive(true);
    }

    /// <summary>
    /// 当关闭武器选择按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickExitSelectWeaponsUI()
    {
        SelectWeaponsUI.SetActive(false);
    }

    /// <summary>
    /// 当某个宝石图标被选中时
    /// </summary>
    public void OnClickJewelDisplay(int type)
    {
        // 重复检测，如果本身宝石已被选中则取消
        // 更新存档内容
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
        // 更新面板显示内容
        UpdateSelectedJewelDisplay();
    }

    /// <summary>
    /// 更新宝石面板选中显示
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

        // 更新左上角玩家宝石UI内容
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
    /// 当宝石选择按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickSelectJewelUI(int selecteIndex)
    {
        SelectJewelUI.SetActive(true);
        currentSelecteJewelIndex = selecteIndex;
    }

    /// <summary>
    /// 当关闭宝石选择按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickExitSelectJewelUI()
    {
        SelectJewelUI.SetActive(false);
    }


    /// <summary>
    /// 当某个套装图标被选中时
    /// </summary>
    public void OnClickCharacterDisplay(int type)
    {
        // 更新存档内容
        PlayerData data = PlayerData.GetInstance();
        data.suit = type;
        // 保存
        data.Save();
        // 更新面板显示内容
        UpdateSelectedCharacterDisplay();
    }

    /// <summary>
    /// 更新套装面板选中显示
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
        // 更新左上角面板
        Img_PlayerImage.sprite = GameManager.Instance.GetSprite(Suit_path + data.suit + "/icon");
    }

    /// <summary>
    /// 当套装选择按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickSelectSuitUI()
    {
        SelectSuitUI.SetActive(true);
    }

    /// <summary>
    /// 当关闭套装选择按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickExitSelectSuitUI()
    {
        SelectSuitUI.SetActive(false);
    }

    /// <summary>
    /// 当主线关卡按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickMainline()
    {
        mUIFacade.currentScenePanelDict[StringManager.MainlinePanel].EnterPanel();
    }

    /// <summary>
    /// 当勇士关卡按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickWarriorChallenge()
    {
        mUIFacade.currentScenePanelDict[StringManager.WarriorChallengePanel].EnterPanel();
    }

    /// <summary>
    /// 当支线关卡按钮被点击时（由编辑器赋值给按钮）
    /// </summary>
    public void OnClickSpurline()
    {
        mUIFacade.currentScenePanelDict[StringManager.SpurlinePanel].EnterPanel();
    }

    /// <summary>
    /// 当世界地图被点击时
    /// </summary>
    public void OnClickWorldMap()
    {
        GameManager.Instance.EnterSelectScene();
    }

    /// <summary>
    /// 当隐藏玩家UI按钮被点击时
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
    /// 当自身界面图标被鼠标滑动经过时，显示文字信息
    /// </summary>
    public void OnPointerEnterIcon(RectTransform rect, string text)
    {
        TextArea.Instance.SetText(text);
        TextArea.Instance.SetLocalPosition(rect.transform, new Vector2(rect.sizeDelta.x / 2, 0), new Vector2(1, -1));
    }

    /// <summary>
    /// 当自身界面图标被鼠标滑动移出时，取消文字信息（由外部添加）
    /// </summary>
    public void OnPointerExitIcon()
    {
        TextArea.ExecuteRecycle();
    }

    /// <summary>
    /// 返回主菜单按钮被点击（由外部添加）
    /// </summary>
    public void OnClickReturnToMain()
    {
        GameManager.Instance.EnterMainScene();
    }
}
