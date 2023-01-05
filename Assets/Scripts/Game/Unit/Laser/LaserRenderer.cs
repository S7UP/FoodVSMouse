using UnityEngine;

/// <summary>
/// 激光渲染管理者
/// </summary>
public class LaserRenderer : MonoBehaviour, IGameControllerMember
{
    private GameObject Go_Head;
    private Animator Ani_Head;
    public SpriteRenderer Spr_Head;
    private GameObject Go_Body;
    private Animator Ani_Body;
    public SpriteRenderer Spr_Body;
    private GameObject Go_Tail;
    private Animator Ani_Tail;
    public SpriteRenderer Spr_Tail;
    private Material laserMaterial;
    
    
    public AnimatorController headAnimatorController = new AnimatorController();
    public AnimatorController hitEffectAnimatorController = new AnimatorController();
    public AnimatorController bodyAnimatorController = new AnimatorController();
    public AnimatorController tailAnimatorController = new AnimatorController();

    private string sortingLayerName;

    private float bodySpriteWidth; // 激光身图片精灵的宽度（用于换算）

    public bool isOpen; // 是否打开激光
    private float VscaleRate; // 纵向缩放比率（用于激光打开和关闭时纵向的伸缩效果）
    private float DeltaVscaleRate; // 上述变量的每帧变化率
    public float mMaxLength; // 激光的最大长度
    public float mCurrentLength; // 激光的当前长度
    public float mVelocity; // 激光延伸的速度
    public Vector2 mRotate; // 激光延伸的方向
    private float mOffsetX; // 与激光播放动作有关
    public int mAliveTime { get; private set; } // 存活时间

    public void Awake()
    {
        Go_Head = transform.Find("Ani_Head").gameObject;
        Ani_Head = Go_Head.GetComponent<Animator>();
        Spr_Head = Go_Head.GetComponent<SpriteRenderer>();
        Go_Body = transform.Find("Ani_Body").gameObject;
        Ani_Body = Go_Body.GetComponent<Animator>();
        Spr_Body = Go_Body.GetComponent<SpriteRenderer>();
        Go_Tail = transform.Find("Ani_Tail").gameObject;
        Ani_Tail = Go_Tail.GetComponent<Animator>();
        Spr_Tail = Go_Tail.GetComponent<SpriteRenderer>();

        headAnimatorController.ChangeAnimator(Ani_Head);
        bodyAnimatorController.ChangeAnimator(Ani_Body);
        tailAnimatorController.ChangeAnimator(Ani_Tail);

        laserMaterial = Spr_Body.material;
    }

    public void MInit()
    {
        headAnimatorController.Initialize();
        hitEffectAnimatorController.Initialize();
        bodyAnimatorController.Initialize();
        tailAnimatorController.Initialize();

        sortingLayerName = "Unit"; // 默认渲染层级名称

        // Sprite与RunTimeAnimatorController清空
        {
            Ani_Head.runtimeAnimatorController = null;
            Spr_Head.sprite = null;
            Ani_Body.runtimeAnimatorController = null;
            Spr_Body.sprite = null;
            Ani_Tail.runtimeAnimatorController = null;
            Spr_Tail.sprite = null;
        }

        // 激光材质参数初始化
        {
            laserMaterial.SetVector("UVOffset", Vector4.zero);
            laserMaterial.SetVector("UVSize", new Vector4(0, 0, 1, 1));
        }

        SetVerticalOpenTime(20);
        SetMaxLength(float.MaxValue);
        mCurrentLength = 0;
        VscaleRate = 0;
        SetVelocity(TransManager.TranToVelocity(108));
        SetRotate(Vector2.right);
        // 默认为关
        isOpen = true;
        SetOpen(false); // 上面的置为false是必要的，否则不会触发对应事件

        mOffsetX = 0;
        mAliveTime = 0;

    }

    public void MUpdate()
    {
        if (isOpen)
        {
            VscaleRate = Mathf.Min(VscaleRate + DeltaVscaleRate, 1);
            mCurrentLength = Mathf.Min(mCurrentLength + mVelocity, mMaxLength);
        }
        else
        {
            VscaleRate = Mathf.Min(VscaleRate - DeltaVscaleRate, 0);
            mCurrentLength = Mathf.Max(mCurrentLength - mVelocity, 0);
        }
        mOffsetX -= mVelocity;
        // 更新材质相关的参数
        laserMaterial.SetVector("UVSize", new Vector4(mCurrentLength/ bodySpriteWidth, 1, 1, 1));
        laserMaterial.SetVector("UVOffset", new Vector4(mOffsetX / bodySpriteWidth, 0, 0, 0));
        // 激光头偏移
        Go_Head.transform.localPosition = new Vector2(mCurrentLength, 0);
        // 激光身纵向缩放
        Go_Body.transform.localScale = new Vector2(1, VscaleRate);
        // 激光尾
        Go_Tail.transform.localScale = new Vector2(VscaleRate, VscaleRate);

        mAliveTime++;
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
        
    }

    public void SetSprites(Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite)
    {
        Spr_Head.sprite = HeadSprite;
        Spr_Body.sprite = BodySprite;
        Spr_Tail.sprite = TailSprite;

        if(BodySprite != null)
        {
            bodySpriteWidth = BodySprite.bounds.size.x;
            //Debug.Log("bodySpriteWidth = "+ bodySpriteWidth);
        }
    }

    /// <summary>
    /// 设置激光各组成部分的RuntimeAnimatorController，如果填null则该部位什么也不会发生（不会置对应部位为null）
    /// </summary>
    /// <param name="HeadRun"></param>
    /// <param name="BodyRun"></param>
    /// <param name="TailRun"></param>
    /// <param name="HitRun"></param>
    public void SetRuntimeAnimatorControllers(RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun)
    {
        if(HeadRun != null)
        {
            Ani_Head.runtimeAnimatorController = HeadRun;
        }
        if(BodyRun != null)
        {
            Ani_Body.runtimeAnimatorController = BodyRun;
        }
        if(TailRun != null)
        {
            Ani_Tail.runtimeAnimatorController = TailRun;
        }
    }

    public void SetOpen(bool enable)
    {
        if (enable == isOpen)
            return;

        if (enable)
        {
            if (Ani_Head.runtimeAnimatorController != null)
                headAnimatorController.Play("Idle", true);
            if (Ani_Body.runtimeAnimatorController != null)
                bodyAnimatorController.Play("Idle", true);
            if (Ani_Tail.runtimeAnimatorController != null)
                tailAnimatorController.Play("Idle", true);
        }
        isOpen = enable;
    }

    /// <summary>
    /// 设置开启或者关闭时纵向伸缩的时间
    /// </summary>
    public void SetVerticalOpenTime(int time)
    {
        DeltaVscaleRate = 1.0f / time;
    }

    public void SetVelocity(float v)
    {
        mVelocity = v;
    }

    public void SetRotate(Vector2 v2)
    {
        mRotate = v2;
        transform.right = v2;
    }

    public void SetMaxLength(float max)
    {
        mMaxLength = max;
    }

    /// <summary>
    /// 获取当前单位所在行下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetRowIndex()
    {
        return MapManager.GetYIndex(transform.position.y);
    }

    /// <summary>
    /// 获取当前单位所在列下标
    /// </summary>
    /// <returns></returns>
    public virtual int GetColumnIndex()
    {
        return MapManager.GetXIndex(transform.position.x);
    }

    /// <summary>
    /// 设置渲染层级
    /// </summary>
    /// <param name="arrayIndex"></param>
    public virtual void UpdateRenderLayer(int arrayIndex)
    {
        int rowIndex = GetRowIndex();
        LayerManager.UnitType unitType = LayerManager.UnitType.Bullet;

        Spr_Tail.sortingLayerName = sortingLayerName;
        Spr_Tail.sortingOrder = LayerManager.CalculateSortingLayer(unitType, rowIndex, 0, arrayIndex + 1);

        Spr_Body.sortingLayerName = sortingLayerName;
        Spr_Body.sortingOrder = LayerManager.CalculateSortingLayer(unitType, rowIndex, 0, arrayIndex);

        Spr_Head.sortingLayerName = sortingLayerName;
        Spr_Head.sortingOrder = LayerManager.CalculateSortingLayer(unitType, rowIndex, 0, arrayIndex + 1);
    }

    /// <summary>
    /// 设置图层名
    /// </summary>
    /// <param name="sortingLayerName"></param>
    public void SetSortingLayerName(string sortingLayerName)
    {
        this.sortingLayerName = sortingLayerName;
        Spr_Tail.sortingLayerName = sortingLayerName;
        Spr_Body.sortingLayerName = sortingLayerName;
        Spr_Head.sortingLayerName = sortingLayerName;
    }



    public bool IsAlive()
    {
        if (isActiveAndEnabled)
        {
            return isOpen;
        }
        else
            return false;
    }

    public void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Laser/LaserRenderer", gameObject);
    }

    public static LaserRenderer GetInstance(Vector2 pos, Vector2 rot, Sprite HeadSprite, Sprite BodySprite, Sprite TailSprite,
        RuntimeAnimatorController HeadRun, RuntimeAnimatorController BodyRun, RuntimeAnimatorController TailRun)
    {
        LaserRenderer lr = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Laser/LaserRenderer").GetComponent<LaserRenderer>();
        lr.MInit();
        lr.SetSprites(HeadSprite, BodySprite, TailSprite);
        lr.SetRuntimeAnimatorControllers(HeadRun, BodyRun, TailRun);
        lr.transform.position = pos;
        lr.SetRotate(rot);
        lr.SetOpen(true); // 默认为开
        return lr;
    }
}
