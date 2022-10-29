using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �ƶ�
/// </summary>
public class Item_Cloud : BaseItem
{
    private List<BaseUnit> unitList = new List<BaseUnit>();
    private int maxBearCount; // �����ص�λ��
    private IntModifier BearInSkyModifier = new IntModifier(1); // �߿ճ���
    private bool isHide;
    private bool isBreak; // �Ƿ�������״̬
    private int timer;
    private int recoverTimeLeft; // ʣ��ָ�ʱ��

    public override void MInit()
    {
        unitList.Clear();
        maxBearCount = 5;
        isHide = true;
        timer = -1;
        isBreak = false;
        recoverTimeLeft = 0;
        base.MInit();
        // �����ж���С
        SetBoxCollider2DParam(Vector2.zero, new Vector2(0.6f*MapManager.gridWidth, 0.6f*MapManager.gridHeight));
        spriteRenderer.enabled = false;
        // ����ʱ���ų��ֶ���

        // Show();
    }

    public override void MUpdate()
    {
        if (timer > -1)
            timer++;

        if (isBreak)
        {
            if (recoverTimeLeft > 0)
                recoverTimeLeft--;
            else
                Recover();
        }


        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var item in unitList)
        {
            if (!item.IsAlive())
                delList.Add(item);
        }
        foreach (var item in delList)
        {
            unitList.Remove(item);
        }
        base.MUpdate();
    }

    private void OnUnitEnter(BaseUnit unit)
    {
        // ΪĿ�����һ��<�߿ճ���>��ǩ
        if (!unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
            unit.NumericBox.IntDict.Add(StringManager.BearInSky, new IntNumeric());
        unit.NumericBox.IntDict[StringManager.BearInSky].AddAddModifier(BearInSkyModifier);
        // ����������ֱ�����أ����ѣ�
        if (unitList.Count >= maxBearCount)
        {
            isBreak = true;
            recoverTimeLeft = 60 * 6;
            Hide();
        }
    }

    private void OnUnitExit(BaseUnit unit)
    {
        // �Ƴ�Ŀ��һ��<�߿ճ���>��ǩ
        if (unit.NumericBox.IntDict.ContainsKey(StringManager.BearInSky))
            unit.NumericBox.IntDict[StringManager.BearInSky].RemoveAddModifier(BearInSkyModifier);
    }

    private void OnCollision(Collider2D collision)
    {
        if(collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Barrier"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            if(!unitList.Contains(unit) && unit.GetHeight() == 0)
            {
                unitList.Add(unit);
                OnUnitEnter(unit);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        OnCollision(collision);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag.Equals("Food") || collision.tag.Equals("Mouse") || collision.tag.Equals("Barrier"))
        {
            BaseUnit unit = collision.GetComponent<BaseUnit>();
            if (unitList.Contains(unit))
            {
                unitList.Remove(unit);
                OnUnitExit(unit);
            }
        }
    }

    public override void CloseCollision()
    {
        // �ͷ����ϵ����е�λ
        foreach (var item in unitList)
        {
            OnUnitExit(item);
        }
        unitList.Clear();
        // �������������ʱ�ǿյ�
        base.CloseCollision();
    }

    public override void BeforeDeath()
    {
        // �ͷ����ϵ����е�λ
        foreach (var item in unitList)
        {
            OnUnitExit(item);
        }
        unitList.Clear();
        base.BeforeDeath();
    }

    public override void AfterDeath()
    {
        // �ͷ����ϵ����е�λ
        foreach (var item in unitList)
        {
            OnUnitExit(item);
        }
        unitList.Clear();
        base.AfterDeath();
    }

    public override void OnTransitionStateEnter()
    {
        if (isHide)
        {
            OpenCollision();
            animatorController.Play("Appear");
        }
        else
        {
            CloseCollision();
            animatorController.Play("Die");
        }
        isHide = !isHide;
    }

    public override void OnTransitionState()
    {
        if (animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            // ��һʱ����ʱʱ�俪ʼ����
            if (timer == -1)
                timer = 0;
            SetActionState(new IdleState(this));
        } 
    }

    public override void OnIdleStateEnter()
    {
        animatorController.Play("Idle", true, (float)timer/animatorController.GetAnimatorStateRecorder("Idle").aniTime);
        spriteRenderer.enabled = !isHide;
    }

    public void Hide()
    {
        if (!isHide)
        {
            SetActionState(new TransitionState(this));
            CloseCollision();
            unitList.Clear();
        }
    }

    public void Show()
    {
        if (isHide && !isBreak)
        {
            spriteRenderer.enabled = true;
            SetActionState(new TransitionState(this));
            OpenCollision();
        }
    }

    /// <summary>
    /// �ָ�
    /// </summary>
    public void Recover()
    {
        if (isBreak)
        {
            recoverTimeLeft = 0;
            isBreak = false;
            Show();
        }
    }

    /// <summary>
    /// ��ȡ��ʵ��
    /// </summary>
    /// <returns></returns>
    public static Item_Cloud GetInstance(int type, bool isShadow)
    {
        Item_Cloud c = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Item/5/0").GetComponent<Item_Cloud>();
        c.MInit();
        c.spriteRenderer.sprite = GameManager.Instance.GetSprite("Item/5/"+type+"/"+(isShadow?0:1));
        return c;
    }

    public override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Item/5/0", gameObject);
    }

    /// <summary>
    /// ����һ���Ʋ���
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public static RetangleAreaEffectExecution GetCloudGroup(int type, Vector2 pos, int count)
    {
        RetangleAreaEffectExecution e = RetangleAreaEffectExecution.GetInstance(pos, count, 1, "Enemy"); // ��ײ����Ϊ��ͬ��������ײ��
        e.SetBoxCollider2D(Vector2.zero, new Vector2(count * MapManager.gridWidth, 0.5f* 1 * MapManager.gridHeight));
        e.FloatDict.Add("offsetX", 0); // ���ڸ� ���� ����ƫ�����ı���
        GameController.Instance.AddAreaEffectExecution(e);

        float length = count*MapManager.gridWidth;
        float leftBoundPos = pos.x - (float)count / 2* MapManager.gridWidth; // ��߽�
        float leftFadePos = leftBoundPos + MapManager.gridWidth; // �󽥳���
        float rightBoundPos = pos.x + (float)count / 2 * MapManager.gridWidth; // �ұ߽�
        float rightFadePos = rightBoundPos - MapManager.gridWidth; // �ҽ�����

        Item_Cloud[] cloudArray = new Item_Cloud[count];
        float[] offsetXArray = new float[count];
        for (int i = 0; i < cloudArray.Length; i++)
        {
            Item_Cloud c = Item_Cloud.GetInstance(type, (i % 2 == 0 ? true : false));
            cloudArray[i] = c;
            offsetXArray[i] = (i - (float)(count-1)/2)* MapManager.gridWidth;
            GameController.Instance.AddItem(c);

            // �ƶ����γ���
            int timeLeft = 120/count * i;
            CustomizationTask tsk = new CustomizationTask();
            tsk.AddTaskFunc(delegate {
                timeLeft--;
                if (timeLeft <= 0)
                {
                    c.Show();
                    return true;
                }
                else
                    return false;
            });
            c.AddTask(tsk);
        }

        float last_offsetX = 0;
        // ����һ���ܿ�����
        Tasker t = GameController.Instance.AddTasker(
            // Init
            delegate { 
                last_offsetX = e.FloatDict["offsetX"];
                for (int i = 0; i < cloudArray.Length; i++)
                    cloudArray[i].transform.position = pos + new Vector2(offsetXArray[i], 0);
            },
            // Update
            delegate {
                float current_offsetX = e.FloatDict["offsetX"];
                float d_offsetX = current_offsetX - last_offsetX;
                if(d_offsetX != 0)
                {
                    for (int i = 0; i < cloudArray.Length; i++)
                    {
                        cloudArray[i].transform.position += d_offsetX * Vector3.right;
                        // ����
                        if(d_offsetX > 0)
                        {
                            if (cloudArray[i].transform.position.x >= rightBoundPos)
                            {
                                cloudArray[i].transform.position -= length * Vector3.right;
                                cloudArray[i].Show();
                                cloudArray[i].Recover();
                            }
                            else if(cloudArray[i].transform.position.x >= rightFadePos)
                            {
                                cloudArray[i].Hide();
                            }
                            else
                            {
                                cloudArray[i].Show();
                            }
                        }
                        else
                        {
                            // ����
                            if (cloudArray[i].transform.position.x <= leftBoundPos)
                            {
                                cloudArray[i].transform.position += length * Vector3.right;
                                cloudArray[i].Show();
                                cloudArray[i].Recover();
                            }
                            else if (cloudArray[i].transform.position.x <= leftFadePos)
                            {
                                cloudArray[i].Hide();
                            }
                            else
                            {
                                cloudArray[i].Show();
                            }
                        }
                    }
                }
                last_offsetX = current_offsetX;
            },
            // EndCondition,
            delegate { return !e.isActiveAndEnabled; },
            // End
            delegate { }
            );

        // go on

        // e.isAffectFood = true;
        return e;
    }
}
