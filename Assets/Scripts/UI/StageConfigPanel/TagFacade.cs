using S7P.State;

using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

namespace UIPanel.StageConfigPanel
{
    public class TagFacade : IGameControllerMember
    {
        /// <summary>
        /// 被禁用的关卡
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
            // 构造界面
            foreach (var arr in list)
            {
                TagArray array = TagArray.GetInstance();
                for (int i = 0; i < arr.Length; i++)
                {
                    TagInfo tagInfo = arr[i];
                    Tag tagBtn = array.GetTag(i);
                    // 为这个按钮添加状态机
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
                mSelectTagUI.ShowDisableMask("当前关卡不支持词条系统！");
            }
            else if (!ConfigManager.IsDeveloperMode() && !(id != null && StageInfoManager.GetLocalStageInfo(id).rank >= 3))
            {
                mSelectTagUI.ShowDisableMask("正常通关一次解锁！");
            }
            else
            {
                isOpen = true;
                mSelectTagUI.HideDisableMask();
            }

            // 根据玩家已选择的词条来设置选择情况
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
            // 未选中的状态
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
            // 已选中状态
            tag.mStateController.AddCreateStateFunc("Selected", delegate {
                UnityAction action = delegate { tag.mStateController.ChangeState("Unselected"); };
                BaseState s = new BaseState();
                s.AddOnEnterAction(delegate {
                    tag.Btn.onClick.AddListener(action);
                    // 取消除自己外当前同源的所有Tag选定
                    for (int i = 0; i < arr.Length; i++)
                    {
                        if (!arr[i].isNull && array.GetTag(i)!=tag)
                            array.GetTag(i).mStateController.ChangeState("Unselected");
                    }
                    // 自身选定
                    tag.Img.color = new Color(172f / 255, 213f / 255, 255f / 255, 1);
                    // 添加选择的词条ID
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

                    // 统计栏里添加词条项
                    if (!limitItemDict.ContainsKey(tagInfo.id))
                    {
                        LimitItem item = LimitItem.GetInstance();
                        item.ChangeSprite(GameManager.Instance.GetSprite("UI/StageConfigPanel/rank"+(tagInfo.rank-1).ToString()));
                        item.SetText(tagInfo.descript);
                        mSettlementUI.AddLimitItem(item);
                        limitItemDict.Add(tagInfo.id, item);
                    }

                    // 刷新一次rankrate显示
                    mSettlementUI.SetRankRateText(Mathf.FloorToInt(data.GetRankRate() * 100).ToString() + "%");
                });
                s.AddOnExitAction(delegate {
                    // 移除选择的词条ID
                    string rm = null;
                    foreach (var tagId in currentStageTagList)
                    {
                        if (tagId.Equals(tagInfo.id))
                            rm = tagId;
                    }
                    if (rm != null)
                        currentStageTagList.Remove(rm);
                    tag.Btn.onClick.RemoveListener(action);

                    // 统计栏里移除词条项
                    if (limitItemDict.ContainsKey(tagInfo.id))
                    {
                        LimitItem item = limitItemDict[tagInfo.id];
                        mSettlementUI.RemoveLimitItem(item);
                        limitItemDict.Remove(tagInfo.id);
                    }

                    // 刷新一次rankrate显示
                    mSettlementUI.SetRankRateText(Mathf.FloorToInt(data.GetRankRate() * 100).ToString() + "%");
                });
                return s;
            });
            // 隐藏状态
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

