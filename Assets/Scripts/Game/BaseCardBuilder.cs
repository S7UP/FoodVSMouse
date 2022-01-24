using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseCardBuilder : MonoBehaviour, IBaseCardBuilder, IGameControllerMember
{
    private GameController mGameController;
    public Dictionary<string, float> mCostDict;
    protected int mBaseCD; // ����CD
    public int mCD; // ��ǰ���CD
    public int mCDLeft; // ��ǰʣ��CD
    public bool isDisable; // �Ƿ񱻽�����
    public GameObject mUIGo; // �����е�UIʵ��

    public void MInit()
    {
        mGameController = GameController.Instance;

        // ����ֻ���⸳ֵ��ʵ��ʵ�����ȡ����Json�ļ�����
        mBaseCD = 7*ConfigManager.fps;
        mCD = mBaseCD;
        mCDLeft = 0;
        isDisable = false;
        mUIGo = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "CardBuilder");
        // mUIGo.transform.SetParent(GameController.Instance.uICanvasGo.transform);
        // ��ȡ��Ϸ�������
        GameNormalPanel panel = (GameNormalPanel)GameManager.Instance.uiManager.mUIFacade.currentScenePanelDict[StringManager.GameNormalPanel];
        panel.AddCardSlot(this); // ��������Ϣ��ӵ�����UI�
    }

    public void Awake()
    {
        MInit();
    }

    public void SetPosition(Vector3 v3)
    {
        mUIGo.transform.position = v3;
    }

    public bool CanConstructe()
    {
        throw new System.NotImplementedException();
    }

    public bool CanSelect()
    {
        throw new System.NotImplementedException();
    }

    public void CardBuilderUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void Constructe()
    {
        throw new System.NotImplementedException();
    }

    public BaseUnit GetResult()
    {
        throw new System.NotImplementedException();
    }

    public void InitInstance()
    {
        throw new System.NotImplementedException();
    }

    public void MDestory()
    {
        throw new System.NotImplementedException();
    }



    public void MPause()
    {
        throw new System.NotImplementedException();
    }

    public void MResume()
    {
        throw new System.NotImplementedException();
    }

    public void MUpdate()
    {
        throw new System.NotImplementedException();
    }
}
