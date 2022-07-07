using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Emp_SelectEnemyItem : MonoBehaviour, IPointerClickHandler
{
    private static Color colorSelected = new Color(255, 255, 0);
    private static Color colorUnSelected = new Color(0, 177, 255);

    private Scr_SelectEnemyType master;
    private Image background;
    private Image img;

    private int arrayIndex; // λ��������±�

    private void Awake()
    {
        background = GetComponent<Image>();
        img = transform.Find("Image").GetComponent<Image>();
        arrayIndex = -1;
    }

    private void OnEnable()
    {
        SetSelected(false);
        arrayIndex = -1;
    }

    public void SetArrayIndex(int index)
    {
        arrayIndex = index;
    }

    public int GetArrayIndex()
    {
        return arrayIndex;
    }

    public void SetMaster(GameObject go)
    {
        master = go.GetComponent<Scr_SelectEnemyType>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (master.GetSelectedEnemyShape() == -1)
        {
            // ��һ�� ѡ���� �����ѡ��������ԭ����¼����ͬ��������һ��Ĭ��ѡ��ԭ����¼�ı��֣��������Ĭ��Ϊ��1��
            if (GetArrayIndex() == master.GetSelectedEnemyType())
            {
                master.UpdateSelectedEnemyShape();
            }
            else
            {
                master.SetSelectedEnemyTypeByArrayIndex(GetArrayIndex());
                master.SetSelectedEnemyShapeByArrayIndex(0);
            }
            master.UpdateUIAndModel();
        }
        else
        {
            // �ڶ��� ѡ����
            master.SetSelectedEnemyShapeByArrayIndex(GetArrayIndex());
            master.UpdateUIAndModel();
            master.gameObject.SetActive(false);
        }
    }

    public void SetSprite(Sprite sprite)
    {
        img.sprite = sprite;
    }

    public void SetSelected(bool select)
    {
        if (select)
        {
            background.color = colorSelected;
        }
        else
        {
            background.color = colorUnSelected;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
