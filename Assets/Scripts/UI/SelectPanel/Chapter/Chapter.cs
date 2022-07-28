using UnityEngine;
/// <summary>
/// 章节（大地图）
/// </summary>
public class Chapter : MonoBehaviour
{
    // 不需要初始化，由创建时自动或外界给出的引用
    private SelectPanel mSelectPanel; // 选关面板
    public ChapterType chapterType; // 章节类型
    // 变量
    public int selectedSceneIndex = -1; // 当前选择关卡下标，-1为未选择 

    public void Initial()
    {
        selectedSceneIndex = -1;

    }

    /// <summary>
    /// 进入本章节某个场景
    /// </summary>
    /// <param name="index">场景编号</param>
    public void EnterScene(int index)
    {
        selectedSceneIndex = index;
        mSelectPanel.LoadcurrentSceneStageList();
        // 通知选关面板显示场景UI
        mSelectPanel.EnterSelectStageUIAndStageInfoUI();
    }

    ////////////////////////////////////////////////////////////////////////以下方法仅供生成本实例的SelectPanel调用//////////////////////////////
    /// <summary>
    /// 设置宿主面板
    /// </summary>
    /// <param name="panel"></param>
    public void SetInfo(SelectPanel panel, ChapterType type)
    {
        mSelectPanel = panel;
        chapterType = type;
    }
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
}
