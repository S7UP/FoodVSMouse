using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// ��λ������
/// </summary>
public class KeyBoardSetting : IGameControllerMember
{
    private Dictionary<KeyCode, List<Action>> KeyActionDict = new Dictionary<KeyCode, List<Action>>();

    public void MInit()
    {
        KeyActionDict.Clear();
        // 1������
        AddAction(KeyCode.Alpha1, delegate {
            GameNormalPanel gameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
            gameNormalPanel.OnShovelSlotTrigger();
            // �����Ч
            if(GameController.Instance.mCardController.isSelectShovel)
                GameController.Instance.mCardController.OnMouseLeftDownWhenSelectedShovel();
        });
        // ���ص�ǰ�صļ�λ����
        GameController.Instance.LoadAndFixKeyBoardSetting();
    }

    public void MUpdate()
    {
        foreach (var keyValuePair in KeyActionDict)
        {
            if (Input.GetKeyDown(keyValuePair.Key))
            {
                foreach (var item in keyValuePair.Value)
                {
                    item();
                }
            }
        }
    }

    public void MPause()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MPauseUpdate()
    {

    }

    public void MDestory()
    {
        
    }

    /// <summary>
    /// ���һ����λ�¼�
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    public void AddAction(KeyCode key, Action action)
    {
        if (!KeyActionDict.ContainsKey(key))
        {
            KeyActionDict.Add(key, new List<Action>());
        }
        KeyActionDict[key].Add(action);
    }
}
