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
    private BossUnit mUnit; // Ŀ�����
    protected bool isAutoDestory; // Ŀ�������ʧʱ�Ƿ��Զ����٣����أ�

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

    private Sprite[] numberSpriteList; // ������ͼ
    private Sprite[] Spr_BossHpBar; // Ѫ����ɫ��ͼ

    // ����
    private int mTotalBarNumber; // һ���м���Ѫ
    private int mCurrentBarNumber; // ��ǰ�м���Ѫ
    private float[] hpRateArray;
    private float drop_percent; // ��Ѫ�����ٶ�
    private int aliveTime;

    private float mVirtualHpBarPercent1; // �²���Ѫ�ٷֱ�
    private float mVirtualHpBarPercent2; // �ϲ���Ѫ�ٷֱ�

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
        mTotalBarNumber = 0; // һ���м���Ѫ
        mCurrentBarNumber = -1; // ��ǰ�м���Ѫ
        hpRateArray = null;

        mVirtualHpBarPercent1 = 0; // �²���Ѫ�ٷֱ�
        mVirtualHpBarPercent2 = 0; // �ϲ���Ѫ�ٷֱ�

        drop_percent = 0;
    }

    /// <summary>
    /// ��ǰ�������Ƿ����
    /// </summary>
    /// <returns></returns>
    public override bool IsFinish()
    {
        return mUnit == null || mUnit.IsValid();
    }

    /// <summary>
    /// ����Ŀ��
    /// </summary>
    /// <param name="unit">Ŀ�굥λ</param>
    /// <param name="barNumber">Ԥ��Ѫ�����������С�ڵ���0���Զ�����Ѫ�������㷨������CalculateBarNumber()��</param>
    public void SetTarget(BossUnit unit)
    {
        if (unit == null || !unit.IsValid())
        {
            Debug.LogWarning("��������Ч��Ŀ�꣡");
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
    /// ����Ѫ�����㵱ǰBOSS�Ľ׶Σ�������ʵ�ʽ׶Σ���������Ѫ���ٷֱ���Ϊ�ο���
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
    /// ���㵱ǰʣ��Ѫ����
    /// </summary>
    private int CalculateCurrentBarNum()
    {
        return mTotalBarNumber - CalculateStage();
    }

    /// <summary>
    /// ���㵱ǰѪ��ʣ��ٷֱ�
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
    /// ���ݵ�ǰѪ������������Ѫ����ͼ
    /// </summary>
    private void UpdateHpBarSprite()
    {
        int currentBarNum = CalculateCurrentBarNum();
        int index;
        // �ϲ�ʵѪ������ɫ�±�Ϊ:(Ѫ������)%7
        index = (currentBarNum) % 7;
        Img_BossHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // �²�ʵѪ������ɫ�±�Ϊ:(Ѫ������-1)%7
        index = (currentBarNum - 1) % 7;
        Img_BossHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // �ϲ���Ѫ������ɫ�±�Ϊ:(Ѫ������+1)%7
        index = (currentBarNum + 1) % 7;
        Img_BossVirtualHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // �²���Ѫ������ɫ�±�Ϊ:(Ѫ������)%7�����ϲ�ʵѪ����ɫӦ��ͬ��
        index = (currentBarNum) % 7;
        Img_BossVirtualHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
    }

    /// <summary>
    /// ����BOSS��Ѫ��
    /// </summary>
    public override void PUpdate()
    {
        if (!isActiveAndEnabled)
            return;
        base.PUpdate();
        if (HasTarget())
        {
            aliveTime++;
            // ����Ѫ��������ʾ
            int currentBarNum = CalculateCurrentBarNum();
            // ��ǰ����Ѫ������ �� ��¼��������ʱ
            if (mCurrentBarNumber != currentBarNum)
            {
                mCurrentBarNumber = currentBarNum;
                // ���¸�λ��������ʾ
                Emp_BossLifeLeft.transform.GetChild(2).GetComponent<Image>().sprite = numberSpriteList[Mathf.Max(0, currentBarNum % 10)];
                // ����ʮλ��������ʾ
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
                // ���°�λ��������ʾ
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
                    // ��ǰ��ʣһ��Ѫʱ�������²�Ѫ��
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
                // ����Ѫ����ɫ
                UpdateHpBarSprite();
                // �ϲ���Ѫ���
                mVirtualHpBarPercent2 = 0;
                // �²���Ѫ�ص���100%
                mVirtualHpBarPercent1 = 1.00f;
            }


            // ���µ�ǰѪ���ٷֱ���ʾ
            float barRate = Mathf.Sin(Mathf.PI / 2 * Mathf.Min(1, (float)aliveTime / 240f));
            float percent = CalculateCurrentBarProgress();
            Img_BossHp1.transform.localScale = new Vector3(Mathf.Min(1, 2*barRate), 1, 1);
            Img_BossHp2.transform.localScale = new Vector3(percent*barRate, 1, 1);
            // 15֡û���������Ѫ����Ч�� �����ٶ�Ϊÿ���30%
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

            // ����ʣ��Ѫ���ٷֱ���ʾ
            Tex_LifePercent.text = (mUnit.mCurrentHp* barRate).ToString("#0") + " / " + mUnit.mMaxHp.ToString("#0") + "       " + (Mathf.Max(TargetUnit.GetHeathPercent()* barRate, 0) * 100).ToString("#0.00") + "%";
            if (mUnit.mCurrentHp <= 0)
            {
                // ûѪ��ʱ�����ؾ�����
                Img_BossHp1.SetActive(false);
                Img_BossHp2.SetActive(false);
                Img_BossVirtualHp1.SetActive(false);
                Img_BossVirtualHp2.SetActive(false);
            }
        }
        else
        {
            // ûѪ��ʱ�����ؾ�����
            Img_BossHp1.SetActive(false);
            Img_BossHp2.SetActive(false);
            Img_BossVirtualHp1.SetActive(false);
            Img_BossVirtualHp2.SetActive(false);
            // ����ʣ��Ѫ���ٷֱ���ʾ
            Tex_LifePercent.text = "";
            mUnit = null;
        }
    }

    /// <summary>
    /// �Ƿ���Ŀ��
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
    /// ����Ŀ��ͷ������ͼ
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
    /// ��������
    /// </summary>
    /// <param name="name"></param>
    public void SetName(string name)
    {
        Tex_BossName.text = name;
    }

    /// <summary>
    /// ����Ϊ�Զ�����
    /// </summary>
    public void AutoDestoryEnable()
    {
        isAutoDestory = true;
    }
}
