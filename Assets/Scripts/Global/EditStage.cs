using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace GlobalData
{
    /// <summary>
    /// 当前正在被编辑的关卡
    /// </summary>
    public class EditStage
    {
        private bool isEditMode;
        private BaseStage.StageInfo stageInfo; // 当前正在编辑的关卡
        private BaseRound.RoundInfo copy_roundInfo; // 当前复制的轮

        private bool isUseCustStageList = false; // 是否启用的是本地关卡表的编辑
        private List<BaseStage.StageInfo> custStageList = new List<BaseStage.StageInfo>(); // 本地的自定义关卡集合
        private int selectedCustStageIndex;

        public void SetUseCustStageListOpen(bool isOpen)
        {
            isUseCustStageList = isOpen;
        }

        public bool IsUseCustStageList()
        {
            return isUseCustStageList;
        }


        public void OpenEditMode(BaseStage.StageInfo stageInfo)
        {
            isEditMode = true;
            this.stageInfo = stageInfo;
        }

        public void CloseEditMode()
        {
            isEditMode = false;
        }

        public bool CanEdit()
        {
            return isEditMode;
        }

        public BaseStage.StageInfo GetStageInfo() 
        {
            return stageInfo;
        }

        /// <summary>
        /// 复制某一轮（不复制引用，而是深度拷贝）
        /// </summary>
        public void CopyRoundInfo(BaseRound.RoundInfo info)
        {
            copy_roundInfo = info.DeepCopy();
        }

        /// <summary>
        /// 获取被复制的轮
        /// </summary>
        /// <returns></returns>
        public BaseRound.RoundInfo GetCopiedRoundInfo()
        {
            return copy_roundInfo;
        }

        public BaseStage.StageInfo CreateEmptyStage(string name, int chapterIndex, int sceneIndex, int stageIndex)
        {
            return new BaseStage.StageInfo()
            {
                fileName = name,
                name = name,
                chapterIndex = chapterIndex,
                sceneIndex = sceneIndex,
                stageIndex = stageIndex,
                apartList = new List<List<int>>() { new List<int>() { 0, 1, 2, 3, 4, 5, 6 } },
                roundInfoList = new List<BaseRound.RoundInfo>(),
                availableCardInfoList = new List<AvailableCardInfo>()
            };
        }

        /// <summary>
        /// （重新）加载一次本地的关卡集合
        /// </summary>
        public void LoadCustStageList()
        {
            custStageList.Clear();

            string local_folder_path = Application.persistentDataPath + "/CustomStage";
            // 本地存放关卡的文件夹不存在则新建一个
            if (!Directory.Exists(local_folder_path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(local_folder_path);
                directoryInfo.Create();
            }

            // 遍历这个文件夹，找到所有存放的关卡
            {
                DirectoryInfo folder = new DirectoryInfo(local_folder_path);
                FileInfo[] fileInfos = folder.GetFiles("*.json", SearchOption.TopDirectoryOnly);

                if (fileInfos.Length == 0)
                {
                    // 如果一个关都没有的话
                    AddNewCustStage();
                }
                else
                {
                    // 遍历它们，并将它们记录下来
                    foreach (var f in fileInfos)
                    {
                        BaseStage.StageInfo info;
                        string fileName = f.Name.Replace(f.Extension, "");
                        if (JsonManager.TryLoadFromLocal("CustomStage/" + fileName, out info))
                        {
                            //if (info.fileName == null || info.fileName.Equals(""))
                            //    info.fileName = info.name;
                            info.fileName = fileName;
                            custStageList.Add(info);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 添加一个新的自定义关卡
        /// </summary>
        public void AddNewCustStage()
        {
            //BaseStage.StageInfo info = CreateEmptyStage(System.DateTime.Now.ToString("F") + "新建关卡", 0, 0, 0);
            BaseStage.StageInfo info = CreateEmptyStage("NewStage"+custStageList.Count, 0, 0, 0);
            custStageList.Add(info);
            SaveCustStage(info);
        }

        /// <summary>
        /// 把自定义关卡保存在本地存档
        /// </summary>
        /// <param name="info"></param>
        public void SaveCustStage(BaseStage.StageInfo info)
        {
            JsonManager.SaveOnLocal(info, "CustomStage/" + info.fileName);
        }

        /// <summary>
        /// 从本地存档中移除自定义关卡
        /// </summary>
        /// <param name="info"></param>
        public void DeleteCustStage(BaseStage.StageInfo info)
        {
            custStageList.Remove(info);
            selectedCustStageIndex = Mathf.Max(0, Mathf.Min(custStageList.Count - 1, selectedCustStageIndex));
            JsonManager.DeleteFromLocal("CustomStage/" + info.fileName);
        }

        public void SetSelectedCustStageIndex(int index)
        {
            index = Mathf.Max(0, Mathf.Min(custStageList.Count - 1, index));
            selectedCustStageIndex = index;
        }

        public int GetSelectedCustStageIndex()
        {
            return selectedCustStageIndex;
        }

        /// <summary>
        /// 获取来自本地的自定义关卡
        /// </summary>
        /// <returns></returns>
        public List<BaseStage.StageInfo> GetCustStageList()
        {
            return custStageList;
        }
    }
}

