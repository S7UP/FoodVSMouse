using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ��Ҷ�ոۣ��գ�
/// </summary>
public class Map_FragrantLeafAirport : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        //// 1357·
        //for (int i = 0; i < 9; i++)
        //{
        //    CreateAndAddGrid(i, 0);
        //    CreateAndAddGrid(i, 2);
        //    CreateAndAddGrid(i, 4);
        //    CreateAndAddGrid(i, 6);
        //}
        //// 2 6ͬ��
        //for (int i = 0; i < 4; i++)
        //{
        //    CreateAndAddGrid(i, 1);
        //    CreateAndAddGrid(i, 5);
        //}
        //// 4��2 6����
        //for (int i = 5; i < 9; i++)
        //{
        //    CreateAndAddGrid(i, 3);
        //}
        // 1357·
        for (int i = 0; i < 9; i++)
        {
            CreateAndAddGrid(i, 0);
            CreateAndAddGrid(i, 2);
            CreateAndAddGrid(i, 4);
            CreateAndAddGrid(i, 6);
        }
        // 2 6ͬ��
        for (int i = 0; i < 4; i++)
        {
            CreateAndAddGrid(i, 1);
            CreateAndAddGrid(i, 3);
            CreateAndAddGrid(i, 5);
        }
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        //List<BaseGrid>[] listArray = new List<BaseGrid>[3];
        //for (int i = 0; i < listArray.Length; i++)
        //{
        //    listArray[i] = new List<BaseGrid>();
        //}

        //// 2 6ͬ��
        //for (int i = 0; i < 4; i++)
        //{
        //    listArray[0].Add(GetGrid(i, 1));
        //    listArray[2].Add(GetGrid(i, 5));
        //}
        //// 4��2 6����
        //for (int i = 5; i < 9; i++)
        //{
        //    listArray[1].Add(GetGrid(i, 3));
        //}

        List<BaseGrid>[] listArray = new List<BaseGrid>[3];
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
        }

        // 2 6ͬ��
        for (int i = 0; i < 4; i++)
        {
            listArray[0].Add(GetGrid(i, 1));
            listArray[1].Add(GetGrid(i, 3));
            listArray[2].Add(GetGrid(i, 5));
        }

        //// ���ĵ�����
        //List<Vector2> centerList = new List<Vector2>()
        //{
        //    mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.up,
        //    mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
        //    mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.down,
        //};

        //// �յ�����
        //List<Vector2> endList = new List<Vector2>()
        //{
        //    mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.up,
        //    mapCenter + 2.5f * MapManager.gridWidth * Vector3.left,
        //    mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.down,
        //};

        // ���ĵ�����
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.left + 2f*MapManager.gridHeight * Vector3.down,
        };

        // �յ�����
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right,
            mapCenter + 2.5f * MapManager.gridWidth * Vector3.right + 2f*MapManager.gridHeight * Vector3.down,
        };

        for (int k = 0; k < 3; k++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // �������ĵ�����
            gridGroup.transform.position = centerList[k];
            // �����Ӧ�ĸ���
            foreach (var grid in listArray[k])
            {
                gridGroup.Add(grid);
            }
            // ���ò������ƶ����
            gridGroup.StartMovement(
                new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = centerList[k],
                        moveTime = 900,
                        strandedTime = 60
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 900,
                        strandedTime = 60
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + "1"),
                new Vector2(),
                false, false);
            gridGroupList.Add(gridGroup);
            gridGroup.transform.SetParent(transform);

            Transform trans = GameController.Instance.mMapController.transform;
            // ���ý�����ļ����
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position, new Vector2(3.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
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
                    r.transform.position = gridGroup.transform.position;
                    return !gridGroup.IsAlive();
                });
                t.AddOnExitAction(delegate {
                    r.MDestory();
                });
                r.taskController.AddTask(t);
            }

            // ������
            {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(gridGroup.transform.position, new Vector2(3.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight));
                r.name = "SkyVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);

                CustomizationTask t = new CustomizationTask();
                t.AddTaskFunc(delegate {
                    r.transform.position = gridGroup.transform.position;
                    return !gridGroup.IsAlive();
                });
                r.taskController.AddTask(t);
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
        Transform trans = GameController.Instance.mMapController.transform;
        // ��ӿ���
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetSkyArea(MapManager.GetGridLocalPosition(4, 3), new Vector2(7.5f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "SkyArea";
            GameController.Instance.AddAreaEffectExecution(r);
        }


        // ����Ʋ�(1357·)
        for (int i = 0; i < 7; i += 2)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 9);
        }
        // ����Ʋ�(246·)
        for (int i = 1; i < 7; i += 2)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(7), MapManager.GetRowY(i)), 1);
        }

        // �������
        {
            // 1357
            for (int i = 0; i <= 6; i+=2)
            {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(0.5f, i), new Vector2(1 * MapManager.gridWidth, 0.75f * MapManager.gridHeight));
                r.name = "SkyVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // 246
            for (int i = 1; i <= 5; i += 2)
            {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(-0.25f, i), new Vector2(0.5f * MapManager.gridWidth, 0.75f * MapManager.gridHeight));
                r.name = "SkyVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
        // �ҳ�����
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(8, 3), new Vector2(0.5f * MapManager.gridWidth, 6.5f * MapManager.gridHeight));
            r.name = "SkyVehicle(grid)";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ��ӷ���
        {
            int[] timeArray = new int[] { 240, 420, 240, 60, 240, 420, 240, 60 };
            float[] start_vArray = new float[] { 0, 1.6875f, 1.6875f, 0, 0, -1.6875f, -1.6875f, 0 };
            float[] end_vArray = new float[] { 1.6875f, 1.6875f, 0, 0, -1.6875f, -1.6875f, 0, 0 };

            List<WindAreaEffectExecution> wList = new List<WindAreaEffectExecution>();
            for (int i = 0; i < 7; i += 2)
            {
                WindAreaEffectExecution w = WindAreaEffectExecution.GetInstance(7.55f, 1, new Vector2(MapManager.GetColumnX(4.525f), MapManager.GetRowY(i)));
                for (int k = 0; k < timeArray.Length; k++)
                {
                    WindAreaEffectExecution.State s = w.GetState(k);
                    s.totoalTime = timeArray[k];
                    s.start_v = TransManager.TranToVelocity(start_vArray[k]);
                    s.end_v = TransManager.TranToVelocity(end_vArray[k]);
                }
                GameController.Instance.AddAreaEffectExecution(w);
                wList.Add(w);
            }

            GameController.Instance.mCurrentStage.AddParamChangeAction("wind_time", (key, oldArray, newArray) => {
                foreach (var w in wList)
                {
                    for (int i = 0; i < newArray.Count; i++)
                    {
                        WindAreaEffectExecution.State s = w.GetState(i);
                        s.totoalTime = (int)newArray[i];
                    }
                }
            });
            GameController.Instance.mCurrentStage.AddParamChangeAction("wind_start_v", (key, oldArray, newArray) => {
                foreach (var w in wList)
                {
                    for (int i = 0; i < newArray.Count; i++)
                    {
                        WindAreaEffectExecution.State s = w.GetState(i);
                        s.start_v = TransManager.TranToVelocity((int)newArray[i]);
                    }
                }
            });
            GameController.Instance.mCurrentStage.AddParamChangeAction("wind_end_v", (key, oldArray, newArray) => {
                foreach (var w in wList)
                {
                    for (int i = 0; i < newArray.Count; i++)
                    {
                        WindAreaEffectExecution.State s = w.GetState(i);
                        s.end_v = TransManager.TranToVelocity((int)newArray[i]);
                    }
                }
            });
            GameController.Instance.mCurrentStage.AddParamChangeAction("wind_add_dmg", (key, oldArray, newArray) => {
                foreach (var w in wList)
                {
                    if (newArray != null)
                        w.add_dmg_rate = newArray[0];
                }
            });
            GameController.Instance.mCurrentStage.AddParamChangeAction("wind_dec_dmg", (key, oldArray, newArray) => {
                foreach (var w in wList)
                {
                    if (newArray != null)
                        w.dec_dmg_rate = newArray[0];
                }
            });
        }

    }
}
