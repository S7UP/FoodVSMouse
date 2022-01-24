using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseSceneState
{
    public void EnterScene();

    public void ExitScene();
}