using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ʮ�������ĵ���ҹ��
/// </summary>
public class Map_ThirteenIncenseCenterIsland2 : ChapterMap
{
    private static Sprite[] SpriteArray;
    private static Sprite[][] BlockSprArray; // ��ͨ��ˮ���ҽ�  ����������
    private SpriteRenderer renderer0;
    private SpriteRenderer renderer1;

    private MovementGridGroup[] gridGroupArray; // ����������
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
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 1; i < 6; i++)
            for (int j = 2; j < 7; j++)
                CreateAndAddGrid(j, i);

        // �ĸ���
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
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        // �ƶ����
        Transform trans = GameController.Instance.mMapController.transform;
        {
            // ��������
            Vector2[,] v2List = new Vector2[,]
            {
            { new Vector2(2, 1), new Vector2(3, 1), new Vector2(4, 1), new Vector2(5, 1), new Vector2(5, 2), new Vector2(5, 3)},
            { new Vector2(6, 1), new Vector2(6, 2), new Vector2(6, 3), new Vector2(6, 4), new Vector2(5, 4), new Vector2(4, 4)},
            { new Vector2(6, 5), new Vector2(5, 5), new Vector2(4, 5), new Vector2(3, 5), new Vector2(3, 4), new Vector2(3, 3)},
            { new Vector2(2, 5), new Vector2(2, 4), new Vector2(2, 3), new Vector2(2, 2), new Vector2(3, 2), new Vector2(4, 2)}
            };

            // �յ�����
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
                // ��
                new Vector2[][]{ 
                    new Vector2[] { new Vector2(1, 1), new Vector2(0.5f, 2.5f) },
                    new Vector2[] { new Vector2(-1, 2), new Vector2(2.5f, 0.5f) },
                },
                // ��
                new Vector2[][]{
                    new Vector2[] { new Vector2(1, -1), new Vector2(2.5f, 0.5f) },
                    new Vector2[] { new Vector2(2, 1), new Vector2(0.5f, 2.5f) },
                },
                // ��
                new Vector2[][]{
                    new Vector2[] { new Vector2(-1, -1), new Vector2(0.5f, 2.5f) },
                    new Vector2[] { new Vector2(1, -2), new Vector2(2.5f, 0.5f) },
                },
                // ��
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
                // �������ĵ�����
                gridGroup.transform.position = mapCenter;
                // �����Ӧ�ĸ���
                for (int j = 0; j < 6; j++)
                {
                    gridGroup.Add(GetGrid(((int)v2List[i, j].x), ((int)v2List[i, j].y)));
                }
                // ���ò������ƶ����
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

                // ���ý�����ļ����
                for (int j = 0; j < 2; j++)
                {
                    Vector3 dPos = new Vector3(checkAreaList[i][j][0].x * MapManager.gridWidth, checkAreaList[i][j][0].y * MapManager.gridHeight);
                    Vector3 size = new Vector3(checkAreaList[i][j][1].x * MapManager.gridWidth, checkAreaList[i][j][1].y*MapManager.gridHeight);
                    // ��������ж�
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

                        // ��������
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

                    // ˮ�������ж�
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

                    // �ҽ��������ж�
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

        // �ĸ�����
        {
            // ��������
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
                // ��ȡ�м��
                if (i == 4)
                    gridGroupArray[4] = gridGroup;
                gridGroup.spriteRenderer0.sprite = spriteList[i];
                gridGroup.spriteRenderer1.sprite = spriteList[i];
                // �������ĵ�����
                gridGroup.transform.position = MapManager.GetGridLocalPosition(v2List[i].x, v2List[i].y);
                // �����Ӧ�ĸ���
                gridGroup.Add(GetGrid(((int)v2List[i].x), ((int)v2List[i].y)));
                gridGroupList.Add(gridGroup);
                gridGroup.transform.SetParent(transform);

                // ˮ�������ж�
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

                // �ҽ��������ж�
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
    /// �Ը��ӽ��мӹ�
    /// </summary>
    public override void ProcessingGridList()
    {

    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }

    public override void OtherProcessing()
    {
        checkArea = null;
        string key = "Change";
        GameController.Instance.mCurrentStage.AddParamChangeAction(key, ChangeAction);
        // ˽�в�����ʼ��
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
    /// ��ˮת½���������仯ʱ
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
            // �л���ͼ����
            renderer1.sprite = renderer0.sprite;
            renderer1.color = new Color(1, 1, 1, 1);
            renderer0.sprite = SpriteArray[newValue];
            SetArea(newValue);
            // ��齥��
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
    /// �����ж�����
    /// </summary>
    /// <param name="index">0½�أ�1ˮ��2�ҽ�</param>
    private void SetArea(int index)
    {
        if(checkArea != null)
        {
            checkArea.MDestory();
            checkArea = null;
        }

        Transform trans = GameController.Instance.mMapController.transform;
        // ���� ����һ����һ�е�ˮ�����ҽ�
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
