using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 选择配置面版
/// </summary>
public class SelectPanel : BasePanel
{
    /// <summary>
    /// 进入战斗场景（开始游戏）
    /// </summary>
    public void EnterComBatScene()
    {
        GameManager.Instance.EnterComBatScene();
    }

    //public override void EnterPanel()
    //{

    //}

    //public override void ExitPanel()
    //{

    //}

    //public override void InitPanel()
    //{

    //}

    //public override void UpdatePanel()
    //{

    //}
}
