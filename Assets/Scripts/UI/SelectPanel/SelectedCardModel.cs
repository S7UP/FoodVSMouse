using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
/// <summary>
/// ѡ���������ѱ�ѡ�еĿ�Ƭģ��
/// </summary>
public class SelectedCardModel : MonoBehaviour
{
    private RectTransform Emp_Card_RectTrans;
    private Text Tex_Cost;
    private Button Btn_Cancel;
    private Dropdown Dro_Rank;
    private Dropdown Dro_Shape;
    private Image mImage;

    private AvailableCardInfo mAvailableCardInfo;

    private void Awake()
    {
        Emp_Card_RectTrans = transform.Find("Emp_Card").GetComponent<RectTransform>();
        Tex_Cost = Emp_Card_RectTrans.Find("Tex_Cost").GetComponent<Text>();
        Btn_Cancel = Emp_Card_RectTrans.Find("Btn_Cancel").GetComponent<Button>();
        Dro_Rank = Emp_Card_RectTrans.Find("Dro_Rank").GetComponent<Dropdown>();
        Dro_Shape = Emp_Card_RectTrans.Find("Dro_Shape").GetComponent<Dropdown>();
        mImage = Emp_Card_RectTrans.GetComponent<Image>();
    }

    public void SetAvailableCardInfo(AvailableCardInfo info)
    {
        mAvailableCardInfo = info;
        // ����rank�����б�
        Dro_Rank.ClearOptions();
        List<Dropdown.OptionData> dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= info.maxLevel; i++)
        {
            dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("UI/Rank2/"+i)));
        }
        Dro_Rank.AddOptions(dataList);
        Dro_Rank.value = info.maxLevel;
        Dro_Rank.onValueChanged.AddListener(delegate { mAvailableCardInfo.maxLevel = Dro_Rank.value; });
        // ����shape�����б�
        Dro_Shape.ClearOptions();
        dataList = new List<Dropdown.OptionData>();
        for (int i = 0; i <= info.maxShape; i++)
        {
            dataList.Add(new Dropdown.OptionData(GameManager.Instance.GetSprite("Food/"+info.type+"/" + i + "/display")));
        }
        Dro_Shape.AddOptions(dataList);
        Dro_Shape.value = info.maxShape;
        Dro_Shape.onValueChanged.AddListener(delegate { mAvailableCardInfo.maxShape = Dro_Shape.value; });
        // ���÷����ı�
        Tex_Cost.text = GameManager.Instance.attributeManager.GetCardBuilderAttribute(info.type, Dro_Shape.value).GetCost(info.maxLevel)+"";
    }

    /// <summary>
    /// ���ű�ѡ�ж���
    /// </summary>
    public void PlaySelectedTween(Vector3 startPosition)
    {
        Hide();
        Tween t = Emp_Card_RectTrans.DOMove(startPosition, 0.05f);
        t.onComplete = delegate { Show();  Emp_Card_RectTrans.DOLocalMove(Vector3.zero, 0.25f).SetEase(Ease.OutCubic); };
    }

    /// <summary>
    /// ����ȡ��ѡ�񶯻�
    /// </summary>
    /// <param name="endPosition"></param>
    public void PlayUnSelectedTween(Vector3 endPosition, Action EndEvent)
    {
        Tween t = Emp_Card_RectTrans.DOMove(endPosition, 0.25f);
        t.SetEase(Ease.OutCubic);
        t.onComplete = delegate { EndEvent(); };
    }

    /// <summary>
    /// ��ȡ��ǰ��Ƭ��Ϣ
    /// </summary>
    public AvailableCardInfo GetInfo()
    {
        return mAvailableCardInfo;
    }

    /// <summary>
    /// ����һ��ʵ��
    /// </summary>
    /// <returns></returns>
    public static SelectedCardModel CreateInstance()
    {
        return GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "SelectPanel/SelectedCardModel").GetComponent<SelectedCardModel>();
    }

    /// <summary>
    /// ���յ�ǰʵ��
    /// </summary>
    public void ExecuteRecycle()
    {
        Btn_Cancel.onClick.RemoveAllListeners();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "SelectPanel/SelectedCardModel", gameObject);
    }

    /// <summary>
    /// Ϊȡ����ť��Ӽ���
    /// </summary>
    public void AddListenerToCancelButton(UnityEngine.Events.UnityAction call)
    {
        Btn_Cancel.onClick.AddListener(call);
    }

    /// <summary>
    /// ����
    /// </summary>
    private void Hide()
    {
        Tex_Cost.enabled = false;
        Btn_Cancel.GetComponent<Image>().enabled = false;
        Dro_Rank.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        Dro_Shape.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 0);
        mImage.enabled = false;
    }

    /// <summary>
    /// ��ʾ
    /// </summary>
    private void Show()
    {
        Tex_Cost.enabled = true;
        Btn_Cancel.GetComponent<Image>().enabled = true;
        Dro_Rank.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        Dro_Shape.transform.Find("Display").GetComponent<Image>().color = new Color(1, 1, 1, 1);
        mImage.enabled = true;
    }
}
