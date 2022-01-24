using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameNormalPanel : MonoBehaviour, IBasePanel
{
    public GameObject mCardControllerUI;
    public GameObject mCardBuilderList;

    private void Awake()
    {
        mCardControllerUI = transform.Find("CardControllerUI").gameObject;
        Debug.Log(mCardControllerUI);
        mCardBuilderList = mCardControllerUI.transform.Find("CardBuilderList").gameObject;
        Debug.Log(mCardBuilderList);
    }

    // 在上方UI中增加一个卡槽
    public void AddCardSlot(BaseCardBuilder CardBuilder)
    {
        CardBuilder.mUIGo.transform.SetParent(mCardBuilderList.transform);
    }

    public void SelectCardBuilder()
    {
        Debug.Log("点击CardBuilder");
    }

    public void InitPanel()
    {
        // throw new System.NotImplementedException();
    }

    public void EnterPanel()
    {
        throw new System.NotImplementedException();
    }

    public void ExitPanel()
    {
        throw new System.NotImplementedException();
    }

    public void UpdatePanel()
    {
        throw new System.NotImplementedException();
    }
}
