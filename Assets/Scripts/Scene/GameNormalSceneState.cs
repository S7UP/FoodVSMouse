
using System.Collections;

public class GameNormalSceneState : BaseSceneState
{
    public GameNormalSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }
    public override IEnumerator LoadScene()
    {
        // ���뱾�ص�BGM
        yield return GameManager.Instance.StartCoroutine(PlayerData.GetInstance().GetCurrentStageInfo().LoadResWhenEnterCombatScene());
        yield return GameManager.Instance.StartCoroutine(GameManager.Instance.LoadSceneAsync("CombatScene"));
        //mUIFacade.AddPanelToDict(StringManager.GameNormalPanel);
    }

    public override void EnterScene()
    {
        //SceneManager.LoadScene("CombatScene");
        mUIFacade.AddPanelToDict(StringManager.GameNormalPanel);
        base.EnterScene();
    }

    public override void ExitScene()
    {
        // �����Ϸ���󹤳������ж���
        GameManager.Instance.ClearGameObjectFactory(FactoryType.GameFactory);
        // ���ͨ�غ�Ľ�������
        PlayerData.GetInstance().SetCurrentStageID(null);
        base.ExitScene();
    }
}