using System.Collections.Generic;

using TMPro;

using UnityEngine;
/// <summary>
/// ���Ӷ����գ�
/// </summary>
public class Map_RabbitHole : ChapterMap
{
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
        RainAreaEffectExecution e = RainAreaEffectExecution.GetInstance(0);
        GameController.Instance.AddAreaEffectExecution(e);

        {
            Vector2[] pos = new Vector2[] 
            { 
                MapManager.GetGridLocalPosition(7, 1), MapManager.GetGridLocalPosition(7, 5)
            };
            int[] moveTime = new int[] 
            {
                1440, 1440
            };
            int[] stayTime = new int[] 
            {
                1440, 1440
            };
            int index = 0;
            bool isMove = false;
            int timeLeft = 0;
            Tasker tasker = GameController.Instance.AddTasker(
            // InitAction
            delegate 
            {
                index = 0;
                isMove = false;
                timeLeft = 0;
                e.transform.position = pos[index];
            }, 
            // UpdateAction
            delegate 
            {
                if (isMove)
                {
                    e.transform.position = Vector2.Lerp(pos[index], pos[(index + 1) % pos.Length], 1f - (float)timeLeft / moveTime[index]);
                    if (timeLeft > 0)
                        timeLeft--;
                    else
                    {
                        isMove = false;
                        timeLeft = stayTime[index];
                        index = (index + 1) % pos.Length;
                    }
                }
                else
                {
                    if (timeLeft > 0)
                        timeLeft--;
                    else
                    {
                        isMove = true;
                        timeLeft = moveTime[index];
                    }
                }
            },
            // EndCondition
            delegate 
            {
                return false;
            }, 
            // EndEvent
            delegate 
            {

            }
        );
        }

    }
}
