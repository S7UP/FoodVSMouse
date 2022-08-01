using UnityEngine;

/// <summary>
/// ��ɫ���������ܿ������жԽ�ɫ���ֵĹ���
/// </summary>
public class CharacterController:IGameControllerMember
{
    private GameNormalPanel mGameNormalPanel;
    public CharacterUnit mCurrentCharacter; // ��ǰ��ɫ

    public CharacterController()
    {
        mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
    }

    public void MInit()
    {
        // ��ȡ���ѡȡ�Ľ�ɫ�����ɽ�ɫ��ʼʵ��������ʱ����Ϊ�ǻ״̬����Ҫ��ҷ��º�ſ��Լ���
        CharacterInfo info = GameManager.Instance.playerData.GetCharacterInfo();
        mCurrentCharacter = GameController.Instance.CreateCharacter(info.type, info.shape);
        mCurrentCharacter.gameObject.SetActive(false);
        // ֱ�ӽ����ɫ����ģʽ
        mGameNormalPanel.EnterCharacterConstructMode();
        GameController.Instance.Pause(); // ��ͣ��Ϸ��ֱ�����˷���ȥΪֹ
    }

    public void MUpdate()
    {
        //HandleSetCharacter();
    }

    public void MPauseUpdate()
    {
        HandleSetCharacter();
    }

    /// <summary>
    /// ������ý�ɫ������¼�
    /// </summary>
    private void HandleSetCharacter()
    {
        if (!IsSetCharacter())
        {
            if (Input.GetMouseButtonDown(0)) // ������Է�����
            {
                // TODO ��ȡ��ǰ��Ƭ�͸�����Ϣ���ۺ��ж��ܷ����ȥ������ȥ����к�������Ȼ���˳��ſ�ģʽ

                if (CanSetCharacter()) // ��һ���ǰѿ�����ȥ������ųɹ�����ȡ����Ƭѡ��
                {
                    ExecuteSetCharacter();
                }
                else
                {
                    Debug.Log("���ý�ɫʧ�ܣ���ѡ�����λ�÷��ý�ɫ��");
                }
            }
        }
    }

    /// <summary>
    /// �Ƿ���ý�ɫ
    /// </summary>
    /// <returns></returns>
    public bool IsSetCharacter()
    {
        return mCurrentCharacter.isActiveAndEnabled;
    }

    /// <summary>
    /// ��ǰ״̬���ܷ���˷���ȥ
    /// </summary>
    /// <returns></returns>
    public bool CanSetCharacter()
    {
        BaseGrid baseGrid = GameController.Instance.GetOverGrid();
        if (baseGrid != null)
        {
            // �ȼ�����״̬�ܷ������쿨
            // Ȼ���ٲ鿴�Ƿ��д˸��ӷ���Ŀ�Ƭ����û���������죬������
            return (baseGrid.CanBuildCard(FoodInGridType.Default) && !baseGrid.IsContainTag(FoodInGridType.Default));
        }
        return false;
    }

    /// <summary>
    /// ִ�����˲���
    /// </summary>
    /// <returns></returns>
    public void ExecuteSetCharacter()
    {
        // �˳���ɫ����ģʽ
        mGameNormalPanel.ExitCharacterConstructMode();
        // �����ͣ
        GameController.Instance.Resume();
        BaseGrid g = GameController.Instance.GetOverGrid();
        mCurrentCharacter.gameObject.SetActive(true);
        mCurrentCharacter.MInit();
        GameController.Instance.AddCharacter(mCurrentCharacter, g.GetColumnIndex(), g.GetRowIndex());
    }

    /// <summary>
    /// ���յ�ǰ�Ľ�ɫ
    /// </summary>
    public void RecycleCurrentCharacter()
    {
        if (mCurrentCharacter != null)
        {
            mCurrentCharacter.ExcuteRecycle();
        }
    }


    public void MPause()
    {
        mCurrentCharacter.MPause();
    }

    public void MResume()
    {
        mCurrentCharacter.MResume();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }
}
