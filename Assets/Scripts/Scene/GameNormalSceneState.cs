using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNormalSceneState : BaseSceneState
{
    public GameNormalSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        mUIFacade.AddPanelToDict(StringManager.GameNormalPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}