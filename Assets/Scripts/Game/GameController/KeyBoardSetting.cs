using System.Collections.Generic;
using UnityEngine;
using System;
/// <summary>
/// 键位控制器
/// </summary>
public class KeyBoardSetting : IGameControllerMember
{
    private Dictionary<KeyCode, List<Action>> KeyActionDict = new Dictionary<KeyCode, List<Action>>();

    public void MInit()
    {
        KeyActionDict.Clear();
        // 1键铲子
        AddAction(KeyCode.Alpha1, delegate {
            GameNormalPanel gameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
            gameNormalPanel.OnShovelSlotTrigger();
            // 快捷生效
            if(GameController.Instance.mCardController.isSelectShovel)
                GameController.Instance.mCardController.OnMouseLeftDownWhenSelectedShovel();
        });
        // ESC菜单
        AddAction(KeyCode.Escape, delegate {
            GameNormalPanel gameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
            gameNormalPanel.OnMenuButtonClick();
        });
        // Space暂停
        AddAction(KeyCode.Space, delegate {
            GameNormalPanel gameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
            gameNormalPanel.OnPauseButtonClick();
        });
        // 加载当前关的键位控制
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
        if (Input.GetKeyDown(KeyCode.Space))
        {
            KeyActionDict[KeyCode.Space][0]();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            KeyActionDict[KeyCode.Escape][0]();
        }
    }

    public void MDestory()
    {
        
    }

    /// <summary>
    /// 添加一个键位事件
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
