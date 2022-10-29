using System.Collections.Generic;
using System;
using UnityEngine;

public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // �����ⲿ����
    public BaseUnit mMasterBaseUnit; // �����������˵�λ

    // ����
    public Animator animator;
    public CircleCollider2D mCircleCollider2D;
    public SpriteRenderer spriteRenderer;

    // ��������
    public float mVelocity;
    public float mAccelerate;
    public int mAccelerateTime;
    public int mMovetime;
    public Vector2 mRotate;
    public float mHeight;
    public float mDamage;
    public bool isDeathState;
    public BulletStyle style;
    private Action<BaseBullet, BaseUnit> HitAction; // �е�����¼������ⲿ��ӣ�
    public bool isnKillSelf;
    public List<ITask> TaskList = new List<ITask>();

    // �����ı�ǩ
    public Dictionary<string, int> TagDict = new Dictionary<string, int>();

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬

    public AnimatorController animatorController = new AnimatorController(); // �������ſ�����

    public virtual void Awake()
    {
        animator = transform.Find("SpriteGo").GetComponent<Animator>();
        mCircleCollider2D = GetComponent<CircleCollider2D>();
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// ÿ�γ�ʼ����Ҫ������
    /// </summary>
    public virtual void MInit()
    {
        animatorController.Initialize();
        animatorController.ChangeAnimator(animator);
        mVelocity = 0.0f;    
        mAccelerate = 0;
        mAccelerateTime = 0;
        mMovetime = 0;
        mRotate = Vector2.zero;
        transform.right = Vector2.right;
        mDamage = 0;
        mHeight = 0;
        isDeathState = false;
        isnKillSelf = false;
        HitAction = null;
        TagDict.Clear();
        TaskList.Clear();
        SetCollision(true);
        SetActionState(new BulletFlyState(this));
    }

    /// <summary>
    /// �����ٶȱ仯�¼�
    /// </summary>
    public void SetVelocityChangeEvent(float v0, float v1, int t)
    {
        if (t < 1)
        {
            Debug.Log("�ٶȱ仯ʱ�䲻��С��1֡");
            return;
        }
        mAccelerate = (v1 - v0) / t;
        mVelocity = v0;
        mMovetime = 0;
        mAccelerateTime = t;
    }

    // �ӵ���Ŀ������˺���TakeDamage�ĵ���ʱ���ǵжԵ�λ����������ӵ���Ȼ�������������ӵ����˺��߼�
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        if (baseUnit != null)
            new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, mDamage).ApplyAction();
        ExecuteHitAction(baseUnit);
        if(!isnKillSelf)
            KillThis();
    }

    /// <summary>
    /// �ӵ��Ա�
    /// </summary>
    public virtual void KillThis()
    {
        SetActionState(new BulletHitState(this));
    }

    public void SetActionState(IBaseActionState state)
    {
        if (mCurrentActionState != null)
        {
            mCurrentActionState.OnExit();
        }
        mCurrentActionState = state;
        mCurrentActionState.OnEnter();
    }

    /// <summary>
    /// �����ж�
    /// </summary>
    public void SetCollision(bool colli)
    {
        mCircleCollider2D.enabled = colli;
    }

    // ������GameController��
    public virtual void MDestory()
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// ���ı��ӵ���ۣ�Ani)���Ҳ��ı���ʽ(style)
    /// </summary>
    /// <param name="style"></param>
    public void ChangeAnimatorWithoutChangeStyle(BulletStyle target_style)
    {
        animator.runtimeAnimatorController = GameManager.Instance.GetRuntimeAnimatorController("Bullet/" + ((int)target_style) + "/0");
    }

    public Vector2 GetRotate()
    {
        return mRotate;
    }

    /// <summary>
    /// �ı䷽��
    /// </summary>
    /// <param name="v"></param>
    public virtual void SetRotate(Vector2 v)
    {
        mRotate = v;
        transform.right = v;
    }

    public float GetDamage()
    {
        return mDamage;
    }

    public void SetDamage(float dmg)
    {
        mDamage = dmg;
    }

    public float GetVelocity()
    {
        return mVelocity;
    }

    /// <summary>
    /// ֱ�������ƶ��ٶ�ֵ���Ƚϳ����Ƽ�ʹ��SetStandardVelocity())����
    /// </summary>
    /// <param name="v"></param>
    public void SetVelocity(float v)
    {
        mVelocity = v;
    }

    /// <summary>
    /// ���ñ�׼���ƶ��ٶȣ�1��λ��׼�ƶ��ٶ�Ϊ 6��������1�� ���ٶ�
    /// </summary>
    public void SetStandardVelocity(float standardVelocity)
    {
        mVelocity = TransManager.TranToVelocity(standardVelocity);
    }

    public virtual void MPause()
    {
        animatorController.Pause();
    }

    public virtual void MResume()
    {
        animatorController.Resume();
    }

    public virtual void MUpdate()
    {
        List<ITask> deleteTask = new List<ITask>();
        foreach (var t in TaskList)
        {
            if (t.IsMeetingExitCondition())
                deleteTask.Add(t);
            else
                t.OnUpdate();
        }
        foreach (var t in deleteTask)
        {
            RemoveTask(t);
        }

        // ��λ����״̬��״̬�����������ƶ���������������������
        mCurrentActionState.OnUpdate();
        animatorController.Update();
        // �ӵ�������������ɾ
        if (!IsInView())
        {
            ExecuteRecycle();
        }
    }

    /// <summary>
    /// ִ�л���
    /// </summary>
    public virtual void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/" + ((int)style), gameObject);
    }

    /// <summary>
    /// �ж��ӵ��ڲ�����Ļ��
    /// </summary>
    public bool IsInView()
    {
        Vector3 p = transform.position;
        if (p.x > MapManager.GetColumnX(-2) && p.x < MapManager.GetColumnX(9) && p.y > MapManager.GetRowY(9) && p.y < MapManager.GetRowY(-3))
            return true;
        return false;
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// ��ȡ��ǰ��λ�������±�
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// �ж��ӵ��ܷ���е�λ 
    /// </summary>
    /// <returns></returns>
    public virtual bool CanHit(BaseUnit unit)
    {
        return !isDeathState;
    }

    public virtual void OnFlyStateEnter()
    {
        animatorController.Play("Fly", true);
    }

    public virtual void OnFlyState()
    {
        transform.position += (Vector3)mRotate * mVelocity;
        if (mMovetime < mAccelerateTime)
            mVelocity += mAccelerate;
        mMovetime ++;
    }

    public virtual void OnFlyStateExit()
    {
        
    }

    public virtual void OnHitStateEnter()
    {
        animatorController.Play("Hit", false);
        SetCollision(false);
        isDeathState = true;
    }

    public virtual void OnHitState()
    {
        // ����������һ�κ�תΪ�ƶ�״̬
        if(animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
        {
            ExecuteRecycle();
        }
    }

    public virtual void OnHitStateExit()
    {
        // ExecuteRecycle();
        //GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Bullet/"+((int)style), gameObject);
    }

    /// <summary>
    /// ������Ⱦ�㼶
    /// </summary>
    /// <param name="arrayIndex"></param>
    public virtual void UpdateRenderLayer(int arrayIndex)
    {
        spriteRenderer.sortingOrder = LayerManager.CalculateSortingLayer(LayerManager.UnitType.Bullet, GetRowIndex(), 0, arrayIndex);
    }

    /// <summary>
    /// ��ȡĳ����ǩ��Ӧ����ֵ
    /// </summary>
    /// <returns></returns>
    public int GetTagCount(string tagName)
    {
        if (TagDict.ContainsKey(tagName))
            return TagDict[tagName];
        return 0;
    }

    public void AddTag(string tagName)
    {
        AddTagCount(tagName, 1);
    }

    public void RemoveTag(string tagName)
    {
        TagDict.Remove(tagName);
    }

    public void AddTagCount(string tagName, int count)
    {
        if (TagDict.ContainsKey(tagName))
            TagDict[tagName] += count;
        else
        {
            TagDict.Add(tagName, count);
        }
    }

    /// <summary>
    /// �����ж�
    /// </summary>
    public virtual void OpenCollision()
    {
        mCircleCollider2D.enabled = true;
    }

    /// <summary>
    /// �ر��ж�
    /// </summary>
    public virtual void CloseCollision()
    {
        mCircleCollider2D.enabled = false;
    }

    /// <summary>
    /// ����͸����
    /// </summary>
    public virtual void SetAlpha(float a)
    {
        spriteRenderer.color = new UnityEngine.Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, a);
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {

    }

    public virtual void OnTriggerStay2D(Collider2D collision)
    {

    }

    public virtual void OnTriggerExit2D(Collider2D collision)
    {

    }

    public virtual void MPauseUpdate()
    {
        
    }

    public void SetHeight(float height)
    {
        mHeight = height;
    }

    /// <summary>
    /// ���ñ����¼�
    /// </summary>
    /// <param name="HitAction"></param>
    public void SetHitAction(Action<BaseBullet, BaseUnit> HitAction)
    {
        this.HitAction = HitAction;
    }

    /// <summary>
    /// ִ�б����¼�
    /// </summary>
    /// <param name="hitedUnit"></param>
    public void ExecuteHitAction(BaseUnit hitedUnit)
    {
        if (HitAction != null)
            HitAction(this, hitedUnit);
    }

    /// <summary>
    /// ���һ������
    /// </summary>
    /// <param name="t"></param>
    public void AddTask(ITask t)
    {
        TaskList.Add(t);
        t.OnEnter();
    }

    /// <summary>
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        TaskList.Remove(t);
        t.OnExit();
    }

    public bool IsAlive()
    {
        return !isDeathState && isActiveAndEnabled;
    }
}



