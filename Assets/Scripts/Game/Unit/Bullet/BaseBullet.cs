using System.Collections.Generic;

using UnityEngine;

public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // �����ⲿ����
    public BaseUnit mMasterBaseUnit; // �����������˵�λ

    // ����
    public Animator animator;
    public CircleCollider2D mCircleCollider2D;
    private SpriteRenderer spriteRenderer;

    // ��������
    public float mVelocity;
    public Vector2 mRotate;
    public float mHeight;
    public float mDamage;
    public bool isDeathState;
    public BulletStyle style;
    // �����ı�ǩ
    public Dictionary<string, int> TagDict = new Dictionary<string, int>();

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬

    public AnimatorController animatorController = new AnimatorController(); // �������ſ�����

    public virtual void Awake()
    {
        animator = transform.GetChild(0).GetComponent<Animator>();
        mCircleCollider2D = GetComponent<CircleCollider2D>();
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
    }

    // �ӵ���Ŀ������˺���TakeDamage�ĵ���ʱ���ǵжԵ�λ����������ӵ���Ȼ�������������ӵ����˺��߼�
    public virtual void TakeDamage(BaseUnit baseUnit)
    {
        if (baseUnit != null)
            new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, mDamage).ApplyAction();
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
    /// ÿ�γ�ʼ����Ҫ������
    /// </summary>
    public virtual void MInit()
    {
        animatorController.Initialize();
        animatorController.ChangeAnimator(animator);
        mVelocity = 0.0f;
        mRotate = Vector2.zero;
        mDamage = 0;
        mHeight = 0;
        isDeathState = false;
        TagDict.Clear();
        SetCollision(true);
        SetActionState(new BulletFlyState(this));
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
    public void SetRotate(Vector2 v)
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
        // ��λ����״̬��״̬�����������ƶ���������������������
        mCurrentActionState.OnUpdate();
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
        Vector3 worldPos = transform.position;
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPos); // ��������תΪ��Ļ���꣬Ȼ���ж��ǲ�����(0,0)-(1,1)֮��
        if (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1)
            return true;
        else
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
    }

    public virtual void OnFlyStateExit()
    {
        
    }

    public virtual void OnHitStateEnter()
    {
        animatorController.Play("Hit");
        SetCollision(false);
        isDeathState = true;
    }

    public virtual void OnHitState()
    {
        // ����������һ�κ�תΪ�ƶ�״̬
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
        //if (info.normalizedTime >= 1.0f)
        if(AnimatorManager.GetCurrentFrame(animator) == AnimatorManager.GetTotalFrame(animator))
        {
            SetActionState(new BaseBulletActionState(this));
        }
    }

    public virtual void OnHitStateExit()
    {
        ExecuteRecycle();
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
}



