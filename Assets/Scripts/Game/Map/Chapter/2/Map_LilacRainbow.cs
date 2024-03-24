using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ����ʺ磨�գ�
/// </summary>
public class Map_LilacRainbow : ChapterMap
{
    /// <summary>
    /// ��������
    /// </summary>
    public override void CreateGridList()
    {
        // 5*7
        for (int i = 0; i < 7; i++)
            for (int j = 0; j < 5; j++)
                CreateAndAddGrid(j, i);
    }

    /// <summary>
    /// ����������
    /// </summary>
    public override void CreateGridGoupList()
    {
        List<BaseGrid>[] listArray = new List<BaseGrid>[7];

        // 7�а��
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
            // �������ĵ�����
            gridGroup.transform.position = mapCenter + 2f * MapManager.gridWidth * Vector3.left + (3-k)*MapManager.gridHeight * Vector3.up;
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
            // ���ý�����ļ����
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
        //Transform trans = GameController.Instance.mMapController.transform;
        //// ��ӿ���
        //{
        //    RetangleAreaEffectExecution r = Environment.SkyManager.GetSkyArea(MapManager.GetGridLocalPosition(4, 3), new Vector2(7.5f * MapManager.gridWidth, 7 * MapManager.gridHeight));
        //    r.name = "SkyArea";
        //    GameController.Instance.AddAreaEffectExecution(r);
        //}

        //// ����һ��Ϊ������
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

        //// ����Ʋ�
        //for (int i = 0; i < 7; i++)
        //    Item_Cloud.GetCloudGroup(0, new Vector2(MapManager.GetColumnX(7f), MapManager.GetRowY(i)), 1);
    }
}
