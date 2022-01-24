using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//�������UI�Ĺ�����
public class UIManager
{
    public UIFacade mUIFacade;
    public Dictionary<string, GameObject> currentScenePanelDict;
    private GameManager mGameManager;

    public UIManager()
    {
        mGameManager = GameManager.Instance;
        currentScenePanelDict = new Dictionary<string, GameObject>();
        mUIFacade = new UIFacade(this);
        //mUIFacade.currentSceneState = new StartLoadSceneState(mUIFacade);
        mUIFacade.currentSceneState = new GameNormalSceneState(mUIFacade);
    }

    // ��UIPanel�Żع���
    private void PushUIPanel(string uiPanelName, GameObject uiPanelGo)
    {
        mGameManager.PushGameObjectToFactory(FactoryType.UIPanelFactory, uiPanelName, uiPanelGo);
    }

    // ����ֵ�
    public void ClearDict()
    {
        foreach (var item in currentScenePanelDict)
        {
            // Debug.Log(item.Value.name);
            PushUIPanel(item.Value.name.Substring(0, item.Value.name.Length - 7), item.Value);
        }

        currentScenePanelDict.Clear();
    }


}
