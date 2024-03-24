using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 香料飞船
/// </summary>
public class Map_SpiceSpaceship : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 中三路
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 6; j++)
                CreateAndAddGrid(j, i);
        // 两侧
        for (int i = 0; i < 7; i++)
        {
            if (i >= 2 && i <= 4)
                continue;
            for (int j = 0; j < 9; j++)
                CreateAndAddGrid(j, i);
        }
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
        // 设置中心点坐标
        gridGroup.transform.position = mapCenter + 1.5f * MapManager.gridWidth * Vector3.left;
        // 加入对应的格子
        for (int i = 2; i < 5; i++)
            for (int j = 0; j < 6; j++)
                gridGroup.Add(GetGrid(j, i));
        // 设置并启用移动版块
        gridGroup.StartMovement(
            new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.left,
                        moveTime = 540,
                        strandedTime = 420
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1.5f*MapManager.gridWidth*Vector3.right,
                        moveTime = 540,
                        strandedTime = 420
                    }
            },
            GameManager.Instance.GetSprite(GetSpritePath() + "1"),
            new Vector2(),
            false, false);
        gridGroupList.Add(gridGroup);
        gridGroup.transform.SetParent(transform);

        Transform trans = GameController.Instance.mMapController.transform;
        // 设置进入板块的检测域
        {
            RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position, new Vector2(5.5f * MapManager.gridWidth, 2.5f * MapManager.gridHeight), "ItemCollideEnemy");
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
                r.transform.position = gridGroup.transform.position;
                return !gridGroup.IsAlive();
            });
            t.AddOnExitAction(delegate {
                r.MDestory();
            });
            r.taskController.AddTask(t);
        }

        // 承载域
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(gridGroup.transform.position, new Vector2(5.5f * MapManager.gridWidth, 2.5f * MapManager.gridHeight));
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

    /// <summary>
    /// 其他加工
    /// </summary>
    public override void OtherProcessing()
    {
        Transform trans = GameController.Instance.mMapController.transform;
        // 添加空域
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetSkyArea(MapManager.GetGridLocalPosition(4, 3), new Vector2(7.5f * MapManager.gridWidth, 7 * MapManager.gridHeight));
            r.name = "SkyArea";
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 左承载域
        {
            // 1267
            for (int i = 0; i < 2; i++)
            {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(0.5f, 0.5f+5*i), new Vector2(1 * MapManager.gridWidth, 1.5f * MapManager.gridHeight));
                r.name = "SkyVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);
            }

            // 中三线
            {
                RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(-0.25f, 3), new Vector2(0.5f * MapManager.gridWidth, 2.5f * MapManager.gridHeight));
                r.name = "SkyVehicle(grid)";
                r.transform.SetParent(trans);
                GameController.Instance.AddAreaEffectExecution(r);
            }
        }
        // 右承载域
        {
            RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(8, 3), new Vector2(0.5f * MapManager.gridWidth, 6.5f * MapManager.gridHeight));
            r.name = "SkyVehicle(grid)";
            r.transform.SetParent(trans);
            GameController.Instance.AddAreaEffectExecution(r);
        }

        // 添加云层(345)
        for (int i = 2; i <= 4; i++)
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(7f), MapManager.GetRowY(i)), 1);
        // 添加云层(1267)
        for (int i = 0; i <= 1; i++)
        {
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 9);
            Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(5 + i)), 9);
        }

        // 添加风域
        {
            //int[] timeArray = new int[] { 360, 120, 1440, 120, 360, 120, 1440, 120 };
            //float[] start_vArray = new float[] { 0, 0, -1, -1, 0, 0, 1, 1 };
            //float[] end_vArray = new float[] { 0, -1, -1, 0, 0, 1, 1, 0 };
            int[] timeArray = new int[] { 60, 420, 60, 420, 60, 420, 60, 420 };
            float[] start_vArray = new float[] { 0,      2.25f, 2.25f, 0, 0,     -2.25f, -2.25f, 0  };
            float[] end_vArray =   new float[] { 2.25f, 2.25f, 0,      0, -2.25f, -2.25f, 0,     0 };

            List<WindAreaEffectExecution> wList = new List<WindAreaEffectExecution>();
            for (int i = 0; i <= 5; i += 5)
            {
                WindAreaEffectExecution w = WindAreaEffectExecution.GetInstance(7.55f, 1.5f, new Vector2(MapManager.GetColumnX(4.525f), MapManager.GetRowY(i + 0.5f)));
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
                    if(newArray != null)
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
