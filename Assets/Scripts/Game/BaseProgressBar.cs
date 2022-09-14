using UnityEngine;

public class BaseProgressBar : MonoBehaviour, IBaseProgressBar
{
    protected BaseProgressController mProgressController;
    public float currentProgress; // ��ǰ����

    public virtual bool IsFinish()
    {
        return false;
    }

    public virtual void PInit()
    {
    }

    public virtual void PUpdate()
    {

    }

    public virtual void Hide()
    {

    }

    public virtual void Show()
    {

    }

    public void SetProgressController(BaseProgressController c)
    {
        mProgressController = c;
    }
}
