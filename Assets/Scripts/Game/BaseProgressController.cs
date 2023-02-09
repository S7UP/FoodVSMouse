using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// 游戏进程管理器
/// </summary>
public class BaseProgressController : MonoBehaviour, IGameControllerMember
{
    // 引用
    private Transform Emp_LeftTimerTrans;
    private Transform HourTrans;
    private Image HourOne;
    private Image HourTen;
    private Image Hourcolon;
    private Transform MinTrans;
    private Image MinOne;
    private Image MinTen;
    private Image Mincolon;
    private Transform SecTrans;
    private Image SecOne;
    private Image SecTen;
    private Image Seccolon;
    private Transform MsTrans;
    private Image MsOne;
    private Image MsTen;
    private Sprite[] greenNumberList;
    private Sprite[] redNumberList;
    private List<Image> Img_TimerList;
    private GameObject Emp_BossHpBar;
    private GameObject Emp_BossHpBar2;
    private GameObject Emp_RoundProgressBar;

    // 管理变量
    public List<BaseProgressBar> mProgressBarList = new List<BaseProgressBar>(); // 内含各种进度条的集合
    public RoundProgressBar mRoundProgressBar; // 轮数进度条引用
    public BossHpBar mBossHpBar;
    private bool lastHasTarget1;
    public BossHpBar mBossHpBar2;
    private bool lastHasTarget2;
    // 时间相关
    private bool isTimeLimit; // 关卡是否有时间限制
    private int totalTimer; // 总时间
    public int currentTimerLeft { get; private set; }


    private void Awake()
    {
        Emp_BossHpBar = transform.Find("Emp_BossHpBar").gameObject;
        mBossHpBar = Emp_BossHpBar.GetComponent<BossHpBar>();
        Emp_BossHpBar2 = transform.Find("Emp_BossHpBar2").gameObject;
        mBossHpBar2 = Emp_BossHpBar2.GetComponent<BossHpBar>();
        Emp_RoundProgressBar = transform.Find("Emp_RoundProgressBar").gameObject;
        Emp_LeftTimerTrans = transform.Find("Emp_LeftTimer");
        HourTrans = Emp_LeftTimerTrans.Find("Hour");
        HourOne = HourTrans.Find("one").GetComponent<Image>();
        HourTen = HourTrans.Find("ten").GetComponent<Image>();
        Hourcolon = HourTrans.Find("colon").GetComponent<Image>();
        MinTrans = Emp_LeftTimerTrans.Find("Min");
        MinOne = MinTrans.Find("one").GetComponent<Image>();
        MinTen = MinTrans.Find("ten").GetComponent<Image>();
        Mincolon = MinTrans.Find("colon").GetComponent<Image>();
        SecTrans = Emp_LeftTimerTrans.Find("Sec");
        SecOne = SecTrans.Find("one").GetComponent<Image>();
        SecTen = SecTrans.Find("ten").GetComponent<Image>();
        Seccolon = SecTrans.Find("colon").GetComponent<Image>();
        MsTrans = Emp_LeftTimerTrans.Find("Ms");
        MsOne = MsTrans.Find("one").GetComponent<Image>();
        MsTen = MsTrans.Find("ten").GetComponent<Image>();
        Img_TimerList = new List<Image>() {
            HourOne, HourTen, MinOne, MinTen, SecOne, SecTen, MsOne, MsTen
        };

        greenNumberList = new Sprite[11]; // 第10号为冒号
        redNumberList = new Sprite[11];
        for (int i = 0; i < greenNumberList.Length; i++)
        {
            greenNumberList[i] = GameManager.Instance.GetSprite("UI/Time_Number/g"+i);
            redNumberList[i] = GameManager.Instance.GetSprite("UI/Time_Number/r" + i);
        }
    }

    /// <summary>
    /// 当前道中进度是否结束
    /// </summary>
    /// <returns></returns>
    public bool IsPathEnd()
    {
        return mRoundProgressBar.IsFinish();
    }

    /// <summary>
    /// 当前BOSS进度是否结束
    /// </summary>
    /// <returns></returns>
    public bool IsBossEnd()
    {
        return mBossHpBar.IsFinish();
    }

    /// <summary>
    /// 是否超时了
    /// </summary>
    public bool IsTimeOut()
    {
        if (!isTimeLimit)
            return false;
        return currentTimerLeft <= 0;
    }

    public void MInit()
    {
        lastHasTarget1 = false;
        lastHasTarget2 = false;
        mProgressBarList.Clear();
        // 获取轮数进度条脚本
        mRoundProgressBar = Emp_RoundProgressBar.GetComponent<RoundProgressBar>();
        mProgressBarList.Add(mRoundProgressBar);
        // 获取BOSS血条脚本
        mProgressBarList.Add(mBossHpBar);
        mProgressBarList.Add(mBossHpBar2);
        // 隐藏血条显示波数进度
        HideBossHpBar();
        // 初始化所有脚本内容
        for (int i = 0; i < mProgressBarList.Count; i++)
        {
            mProgressBarList[i].SetProgressController(this);
            mProgressBarList[i].PInit();
        }

        // 时间相关
        isTimeLimit = GameController.Instance.mCurrentStage.mStageInfo.isEnableTimeLimit;
        totalTimer = GameController.Instance.mCurrentStage.mStageInfo.totalSeconds * 60; 
        if (!isTimeLimit)
            Emp_LeftTimerTrans.gameObject.SetActive(false);
        else
            Emp_LeftTimerTrans.gameObject.SetActive(true);
        currentTimerLeft = totalTimer - GameController.Instance.GetCurrentStageFrame();

        UpdateTimerDisplayer();
    }

    public void MUpdate()
    {
        // 更新所有脚本内容
        for (int i = 0; i < mProgressBarList.Count; i++)
        {
            mProgressBarList[i].PUpdate();
        }
        // 计算剩余时间
        if (isTimeLimit)
        {
            currentTimerLeft = totalTimer - GameController.Instance.GetCurrentStageFrame();
            UpdateTimerDisplayer();
        }
        // 更新血条
        UpdateBossHpBar();
    }

    public void SetBossHpBarTarget(BaseUnit unit, int barNumber) 
    {
        BossHpBar b;
        if (!mBossHpBar.HasTarget())
            b = mBossHpBar;
        else if (!mBossHpBar2.HasTarget())
            b = mBossHpBar2;
        else
            return;

        b.SetTarget(unit, barNumber);
        ShowBossHpBar();
        
        if (unit is BossUnit)
        {
            b.SetIcon(GameManager.Instance.GetSprite("UI/BossIcon/"+unit.mType));
            b.SetName(GameManager.Instance.attributeManager.GetBossUnitAttribute(unit.mType, unit.mShape).baseAttrbute.name);
        }
        else
        {
            b.SetIcon(null);
            b.SetName("unknow");
        }
    }

    public void HideBossHpBar()
    {
        mBossHpBar.gameObject.SetActive(false);
        mBossHpBar2.gameObject.SetActive(false);
        mRoundProgressBar.gameObject.SetActive(true);
    }

    public void ShowBossHpBar()
    {
        if(mBossHpBar.HasTarget())
            mBossHpBar.gameObject.SetActive(true);
        else
            mBossHpBar.gameObject.SetActive(false);
        if (mBossHpBar2.HasTarget())
            mBossHpBar2.gameObject.SetActive(true);
        else
            mBossHpBar2.gameObject.SetActive(false);
        mRoundProgressBar.gameObject.SetActive(false);
    }

    private void UpdateBossHpBar()
    {
        // 检测BOSS存在情况与上一帧是否相同，如果相同则跳过更新
        if (mBossHpBar.HasTarget() == lastHasTarget1 && mBossHpBar2.HasTarget() == lastHasTarget2)
            return;
        lastHasTarget1 = mBossHpBar.HasTarget();
        lastHasTarget2 = mBossHpBar2.HasTarget();
        // 否则检测两BOSS存在情况
        if (lastHasTarget1 || lastHasTarget2)
        {
            // 只要有存在一只血条就会显示
            ShowBossHpBar();
        }
        else
        {
            // 否则取消血条显示
            HideBossHpBar();
        }

    }

    /// <summary>
    /// 更新时间显示
    /// </summary>
    private void UpdateTimerDisplayer()
    {
        int left = Mathf.Max(currentTimerLeft, 0);
        int hour = left / 216000;
        left = left % 216000;
        int min = left / 3600;
        left = left % 3600;
        int sec = left / 60;
        left = left % 60;
        int ms = 100*left/60;

        // 倒计时小于30秒时使用红色数字，否则使用绿色
        if (currentTimerLeft < 1800)
        {
            // 没到一个小时就直接隐藏小时计时器
            if(hour <= 0)
            {
                HourTrans.gameObject.SetActive(false);
            }
            else
            {
                if(!HourTrans.gameObject.activeSelf)
                    HourTrans.gameObject.SetActive(true);
                HourOne.sprite = redNumberList[hour%10];
                HourTen.sprite = redNumberList[hour / 10];
                Hourcolon.sprite = redNumberList[10];
            }
            // 分钟
            MinOne.sprite = redNumberList[min % 10];
            MinTen.sprite = redNumberList[min / 10];
            Mincolon.sprite = redNumberList[10];
            // 秒
            SecOne.sprite = redNumberList[sec % 10];
            SecTen.sprite = redNumberList[sec / 10];
            Seccolon.sprite = redNumberList[10];
            // 毫秒
            MsOne.sprite = redNumberList[ms % 10];
            MsTen.sprite = redNumberList[ms / 10];
        }
        else
        {
            // 没到一个小时就直接隐藏小时计时器
            if (hour <= 0)
            {
                HourTrans.gameObject.SetActive(false);
            }
            else
            {
                if (!HourTrans.gameObject.activeSelf)
                    HourTrans.gameObject.SetActive(true);
                HourOne.sprite = greenNumberList[hour % 10];
                HourTen.sprite = greenNumberList[hour / 10];
                Hourcolon.sprite = greenNumberList[10];
            }
            // 分钟
            MinOne.sprite = greenNumberList[min % 10];
            MinTen.sprite = greenNumberList[min / 10];
            Mincolon.sprite = greenNumberList[10];
            // 秒
            SecOne.sprite = greenNumberList[sec % 10];
            SecTen.sprite = greenNumberList[sec / 10];
            Seccolon.sprite = greenNumberList[10];
            // 毫秒
            MsOne.sprite = greenNumberList[ms % 10];
            MsTen.sprite = greenNumberList[ms / 10];
        }
        foreach (var item in Img_TimerList)
        {
            item.SetNativeSize();
        }
    }


    public void MDestory()
    {
        
    }

    public void MPause()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MPauseUpdate()
    {
        
    }
}
