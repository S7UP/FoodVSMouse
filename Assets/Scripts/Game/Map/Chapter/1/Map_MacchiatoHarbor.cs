using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// ������
/// </summary>
public class Map_MacchiatoHarbor : ChapterMap
{
    private Dictionary<Vector2, TeleportGridType> tpDict = new Dictionary<Vector2, TeleportGridType>();
    private Dictionary<Vector2, FogAreaEffectExecution> fogDict = new Dictionary<Vector2, FogAreaEffectExecution>();


    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {

    }

    /// <summary>
    /// �Ը��ӽ��мӹ�
    /// </summary>
    public override void ProcessingGridList()
    {
        tpDict.Clear();
        fogDict.Clear();
        // �綴
        {
            List<Vector2> list = new List<Vector2>()
            {
                new Vector2(8, 1), new Vector2(8, 3), new Vector2(8, 5),
                new Vector2(6, 0), new Vector2(6, 2), new Vector2(6, 4), new Vector2(6, 6),
                new Vector2(4, 1), new Vector2(4, 3), new Vector2(4, 5),
            };

            foreach (var v in list)
            {
                TeleportGridType tg = BaseGridType.GetInstance(GridType.Teleport, 0).GetComponent<TeleportGridType>();
                GetGrid((int)v.x, (int)v.y).AddGridType(GridType.Teleport, tg);
                tg.SetOpen(true);
                tpDict.Add(v, tg);
            }
        }

        // ��
        {
            List<Vector2> list = new List<Vector2>()
            {
                new Vector2(8, 0), new Vector2(8, 2), new Vector2(8, 4), new Vector2(8, 6),
                new Vector2(7, 0), new Vector2(7, 2), new Vector2(7, 4), new Vector2(7, 6),
                new Vector2(5, 1), new Vector2(5, 3), new Vector2(5, 5),
            };

            foreach (var v in list)
            {
                FogAreaEffectExecution e = FogAreaEffectExecution.GetInstance(new Vector2(MapManager.GetColumnX(v.x), MapManager.GetRowY(v.y)));
                e.SetOpen();
                GameController.Instance.AddAreaEffectExecution(e);
                fogDict.Add(v, e);
            }
        }
    }

    /// <summary>
    /// �Ը�����ӹ�
    /// </summary>
    public override void ProcessingGridGroupList()
    {

    }

    public override void OtherProcessing()
    {
        CloseAll();
        string key = "Change";
        GameController.Instance.mCurrentStage.AddParamChangeAction(key, ChangeAction);
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
            oldValue = Mathf.Min(6, Mathf.Max(0, Mathf.FloorToInt(oldArray[0])));
        }
        newValue = Mathf.Min(6, Mathf.Max(0, Mathf.FloorToInt(newArray[0])));
        if (oldValue != newValue)
        {
            CloseAll();
            // �򿪲��ֵ���
            Open(newValue);
        }
    }

    private void CloseAll()
    {
        // �綴
        {
            foreach (var keyValuePair in tpDict)
            {
                TeleportGridType tg = keyValuePair.Value;
                tg.SetOpen(false);
            }
        }

        // ��
        {
            foreach (var keyValuePair in fogDict)
            {
                FogAreaEffectExecution e = keyValuePair.Value;
                e.SetClose();
            }
        }
    }

    private void OpenAll()
    {
        // �綴
        {
            foreach (var keyValuePair in tpDict)
            {
                TeleportGridType tg = keyValuePair.Value;
                tg.SetOpen(true);
            }
        }

        // ��
        {
            foreach (var keyValuePair in fogDict)
            {
                FogAreaEffectExecution e = keyValuePair.Value;
                e.SetOpen();
            }
        }
    }

    private void Open(int mapIndex)
    {
        // ��������Ҫ�����ĸ���
        switch (mapIndex)
        {
            case 0:break;
            case 1:
                // �綴
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(8, 1), new Vector2(8, 3), new Vector2(8, 5),
                    };

                    foreach (var v in list)
                    {
                        TeleportGridType tg = tpDict[v];
                        tg.SetOpen(true);
                    }
                }
                break;
            case 2:
                // �綴
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(8, 1), new Vector2(8, 3), new Vector2(8, 5),
                        new Vector2(4, 1), new Vector2(4, 3), new Vector2(4, 5),
                    };

                    foreach (var v in list)
                    {
                        TeleportGridType tg = tpDict[v];
                        tg.SetOpen(true);
                    }
                }
                // ��
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(5, 1), new Vector2(5, 3), new Vector2(5, 5),
                    };

                    foreach (var v in list)
                    {
                        FogAreaEffectExecution e = fogDict[v];
                        e.SetOpen();
                    }
                }
                break;
            case 3:
                // �綴
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(6, 0), new Vector2(6, 2), new Vector2(6, 4), new Vector2(6, 6),
                    };

                    foreach (var v in list)
                    {
                        TeleportGridType tg = tpDict[v];
                        tg.SetOpen(true);
                    }
                }
                // ��
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(8, 0), new Vector2(8, 2), new Vector2(8, 4), new Vector2(8, 6),
                        new Vector2(7, 0), new Vector2(7, 2), new Vector2(7, 4), new Vector2(7, 6),
                    };

                    foreach (var v in list)
                    {
                        FogAreaEffectExecution e = fogDict[v];
                        e.SetOpen();
                    }
                }
                break;
            case 4:
                // �綴
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(8, 1), new Vector2(8, 3), new Vector2(8, 5),
                        new Vector2(6, 0), new Vector2(6, 2), new Vector2(6, 4), new Vector2(6, 6),
                    };

                    foreach (var v in list)
                    {
                        TeleportGridType tg = tpDict[v];
                        tg.SetOpen(true);
                    }
                }
                // ��
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(8, 0), new Vector2(8, 2), new Vector2(8, 4), new Vector2(8, 6),
                        new Vector2(7, 0), new Vector2(7, 2), new Vector2(7, 4), new Vector2(7, 6),
                    };

                    foreach (var v in list)
                    {
                        FogAreaEffectExecution e = fogDict[v];
                        e.SetOpen();
                    }
                }
                break;
            case 5:
                OpenAll();
                break;
            case 6:
                Debug.Log("abc");
                // �綴
                {
                    List<Vector2> list = new List<Vector2>()
                    {
                        new Vector2(8, 1), new Vector2(8, 3), new Vector2(8, 5),
                        new Vector2(6, 2), new Vector2(6, 4)
                    };

                    foreach (var v in list)
                    {
                        TeleportGridType tg = tpDict[v];
                        tg.SetOpen(true);
                    }
                }
                break;
            default:
                break;
        }
    }
}
