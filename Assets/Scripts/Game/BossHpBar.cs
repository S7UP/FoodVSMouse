using UnityEngine;
using UnityEngine.UI;

public class BossHpBar : BaseProgressBar
{
    public BaseUnit TargetUnit
    {
        get
        {
            return mUnit;
        }
        set
        {
            SetTarget(value, 0);
        }
    }
    private BaseUnit mUnit; // 目标对象
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
    private float mHpPerBar; // 每管血代表多少生命值

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
            //Spr_BossHpBar[i] = GameManager.Instance.GetSprite("UI/BossHpBar_" + i);
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
        mTotalBarNumber = 0; // 一共有几管血
        mCurrentBarNumber = 0; // 当前有几管血
        mHpPerBar = 0; // 每管血代表多少生命值

        mVirtualHpBarPercent1 = 0; // 下层虚血百分比
        mVirtualHpBarPercent2 = 0; // 上层虚血百分比
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
    public void SetTarget(BaseUnit unit, int barNumber)
    {
        if (unit == null || !unit.IsValid())
        {
            Debug.LogWarning("设置了无效的目标！");
            return;
        }
        mUnit = unit;
        mTotalBarNumber = barNumber;
        // 根据目标的最大生命值来计算血条管数
        CalculateBarNumber();
    }

    /// <summary>
    /// 根据目标的最大生命值来计算血条管数
    /// </summary>
    private void CalculateBarNumber()
    {
        // 如果没有预设血条数，则自动计算
        if(mTotalBarNumber <= 0)
        {
            if (mUnit.mMaxHp <= 10000.0)
            {
                // 目标最大生命值小于等于1万时只启用一管血
                mTotalBarNumber = 1;
            }
            else
            {
                // 否则启用 目标 (最大生命值/10000)的值向上取整管血
                mTotalBarNumber = Mathf.CeilToInt(mUnit.mMaxHp / 10000);
            }
        }


        // 计算每管血代表的实际生命值
        mHpPerBar = mUnit.mMaxHp / mTotalBarNumber;
    }

    /// <summary>
    /// 计算当前剩余血条数，向上取整，即最后一管血返回的值为1
    /// </summary>
    private int CalculateCurrentBarNum()
    {
        return Mathf.CeilToInt(mUnit.mCurrentHp / mHpPerBar);
    }

    /// <summary>
    /// 计算当前血条剩余百分比
    /// </summary>
    /// <returns></returns>
    private float CalculateCurrentBarProgress()
    {
        float currentBarHpLeft = mUnit.mCurrentHp - mHpPerBar * (CalculateCurrentBarNum() - 1);
        return currentBarHpLeft / mHpPerBar;
    }

    /// <summary>
    /// 根据当前血条管数来更新血条贴图
    /// </summary>
    private void UpdateHpBarSprite()
    {
        int currentBarNum = CalculateCurrentBarNum();
        int index;
        // 上层实血条的颜色下标为:(血条管数-1)%7
        index = (currentBarNum - 1) % 7;
        Img_BossHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // 下层实血条的颜色下标为:(血条管数-2)%7
        index = (currentBarNum - 2) % 7;
        Img_BossHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // 上层虚血条的颜色下标为:(血条管数)%7
        index = (currentBarNum) % 7;
        Img_BossVirtualHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // 下层虚血条的颜色下标为:(血条管数-1)%7，与上层实血条颜色应当同步
        index = (currentBarNum - 1) % 7;
        Img_BossVirtualHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
    }

    /// <summary>
    /// 更新BOSS的血条
    /// </summary>
    public override void PUpdate()
    {
        base.PUpdate();
        if (HasTarget())
        {
            // 更新血条管数显示
            int currentBarNum = CalculateCurrentBarNum();
            // 当前计算血条管数 与 记录管数不符时
            if (mCurrentBarNumber != currentBarNum)
            {
                mCurrentBarNumber = currentBarNum;
                // 更新个位数数据显示
                Emp_BossLifeLeft.transform.GetChild(2).GetComponent<Image>().sprite = numberSpriteList[Mathf.Max(0, currentBarNum % 10)];
                // 更新十位数数据显示
                Emp_BossLifeLeft.transform.GetChild(1).GetComponent<Image>().sprite = numberSpriteList[Mathf.Max(0, Mathf.FloorToInt((currentBarNum % 100) / 10))];
                // 更新百位数数据显示
                Emp_BossLifeLeft.transform.GetChild(0).GetComponent<Image>().sprite = numberSpriteList[Mathf.Max(0, Mathf.FloorToInt((currentBarNum % 1000) / 100))];

                if (currentBarNum <= 1)
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
                // 上层虚血继承下层虚血百分比
                // mVirtualHpBarPercent2 = mVirtualHpBarPercent1;
                // 上层虚血清空
                mVirtualHpBarPercent2 = 0;
                // 下层虚血回调至100%
                mVirtualHpBarPercent1 = 1.00f;
            }


            // 更新当前血条百分比显示
            float percent = CalculateCurrentBarProgress();
            Img_BossHp2.transform.localScale = new Vector3(percent, 1, 1);
            // 虚血掉落效果 掉落速度为每秒掉10%
            mVirtualHpBarPercent1 = Mathf.Max(percent, mVirtualHpBarPercent1 - (float)(0.10 / 60));
            mVirtualHpBarPercent2 = Mathf.Max(0, mVirtualHpBarPercent2 - (float)(0.10 / 60));
            if (mVirtualHpBarPercent2 <= 0)
            {
                Img_BossVirtualHp2.SetActive(false);
            }
            else
            {
                Img_BossVirtualHp2.SetActive(true);
            }
            Img_BossVirtualHp1.transform.localScale = new Vector3(mVirtualHpBarPercent1, 1, 1);
            Img_BossVirtualHp2.transform.localScale = new Vector3(mVirtualHpBarPercent2, 1, 1);
            // 更新剩余血量百分比显示
            Tex_LifePercent.text = (Mathf.Max(TargetUnit.GetHeathPercent(), 0)*100).ToString("#0.00") + "%";

            if (!mUnit.IsAlive())
            {
                // 没血量时都隐藏就行了
                Img_BossHp1.SetActive(false);
                Img_BossHp2.SetActive(false);
                Img_BossVirtualHp1.SetActive(false);
                Img_BossVirtualHp2.SetActive(false);
            }
        }
        //else
        //{
        //    // 如果目标噶了就隐藏自己并重新显示道中
        //    mProgressController.HideBossHpBar();
        //}
    }

    /// <summary>
    /// 是否有目标
    /// </summary>
    /// <returns></returns>
    public bool HasTarget()
    {
        return mUnit != null && mUnit.IsValid();
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
