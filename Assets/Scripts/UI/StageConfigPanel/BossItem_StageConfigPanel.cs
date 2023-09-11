using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
/// <summary>
/// 关卡情报面板左侧Boss图标按钮
/// </summary>
public class BossItem_StageConfigPanel : MonoBehaviour
{
    private Button Btn;
    private Image Img_Display;
    public BaseEnemyGroup enemyGroupInfo;

    public void Awake()
    {
        Btn = GetComponent<Button>();
        Img_Display = transform.Find("Display").GetComponent<Image>();
    }

    public void Initial()
    {
        enemyGroupInfo = null;
        Img_Display.sprite = null;
        Btn.onClick.RemoveAllListeners();
    }

    public void SetParam(BaseEnemyGroup enemyGroupInfo, UnityAction call)
    {
        this.enemyGroupInfo = enemyGroupInfo;
        Img_Display.sprite = GameManager.Instance.GetSprite("Boss/" + enemyGroupInfo.mEnemyInfo.type + "/" + enemyGroupInfo.mEnemyInfo.shape + "/icon");
        Btn.onClick.AddListener(call);
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "StageConfigPanel/BossItem", gameObject);
    }

    public static BossItem_StageConfigPanel GetInstance(BaseEnemyGroup enemyGroupInfo, UnityAction call)
    {
        BossItem_StageConfigPanel item = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "StageConfigPanel/BossItem").GetComponent<BossItem_StageConfigPanel>();
        item.Initial();
        item.SetParam(enemyGroupInfo, call);
        return item;
    }
}
