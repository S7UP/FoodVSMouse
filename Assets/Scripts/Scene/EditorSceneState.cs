using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorSceneState : BaseSceneState
{
    public EditorSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override void EnterScene()
    {
        mUIFacade.AddPanelToDict(StringManager.EditorPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        base.ExitScene();
    }
}
