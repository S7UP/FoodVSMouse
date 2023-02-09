using UnityEngine.UI;
using UnityEngine;
/// <summary>
/// ÿ��Ҫ����ʱ������������
/// </summary>
public class StartLoadPanel : MonoBehaviour
{
    private float fakeProgress; // ��ٵļ��ؽ���ֵ(ȡֵ0~90)
    private float realProgress; // ��ʵ�ļ��ؽ���ֵ(ȡֵ0~10)
    private GameObject Go_Tips; // С��ʿ
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
    /// ��ʾһ��tips(�����Appear()֮��
    /// </summary>
    /// <param name="s"></param>
    public void ShowTips(string s)
    {
        Go_Tips.gameObject.SetActive(true);
        Tex_Tips.text = s;
    }


    /// <summary>
    /// ����
    /// </summary>
    public void Appear()
    {
        fakeProgress = 0;
        realProgress = 0;
        gameObject.SetActive(true);
        Go_Tips.gameObject.SetActive(false);
    }

    /// <summary>
    /// ��ʧ
    /// </summary>
    public void Disappear()
    {
        gameObject.SetActive(false);
        Go_Tips.gameObject.SetActive(false);
    }

    /// <summary>
    /// ÿ�����һ֡����ٵĽ��Ⱦ�+1
    /// </summary>
    public void AddFakeProgress()
    {
        fakeProgress = Mathf.Min(fakeProgress + 2, 90);
    }

    /// <summary>
    /// ������ʵ�ļ��ؽ���ֵ
    /// </summary>
    /// <param name="percent">ȡֵ0~1</param>
    public void SetRealProgress(float percent)
    {
        realProgress = percent * 10;
    }

    /// <summary>
    /// �������Ƿ�������
    /// </summary>
    /// <returns></returns>
    public bool IsFinishProgress()
    {
        return (realProgress + fakeProgress) >= 100;
    }
}
