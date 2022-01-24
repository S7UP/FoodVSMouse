//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI;
///// <summary>
///// 小关卡选择面板
///// </summary>
//public class GameNormalLevelPanel : BasePanel
//{
//    private string filePath;
//    public int currentBigLevelID;
//    public int currentLevelID;
//    private string theSpritePath;

//    private Transform levelContentTrans;
//    private GameObject img_LockBtnGo;
//    private Transform emp_TowerTrans;
//    private Image img_BGLeft;
//    private Image img_BGRight;
//    private Image img_Carrot;
//    private Image img_AllClear;
//    private Text tex_TotalWaves;

//    private PlayerManager playerManager;
//    private SlideScrollView slideScrollView;

//    private List<GameObject> levelContentImageGos; // 实例化出来的地图卡片UI
//    private List<GameObject> towerContentImageGos; // 实例化出来的建塔列表UI


//    protected override void Awake()
//    {
//        base.Awake();
//        filePath = "GameOption/Normal/Level/";
//        playerManager = mUIFacade.mPlayerManager;
//        levelContentImageGos = new List<GameObject>();
//        towerContentImageGos = new List<GameObject>();
//        levelContentTrans = transform.Find("Scroll View").Find("Viewport").Find("Content");
//        img_LockBtnGo = transform.Find("Img_LockBtn").gameObject;
//        emp_TowerTrans = transform.Find("Emp_Tower");
//        img_BGLeft = transform.Find("Img_BGLeft").GetComponent<Image>();
//        img_BGRight = transform.Find("Img_BGRight").GetComponent<Image>();
//        tex_TotalWaves = transform.Find("Img_TotalWaves").Find("Text").GetComponent<Text>();
//        slideScrollView = transform.Find("Scroll View").GetComponent<SlideScrollView>();
//        currentBigLevelID = 1;
//        currentLevelID = 1;
//    }

//    private void LoadResource()
//    {
//        mUIFacade.GetSprite(filePath+"AllClear");
//        mUIFacade.GetSprite(filePath + "Carrot_1");
//        mUIFacade.GetSprite(filePath + "Carrot_2");
//        mUIFacade.GetSprite(filePath + "Carrot_3");
//        for(int i = 0; i < 4; i++)
//        {
//            string spritePath = filePath + i.ToString() + "/";
//            mUIFacade.GetSprite(spritePath + "BG_Left");
//            mUIFacade.GetSprite(spritePath + "BG_Right");
//            for(int j = 1; j < 6; j++)
//            {
//                mUIFacade.GetSprite(spritePath + "Level_" + j.ToString());
//            }
//            for(int j = 1; j < 13; j++)
//            {
//                mUIFacade.GetSprite(filePath + "Tower/Tower_" + j.ToString());
//            }
//        }
//    }

//    // 更新地图UI的方法（动态UI）
//    public void UpdateMapUI(string spritePath)
//    {
//        img_BGLeft.sprite = mUIFacade.GetSprite(spritePath + "BG_Left");
//        img_BGRight.sprite = mUIFacade.GetSprite(spritePath + "BG_Right");
//        for(int i = 0; i < 5; i++)
//        {
//            levelContentImageGos.Add( CreateUIAndSetUIPosition("Img_Level", levelContentTrans));
//            // 更换关卡图片
//            levelContentImageGos[i].GetComponent<Image>().sprite = mUIFacade.GetSprite(spritePath+"Level_"+(i+1).ToString());
//            Stage stage = playerManager.unLockedNormalModelLevelList[(currentBigLevelID-1)*5+i];
//            levelContentImageGos[i].transform.Find("Img_Carrot").gameObject.SetActive(false);
//            levelContentImageGos[i].transform.Find("Img_AllClear").gameObject.SetActive(false);
//            if (stage.unLocked)
//            {
//                // 已解锁
//                if (stage.mAllClear)
//                {
//                    levelContentImageGos[i].transform.Find("Img_AllClear").gameObject.SetActive(true);
//                }
//                if (stage.mCarrotState != 0)
//                {
//                    Image carrotImageGo = levelContentImageGos[i].transform.Find("Img_Carrot").GetComponent<Image>();
//                    carrotImageGo.gameObject.SetActive(true);
//                    carrotImageGo.sprite = mUIFacade.GetSprite(filePath + "Carrot_"+stage.mCarrotState);
//                }
//                levelContentImageGos[i].transform.Find("Img_Lock").gameObject.SetActive(false);
//                levelContentImageGos[i].transform.Find("Img_BG").gameObject.SetActive(false);
//            }
//            else
//            {
//                // 未解锁
//                if (stage.mIsRewardLevel)
//                {
//                    // 奖励关卡
//                    levelContentImageGos[i].transform.Find("Img_Lock").gameObject.SetActive(false);
//                    levelContentImageGos[i].transform.Find("Img_BG").gameObject.SetActive(true);
//                    Image monsterPetImage = levelContentImageGos[i].transform.Find("Img_Monster").GetComponent<Image>();
//                    monsterPetImage.sprite = mUIFacade.GetSprite("MonsterNest/Monster/Baby/"+currentBigLevelID.ToString());
//                    monsterPetImage.SetNativeSize(); //更换图片后，自动设置成保持原有图片比例
//                    monsterPetImage.transform.localScale = new Vector3(2, 2, 2); // 2倍Scale
//                }
//                else
//                {
//                    // 不是奖励关卡
//                    levelContentImageGos[i].transform.Find("Img_Lock").gameObject.SetActive(true);
//                    levelContentImageGos[i].transform.Find("Img_BG").gameObject.SetActive(false);
//                }
//            }
//        }
//        // 设置滚动视图Content大小
//        slideScrollView.SetContentLength(5);
//    }

//    // 销毁地图卡
//    private void DestoryMapUI()
//    {
//        if (levelContentImageGos.Count > 0)
//        {
//            for(int i = 0; i < 5; i++)
//            {
//                mUIFacade.PushGameObjectToFactory(FactoryType.UIFactory, "Img_Level", levelContentImageGos[i]);
//            }
//            slideScrollView.InitScrollLength();
//            levelContentImageGos.Clear();
//        }
//    }

//    // 更新静态UI
//    public void UpdateLevelUI(string SpritePath)
//    {
//        if (towerContentImageGos.Count != 0)
//        {
//            for(int i = 0; i < towerContentImageGos.Count; i++)
//            {
//                towerContentImageGos[i].GetComponent<Image>().sprite = null;
//                mUIFacade.PushGameObjectToFactory(FactoryType.UIFactory, "Img_Tower", towerContentImageGos[i]);
//            }
//            towerContentImageGos.Clear();
//        }

//        Stage stage = playerManager.unLockedNormalModelLevelList[(currentBigLevelID - 1) * 5 + currentLevelID - 1];
//        if (stage.unLocked)
//        {
//            img_LockBtnGo.SetActive(false);
//        }
//        else
//        {
//            img_LockBtnGo.SetActive(true);
//        }
//        tex_TotalWaves.text = stage.mTotalRound.ToString();
//        for(int i = 0; i < stage.mTowerIDListLength; i++)
//        {
//            towerContentImageGos.Add(CreateUIAndSetUIPosition("Img_Tower", emp_TowerTrans));
//            towerContentImageGos[i].GetComponent<Image>().sprite = mUIFacade.GetSprite(filePath+"Tower"+"/Tower_"+stage.mTowerIDList[i].ToString());
            
//        }
//    }


//    /// <summary>
//    /// 处理UI面板的方法
//    /// </summary>
//    /// <param name="currentBigLevel"></param>


//    // 外部调用的进入当前页面的方法
//    public void ToThisPanel(int currentBigLevel)
//    {
//        currentBigLevelID = currentBigLevel;
//        currentLevelID = 1;
//        EnterPanel();
//    }


//    public override void InitPanel()
//    {
//        base.InitPanel();
//        gameObject.SetActive(false);
//    }

//    public override void EnterPanel()
//    {
//        base.EnterPanel();
//        gameObject.SetActive(true);
//        theSpritePath = filePath + currentBigLevelID.ToString() + "/";
//        DestoryMapUI();
//        UpdateMapUI(theSpritePath);
//        UpdateLevelUI(theSpritePath);
//        slideScrollView.Init();
//    }

//    public override void UpdatePanel()
//    {
//        base.UpdatePanel();
//        UpdateLevelUI(theSpritePath);
//    }

//    public override void ExitPanel()
//    {
//        base.ExitPanel();
//        gameObject.SetActive(false);
//    }

//    public void ToGamePanel()
//    {
//        GameManager.Instance.currentStage = playerManager.unLockedNormalModelLevelList[(currentBigLevelID - 1) * 5 + currentLevelID - 1];
//        mUIFacade.currentScenePanelDict[StringManager.GameLoadPanel].EnterPanel();
//        mUIFacade.ChangeSceneState(new NormalModelSceneState(mUIFacade));
//    }

//    /// <summary>
//    /// 帮助更新UI的方法
//    /// </summary>
//    /// <param name="uiName"></param>
//    /// <param name="parentTrans"></param>
//    /// <returns></returns>

//    // 实例化UI
//    public GameObject CreateUIAndSetUIPosition(string uiName, Transform parentTrans)
//    {
//        GameObject itemGo = mUIFacade.GetGameObjectResource(FactoryType.UIFactory, uiName);
//        itemGo.transform.SetParent(parentTrans);
//        itemGo.transform.localPosition = Vector3.zero;
//        itemGo.transform.localScale = Vector3.one;
//        return itemGo;
//    }

//    /// <summary>
//    /// 按钮实现翻页效果
//    /// </summary>
//    public void ToNextLevel()
//    {
//        currentLevelID++;
//        UpdatePanel();
//    }

//    public void ToLastLevel()
//    {
//        currentLevelID--;
//        UpdatePanel();
//    }


//}
