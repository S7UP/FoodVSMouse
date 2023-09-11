using System.Collections;
public class GameNormalSceneState : BaseSceneState
{
    public GameNormalSceneState(UIFacade uiFacade) : base(uiFacade)
    {

    }
    public override IEnumerator LoadScene()
    {
        PlayerData data = PlayerData.GetInstance();
        // ���뱾�ص���Դ
        yield return GameManager.Instance.StartCoroutine(data.GetCurrentStageInfo().LoadResWhenEnterCombatScene());
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
        // ж�ر��ؼ��ص���Դ
        PlayerData.GetInstance().GetCurrentStageInfo().UnLoadResWhenExitCombatScene();
        // �뿪ս��������̬�ؿ���Ϣ����Ȼ����������ʹ��
        PlayerData.GetInstance().SetCurrentDynamicStageInfo(null);
        base.ExitScene();
    }
}