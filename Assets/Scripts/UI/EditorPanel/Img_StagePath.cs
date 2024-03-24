
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 编辑面板上的关卡路径面板
/// </summary>
namespace EditorPanel
{
    public class Img_StagePath : MonoBehaviour
    {
        private EditorPanel mEditorPanel;

        private Dropdown Dro_StageType;

        private GameObject Emp_ChapterArea;
        private Transform Trans_Stage;
        private Dropdown Dro_Chapter;
        private Dropdown Dro_Scene;
        private Dropdown Dro_Stage;

        private bool isUpdate;

        private int chapterIndex;
        private int sceneIndex;
        private int stageIndex;

        private bool isLoadCust; // 是否已经加载过了本地自定义关卡

        private void Awake()
        {
            Dro_StageType = transform.Find("Emp_StageType").Find("Dropdown").GetComponent<Dropdown>();
            Emp_ChapterArea = transform.Find("Emp_ChapterArea").gameObject;
            Dro_Chapter = Emp_ChapterArea.transform.Find("Emp_Chapter").Find("Dropdown").GetComponent<Dropdown>();
            Trans_Stage = Emp_ChapterArea.transform.Find("Emp_Scene");
            Dro_Scene = Trans_Stage.Find("Dropdown").GetComponent<Dropdown>();
            Dro_Stage = Emp_ChapterArea.transform.Find("Emp_Stage").Find("Dropdown").GetComponent<Dropdown>();

            Dro_StageType.onValueChanged.AddListener(delegate {
                GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;
                if (Dro_StageType.value == 0)
                {
                    edit.SetUseCustStageListOpen(false);
                } else if(Dro_StageType.value == 1)
                {
                    edit.SetUseCustStageListOpen(true);
                    edit.SetSelectedCustStageIndex(0); // 默认为第一个关卡
                }
                UpdateSelection();
            });

            // 当章节选项发生改变时
            Dro_Chapter.onValueChanged.AddListener(delegate {
                if (isUpdate)
                    return;
                GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;
                if (!edit.IsUseCustStageList())
                {
                    chapterIndex = Dro_Chapter.value;
                    sceneIndex = 0;
                    stageIndex = 0;
                }
                else
                {
                    BaseStage.StageInfo info = edit.GetStageInfo();
                    info.chapterIndex = Dro_Chapter.value;
                    info.sceneIndex = 0;
                }
                UpdateSelection();
            });
            // 当场景选项发生改变时
            Dro_Scene.onValueChanged.AddListener(delegate {
                if (isUpdate)
                    return;
                GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;
                if (!edit.IsUseCustStageList())
                {
                    sceneIndex = Dro_Scene.value;
                    stageIndex = 0;
                }
                else
                {
                    BaseStage.StageInfo info = edit.GetStageInfo();
                    info.sceneIndex = Dro_Scene.value;
                }
                UpdateSelection();
            });
            
            Dro_Stage.onValueChanged.AddListener(delegate {
                if (isUpdate)
                    return;
                GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;
                if (!edit.IsUseCustStageList())
                {
                    if (Dro_Stage.value > 0)
                    {
                        // 加载对应关卡
                        stageIndex = Dro_Stage.value - 1;
                    }
                    else
                    {
                        // 添加新关卡
                        stageIndex = GameManager.Instance.attributeManager.GetStageInfoListFromScene(chapterIndex, sceneIndex).Count;
                        BaseStage.Save(GlobalData.Manager.Instance.mEditStage.CreateEmptyStage("新建关卡" + stageIndex, chapterIndex, sceneIndex, stageIndex));
                        // 强制从本地读取一遍这个关卡到内存中
                        GameManager.Instance.attributeManager.GetStageInfo(chapterIndex, sceneIndex, stageIndex);
                    }
                }
                else
                {
                    if (Dro_Stage.value > 0)
                    {
                        // 加载对应关卡
                        edit.SetSelectedCustStageIndex(Dro_Stage.value - 1);
                    }
                    else
                    {
                        // 新建一个新关卡，并加载它
                        edit.AddNewCustStage();
                        edit.SetSelectedCustStageIndex(edit.GetCustStageList().Count - 1);
                    }
                }
                UpdateSelection();
            });
        }


        public void Initial()
        {
            Dro_StageType.ClearOptions();
            {
                List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
                dataList.Add(new Dropdown.OptionData("默认"));
                dataList.Add(new Dropdown.OptionData("本地"));
                Dro_StageType.AddOptions(dataList);
                Dro_StageType.value = 1;
            }

            chapterIndex = 0;
            sceneIndex = 0;
            stageIndex = 0;

            // 章节是固定的
            Dro_Chapter.ClearOptions();
            {
                List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
                foreach (var item in ChapterManager.GetChapterNameList())
                    dataList.Add(new Dropdown.OptionData(item));
                Dro_Chapter.AddOptions(dataList);
                Dro_Chapter.value = chapterIndex;
            }

            UpdateSelection();
        }

        public void UpdateSelection()
        {
            isUpdate = true;

            GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;


            if (!edit.IsUseCustStageList())
            {
                UpdateSceneDropDown();
                UpdateStageDropDown();
                LoadStage();
                mEditorPanel.ChangeBackground(chapterIndex, sceneIndex);
            }
            else
            {
                LoadStage();
                BaseStage.StageInfo info = edit.GetStageInfo();
                Dro_Chapter.value = info.chapterIndex;
                UpdateSceneDropDown();
                UpdateStageDropDown();
                mEditorPanel.ChangeBackground(info.chapterIndex, info.sceneIndex);
            }
            
            isUpdate = false;
        }

        /// <summary>
        /// 设置宿主Panel，由宿主调用
        /// </summary>
        public void SetEditorPanel(EditorPanel panel)
        {
            mEditorPanel = panel;
        }



        ////////////////////////////////////////////////以下为私有方法//////////////////////////////////////////////////////

        /// <summary>
        /// 更新场景选择下拉列表项
        /// </summary>
        private void UpdateSceneDropDown()
        {
            GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;

            Dro_Scene.ClearOptions();
            List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();

            if (!edit.IsUseCustStageList())
            {
                foreach (var item in ChapterManager.GetSceneNameList((ChapterNameTypeMap)chapterIndex))
                    dataList.Add(new Dropdown.OptionData(item));
                Dro_Scene.AddOptions(dataList);
                Dro_Scene.value = sceneIndex;
            }
            else
            {
                BaseStage.StageInfo info = edit.GetStageInfo();
                foreach (var item in ChapterManager.GetSceneNameList((ChapterNameTypeMap)info.chapterIndex))
                    dataList.Add(new Dropdown.OptionData(item));
                Dro_Scene.AddOptions(dataList);
                Dro_Scene.value = info.sceneIndex;
            }
        }

        /// <summary>
        /// 更新关卡下拉列表项
        /// </summary>
        private void UpdateStageDropDown()
        {
            GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;

            Dro_Stage.ClearOptions();
            List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
            dataList.Add(new Dropdown.OptionData("添加新关卡"));
            if (!edit.IsUseCustStageList())
            {
                List<BaseStage.StageInfo> list = GameManager.Instance.attributeManager.GetStageInfoListFromScene(chapterIndex, sceneIndex);
                foreach (var item in list)
                    dataList.Add(new Dropdown.OptionData(item.name));
                Dro_Stage.AddOptions(dataList);
                stageIndex = Mathf.Min(list.Count - 1, stageIndex);
                Dro_Stage.value = stageIndex + 1;
            }
            else
            {
                // 如果之前没有读取过本地的关卡则需要读取一次
                if (!isLoadCust)
                {
                    isLoadCust = true;
                    edit.LoadCustStageList();
                }

                BaseStage.StageInfo info = edit.GetStageInfo();
                List<BaseStage.StageInfo> list = edit.GetCustStageList();
                foreach (var item in list)
                    dataList.Add(new Dropdown.OptionData(item.fileName));
                Dro_Stage.AddOptions(dataList);
                Dro_Stage.value = edit.GetSelectedCustStageIndex() + 1;
            }
        }

        /// <summary>
        /// 加载关卡相关内容
        /// </summary>
        private void LoadStage()
        {
            GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;

            if (!edit.IsUseCustStageList())
            {
                BaseStage.StageInfo info = GameManager.Instance.attributeManager.GetStageInfo(chapterIndex, sceneIndex, stageIndex);
                mEditorPanel.LoadStage(info);
            }else
            {
                // 如果之前没有读取过本地的关卡则需要读取一次
                if (!isLoadCust)
                {
                    isLoadCust = true;
                    edit.LoadCustStageList();
                }

                if(edit.GetCustStageList().Count > 0)
                {
                    // 把当前选中的本地关卡载入编辑
                    mEditorPanel.LoadStage(edit.GetCustStageList()[edit.GetSelectedCustStageIndex()]);
                }
                else
                {
                    Debug.LogError("警告！关卡文件夹里没有关卡！");
                }
            }

        }
    }
}

