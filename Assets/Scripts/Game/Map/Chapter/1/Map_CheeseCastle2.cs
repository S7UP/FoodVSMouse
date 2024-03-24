using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 芝士城堡（勇士）
/// </summary>
public class Map_CheeseCastle2 : ChapterMap
{
    private static Sprite[] SpriteArray;
    private SpriteRenderer renderer0;
    private SpriteRenderer renderer1;
    private const float deltaAlpha = 2f / 60;

    public void Awake()
    {
        if(SpriteArray == null)
        {
            SpriteArray = new Sprite[4];
            for (int i = 0; i < 3; i++)
                SpriteArray[i] = GameManager.Instance.GetSprite("Chapter/1/15/" + i);
        }
        renderer0 = transform.Find("0").GetComponent<SpriteRenderer>();
        renderer1 = transform.Find("1").GetComponent<SpriteRenderer>();
    }


    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {

    }

    /// <summary>
    /// 对格子进行加工
    /// </summary>
    public override void ProcessingGridList()
    {
        // 铺上水格子
        AddWater(0);
        // 铺上障碍
        BaseBarrier b = GameController.Instance.CreateItem(6, 1, (int)ItemNameTypeMap.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
        b.mEffectController.HideEffect(true);
        b = GameController.Instance.CreateItem(6, 5, (int)ItemNameTypeMap.Barrier, 0).GetComponent<BaseBarrier>();
        b.SetHide(true);
        b.mEffectController.HideEffect(true);
    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }

    /// <summary>
    /// 其他加工
    /// </summary>
    public override void OtherProcessing()
    {
        // 为全图添加黑夜BUFF
        //ShadeAreaEffectExecution e = ShadeAreaEffectExecution.GetInstance(11, 7, new UnityEngine.Vector2(MapManager.GetColumnX(4), MapManager.GetRowY(3)));
        //GameController.Instance.AddAreaEffectExecution(e);

        // 在当前关卡添加监听——若检测到水转陆参数变化则执行对应的逻辑
        string key = "Change";
        GameController.Instance.mCurrentStage.AddParamChangeAction(key, ChangeAction);
        // 私有参数初始化
        renderer0.sprite = SpriteArray[0];
        renderer1.sprite = renderer0.sprite;
        renderer1.color = new Color(1, 1, 1, 0);
    }

    /// <summary>
    /// 当水转陆参数发生变化时
    /// </summary>
    /// <param name="key"></param>
    /// <param name="oldArray"></param>
    /// <param name="newArray"></param>
    private void ChangeAction(string key, List<float> oldArray, List<float> newArray)
    {
        int oldValue;
        int newValue;
        if(oldArray == null)
        {
            oldValue = 0;
        }
        else
        {
            oldValue = Mathf.Min(3, Mathf.Max(0, Mathf.FloorToInt(oldArray[0])));
        }
        newValue = Mathf.Min(3, Mathf.Max(0, Mathf.FloorToInt(newArray[0])));
        if(oldValue != newValue)
        {
            // 切换地图渐变
            renderer1.sprite = renderer0.sprite;
            renderer1.color = new Color(1, 1, 1, 1);
            //renderer0.sprite = SpriteArray[newValue];
            renderer0.sprite = GameManager.Instance.GetSprite("Chapter/1/15/" + newValue);
            AddWater(newValue);
        }
    }

    public override void AfterUpdate()
    {
        renderer1.color = new Color(1, 1, 1, Mathf.Max(0, renderer1.color.a - deltaAlpha));
        if(renderer1.color.a <= 0)
        {
            renderer1.gameObject.SetActive(false);
        }else
            renderer1.gameObject.SetActive(true);
    }

    private void AddWater(int mapIndex)
    {
        List<Vector2> list = new List<Vector2>();
        // 分类讨论要保留的格子
        switch (mapIndex)
        {
            case 0:
                for (int i = 0; i < 7; i++)
                    for (int j = 0; j < 9; j++)
                    {
                        //BaseGrid g = GetGrid(j, i);
                        //g.AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
                        list.Add(new Vector2(j, i));
                    }
                break;
            case 1:
                // (2,1)
                {
                    //BaseGrid g = GetGrid(2, 1);
                    //g.AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
                    list.Add(new Vector2(2, 1));
                }
                // (3,1)到(3,4)
                for (int i = 1; i < 5; i++)
                {
                    //BaseGrid g = GetGrid(3, i);
                    //g.AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
                    list.Add(new Vector2(3, i));
                }
                // (4,1)到(8,5)的矩形区域
                for (int i = 1; i < 6; i++)
                    for (int j = 4; j < 9; j++)
                    {
                        //BaseGrid g = GetGrid(j, i);
                        //g.AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
                        list.Add(new Vector2(j, i));
                    }
                break;
            case 2:
                // (7,2)到(7,3)
                for (int i = 2; i < 4; i++)
                {
                    //BaseGrid g = GetGrid(7, i);
                    //g.AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
                    list.Add(new Vector2(7, i));
                }
                // (8,2)到(8,5)
                for (int i = 2; i < 6; i++)
                {
                    //BaseGrid g = GetGrid(8, i);
                    //g.AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
                    list.Add(new Vector2(8, i));
                }
                break;
            default:
                break;
        }
        // 移除或添加水格子
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 9; j++)
            {
                BaseGrid g = GetGrid(j, i);
                Vector2 v2 = new Vector2(j, i);
                if (list.Contains(v2))
                {
                    if (!g.IsContainGridType(GridType.Water))
                        g.AddGridType(GridType.Water, BaseGridType.GetInstance(GridType.Water, 0));
                }
                else
                {
                    if (g.IsContainGridType(GridType.Water))
                        g.RemoveGridType(GridType.Water);
                }
            }

    }
}
