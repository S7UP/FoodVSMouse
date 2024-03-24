using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 十三香中心岛（夜）
/// </summary>
public class Map_ThirteenIncenseCenterIsland2 : ChapterMap
{
    private static Sprite[] SpriteArray;
    private static Sprite[][] BlockSprArray; // 普通、水、岩浆  上右下左中
    private SpriteRenderer renderer0;
    private SpriteRenderer renderer1;

    private MovementGridGroup[] gridGroupArray; // 上右下左中
    private const float deltaAlpha = 1f / 60;
    private RetangleAreaEffectExecution checkArea;

    public void Awake()
    {
        if (SpriteArray == null)
        {
            SpriteArray = new Sprite[3];
            SpriteArray[0] = GameManager.Instance.GetSprite("Chapter/2/16/0");
            SpriteArray[1] = GameManager.Instance.GetSprite("Chapter/2/16/1");
            SpriteArray[2] = GameManager.Instance.GetSprite("Chapter/2/16/2");

            BlockSprArray = new Sprite[3][];
            for (int i = 0; i < BlockSprArray.Length; i++)
            {
                Sprite[] s_a = new Sprite[5];
                s_a[0] = GameManager.Instance.GetSprite("Chapter/2/16/" + i + "/Up");
                s_a[1] = GameManager.Instance.GetSprite("Chapter/2/16/" + i + "/Right");
                s_a[2] = GameManager.Instance.GetSprite("Chapter/2/16/" + i + "/Down");
                s_a[3] = GameManager.Instance.GetSprite("Chapter/2/16/" + i + "/Left");
                s_a[4] = GameManager.Instance.GetSprite("Chapter/2/16/" + i + "/Grid");
                BlockSprArray[i] = s_a;
            }
        }
        renderer0 = transform.Find("0").GetComponent<SpriteRenderer>();
        renderer1 = transform.Find("1").GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 1; i < 6; i++)
            for (int j = 2; j < 7; j++)
                CreateAndAddGrid(j, i);

        // 四个角
        Vector2[] v2List = new Vector2[]
        {
            new Vector2(1, 0), new Vector2(7, 0), new Vector2(1, 6), new Vector2(7, 6)
        };

        for (int i = 0; i < 4; i++)
        {
            CreateAndAddGrid((int)v2List[i].x, (int)v2List[i].y);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        // 移动板块
        Transform trans = GameController.Instance.mMapController.transform;
        {
            // 格子坐标
            Vector2[,] v2List = new Vector2[,]
            {
            { new Vector2(2, 1), new Vector2(3, 1), new Vector2(4, 1), new Vector2(5, 1), new Vector2(5, 2), new Vector2(5, 3)},
            { new Vector2(6, 1), new Vector2(6, 2), new Vector2(6, 3), new Vector2(6, 4), new Vector2(5, 4), new Vector2(4, 4)},
            { new Vector2(6, 5), new Vector2(5, 5), new Vector2(4, 5), new Vector2(3, 5), new Vector2(3, 4), new Vector2(3, 3)},
            { new Vector2(2, 5), new Vector2(2, 4), new Vector2(2, 3), new Vector2(2, 2), new Vector2(3, 2), new Vector2(4, 2)}
            };

            // 终点坐标
            Vector2[] endPointList = new Vector2[]
            {
            mapCenter + new Vector3(0, gridHeight, 0),
            mapCenter + new Vector3(gridWidth, 0, 0),
            mapCenter + new Vector3(0, -gridHeight, 0),
            mapCenter + new Vector3(-gridWidth, 0, 0)
            };

            Vector2[] offsetList = new Vector2[]
            {
            new Vector2(-gridWidth/2, gridHeight),
            new Vector2(gridWidth, gridHeight/2),
            new Vector2(gridWidth/2, -gridHeight),
            new Vector2(-gridWidth, -gridHeight/2)
            };

            //if(BlockSprArray == null)
            //{

            //}

            Sprite[] spriteList = new Sprite[] {
                BlockSprArray[0][0],
                BlockSprArray[0][1],
                BlockSprArray[0][2],
                BlockSprArray[0][3],
            };

            Vector2[][][] checkAreaList = new Vector2[][][]
            {
                // 上
                new Vector2[][]{ 
                    new Vector2[] { new Vector2(1, 1), new Vector2(0.5f, 2.5f) },
                    new Vector2[] { new Vector2(-1, 2), new Vector2(2.5f, 0.5f) },
                },
                // 右
                new Vector2[][]{
                    new Vector2[] { new Vector2(1, -1), new Vector2(2.5f, 0.5f) },
                    new Vector2[] { new Vector2(2, 1), new Vector2(0.5f, 2.5f) },
                },
                // 下
                new Vector2[][]{
                    new Vector2[] { new Vector2(-1, -1), new Vector2(0.5f, 2.5f) },
                    new Vector2[] { new Vector2(1, -2), new Vector2(2.5f, 0.5f) },
                },
                // 左
                new Vector2[][]{
                    new Vector2[] { new Vector2(-1, 1), new Vector2(2.5f, 0.5f) },
                    new Vector2[] { new Vector2(-2, -1), new Vector2(0.5f, 2.5f) },
                },
            };

            gridGroupArray = new MovementGridGroup[5];
            for (int i = 0; i < 4; i++)
            {
                MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
                gridGroupArray[i] = gridGroup;
                // 设置中心点坐标
                gridGroup.transform.position = mapCenter;
                // 加入对应的格子
                for (int j = 0; j < 6; j++)
                {
                    gridGroup.Add(GetGrid(((int)v2List[i, j].x), ((int)v2List[i, j].y)));
                }
                // 设置并启用移动版块
                gridGroup.StartMovement(
                    new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter,
                        moveTime = 180,
                        strandedTime = 720
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endPointList[i],
                        moveTime = 180,
                        strandedTime = 720
                    }
                    },
                    spriteList[i],
                    offsetList[i],
                    false, false);
                gridGroupList.Add(gridGroup);
                gridGroup.transform.SetParent(transform);

                // 设置进入板块的检测域
                for (int j = 0; j < 2; j++)
                {
                    Vector3 dPos = new Vector3(checkAreaList[i][j][0].x * MapManager.gridWidth, checkAreaList[i][j][0].y * MapManager.gridHeight);
                    Vector3 size = new Vector3(checkAreaList[i][j][1].x * MapManager.gridWidth, checkAreaList[i][j][1].y*MapManager.gridHeight);
                    // 板块检测域判定
                    {
                        RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position + dPos, size, "ItemCollideEnemy");
                        r.name = "GridGroupEnterCheckArea";
                        r.transform.SetParent(trans);
                        r.isAffectMouse = true;
                        r.SetAffectHeight(0);
                        r.SetOnEnemyEnterAction((u) => {
                            gridGroup.TryEnter(u);
                        });
                        r.SetOnEnemyExitAction((u) => {
                            gridGroup.TryExit(u);
                        });
                        GameController.Instance.AddAreaEffectExecution(r);

                        // 跟随任务
                        CustomizationTask t = new CustomizationTask();
                        t.AddTaskFunc(delegate {
                            r.transform.position = gridGroup.transform.position + dPos;
                            return !gridGroup.IsAlive();
                        });
                        t.AddOnExitAction(delegate {
                            r.MDestory();
                        });
                        r.taskController.AddTask(t);
                    }

                    // 水承载域判定
                    {
                        RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(gridGroup.transform.position + dPos, size, new S7P.Numeric.FloatModifier(0));
                        r.name = "WaterVehicle(gridGroup)";
                        r.transform.SetParent(trans);
                        GameController.Instance.AddAreaEffectExecution(r);

                        CustomizationTask t = new CustomizationTask();
                        t.AddTaskFunc(delegate
                        {
                            r.transform.position = gridGroup.transform.position + dPos;
                            return !gridGroup.IsAlive();
                        });
                        r.taskController.AddTask(t);
                    }

                    // 岩浆承载域判定
                    {
                        RetangleAreaEffectExecution r = Environment.LavaManager.GetVehicleArea(gridGroup.transform.position + dPos, size);
                        r.name = "LavaVehicle(gridGroup)";
                        r.transform.SetParent(trans);
                        GameController.Instance.AddAreaEffectExecution(r);

                        CustomizationTask t = new CustomizationTask();
                        t.AddTaskFunc(delegate
                        {
                            r.transform.position = gridGroup.transform.position + dPos;
                            return !gridGroup.IsAlive();
                        });
                        r.taskController.AddTask(t);
                    }
                }
            }
        }

        // 四个角落
        {
            // 格子坐标
            Vector2[] v2List = new Vector2[]
            {
                new Vector2(1, 0), new Vector2(7, 0), new Vector2(1, 6), new Vector2(7, 6), new Vector2(4, 3)
            };

            Sprite[] spriteList = new Sprite[] {
                GameManager.Instance.GetSprite(GetSpritePath() + "Grid2"),
                GameManager.Instance.GetSprite(GetSpritePath() + "Grid2"),
                GameManager.Instance.GetSprite(GetSpritePath() + "Grid2"),
                GameManager.Instance.GetSprite(GetSpritePath() + "Grid2"),
                BlockSprArray[0][4],
            };

            for (int i = 0; i < 5; i++)
            {
                MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
                // 获取中间格
                if (i == 4)
                    gridGroupArray[4] = gridGroup;
                gridGroup.spriteRenderer0.sprite = spriteList[i];
                gridGroup.spriteRenderer1.sprite = spriteList[i];
                // 设置中心点坐标
                gridGroup.transform.position = MapManager.GetGridLocalPosition(v2List[i].x, v2List[i].y);
                // 加入对应的格子
                gridGroup.Add(GetGrid(((int)v2List[i].x), ((int)v2List[i].y)));
                gridGroupList.Add(gridGroup);
                gridGroup.transform.SetParent(transform);

                // 水承载域判定
                {
                    RetangleAreaEffectExecution r = Environment.WaterManager.GetVehicleArea(gridGroup.transform.position, new Vector2(0.5f*MapManager.gridWidth, 0.5f*MapManager.gridHeight), new S7P.Numeric.FloatModifier(0));
                    r.name = "WaterVehicle(gridGroup)";
                    r.transform.SetParent(trans);
                    GameController.Instance.AddAreaEffectExecution(r);

                    CustomizationTask t = new CustomizationTask();
                    t.AddTaskFunc(delegate
                    {
                        r.transform.position = gridGroup.transform.position;
                        return !gridGroup.IsAlive();
                    });
                    r.taskController.AddTask(t);
                }

                // 岩浆承载域判定
                {
                    RetangleAreaEffectExecution r = Environment.LavaManager.GetVehicleArea(gridGroup.transform.position, new Vector2(0.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight));
                    r.name = "LavaVehicle(gridGroup)";
                    r.transform.SetParent(trans);
                    GameController.Instance.AddAreaEffectExecution(r);

                    CustomizationTask t = new CustomizationTask();
                    t.AddTaskFunc(delegate
                    {
                        r.transform.position = gridGroup.transform.position;
                        return !gridGroup.IsAlive();
                    });
                    r.taskController.AddTask(t);
                }
            }


        }
    }

    /// <summary>
    /// 对格子进行加工
    /// </summary>
    public override void ProcessingGridList()
    {

    }

    /// <summary>
    /// 对格子组加工
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }

    public override void OtherProcessing()
    {
        checkArea = null;
        string key = "Change";
        GameController.Instance.mCurrentStage.AddParamChangeAction(key, ChangeAction);
        // 私有参数初始化
        renderer0.sprite = SpriteArray[0];
        renderer1.sprite = renderer0.sprite;
        renderer1.color = new Color(1, 1, 1, 0);
        SetArea(0);
    }

    public override void AfterUpdate()
    {
        renderer1.color = new Color(1, 1, 1, Mathf.Min(1, Mathf.Max(0, renderer1.color.a - deltaAlpha)));
        if (renderer1.color.a <= 0)
        {
            renderer1.gameObject.SetActive(false);
        }
        else
            renderer1.gameObject.SetActive(true);
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
        if (oldArray == null)
        {
            oldValue = 0;
        }
        else
        {
            oldValue = Mathf.Min(2, Mathf.Max(0, Mathf.FloorToInt(oldArray[0])));
        }
        newValue = Mathf.Min(2, Mathf.Max(0, Mathf.FloorToInt(newArray[0])));
        if (oldValue != newValue)
        {
            // 切换地图渐变
            renderer1.sprite = renderer0.sprite;
            renderer1.color = new Color(1, 1, 1, 1);
            renderer0.sprite = SpriteArray[newValue];
            SetArea(newValue);
            // 板块渐变
            for (int i = 0; i < gridGroupArray.Length; i++)
            {
                MovementGridGroup mg = gridGroupArray[i];
                mg.spriteRenderer1.sprite = mg.spriteRenderer0.sprite;
                mg.spriteRenderer1.color = new Color(1, 1, 1, 1);
                mg.spriteRenderer0.sprite = BlockSprArray[newValue][i];
            }
        }
    }

    /// <summary>
    /// 设置判定区域
    /// </summary>
    /// <param name="index">0陆地，1水，2岩浆</param>
    private void SetArea(int index)
    {
        if(checkArea != null)
        {
            checkArea.MDestory();
            checkArea = null;
        }

        Transform trans = GameController.Instance.mMapController.transform;
        // 创建 从左一到右一列的水或者岩浆
        if(index == 1){
            RetangleAreaEffectExecution r = Environment.WaterManager.GetWaterArea(MapManager.GetGridLocalPosition(4f, 3), new Vector2(7.75f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "WaterArea";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
            checkArea = r;
        }else if(index == 2)
        {
            RetangleAreaEffectExecution r = Environment.LavaManager.GetLavaArea(MapManager.GetGridLocalPosition(4f, 3), new Vector2(7.75f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "LavaArea";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
            checkArea = r;
        }
    }
}
