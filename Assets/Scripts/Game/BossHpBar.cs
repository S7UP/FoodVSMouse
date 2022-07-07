
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
            SetTarget(value);
        }
    }
    private BaseUnit mUnit; // Ŀ�����
    protected bool isAutoDestory; // Ŀ�������ʧʱ�Ƿ��Զ����٣����أ�

    private GameObject Img_BossIconDisplayer;
    private GameObject Img_BossVirtualHp1;
    private GameObject Emp_BossHp;
    private GameObject Img_BossHp1;
    private GameObject Img_BossVirtualHp2;
    private GameObject Img_BossHp2;
    private GameObject Img_BossHpBar1;
    private GameObject Img_BossHpBar2;
    private GameObject Img_BossHpBar3;
    private GameObject Img_BossName;
    private GameObject Emp_BossLifeLeft;

    private Sprite[] numberSpriteList; // ������ͼ
    private Sprite[] Spr_BossHpBar; // Ѫ����ɫ��ͼ

    // ����
    private int mTotalBarNumber; // һ���м���Ѫ
    private int mCurrentBarNumber; // ��ǰ�м���Ѫ
    private float mHpPerBar; // ÿ��Ѫ�����������ֵ

    private float mVirtualHpBarPercent1; // �²���Ѫ�ٷֱ�
    private float mVirtualHpBarPercent2; // �ϲ���Ѫ�ٷֱ�

    private void Awake()
    {
        Img_BossIconDisplayer = transform.Find("Img_BossIconDisplayer").gameObject;
        Emp_BossHp = transform.Find("Emp_BossHp").gameObject;
        Img_BossVirtualHp1 = Emp_BossHp.transform.Find("Img_BossVirtualHp1").gameObject;
        Img_BossHp1 = Emp_BossHp.transform.Find("Img_BossHp1").gameObject;
        Img_BossVirtualHp2 = Emp_BossHp.transform.Find("Img_BossVirtualHp2").gameObject;
        Img_BossHp2 = Emp_BossHp.transform.Find("Img_BossHp2").gameObject;
        Img_BossHpBar1 = transform.Find("Img_BossHpBar1").gameObject;
        Img_BossHpBar2 = transform.Find("Img_BossHpBar2").gameObject;
        Img_BossHpBar3 = transform.Find("Img_BossHpBar3").gameObject;
        Img_BossName = transform.Find("Img_BossName").gameObject;
        Emp_BossLifeLeft = transform.Find("Emp_BossLifeLeft").gameObject;

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
        mTotalBarNumber = 0; // һ���м���Ѫ
        mCurrentBarNumber = 0; // ��ǰ�м���Ѫ
        mHpPerBar = 0; // ÿ��Ѫ�����������ֵ

        mVirtualHpBarPercent1 = 0; // �²���Ѫ�ٷֱ�
        mVirtualHpBarPercent2 = 0; // �ϲ���Ѫ�ٷֱ�
    }

    /// <summary>
    /// ����Ŀ��
    /// </summary>
    public void SetTarget(BaseUnit unit)
    {
        Debug.Log("use SetTarget()!");
        if (unit == null || !unit.IsValid())
        {
            Debug.LogWarning("��������Ч��Ŀ�꣡");
            return;
        }
        mUnit = unit;
        // ����Ŀ����������ֵ������Ѫ������
        CalculateBarNumber();
    }

    /// <summary>
    /// ����Ŀ����������ֵ������Ѫ������
    /// </summary>
    private void CalculateBarNumber()
    {
        // Ŀ���������ֵС�ڵ���1��ʱֻ����һ��Ѫ
        if (mUnit.mMaxHp <= 10000.0)
        {
            mTotalBarNumber = 1;
        }
        else
        {
            // �������� Ŀ�� (�������ֵ/10000)��ֵ����ȡ����Ѫ
            mTotalBarNumber = Mathf.CeilToInt(mUnit.mMaxHp / 10000);
        }
        // ����ÿ��Ѫ�����ʵ������ֵ
        mHpPerBar = mUnit.mMaxHp / mTotalBarNumber;
    }

    /// <summary>
    /// ���㵱ǰʣ��Ѫ����������ȡ���������һ��Ѫ���ص�ֵΪ1
    /// </summary>
    private int CalculateCurrentBarNum()
    {
        return Mathf.CeilToInt(mUnit.mCurrentHp / mHpPerBar);
    }

    /// <summary>
    /// ���㵱ǰѪ��ʣ��ٷֱ�
    /// </summary>
    /// <returns></returns>
    private float CalculateCurrentBarProgress()
    {
        float currentBarHpLeft = mUnit.mCurrentHp - mHpPerBar * (CalculateCurrentBarNum() - 1);
        return currentBarHpLeft / mHpPerBar;
    }

    /// <summary>
    /// ���ݵ�ǰѪ������������Ѫ����ͼ
    /// </summary>
    private void UpdateHpBarSprite()
    {
        int currentBarNum = CalculateCurrentBarNum();
        int index;
        // �ϲ�ʵѪ������ɫ�±�Ϊ:(Ѫ������-1)%7
        index = (currentBarNum - 1) % 7;
        Img_BossHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // �²�ʵѪ������ɫ�±�Ϊ:(Ѫ������-2)%7
        index = (currentBarNum - 2) % 7;
        Img_BossHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // �ϲ���Ѫ������ɫ�±�Ϊ:(Ѫ������)%7
        index = (currentBarNum) % 7;
        Img_BossVirtualHp2.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
        // �²���Ѫ������ɫ�±�Ϊ:(Ѫ������-1)%7�����ϲ�ʵѪ����ɫӦ��ͬ��
        index = (currentBarNum - 1) % 7;
        Img_BossVirtualHp1.GetComponent<Image>().sprite = Spr_BossHpBar[index < 0 ? index += 7 : index];
    }

    /// <summary>
    /// ����BOSS��Ѫ��
    /// </summary>
    public override void PUpdate()
    {
        base.PUpdate();
        if (mUnit != null && mUnit.IsValid())
        {
            // ����Ѫ��������ʾ
            int currentBarNum = CalculateCurrentBarNum();
            // ��ǰ����Ѫ������ �� ��¼��������ʱ
            if (mCurrentBarNumber != currentBarNum)
            {
                mCurrentBarNumber = currentBarNum;
                // ���¸�λ��������ʾ
                Emp_BossLifeLeft.transform.GetChild(2).GetComponent<Image>().sprite = numberSpriteList[currentBarNum % 10];
                // ����ʮλ��������ʾ
                Emp_BossLifeLeft.transform.GetChild(1).GetComponent<Image>().sprite = numberSpriteList[Mathf.FloorToInt((currentBarNum % 100) / 10)];
                // ���°�λ��������ʾ
                Emp_BossLifeLeft.transform.GetChild(0).GetComponent<Image>().sprite = numberSpriteList[Mathf.FloorToInt((currentBarNum % 1000) / 100)];

                if (currentBarNum == 1)
                {
                    // ��ǰ��ʣһ��Ѫʱ�������²�Ѫ��
                    Img_BossHp1.SetActive(false);
                }
                // ����Ѫ����ɫ
                UpdateHpBarSprite();
                // �ϲ���Ѫ�̳��²���Ѫ�ٷֱ�
                mVirtualHpBarPercent2 = mVirtualHpBarPercent1;
                // �²���Ѫ�ص���100%
                mVirtualHpBarPercent1 = 1.00f;
            }


            // ���µ�ǰѪ���ٷֱ���ʾ
            float percent = CalculateCurrentBarProgress();
            Img_BossHp2.transform.localScale = new Vector3(percent, 1, 1);
            // ��Ѫ����Ч�� �����ٶ�Ϊÿ���10%
            mVirtualHpBarPercent1 = Mathf.Max(percent, mVirtualHpBarPercent1 - (float)(0.10 / ConfigManager.fps));
            mVirtualHpBarPercent2 = Mathf.Max(0, mVirtualHpBarPercent2 - (float)(0.10 / ConfigManager.fps));
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
        }
    }

    /// <summary>
    /// ����Ϊ�Զ�����
    /// </summary>
    public void AutoDestoryEnable()
    {
        isAutoDestory = true;
    }
}
