using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 大章节面版
/// </summary>
namespace LocalStagePanel
{
    public class LocalStagePanel : BasePanel
    {
        private Transform Trans_UI;
        private ScrollRect Scr;
        private List<Item> itemList = new List<Item>();
        private Button Btn_Exit;

        protected override void Awake()
        {
            base.Awake();
            Trans_UI = transform.Find("UI");
            
            Scr = Trans_UI.Find("Scr").GetComponent<ScrollRect>();
            Btn_Exit = Trans_UI.Find("Btn_Exit").GetComponent<Button>();
        }

        public override void InitPanel()
        {
            base.InitPanel();

            // 按钮
            {
                Btn_Exit.onClick.RemoveAllListeners();
                Btn_Exit.onClick.AddListener(delegate { mUIFacade.currentScenePanelDict[StringManager.LocalStagePanel].ExitPanel(); });
            }



        }

        public override void EnterPanel()
        {
            base.EnterPanel();
            // 从本地读取关卡
            foreach (var item in itemList)
                item.ExecuteRecycle();
            itemList.Clear();
            {
                GlobalData.EditStage edit = GlobalData.Manager.Instance.mEditStage;
                edit.LoadCustStageList();
                foreach (var info in edit.GetCustStageList())
                {
                    Item item = Item.GetInstance(info.name, info.difficulty);
                    itemList.Add(item);
                    item.AddOnClickAction(delegate
                    {
                        {
                            PlayerData data = PlayerData.GetInstance();
                            PlayerData.StageInfo_Dynamic info_dynamic = new PlayerData.StageInfo_Dynamic();
                            info_dynamic.info = info;
                            data.SetCurrentDynamicStageInfo(info_dynamic);
                        }
                        mUIFacade.currentScenePanelDict[StringManager.StageConfigPanel].EnterPanel();
                    });
                    item.transform.SetParent(Scr.content);
                    item.transform.localScale = Vector2.one;
                }

            }
        }
    }

}
