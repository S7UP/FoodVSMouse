using UnityEngine;
using UnityEngine.UI;

public class BossHpBar : BaseProgressBar
{
    public BossUnit TargetUnit
    {
        get
        {
            return mUnit;
        }
        set
        {
            SetTarget(value);
        }
    }
    private BossUnit mUnit; // 目标对象
    protected bool isAutoDestory; // 目标对象消失时是否自动销毁（隐藏）

    private Image Img_BossIcon;
    private GameObject Img_BossVirtualHp1;
    private GameObject Emp_BossHp;
    private GameObject Img_BossHp1;
    private GameObject Img_BossVirtualHp2;
    private GameObject Img_BossHp2;
    private GameObject Img_BossHpBar1;
    private GameObject Img_BossHpBar2;
    private GameObject Img_BossHpBar3;
    private Text Tex_BossName;
    private GameObject Emp_BossLifeLeft;
    private Text Tex_LifePercent;

    private Sprite[] numberSpriteList; // 数字贴图
    private Sprite[] Spr_BossHpBar; // 血条颜色贴图

    // 变量
    private int mTotalBarNumber; // 一共有几管血
    private int mCurrentBarNumber; // 当前有几管血
    private float[] hpRateArray;
    private float drop_percent; // 虚血掉落速度
    private int aliveTime;

    private float mVirtualHpBarPercent1; // 下层虚血百分比
    private float mVirtualHpBarPercent2; // 上层虚血百分比

    private void Awake()
    {
        Img_BossIcon = transform.Find("Img_BossIconDisplayer").Find("Image").GetComponent<Image>();
        Emp_BossHp = transform.Find("Emp_BossHp").gameObject;
        Img_BossVirtualHp1 = Emp_BossHp.transform.Find("Img_BossVirtualHp1").gameObject;
        Img_BossHp1 = Emp_BossHp.transform.Find("Img_BossHp1").gameObject;
        Img_BossVirtualHp2 = Emp_BossHp.transform.Find("Img_BossVirtualHp2").gameObject;
        Img_BossHp2 = Emp_BossHp.transform.Find("Img_BossHp2").gameObject;
        Img_BossHpBar1 = transform.Find("Img_BossHpBar1").gameObject;
        Img_BossHpBar2 = transform.Find("Img_BossHpBar2").gameObject;
        Img_BossHpBar3 = transform.Find("Img_BossHpBar3").gameObject;
        Tex_BossName = transform.Find("Img_BossName").Find("Text").GetComponent<Text>();
        Emp_BossLifeLeft = transform.Find("Emp_BossLifeLeft").gameObject;
        Tex_LifePercent = transform.Find("Tex_LifePercent").GetComponent<Text>();

        numberSpriteList = new Sprite[10];
        for (int i = 0; i < numberSpriteList.Length; i++)
        {
            numberSpriteList[i] = GameManager.Instance.GetSprite("UI/Wave_Number/" + i);
        }
        Spr_BossHpBar = new Sprite[7];
        Sprite[] sprites = GameManager.Instance.GetSprites("Pictures/UI/BossHpBar");
        for (int i = 0; i < Spr_BossHpBar.Length; i++)
        {
            for (int j = 0; j < sprites.Length; j++)
            {
                if (sprites[j].name.Equals("BossHpBar_" + i))
                {
                    Spr_BossHpBar[i] = sprites[j];
                    break;
                }
            }
        }
    }

    public override void PInit()
    {
        base.PInit();
        aliveTime = 0;
        mTotalBarNumber = 0; // 一共有几管血
        mCurrentBarNumber = -1; // 当前有几管血
        hpRateArray = null;

        mVirtualHpBarPercent1 = 0; // 下层虚血百分比
        mVirtualHpBarPercent2 = 0; // 上层虚血百分比

        drop_percent = 0;
    }

    /// <summary>
    /// 当前进度条是否结束
    /// </summary>
    /// <returns></returns>
    public override bool IsFinish()
    {
        return mUnit == null || mUnit.IsValid();
    }

    /// <summary>
    /// 设置目标
    /// </summary>
    /// <param name="unit">目标单位</param>
    /// <param name="barNumber">预设血条数，如果填小于等于0则自动计算血条数，算法定义在CalculateBarNumber()中</param>
    public void SetTarget(BossUnit unit)
    {
        if (unit == null || !unit.IsValid())
        {
            Debug.LogWarning("设置了无效的目标！");
            return;
        }
        PInit();

        mUnit = unit;
        mTotalBarNumber = 1;
        float[] arr = unit.GetParamArray("hpRate");
        for (int i = 0; i < arr.Length; i++)
        {
            mTotalBarNumber = i + 1;
            if (arr[i] <= 0)
                break;
        }

        hpRateArray = new float[mTotalBarNumber];
        for (int i = 0; i < mTotalBarNumber; i++)
        {
            hpRateArray[i] = arr[i];
        }
        aliveTime = 0;
    }

    /// <summary>
    /// 根据血量计算当前BOSS的阶段（并不是实际阶段，仅仅是以血量百分比作为参考）
    /// </summary>
    /// <returns></returns>
    private int CalculateStage()
    {
        int stage = hpRateArray.Length;
        float percent = mUnit.GetHeathPercent();
        for (int i = 0; i < hpRateArray.Length; i++)
        {
            if (percent > hpRateArray[i])
            {
                stage = i;
                break;
            }
        }
        return stage;
    }

    /// <summary>
    /// 计算当前剩余血条数
    /// </summary>
    private int CalculateCurrentBarNum()
    {
        return mTotalBarNumber - CalculateStage();
    }

    /// <summary>
    /// 计算当前血条剩余百分比
    /// </summary>
    /// <returns></returns>
    private float CalculateCurrentBarProgress()
    {
        //float currentBarHpLeft = mUnit.mCurrentHp - mHpPerBar * (CalculateCurrentBarNum() - 1);
        //return currentBarHpLeft / mHpPerBar;
        int stage = CalculateStage();
        return mUnit.GetStageLeftHp(stage) / mUnit.GetStageTotalHp(stage);
    }

    /// <summary>
    /// 根据当前血条管数来更新血条贴图
    /// </summary>
    private void UpdateHpBarSprite()
    {
        int currentBarNum = CalculateCurrentBarNum();
        int index;
        // 上层实血条的颜色下标为:(血条管数)%7
        index = (currentBarNum) % 7;
        Img_BossHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // 下层实血条的颜色下标为:(血条管数-1)%7
        index = (currentBarNum - 1) % 7;
        Img_BossHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // 上层虚血条的颜色下标为:(血条管数+1)%7
        index = (currentBarNum + 1) % 7;
        Img_BossVirtualHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // 下层虚血条的颜色下标为:(血条管数)%7，与上层实血条颜色应当同步
        index = (currentBarNum) % 7;
        Img_BossVirtualHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
    }

    /// <summary>
    /// 更新BOSS的血条
    /// </summary>
    public override void PUpdate()
    {
        if (!isActiveAndEnabled)
            return;
        base.PUpdate();
        if (HasTarget())
        {
            aliveTime++;
            // 更新血条管数显示
            int currentBarNum = CalculateCurrentBarNum();
            // 当前计算血条管数 与 记录管数不符时
            if (mCurrentBarNumber != currentBarNum)
            {
                mCurrentBarNumber = currentBarNum;
                // 更新个位数数据显示
                Emp_BossLifeLeft.transform.GetChild(2).GetComponent<Image>().sprite = numberSpriteList[Mathf.Max(0, currentBarNum % 10)];
                // 更新十位数数据显示
                int ten = Mathf.Max(0, Mathf.FloorToInt((currentBarNum % 100) / 10));
                if(ten > 0)
                {
                    Emp_BossLifeLeft.transform.GetChild(1).gameObject.SetActive(true);
                    Emp_BossLifeLeft.transform.GetChild(1).GetComponent<Image>().sprite = numberSpriteList[ten];
                }
                else
                {
                    Emp_BossLifeLeft.transform.GetChild(1).gameObject.SetActive(false);
                }
                // 更新百位数数据显示
                int hun = Mathf.Max(0, Mathf.FloorToInt((currentBarNum % 1000) / 100));
                if(hun > 0)
                {
                    Emp_BossLifeLeft.transform.GetChild(0).gameObject.SetActive(true);
                    Emp_BossLifeLeft.transform.GetChild(0).GetComponent<Image>().sprite = numberSpriteList[hun];
                }
                else
                {
                    Emp_BossLifeLeft.transform.GetChild(0).gameObject.SetActive(false);
                }

                if (currentBarNum <= 0)
                {
                    // 当前仅剩一管血时，隐藏下层血条
                    Img_BossHp1.SetActive(false);
                    Img_BossHp2.SetActive(true);
                    Img_BossVirtualHp1.SetActive(true);
                    Img_BossVirtualHp2.SetActive(true);
                }
                else
                {
                    Img_BossHp1.SetActive(true);
                    Img_BossHp2.SetActive(true);
                    Img_BossVirtualHp1.SetActive(true);
                    Img_BossVirtualHp2.SetActive(true);
                }
                // 更新血条颜色
                UpdateHpBarSprite();
                // 上层虚血清空
                mVirtualHpBarPercent2 = 0;
                // 下层虚血回调至100%
                mVirtualHpBarPercent1 = 1.00f;
            }


            // 更新当前血条百分比显示
            float barRate = Mathf.Sin(Mathf.PI / 2 * Mathf.Min(1, (float)aliveTime / 240f));
            float percent = CalculateCurrentBarProgress();
            Img_BossHp1.transform.localScale = new Vector3(Mathf.Min(1, 2*barRate), 1, 1);
            Img_BossHp2.transform.localScale = new Vector3(percent*barRate, 1, 1);
            // 15帧没挨打就有虚血掉落效果 掉落速度为每秒掉30%
            if (mUnit.hitBox.GetLastHitTime() > 15)
            {
                drop_percent = Mathf.Min(0.3f / 60, drop_percent + 0.0025f / 60);
            }
            else
            {
                drop_percent = 0;
            }
            mVirtualHpBarPercent1 = Mathf.Max(percent, mVirtualHpBarPercent1 - drop_percent);
            mVirtualHpBarPercent2 = Mathf.Max(0, mVirtualHpBarPercent2 - drop_percent);

            if (mVirtualHpBarPercent2 <= 0)
            {
                Img_BossVirtualHp2.SetActive(false);
            }
            else
            {
                Img_BossVirtualHp2.SetActive(true);
            }
            Img_BossVirtualHp1.transform.localScale = new Vector3(mVirtualHpBarPercent1 * barRate, 1, 1);
            Img_BossVirtualHp2.transform.localScale = new Vector3(mVirtualHpBarPercent2 * barRate, 1, 1);

            // 更新剩余血量百分比显示
            Tex_LifePercent.text = (mUnit.mCurrentHp* barRate).ToString("#0") + " / " + mUnit.mMaxHp.ToString("#0") + "       " + (Mathf.Max(TargetUnit.GetHeathPercent()* barRate, 0) * 100).ToString("#0.00") + "%";
            if (mUnit.mCurrentHp <= 0)
            {
                // 没血量时都隐藏就行了
                Img_BossHp1.SetActive(false);
                Img_BossHp2.SetActive(false);
                Img_BossVirtualHp1.SetActive(false);
                Img_BossVirtualHp2.SetActive(false);
            }
        }
        else
        {
            // 没血量时都隐藏就行了
            Img_BossHp1.SetActive(false);
            Img_BossHp2.SetActive(false);
            Img_BossVirtualHp1.SetActive(false);
            Img_BossVirtualHp2.SetActive(false);
            // 更新剩余血量百分比显示
            Tex_LifePercent.text = "";
            mUnit = null;
        }
    }

    /// <summary>
    /// 是否有目标
    /// </summary>
    /// <returns></returns>
    public bool HasTarget()
    {
        bool flag = mUnit != null && mUnit.IsValid();
        if (!flag)
            mUnit = null;
        return flag;
    }

    /// <summary>
    /// 设置目标头像缩略图
    /// </summary>
    /// <param name="s"></param>
    public void SetIcon(Sprite s)
    {
        if (s == null)
            Img_BossIcon.gameObject.SetActive(false);
        else
        {
            Img_BossIcon.gameObject.SetActive(true);
            Img_BossIcon.sprite = s;
        }
    }

    /// <summary>
    /// 设置名字
    /// </summary>
    /// <param name="name"></param>
    public void SetName(string name)
    {
        Tex_BossName.text = name;
    }

    /// <summary>
    /// 设置为自动销毁
    /// </summary>
    public void AutoDestoryEnable()
    {
        isAutoDestory = true;
    }
}
