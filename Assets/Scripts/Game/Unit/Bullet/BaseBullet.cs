using System.Collections.Generic;
using System;
using UnityEngine;
using S7P.Numeric;
public class BaseBullet : MonoBehaviour, IBaseBullet, IGameControllerMember
{
    // �����ⲿ����
    public BaseUnit mMasterBaseUnit; // �����������˵�λ

    // ����
    public Animator animator;
    public CircleCollider2D mCircleCollider2D;
    public SpriteRenderer spriteRenderer;
    private Transform Trans_Sprite;

    // ��������
    public float mVelocity;
    public float mAccelerate;
    public int mAccelerateTime;
    public int mMovetime;
    public Vector2 mRotate;
    public bool isIgnoreHeight; // �Ƿ����Ӹ߶�����
    public float mHeight;
    public float mDamage;
    public bool isDeathState;
    public BulletStyle style;
    public List<Func<BaseBullet, BaseUnit, bool>> CanHitFuncList = new List<Func<BaseBullet, BaseUnit, bool>>(); // �ӵ��뵥λ�ܷ��໥��ײ�Ķ����ж��¼�
    private List<Action<BaseBullet, BaseUnit>> HitActionList = new List<Action<BaseBullet, BaseUnit>>(); // �е�����¼������ⲿ��ӣ�
    protected List<BaseUnit> unitList = new List<BaseUnit>();
    public bool isnKillSelf;
    public bool isnUseHitEffect; // �����û��ж���
    public TaskController taskController = new TaskController();
    public int aliveTime;
    public bool isnDelOutOfBound;
    public float mAngle; // ��ǰ�ӵ��Ƕ�
    private float lastAngle; // �Ƕȣ���һ֡
    public bool isNavi;
    // ��������ΪͼƬ����ƫ����
    public FloatNumeric SpriteOffsetX = new FloatNumeric();
    public FloatNumeric SpriteOffsetY = new FloatNumeric();
    public Vector2 SpriteOffset { get { return new Vector2(SpriteOffsetX.Value, SpriteOffsetY.Value); } }
    public string hitSoundsRefName = null;

    // �����ı�ǩ
    public Dictionary<string, int> TagDict = new Dictionary<string, int>();

    public IBaseActionState mCurrentActionState; //+ ��ǰ����״̬

    public AnimatorController animatorController = new AnimatorController(); // �������ſ�����

    public virtual void Awake()
    {
        animator = transform.Find("SpriteGo").GetComponent<Animator>();
        mCircleCollider2D = GetComponent<CircleCollider2D>();
        spriteRenderer = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
        Trans_Sprite = transform.Find("SpriteGo");
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
        mAngle = 0;
        lastAngle = 0;
        Trans_Sprite.transform.right = Vector2.right;
        transform.right = Vector2.right;
        mDamage = 0;
        mHeight = 0;
        isIgnoreHeight = false;
        aliveTime = 0;
        isDeathState = false;
        isnKillSelf = false;
        isnDelOutOfBound = false;
        isnUseHitEffect = false;
        SpriteOffsetX.Initialize(); SpriteOffsetY.Initialize();
        SetSpriteLocalPosition(Vector2.zero);
        SetSpriteRight(Vector2.right);
        HitActionList.Clear();
        CanHitFuncList.Clear();
        TagDict.Clear();
        taskController.Initial();
        unitList.Clear();
        SetCollision(true);
        SetActionState(new BulletFlyState(this));
        transform.localScale = Vector2.one;
        Trans_Sprite.transform.localRotation = new Quaternion(0, 0, 0, 0);
        isNavi = true; // Ĭ����ͬ������
        Show(); // ��ʾ�Լ�
        hitSoundsRefName = null;
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
        // �ӵ�������״̬�²���ЧTakeDamage����ÿ����λֻ�ܱ���Чһ��
        if (isDeathState || unitList.Contains(baseUnit))
            return;
        if (baseUnit != null)
            new DamageAction(CombatAction.ActionType.CauseDamage, mMasterBaseUnit, baseUnit, mDamage).ApplyAction();
        ExecuteHitAction(baseUnit);
        if(hitSoundsRefName != null)
            GameManager.Instance.audioSourceController.PlayEffectMusic(hitSoundsRefName);
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
        // transform.right = v;
        if(isNavi)
            Trans_Sprite.transform.right = v;
    }

    /// <summary>
    /// �ı䷽�򣨴��������ǽǶ��ƵĲ�����
    /// </summary>
    /// <param name="angle"></param>
    public void SetRotate(float angle)
    {
        mAngle = angle;
        lastAngle = mAngle;
        angle = angle / 180 * Mathf.PI;
        SetRotate(new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)));
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
        if (mAngle != lastAngle)
        {
            SetRotate(mAngle);
        }

        List<BaseUnit> delList = new List<BaseUnit>();
        foreach (var u in unitList)
        {
            if (!u.IsAlive())
                delList.Add(u);
        }
        foreach (var u in delList)
        {
            unitList.Remove(u);
        }

        taskController.Update();

        // ��λ����״̬��״̬�����������ƶ���������������������
        mCurrentActionState.OnUpdate();
        animatorController.Update();
        // �ӵ�������������ɾ
        if (!isnDelOutOfBound && !IsInView())
        {
            ExecuteRecycle();
        }

        aliveTime++;
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
        return IsAlive() && (isIgnoreHeight || mHeight == unit.mHeight);
    }

    public virtual void OnFlyStateEnter()
    {
        if(animatorController.animator.runtimeAnimatorController != null)
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
        if (animatorController.animator.runtimeAnimatorController != null && !isnUseHitEffect)
        {
            animatorController.Play("Hit", false);
            SetCollision(false);
            isDeathState = true;
        }
        else
            ExecuteRecycle();
    }

    public virtual void OnHitState()
    {
        // ����������һ�κ�תΪ�ƶ�״̬
        if(isnUseHitEffect || animatorController.GetCurrentAnimatorStateRecorder().IsFinishOnce())
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
    /// ��ӱ����¼�
    /// </summary>
    /// <param name="HitAction"></param>
    public void AddHitAction(Action<BaseBullet, BaseUnit> HitAction)
    {
        HitActionList.Add(HitAction);
    }

    /// <summary>
    /// �Ƴ������¼�
    /// </summary>
    /// <param name="HitAction"></param>
    public void RemoveHitAction(Action<BaseBullet, BaseUnit> HitAction)
    {
        HitActionList.Remove(HitAction);
    }

    /// <summary>
    /// ִ�б����¼�
    /// </summary>
    /// <param name="hitedUnit"></param>
    public void ExecuteHitAction(BaseUnit hitedUnit)
    {
        foreach (var action in HitActionList)
        {
            action(this, hitedUnit);
        }
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
    /// �Ƴ�һ������
    /// </summary>
    /// <param name="t"></param>
    public void RemoveTask(ITask t)
    {
        taskController.RemoveTask(t);
    }

    public bool IsAlive()
    {
        return !isDeathState && isActiveAndEnabled;
    }

    public void AddCanHitFunc(Func<BaseBullet, BaseUnit, bool> action)
    {
        CanHitFuncList.Add(action);
    }

    public void RemoveCanHitFunc(Func<BaseBullet, BaseUnit, bool> action)
    {
        CanHitFuncList.Remove(action);
    }

    /// <summary>
    /// �ı���ͼXƫ��
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// �Ƴ��ı���ͼXƫ��
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetX(FloatModifier f)
    {
        SpriteOffsetX.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// �Ƴ��ı���ͼXƫ��
    /// </summary>
    /// <param name="f"></param>
    public void AddSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.AddAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// �Ƴ��ı���ͼYƫ��
    /// </summary>
    /// <param name="f"></param>
    public void RemoveSpriteOffsetY(FloatModifier f)
    {
        SpriteOffsetY.RemoveAddModifier(f);
        SetSpriteLocalPosition(SpriteOffset);
    }

    /// <summary>
    /// ������ͼ��������
    /// </summary>
    public virtual void SetSpriteLocalPosition(Vector2 vector2)
    {
        spriteRenderer.transform.localPosition = vector2;
    }

    /// <summary>
    /// ��ȡ��ͼ�����������
    /// </summary>
    /// <returns></returns>
    public virtual Vector2 GetSpriteLocalPosition()
    {
        return spriteRenderer.transform.localPosition;
    }

    public virtual void SetSpriteRight(Vector2 vector2)
    {
        spriteRenderer.transform.right = vector2;
    }

    /// <summary>
    /// ������ͼ�ĽǶ�
    /// </summary>
    /// <param name="angle"></param>
    public void SetSpriteRotate(float angle)
    {
        //Trans_Sprite.transform.localRotation = new Quaternion(0, 0, angle, 0);
        angle = angle * Mathf.PI / 180;
        Trans_Sprite.transform.right = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>
    /// �����Լ�
    /// </summary>
    public void Hide()
    {
        spriteRenderer.enabled = false;
    }

    /// <summary>
    /// ��ʾ�Լ�
    /// </summary>
    public void Show()
    {
        spriteRenderer.enabled = true;
    }

    /// <summary>
    /// ���û�����Ч����null��ΪĬ���ޣ�
    /// </summary>
    /// <param name="refName"></param>
    public void SetHitSoundEffect(string refName)
    {
        hitSoundsRefName = refName;
    }
}



