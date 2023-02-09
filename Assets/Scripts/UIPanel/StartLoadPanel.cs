using UnityEngine.UI;
using UnityEngine;
/// <summary>
/// 每次要加载时用这个场景面板
/// </summary>
public class StartLoadPanel : MonoBehaviour
{
    private float fakeProgress; // 虚假的加载进度值(取值0~90)
    private float realProgress; // 真实的加载进度值(取值0~10)
    private GameObject Go_Tips; // 小贴士
    private Text Tex_Tips;

    protected void Awake()
    {
        Go_Tips = transform.Find("Img_Tips").gameObject;
        Go_Tips.gameObject.SetActive(false);
        Tex_Tips = Go_Tips.transform.Find("Text").GetComponent<Text>();
        // base.Awake();
        Invoke("LoadNextScene", 5);
        // LoadNextScene();
    }

    private void LoadNextScene()
    {
        //GameManager.Instance.uiManager.mUIFacade.ChangeSceneState(new MainSceneState(GameManager.Instance.uiManager.mUIFacade));
        GameManager.Instance.EnterMainScene();
    }

    /// <summary>
    /// 显示一段tips(请放在Appear()之后）
    /// </summary>
    /// <param name="s"></param>
    public void ShowTips(string s)
    {
        Go_Tips.gameObject.SetActive(true);
        Tex_Tips.text = s;
    }


    /// <summary>
    /// 出现
    /// </summary>
    public void Appear()
    {
        fakeProgress = 0;
        realProgress = 0;
        gameObject.SetActive(true);
        Go_Tips.gameObject.SetActive(false);
    }

    /// <summary>
    /// 消失
    /// </summary>
    public void Disappear()
    {
        gameObject.SetActive(false);
        Go_Tips.gameObject.SetActive(false);
    }

    /// <summary>
    /// 每多加载一帧，虚假的进度就+1
    /// </summary>
    public void AddFakeProgress()
    {
        fakeProgress = Mathf.Min(fakeProgress + 2, 90);
    }

    /// <summary>
    /// 设置真实的加载进度值
    /// </summary>
    /// <param name="percent">取值0~1</param>
    public void SetRealProgress(float percent)
    {
        realProgress = percent * 10;
    }

    /// <summary>
    /// 进度条是否走完了
    /// </summary>
    /// <returns></returns>
    public bool IsFinishProgress()
    {
        return (realProgress + fakeProgress) >= 100;
    }
}
