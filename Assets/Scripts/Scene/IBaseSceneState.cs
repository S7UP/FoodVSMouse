using System.Collections;

public interface IBaseSceneState
{
    public IEnumerator LoadScene();

    public void EnterScene();

    public void ExitScene();
}