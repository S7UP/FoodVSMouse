using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ��ɫ���������ܿ������жԽ�ɫ���ֵĹ���
/// </summary>
public class CharacterController:IGameControllerMember
{
    private GameNormalPanel mGameNormalPanel;
    public CharacterUnit mCurrentCharacter; // ��ǰ��ɫ

    public void MInit()
    {
        mGameNormalPanel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];

        // TODO ��ȡ���ѡȡ�Ľ�ɫ�����ɽ�ɫ��ʼʵ��������ʱ����Ϊ�ǻ״̬����Ҫ��ҷ��º�ſ��Լ���
        mCurrentCharacter = GameController.Instance.CreateCharacter(0, 0); // ������ʱĬ����type=0 shape=0 �Ľ�ɫ
        mCurrentCharacter.gameObject.SetActive(false);
        // ֱ�ӽ����ɫ����ģʽ
        mGameNormalPanel.EnterCharacterConstructMode();
        GameController.Instance.Pause(); // ��ͣ��Ϸ��ֱ�����˷���ȥΪֹ
    }

    public void MUpdate()
    {
        HandleSetCharacter();
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
        BaseGrid g = GameController.Instance.GetOverGrid();
        mCurrentCharacter.gameObject.SetActive(true);
        mCurrentCharacter.MInit();
        GameController.Instance.AddCharacter(mCurrentCharacter, g.GetColumnIndex(), g.GetRowIndex());
        // �˳���ɫ����ģʽ
        mGameNormalPanel.ExitCharacterConstructMode();
        // �����ͣ
        GameController.Instance.Resume();
    }



    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }
}
