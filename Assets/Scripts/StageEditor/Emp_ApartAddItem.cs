using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Emp_ApartAddItem : MonoBehaviour, IPointerEnterHandler
{
    public int index;
    public Emp_ApartAdd master;
    private Image img;

    private void Awake()
    {
        img = GetComponent<Image>();
    }

    public void Hide()
    {
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0);
        //img.enabled = false;
    }

    public void Show()
    {
        img.color = new Color(img.color.r, img.color.g, img.color.b, 0.35f);
        //img.enabled = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        master.SetIndex(index);
    }
}
