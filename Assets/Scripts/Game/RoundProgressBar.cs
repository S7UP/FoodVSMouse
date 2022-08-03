using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ����������
/// </summary>
public class RoundProgressBar : BaseProgressBar
{
    private RectTransform Img_MouseIconTrans;
    private Image Img_One; // ������λ��
    private Image Img_Ten; // ����ʮλ��
    private RectTransform Emp_FlagListRectTrans; // ������ʾ
    private RectTransform Img_BarRectTrans; // ������
    private float FlagListTotalWidth = 620f; // ���Ĳ��ܳ�
    private float BarTotalWidth = 620f; // �������ܳ�

    private float startX = 660; // ��������ʼ���λ��
    private float endX = 40;// �������յ����λ��
    private Sprite[] numberSpriteList; // ������ͼ

    private void Awake()
    {
        Img_MouseIconTrans = transform.Find("Img_MouseIcon").GetComponent<RectTransform>();
        Img_One = transform.Find("Img_Wave").Find("Img_One").GetComponent<Image>();
        Img_Ten = transform.Find("Img_Wave").Find("Img_Ten").GetComponent<Image>();
        Emp_FlagListRectTrans = transform.Find("Emp_FlagList").GetComponent<RectTransform>();
        Img_BarRectTrans = transform.Find("Img_Bar").GetComponent<RectTransform>();
        numberSpriteList = new Sprite[10];
        for (int i = 0; i < numberSpriteList.Length; i++)
        {
            numberSpriteList[i] = GameManager.Instance.GetSprite("UI/Wave_Number/" + i);
        }
    }

    /// <summary>
    /// ��������������������
    /// </summary>
    public void SetTotalRoundCount(int totalRoundCount)
    {
        // �����
        List<GameObject> list = new List<GameObject>();
        for (int i = 0; i < Emp_FlagListRectTrans.childCount; i++)
        {
            list.Add(Emp_FlagListRectTrans.GetChild(i).gameObject);
        }
        foreach (var item in list)
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Img_Flag", item);
        }

        float per = 1.0f / (totalRoundCount + 1);
        Emp_FlagListRectTrans.sizeDelta = new Vector2(FlagListTotalWidth*(1.0f-per) + 25, Emp_FlagListRectTrans.rect.height);
        for (int i = 0; i < totalRoundCount; i++)
        {
            GameObject go = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Img_Flag");
            go.transform.SetParent(Emp_FlagListRectTrans.transform);
            go.transform.localScale = new Vector3(1, 1, 1);
        }
    }

    /// <summary>
    /// ���õ�ǰ����
    /// </summary>
    /// <param name="progress"></param>
    public void SetCurrentProgress(float progress)
    {
        // ����UI
        if (currentProgress != progress)
        {
            Img_MouseIconTrans.anchoredPosition3D = new Vector3(startX+(endX-startX)*progress, Img_MouseIconTrans.anchoredPosition3D.y, Img_MouseIconTrans.anchoredPosition3D.z);
            Img_BarRectTrans.sizeDelta = new Vector2(BarTotalWidth*progress, Img_BarRectTrans.rect.height);
        }
        currentProgress = progress;
    }
    
    /// <summary>
    /// ��������UI
    /// </summary>
    public void UpdateRoundCountUI(int roundCount)
    {
        roundCount = Mathf.Max(0, Mathf.Min(roundCount, 99));
        Img_Ten.sprite = numberSpriteList[roundCount / 10];
        Img_One.sprite = numberSpriteList[roundCount % 10];
    }

    /// <summary>
    /// ��ǰ�������Ƿ����
    /// </summary>
    /// <returns></returns>
    public override bool IsFinish()
    {
        return currentProgress >= 1.0f;
    }

    /// <summary>
    /// ��ʼ����ʾ
    /// </summary>
    public override void PInit()
    {
        UpdateRoundCountUI(0);
        SetCurrentProgress(0);
    }

    public override void PUpdate()
    {

    }

    public override void Hide()
    {

    }

    public override void Show()
    {

    }
}
