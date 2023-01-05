using UnityEngine;

public class BasePanel : MonoBehaviour, IBasePanel
{
    protected UIFacade mUIFacade;
    public virtual void EnterPanel()
    {
        gameObject.SetActive(true);
    }

    public virtual void ExitPanel()
    {
        gameObject.SetActive(false);
    }

    public virtual void InitPanel()
    {

    }

    public virtual void UpdatePanel()
    {

    }

    protected virtual void Awake()
    {
        mUIFacade = GameManager.Instance.uiManager.mUIFacade;
    }
}
