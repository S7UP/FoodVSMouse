using System.Collections;

public class EditorSceneState : BaseSceneState
{
    public EditorSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }

    public override IEnumerator LoadScene()
    {
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadSceneAsync("EditorScene"));
    }

    public override void EnterScene()
    {
        // SceneManager.LoadScene("EditorScene");
        mUIFacade.AddPanelToDict(StringManager.EditorPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        GlobalData.Manager.Instance.mEditStage.CloseEditMode();
        base.ExitScene();
    }
}
