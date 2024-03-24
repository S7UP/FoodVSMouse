using System.Collections.Generic;
using System.IO;

using UnityEngine;

namespace GlobalData
{
    /// <summary>
    /// ��ǰ���ڱ��༭�Ĺؿ�
    /// </summary>
    public class EditStage
    {
        private bool isEditMode;
        private BaseStage.StageInfo stageInfo; // ��ǰ���ڱ༭�Ĺؿ�
        private BaseRound.RoundInfo copy_roundInfo; // ��ǰ���Ƶ���

        private bool isUseCustStageList = false; // �Ƿ����õ��Ǳ��عؿ���ı༭
        private List<BaseStage.StageInfo> custStageList = new List<BaseStage.StageInfo>(); // ���ص��Զ���ؿ�����
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
        /// ����ĳһ�֣����������ã�������ȿ�����
        /// </summary>
        public void CopyRoundInfo(BaseRound.RoundInfo info)
        {
            copy_roundInfo = info.DeepCopy();
        }

        /// <summary>
        /// ��ȡ�����Ƶ���
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
        /// �����£�����һ�α��صĹؿ�����
        /// </summary>
        public void LoadCustStageList()
        {
            custStageList.Clear();

            string local_folder_path = Application.persistentDataPath + "/CustomStage";
            // ���ش�Źؿ����ļ��в��������½�һ��
            if (!Directory.Exists(local_folder_path))
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(local_folder_path);
                directoryInfo.Create();
            }

            // ��������ļ��У��ҵ����д�ŵĹؿ�
            {
                DirectoryInfo folder = new DirectoryInfo(local_folder_path);
                FileInfo[] fileInfos = folder.GetFiles("*.json", SearchOption.TopDirectoryOnly);

                if (fileInfos.Length == 0)
                {
                    // ���һ���ض�û�еĻ�
                    AddNewCustStage();
                }
                else
                {
                    // �������ǣ��������Ǽ�¼����
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
        /// ���һ���µ��Զ���ؿ�
        /// </summary>
        public void AddNewCustStage()
        {
            //BaseStage.StageInfo info = CreateEmptyStage(System.DateTime.Now.ToString("F") + "�½��ؿ�", 0, 0, 0);
            BaseStage.StageInfo info = CreateEmptyStage("NewStage"+custStageList.Count, 0, 0, 0);
            custStageList.Add(info);
            SaveCustStage(info);
        }

        /// <summary>
        /// ���Զ���ؿ������ڱ��ش浵
        /// </summary>
        /// <param name="info"></param>
        public void SaveCustStage(BaseStage.StageInfo info)
        {
            JsonManager.SaveOnLocal(info, "CustomStage/" + info.fileName);
        }

        /// <summary>
        /// �ӱ��ش浵���Ƴ��Զ���ؿ�
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
        /// ��ȡ���Ա��ص��Զ���ؿ�
        /// </summary>
        /// <returns></returns>
        public List<BaseStage.StageInfo> GetCustStageList()
        {
            return custStageList;
        }
    }
}

