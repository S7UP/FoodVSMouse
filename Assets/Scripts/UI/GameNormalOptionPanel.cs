//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class GameNormalOptionPanel : BasePanel
//{
//    [HideInInspector]
//    public bool isInBigLevelPanel = true;

//    public void ReturnToLastPanel()
//    {
//        if (isInBigLevelPanel)
//        {
//            // ����������
//            mUIFacade.ChangeSceneState(new MainSceneState(mUIFacade));
//        }
//        else
//        {
//            // ���ش�ؿ�ѡ�����
//            mUIFacade.currentScenePanelDict[StringManager.GameNormalLevelPanel].ExitPanel();
//            mUIFacade.currentScenePanelDict[StringManager.GameNormalBigLevelPanel].EnterPanel();

//        }
//        isInBigLevelPanel = true;
//    }

//    public void ToHelpPanel()
//    {
//        mUIFacade.currentScenePanelDict[StringManager.HelpPanel].EnterPanel();
//    }
//}
