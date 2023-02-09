using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// ������˺��ļ���
/// </summary>
public class BaseLaser : MonoBehaviour, IGameControllerMember
{
    // ������Ⱦ���������Update�������������
    public LaserRenderer laserRenderer;

    // ���������Ч��Sprite �� Runtime
    private Sprite hitSprite;
    private RuntimeAnimatorController HitRuntimeAnimatorController;

    // ������Щȫ�Ǽ�����Ⱦ��������
    public bool isOpen { get { return laserRenderer.isOpen; } set { laserRenderer.isOpen = value; } } // �Ƿ�򿪼���
    public float mMaxLength { get { return laserRenderer.mMaxLength; } set { laserRenderer.mMaxLength = value; } } // �������󳤶�
    public float mCurrentLength { get { return laserRenderer.mCurrentLength; } set { laserRenderer.mCurrentLength = value; } } // ����ĵ�ǰ����
    public float mVelocity { get { return laserRenderer.mVelocity; } set { laserRenderer.mVelocity = value; } } // ����������ٶ�
    public Vector2 mRotate { get { return laserRenderer.mRotate; } set { laserRenderer.mRotate = value; } } // ��������ķ���
    public int mAliveTime { get { return laserRenderer.mAliveTime; }} // ���ʱ��
    public LayerMask mask;

    // �Լ�������
    public bool isPenetration; // �Ƿ�͸
    public bool isShowHitEffect; // �Ƿ���ֻ�����Ч
    public int hitEffectInterval; // ������Чˢ�¼��
    private Dictionary<BaseUnit, int> targetNextHitEffectTimeDict = new Dictionary<BaseUnit, int>();
    public List<BaseUnit> currentHitedUnitList = new List<BaseUnit>(); // ��ǰ֡����ײ������Ŀ��
    public string laserHitKey; // ��Ч������ֵ��
    public BaseUnit master;
    public float damage;
    public bool isCollide;
    public TaskController taskController = new TaskController();

    public void Awake()
    {

    }

    public void MInit()
    {
        laserRenderer = null;
        SetCollisionLayer("EnemyLaser");
        mask = LayerMask.GetMask("Ally");
        SetTag("Untagged");

        isPenetration = true; // Ĭ��Ϊ��͸
        isShowHitEffect = true; // Ĭ��Ϊ��ʾ������Ч
        hitEffectInterval = 16; // Ĭ��16֡ˢ��һ�λ�����Ч
        targetNextHitEffectTimeDict.Clear();
        currentHitedUnitList.Clear();

        laserHitKey = "DefaultLaserHitEffect";

        master = null;
        damage = 10f/60;

        isCollide = true; // �Ƿ����ж�

        taskController.Initial();
    }

    public void MUpdate()
    {
        currentHitedUnitList.Clear(); // ����յ�ǰ֡���е�λ��
        laserRenderer.MUpdate();
        // ����ͬ��
        laserRenderer.transform.position = transform.position;
        if (isOpen && isCollide)
        {
            if (isPenetration)
            {
                // ��͸���
                RaycastHit2D[] rsList = Physics2D.RaycastAll(transform.position, mRotate, float.MaxValue, mask);
                foreach (var rs in rsList)
                {
                    float dist = rs.distance;
                    // ���������ж��������Ǽ��ⳤ��Ҫ����Ŀ�굽����Դ�ľ���
                    if (mCurrentLength >= dist)
                    {
                        HandleRaycastHit2DByUpdate(rs);
                    }
                }
            }
            else
            {
                // �Ǵ�͸���
                // ����һ������ȥ���Ŀ��
                RaycastHit2D rs = Physics2D.Raycast(transform.position, mRotate, float.MaxValue, mask);
                float dist = rs.distance;
                // ���������ж��������Ǽ��ⳤ��Ҫ����Ŀ�굽����Դ�ľ���
                if (mCurrentLength >= dist)
                {
                    HandleRaycastHit2DByUpdate(rs);
                    // ǿ��ͬ������ĳ���Ϊ����
                    mCurrentLength = dist;
                }
            }
            // ���»�����Ч
            UpdateHitEffectDict();
        }

        taskController.Update();
    }

    public void MPause()
    {
        
    }

    public void MPauseUpdate()
    {
        
    }

    public void MResume()
    {
        
    }

    public void MDestory()
    {
        ExecuteRecycle();
    }

    public void SetOpen(bool enable)
    {
        laserRenderer.SetOpen(enable);
    }

    /// <summary>
    /// ����Update�ﴦ��������RaycastHit2D
    /// </summary>
    private void HandleRaycastHit2DByUpdate(RaycastHit2D rs)
    {
        BaseUnit u;
        rs.collider.TryGetComponent<BaseUnit>(out u);
        if (u != null)
        {
            currentHitedUnitList.Add(u);
            if (!targetNextHitEffectTimeDict.ContainsKey(u))
                targetNextHitEffectTimeDict.Add(u, 0);

            // �����˺�
            TakeDamage(u);

            // ���������Ч
            if (targetNextHitEffectTimeDict.ContainsKey(u) && targetNextHitEffectTimeDict[u] <= 0 && !u.IsContainEffect(laserHitKey))
            {
                BaseEffect e = BaseEffect.CreateInstance(HitRuntimeAnimatorController, null, "Idle", null, false);
                u.AddEffectToDict(laserHitKey, e, rs.point - (Vector2)u.transform.position);
                targetNextHitEffectTimeDict[u] += hitEffectInterval;
            }
        }
    }

    /// <summary>
    /// ���»�����Ч
    /// </summary>
    private void UpdateHitEffectDict()
    {
        List<BaseUnit> delList = new List<BaseUnit>();
        // �ȸ����ѻ����ֵ�ĵ�λ���޳�û�е�Ŀ��
        foreach (var keyValuePair in targetNextHitEffectTimeDict)
        {
            BaseUnit unit = keyValuePair.Key;
            if (!currentHitedUnitList.Contains(unit))
            {
                delList.Add(unit);
            }
        }
        foreach (var unit in delList)
        {
            targetNextHitEffectTimeDict.Remove(unit);
        }
        
        // ʣ�µļ��������Ч
        if (isShowHitEffect)
        {
            List<int> timeLeftList = new List<int>(targetNextHitEffectTimeDict.Values);
            List<BaseUnit> unitList = new List<BaseUnit>(targetNextHitEffectTimeDict.Keys);

            for (int i = 0; i < unitList.Count; i++)
            {
                BaseUnit unit = unitList[i];
                int timeLeft = timeLeftList[i];
                targetNextHitEffectTimeDict[unit]--;
            }
        }
    }


    public void TakeDamage(BaseUnit unit)
    {
        if (unit != null)
            new DamageAction(CombatAction.ActionType.CauseDamage, master, unit, damage).ApplyAction();
    }

    public void SetCollisionLayer(string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
    }

    public void SetTag(string tag)
    {
        this.tag = tag;
    }

    public void ExecuteRecycle()
    {
        laserRenderer.ExecuteRecycle();
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Laser/BaseLaser", gameObject);
    }

    /// <summary>
    /// ���һ������
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        taskController.AddTask(t);
    }

    /// <summary>
    /// �Ƴ�Ψһ������
    /// </summary>
    public void RemoveUniqueTask(string key)
    {
        taskController.RemoveUniqueTask(key);
    }

    /// <summary>
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    /// <summary>
    /// ��ȡĳ�����Ϊkey������
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public ITask GetTask(string key)
    {
        return taskController.GetTask(key);
    }

    public static BaseLaser GetInstance(BaseUnit master, float damage, string layerName, LayerMask mask, Vector2 pos, Vector2 rot, Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite, Sprite HitEffectSprite,
        RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun, RuntimeAnimatorController HitRun)
    {
        BaseLaser l = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Laser/BaseLaser").GetComponent<BaseLaser>();
        l.MInit();
        l.transform.position = pos;
        l.master = master;
        l.damage = damage;
        l.laserRenderer = LaserRenderer.GetInstance(pos, rot, HeadSprite, BodySprite, TailSprite, HeadRun, BodyRun, TailRun);
        l.SetCollisionLayer(layerName);
        l.mask = mask;
        l.hitSprite = HitEffectSprite;
        l.HitRuntimeAnimatorController = HitRun;
        return l;
    }

    public static BaseLaser GetEnemyInstance(BaseUnit master, float damage, Vector2 pos, Vector2 rot, Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite, Sprite HitEffectSprite,
        RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun, RuntimeAnimatorController HitRun)
    {
        return BaseLaser.GetInstance(master, damage, "EnemyLaser", LayerMask.GetMask("Ally"), pos, rot, HeadSprite, BodySprite, TailSprite, HitEffectSprite, HeadRun, BodyRun, TailRun, HitRun);
    }

    public static BaseLaser GetAllyInstance(BaseUnit master, float damage, Vector2 pos, Vector2 rot, Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite, Sprite HitEffectSprite,
    RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun, RuntimeAnimatorController HitRun)
    {
        return BaseLaser.GetInstance(master, damage, "AllyLaser", LayerMask.GetMask("Enemy"), pos, rot, HeadSprite, BodySprite, TailSprite, HitEffectSprite, HeadRun, BodyRun, TailRun, HitRun);
    }
}
