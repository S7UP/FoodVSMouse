using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �¹���գ�ҹ��
/// </summary>
public class Map_LaurelSky2 : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        //// ������
        //for (int i = 2; i < 5; i++)
        //    for (int j = 0; j < 9; j++)
        //        CreateAndAddGrid(j, i);
        //// �ϰ�
        //for (int i = 0; i < 2; i++)
        //{
        //    for (int j = 0; j < 5; j++)
        //        CreateAndAddGrid(j, i);
        //}
        //// �°�
        //for (int i = 5; i < 7; i++)
        //{
        //    for (int j = 4; j < 9; j++)
        //        CreateAndAddGrid(j, i);
        //}
        // ������
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
        // �ϰ�
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
        }
        // �°�
        for (int i = 5; i < 7; i++)
        {
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
        }
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        //List<BaseGrid>[] listArray = new List<BaseGrid>[2];
        //listArray[0] = new List<BaseGrid>();
        //// �ϰ�
        //for (int i = 0; i < 2; i++)
        //{
        //    for (int j = 0; j < 5; j++)
        //        listArray[0].Add(GetGrid(j, i));
        //}
        //// �°�
        //listArray[1] = new List<BaseGrid>();
        //for (int i = 5; i < 7; i++)
        //{
        //    for (int j = 4; j < 9; j++)
        //        listArray[1].Add(GetGrid(j, i));
        //}

        List<BaseGrid>[] listArray = new List<BaseGrid>[2];
        listArray[0] = new List<BaseGrid>();
        // �ϰ�
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 5; j++)
                listArray[0].Add(GetGrid(j, i));
        }
        // �°�
        listArray[1] = new List<BaseGrid>();
        for (int i = 5; i < 7; i++)
        {
            for (int j = 0; j < 5; j++)
                listArray[1].Add(GetGrid(j, i));
        }

        //// ���ĵ�����
        //List<Vector2> centerList = new List<Vector2>()
        //{
        //    mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.up,
        //    mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.down
        //};

        //// �յ�����
        //List<Vector2> endList = new List<Vector2>()
        //{
        //    mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.up,
        //    mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.down
        //};

        // ���ĵ�����
        List<Vector2> centerList = new List<Vector2>()
        {
            mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2f * MapManager.gridWidth * Vector3.left + 2.5f*MapManager.gridHeight * Vector3.down
        };

        // �յ�����
        List<Vector2> endList = new List<Vector2>()
        {
            mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.up,
            mapCenter + 2f * MapManager.gridWidth * Vector3.right + 2.5f*MapManager.gridHeight * Vector3.down
        };

        for (int k = 0; k < 2; k++)
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
                        moveTime = 720,
                        strandedTime = 240
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = endList[k],
                        moveTime = 720,
                        strandedTime = 240
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
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position, new Vector2(4.5f * MapManager.gridWidth, 1.5f * MapManager.gridHeight), "ItemCollideEnemy");
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
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(gridGroup.transform.position, new Vector2(4.5f * MapManager.gridWidth, 1.5f * MapManager.gridHeight));
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

    }

    /// <summary>
    /// �����ӹ�
    /// </summary>
    public override void OtherProcessing()
    {
        Transform trans = GameController.Instance.mMapController.transform;

        // ��ӿ���
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetSkyArea(MapManager.GetGridLocalPosition(4, 3), new Vector2(7.5f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "SkyArea";
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // ����Ʋ�(1267·��
        for (int i = 0; i < 2; i++)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(7f), MapManager.GetRowY(i)), 1);
        }
        for (int i = 5; i < 7; i++)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(7f), MapManager.GetRowY(i)), 1);
        }
        // ����Ʋ�(3-5·��
        for (int i = 2; i < 5; i++)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 9);
        }

        // �������
        {
            // 1267
            for (int i = 0; i < 2; i++)
            {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(-0.25f, 0.5f + 5 * i), new Vector2(0.5f * MapManager.gridWidth, 1.5f * MapManager.gridHeight));
                r.name = "SkyVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // ������
            {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(0.5f, 3), new Vector2(1 * MapManager.gridWidth, 2.5f * MapManager.gridHeight));
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
            int[] timeArray = new int[] { 150, 420, 150, 240, 150, 420, 150, 240 };
            float[] start_vArray = new float[] { 0, 1.89f, 1.89f, 0, 0, -1.89f, -1.89f, 0 };
            float[] end_vArray = new float[] { 1.89f, 1.89f, 0, 0, -1.89f, -1.89f, 0, 0 };

            List<WindAreaEffectExecution> wList = new List<WindAreaEffectExecution>();
            {
                WindAreaEffectExecution w = WindAreaEffectExecution.GetInstance(7.55f, 3, new Vector2(MapManager.GetColumnX(4.525f), MapManager.GetRowY(3)));
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
