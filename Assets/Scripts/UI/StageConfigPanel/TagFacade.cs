using S7P.State;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UIPanel.StageConfigPanel
{
    public class TagFacade : IGameControllerMember
    {
        /// <summary>
        /// �����õĹؿ�
        /// </summary>
        private static List<string> lockedStageIdList = new List<string>() {
            "LC1-1", "LC1-2", "LC1-3", "LC1-4", "LC1-5", "LC1-6", "LC1-7", "LC1-8"
        };
        // model
        private Dictionary<string, Tag> tagBtnDict = new Dictionary<string, Tag>();
        private Dictionary<string, LimitItem> limitItemDict = new Dictionary<string, LimitItem>();
        // view
        private SelectTagUI mSelectTagUI;
        private SettlementUI mSettlementUI;

        public TagFacade(SelectTagUI mSelectTagUI, SettlementUI mSettlementUI)
        {
            this.mSelectTagUI = mSelectTagUI;
            this.mSettlementUI = mSettlementUI;
        }

        public void MInit()
        {
            tagBtnDict.Clear();
            limitItemDict.Clear();
            mSelectTagUI.MInit();
            mSettlementUI.MInit();

            PlayerData data = GameManager.Instance.playerData;
            BaseStage.StageInfo info = data.GetCurrentStageInfo();
            List<TagInfo[]> list = TagsManager.GetTagArrayList(info.chapterIndex, info.sceneIndex, info.stageIndex);
            // �������
            foreach (var arr in list)
            {
                TagArray array = TagArray.GetInstance();
                for (int i = 0; i < arr.Length; i++)
                {
                    TagInfo tagInfo = arr[i];
                    Tag tagBtn = array.GetTag(i);
                    // Ϊ�����ť���״̬��
                    InitStateController(tagBtn, tagInfo, arr, array);
                    if (tagInfo.isNull)
                        tagBtn.mStateController.ChangeState("Hide");
                    else
                    {
                        tagBtnDict.Add(tagInfo.id, tagBtn);
                        tagBtn.ChangeSprite(GameManager.Instance.GetSprite("UI/Tags/" + tagInfo.path));
                        tagBtn.mStateController.ChangeState("Unselected");
                    }
                }
                mSelectTagUI.AddTagArray(array);
            }

            PlayerData.StageInfo_Dynamic info_dynamic = data.GetCurrentDynamicStageInfo();
            string id = null;
            if (info_dynamic != null)
                id = info_dynamic.id;
            bool isOpen = false;
            if (id != null && lockedStageIdList.Contains(id))
            {
                mSelectTagUI.ShowDisableMask("��ǰ�ؿ���֧�ִ���ϵͳ��");
            }
            else if (!ConfigManager.IsDeveloperMode() && !(id != null && StageInfoManager.GetLocalStageInfo(id).rank >= 3))
            {
                mSelectTagUI.ShowDisableMask("����ͨ��һ�ν�����");
            }
            else
            {
                isOpen = true;
                mSelectTagUI.HideDisableMask();
            }

            // ���������ѡ��Ĵ���������ѡ�����
            if (isOpen)
            {
                List<string> currentStageTagList = data.GetTagList(info.chapterIndex, info.sceneIndex, info.stageIndex);
                foreach (var tagId in currentStageTagList)
                {
                    if (tagBtnDict.ContainsKey(tagId))
                    {
                        tagBtnDict[tagId].mStateController.ChangeState("Selected");
                    }
                }
            }

            string rankRate = Mathf.FloorToInt(data.GetRankRate() * 100).ToString() + "%";
            mSettlementUI.SetRankRateText(rankRate);
        }
        public void MUpdate()
        {

        }
        public void MPause()
        {
        }

        public void MPauseUpdate()
        {
        }

        public void MResume()
        {
        }
        public void MDestory()
        {
            tagBtnDict.Clear();
            limitItemDict.Clear();
            mSelectTagUI.MDestory();
            mSettlementUI.MDestory();
        }

        private void InitStateController(Tag tag, TagInfo tagInfo, TagInfo[] arr, TagArray array)
        {
            PlayerData data = GameManager.Instance.playerData;
            BaseStage.StageInfo info = data.GetCurrentStageInfo();
            List<string> currentStageTagList = data.GetTagList(info.chapterIndex, info.sceneIndex, info.stageIndex);

            tag.mStateController.Initial();
            // δѡ�е�״̬
            tag.mStateController.AddCreateStateFunc("Unselected", delegate {
                UnityAction action = delegate { tag.mStateController.ChangeState("Selected"); };
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    tag.Img.color = new Color(73f / 255, 121f / 255, 168f / 255, 1);
                    tag.Btn.onClick.AddListener(action);
                });
                s.AddOnExitAction(delegate {
                    tag.Btn.onClick.RemoveListener(action);
                });
                return s;
            });
            // ��ѡ��״̬
            tag.mStateController.AddCreateStateFunc("Selected", delegate {
                UnityAction action = delegate { tag.mStateController.ChangeState("Unselected"); };
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    tag.Btn.onClick.AddListener(action);
                    // ȡ�����Լ��⵱ǰͬԴ������Tagѡ��
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!arr[i].isNull && array.GetTag(i)!=tag)
                            array.GetTag(i).mStateController.ChangeState("Unselected");
                    }
                    // ����ѡ��
                    tag.Img.color = new Color(172f / 255, 213f / 255, 255f / 255, 1);
                    // ���ѡ��Ĵ���ID
                    bool flag = true;
                    foreach (var tagId in currentStageTagList)
                    {
                        if (tagId.Equals(tagInfo.id))
                        {
                            flag = false;
                            break;
                        }
                    }
                    if(flag)
                        currentStageTagList.Add(tagInfo.id);

                    // ͳ��������Ӵ�����
                    if (!limitItemDict.ContainsKey(tagInfo.id))
                    {
                        LimitItem item = LimitItem.GetInstance();
                        item.ChangeSprite(GameManager.Instance.GetSprite("UI/StageConfigPanel/rank"+(tagInfo.rank-1).ToString()));
                        item.SetText(tagInfo.descript);
                        mSettlementUI.AddLimitItem(item);
                        limitItemDict.Add(tagInfo.id, item);
                    }

                    // ˢ��һ��rankrate��ʾ
                    mSettlementUI.SetRankRateText(Mathf.FloorToInt(data.GetRankRate() * 100).ToString() + "%");
                });
                s.AddOnExitAction(delegate {
                    // �Ƴ�ѡ��Ĵ���ID
                    string rm = null;
                    foreach (var tagId in currentStageTagList)
                    {
                        if (tagId.Equals(tagInfo.id))
                            rm = tagId;
                    }
                    if (rm != null)
                        currentStageTagList.Remove(rm);
                    tag.Btn.onClick.RemoveListener(action);

                    // ͳ�������Ƴ�������
                    if (limitItemDict.ContainsKey(tagInfo.id))
                    {
                        LimitItem item = limitItemDict[tagInfo.id];
                        mSettlementUI.RemoveLimitItem(item);
                        limitItemDict.Remove(tagInfo.id);
                    }

                    // ˢ��һ��rankrate��ʾ
                    mSettlementUI.SetRankRateText(Mathf.FloorToInt(data.GetRankRate() * 100).ToString() + "%");
                });
                return s;
            });
            // ����״̬
            tag.mStateController.AddCreateStateFunc("Hide", delegate {
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    tag.Img.color = new Color(1, 1, 1, 0);
                    tag.BackImg.color = new Color(0, 32f / 255, 63f / 255, 0);
                    tag.Btn.interactable = false;
                });
                s.AddOnExitAction(delegate {
                    tag.BackImg.color = new Color(0, 32f / 255, 63f / 255, 1);
                    tag.Btn.interactable = true;
                });
                return s;
            });
        }
    }
}

