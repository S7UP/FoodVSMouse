using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 轮数进度条
/// </summary>
public class RoundProgressBar : BaseProgressBar
{
    private RectTransform Img_MouseIconTrans;
    private Image Img_One; // 波数个位数
    private Image Img_Ten; // 波数十位数
    private RectTransform Emp_FlagListRectTrans; // 旗帜显示
    private RectTransform Img_BarRectTrans; // 进度条
    private List<GameObject> flagList = new List<GameObject>(); // 旗帜表
    private float FlagListTotalWidth = 620f; // 旗帜槽总长
    private float BarTotalWidth = 620f; // 进度条总长

    private float startX = 660; // 进度条起始相对位置
    private float endX = 40;// 进度条终点相对位置
    private Sprite[] numberSpriteList; // 数字贴图

    private void Awake()
    {
        Img_MouseIconTrans = transform.Find("Emp_FlagList").Find("Img_MouseIcon").GetComponent<RectTransform>();
        Img_One = transform.Find("Img_Wave").Find("Img_One").GetComponent<Image>();
        Img_Ten = transform.Find("Img_Wave").Find("Img_Ten").GetComponent<Image>();
        Emp_FlagListRectTrans = transform.Find("Emp_FlagList").GetComponent<RectTransform>();
        Img_BarRectTrans = transform.Find("Emp_FlagList").Find("Img_Bar").GetComponent<RectTransform>();
        numberSpriteList = new Sprite[10];
        for (int i = 0; i < numberSpriteList.Length; i++)
        {
            numberSpriteList[i] = GameManager.Instance.GetSprite("UI/Wave_Number/" + i);
        }
    }

    /// <summary>
    /// 设置总轮数（旗帜数）
    /// </summary>
    public void SetTotalRoundCount(List<BaseRound> roundList)
    {
        // 先清空
        foreach (var item in flagList)
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "Img_Flag", item);
        flagList.Clear();
        // 计算所有轮总时间
        int totalTime = 0;
        foreach (var round in roundList)
        {
            if(!round.mRoundInfo.isBossRound)
                totalTime += round.GetTotalTimer();
        }

        // float per = 1.0f / (totalRoundCount + 1);
        // Emp_FlagListRectTrans.sizeDelta = new Vector2(FlagListTotalWidth*(1.0f-per) + 25, Emp_FlagListRectTrans.rect.height);
        float per = 0;
        foreach (var round in roundList)
        {
            if (!round.mRoundInfo.isBossRound)
            {
                per += (float)round.GetTotalTimer() / totalTime;
                GameObject go = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "Img_Flag");
                go.transform.SetParent(Emp_FlagListRectTrans);
                go.transform.localScale = new Vector2(1, 1);
                go.GetComponent<RectTransform>().anchoredPosition = new Vector2(-Emp_FlagListRectTrans.sizeDelta.x * per, 0);
                flagList.Add(go);
            }
        }
    }

    /// <summary>
    /// 设置当前进度
    /// </summary>
    /// <param name="progress"></param>
    public void SetCurrentProgress(float progress)
    {
        // 更新UI
        // if (currentProgress != progress)
        {
            //Img_MouseIconTrans.localPosition = new Vector2(-Emp_FlagListRectTrans.sizeDelta.x * progress, Img_MouseIconTrans.transform.localPosition.y);
            Img_MouseIconTrans.anchoredPosition = new Vector2(-Emp_FlagListRectTrans.sizeDelta.x * progress, Img_MouseIconTrans.anchoredPosition3D.y);
            //Img_BarRectTrans.sizeDelta = new Vector2(BarTotalWidth*progress, Img_BarRectTrans.rect.height);
            Img_BarRectTrans.transform.localScale = new Vector2(progress, 1);
        }
        currentProgress = progress;
    }
    
    /// <summary>
    /// 设置轮数UI
    /// </summary>
    public void UpdateRoundCountUI(int roundCount)
    {
        roundCount = Mathf.Max(0, Mathf.Min(roundCount, 99));
        Img_Ten.sprite = numberSpriteList[roundCount / 10];
        Img_One.sprite = numberSpriteList[roundCount % 10];
    }

    /// <summary>
    /// 当前进度条是否结束
    /// </summary>
    /// <returns></returns>
    public override bool IsFinish()
    {
        return currentProgress >= 1.0f;
    }

    /// <summary>
    /// 初始化显示
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
