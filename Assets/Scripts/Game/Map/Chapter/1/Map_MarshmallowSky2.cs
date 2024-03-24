using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// 棉花糖天空（夜）
/// </summary>
public class Map_MarshmallowSky2 : ChapterMap
{
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
        for (int i = 2; i < 7; i++)
            for (int j = 0; j < 7; j++)
                GetGrid(i, j).AddGridType(GridType.Sky, BaseGridType.GetInstance(GridType.Sky, 0));
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
        // 添加云层
        {
            for (int i = 0; i < 7; i++)
            {
                Item_Cloud.GetCloudGroup(1, new Vector2(MapManager.GetColumnX(4f), MapManager.GetRowY(i)), 9);
            }
        }

        // 添加风域
        {
            int[] timeArray = new int[] { 360, 120, 1440, 120, 360, 120, 1440, 120 };
            float[] start_vArray = new float[] { 0, 0, 1, 1, 0, 0, -1, -1 };
            float[] end_vArray = new float[] { 0, 1, 1, 0, 0, -1, -1, 0 };

            List<WindAreaEffectExecution> wList = new List<WindAreaEffectExecution>();
            WindAreaEffectExecution w = WindAreaEffectExecution.GetInstance(7.55f, 7, new Vector2(MapManager.GetColumnX(4.525f), MapManager.GetRowY(3)));
            for (int k = 0; k < timeArray.Length; k++)
            {
                WindAreaEffectExecution.State s = w.GetState(k);
                s.totoalTime = timeArray[k];
                s.start_v = TransManager.TranToVelocity(start_vArray[k]);
                s.end_v = TransManager.TranToVelocity(end_vArray[k]);
            }
            GameController.Instance.AddAreaEffectExecution(w);
            wList.Add(w);

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
