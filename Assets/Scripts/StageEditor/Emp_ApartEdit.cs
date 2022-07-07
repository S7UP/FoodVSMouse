using UnityEngine;
using UnityEngine.UI;

public class Emp_ApartEdit : MonoBehaviour
{
    private int arrayIndex; // 在数组中的位置
    private int position;
    private int selectedIndex;
    private Image[] images;
    private Transform InfoDisplayer;
    private Text tex;
    public Button Btn_Del;

    private void Awake()
    {
        selectedIndex = -1;
        images = new Image[transform.childCount];
        for (int i = 0; i < images.Length; i++)
        {
            images[i] = transform.GetChild(i).GetComponent<Image>();
        }
        InfoDisplayer = transform.Find("Image").Find("InfoDisplayer");
        tex = InfoDisplayer.Find("Text").GetComponent<Text>();
        Btn_Del = InfoDisplayer.Find("Button").GetComponent<Button>();
    }

    public void SetSelectedIndex(int i)
    {
        selectedIndex = i;
        UpdateDisplay();
    }

    public void SetPosition(int pos)
    {
        position = pos;
        tex.text = (position + 1).ToString();
    }

    private void UpdateDisplay()
    {
        foreach (var item in images)
        {
            item.enabled = false;
        }
        if(selectedIndex>-1 && selectedIndex < images.Length)
        {
            images[selectedIndex].enabled = true;
            InfoDisplayer.SetParent(images[selectedIndex].transform);
            InfoDisplayer.localPosition = Vector3.zero;
        }
    }
}
