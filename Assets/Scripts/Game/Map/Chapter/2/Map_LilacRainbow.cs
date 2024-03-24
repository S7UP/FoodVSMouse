using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 丁香彩虹（日）
/// </summary>
public class Map_LilacRainbow : ChapterMap
{
    /// <summary>
    /// 创建格子
    /// </summary>
    public override void CreateGridList()
    {
        // 5*7
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// 创建格子组
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[7];

        // 7行板块
        for (int i = 0; i < listArray.Length; i++)
        {
            listArray[i] = new List<BaseGrid>();
            for (int j = 0; j < 5; j++)
            {
                listArray[i].Add(GetGrid(j, i));
            }
        }

        for (int k = 0; k < 7; k++)
        {
            MovementGridGroup gridGroup = MovementGridGroup.GetInstance();
            // 设置中心点坐标
            gridGroup.transform.position = mapCenter + 2f * MapManager.gridWidth * Vector3.left + (3-k)*MapManager.gridHeight * Vector3.up;
            // 加入对应的格子
            foreach (var grid in listArray[k])
            {
                gridGroup.Add(grid);
            }
            // 设置并启用移动版块
            gridGroup.StartMovement(
                new List<MovementGridGroup.PointInfo>() {
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1f * MapManager.gridWidth * Vector3.left + (3-k)*MapManager.gridHeight * Vector3.up,
                        moveTime = 360,
                        strandedTime = 540
                    },
                    new MovementGridGroup.PointInfo()
                    {
                        targetPosition = mapCenter + 1f * MapManager.gridWidth * Vector3.right + (3-k)*MapManager.gridHeight * Vector3.up,
                        moveTime = 360,
                        strandedTime = 540
                    }
                },
                GameManager.Instance.GetSprite(GetSpritePath() + (k + 1)),
                new Vector2(),
                false, false);
            gridGroup.SetCurrentMovementPercent(1 - (float)Mathf.Abs(k - 3) / 3);
            gridGroupList.Add(gridGroup);
            gridGroup.transform.SetParent(transform);

            Transform trans = GameController.Instance.mMapController.transform;
            // 设置进入板块的检测域
            {
                RetangleAreaEffectExecution r = RetangleAreaEffectExecution.GetInstance(gridGroup.transform.position, new Vector2(4.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight), "ItemCollideEnemy");
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
            //{
            //    RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(gridGroup.transform.position, new Vector2(4.5f * MapManager.gridWidth, 0.5f * MapManager.gridHeight));
            //    r.name = "SkyVehicle(grid)";
            //    r.transform.SetParent(trans);
            //    GameController.Instance.AddAreaEffectExecution(r);

            //    CustomizationTask t = new CustomizationTask();
            //    t.AddTaskFunc(delegate {
            //        r.transform.position = gridGroup.transform.position;
            //        return !gridGroup.IsAlive();
            //    });
            //    r.taskController.AddTask(t);
            //}
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
        //Transform trans = GameController.Instance.mMapController.transform;
        //// 添加空域
        //{
        //    RetangleAreaEffectExecution r = Environment.SkyManager.GetSkyArea(MapManager.GetGridLocalPosition(4, 3), new Vector2(7.5f * MapManager.gridWidth, 7 * MapManager.gridHeight));
        //    r.name = "SkyArea";
        //    GameController.Instance.AddAreaEffectExecution(r);
        //}

        //// 左右一列为承载域
        //{
        //    RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(0, 3), new Vector2(0.5f * MapManager.gridWidth, 6.5f * MapManager.gridHeight));
        //    r.name = "SkyVehicle(grid)";
        //    r.transform.SetParent(trans);
        //    GameController.Instance.AddAreaEffectExecution(r);
        //}
        //{
        //    RetangleAreaEffectExecution r = Environment.SkyManager.GetVehicleArea(MapManager.GetGridLocalPosition(8, 3), new Vector2(0.5f * MapManager.gridWidth, 6.5f * MapManager.gridHeight));
        //    r.name = "SkyVehicle(grid)";
        //    r.transform.SetParent(trans);
        //    GameController.Instance.AddAreaEffectExecution(r);
        //}

        //// 添加云层
        //for (int i = 0; i < 7; i++)
        //    Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(7f), MapManager.GetRowY(i)), 1);
    }
}
