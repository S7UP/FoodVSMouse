using System.Collections.Generic;

using UnityEngine;
/// <summary>
/// �ƶ����
/// </summary>
public class MovementGridGroup : GridGroup
{
    /// <summary>
    /// ÿ��·������Ϣ
    /// </summary>
    public struct PointInfo
    {
        public Vector3 targetPosition; // Ŀ��λ��
        public int moveTime; // ����ʱ�䣨֡��
        public int strandedTime; // ����ʱ�䣨֡��
    }

    // ����
    public SpriteRenderer spriteRenderer0;
    public SpriteRenderer spriteRenderer1;
    // ����
    public List<PointInfo> positionList = new List<PointInfo>();
    public int currentMoveCount; // ��ǰ���ƶ�����·�������
    public int currentPositionListIndex; // ��ǰ�±� 
    public int currentTime; // ��ǰ·�����¼����ѻ���ʱ��
    public Vector3 startPosition;
    public Vector3 endPosition;

    private const float deltaAlpha = 1f / 60;

    public bool isPause { get; private set; }

    public override void Awake()
    {
        spriteRenderer0 = transform.Find("SpriteGo").GetComponent<SpriteRenderer>();
        spriteRenderer1 = transform.Find("SpriteGo1").GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// ��ʼ�ƶ�
    /// </summary>
    public void StartMovement(List<PointInfo> positionList, Sprite sprite, Vector2 offset, bool filpX, bool filpY)
    {
        foreach (var item in positionList)
        {
            this.positionList.Add(item);
        }

        if (sprite != null)
        {
            spriteRenderer0.sprite = sprite;
            spriteRenderer1.sprite = sprite;
        }
        spriteRenderer0.flipX = filpX;
        spriteRenderer1.flipX = filpX;
        spriteRenderer0.flipY = filpY;
        spriteRenderer1.flipY = filpY;
        spriteRenderer0.transform.localPosition = offset;
        spriteRenderer1.transform.localPosition = offset;
        currentPositionListIndex++; // 1Ϊ��ʼ
        startPosition = this.positionList[currentPositionListIndex-1].targetPosition; // 0
        endPosition = this.positionList[currentPositionListIndex].targetPosition; // 1
        SetPosition(startPosition);
        isPause = false;
    }

    public override void MInit()
    {
        base.MInit();
        spriteRenderer0.sprite = null;
        spriteRenderer1.sprite = null;
        positionList.Clear();
        currentMoveCount = 0;
        currentPositionListIndex = 0;
        currentTime = 0;
        isPause = true; // Ĭ������ͣ
    }

    public override void MUpdate()
    {
        base.MUpdate();
        spriteRenderer1.color = new Color(1, 1, 1, Mathf.Min(1, Mathf.Max(0, spriteRenderer1.color.a - deltaAlpha)));

        if (isPause)
            return;
        currentTime++;
        if (currentTime<= positionList[currentPositionListIndex].moveTime)
        {
            // �ƶ�״̬
            SetPosition(Vector3.Lerp(startPosition, endPosition, (float)currentTime/ positionList[currentPositionListIndex].moveTime));
        }else if(currentTime <= positionList[currentPositionListIndex].moveTime + positionList[currentPositionListIndex].strandedTime)
        {
            // ͣ��״̬
        }
        else
        {
            // �л�Ϊ��һ̬
            currentMoveCount++;
            currentPositionListIndex++;
            if (currentPositionListIndex == positionList.Count)
                currentPositionListIndex = 0;
            startPosition = endPosition;
            endPosition = positionList[currentPositionListIndex].targetPosition;
            currentTime = 0;
        }

        
    }

    /// <summary>
    /// ���õ�ǰ�ƶ����̰ٷֱ�
    /// </summary>
    public void SetCurrentMovementPercent(float percent)
    {
        currentTime = Mathf.FloorToInt(positionList[currentPositionListIndex].moveTime*percent);
        SetPosition(Vector3.Lerp(startPosition, endPosition, (float)currentTime / positionList[currentPositionListIndex].moveTime));
    }

    /// <summary>
    /// ��ȡһ��ʵ��
    /// </summary>
    public static MovementGridGroup GetInstance()
    {
        MovementGridGroup group = GameManager.Instance.GetGameObjectResource(FactoryType.GameFactory, "Grid/MovementGridGroup").GetComponent<MovementGridGroup>();
        group.MInit();
        return group;
    }

    /// <summary>
    /// ��������������
    /// </summary>
    protected override void ExecuteRecycle()
    {
        GameManager.Instance.PushGameObjectToFactory(FactoryType.GameFactory, "Grid/MovementGridGroup", this.gameObject);
    }
}
