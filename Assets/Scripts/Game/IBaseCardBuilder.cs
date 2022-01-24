using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseCardBuilder
{
    public bool CanSelect();

    public bool CanConstructe();

    public void Constructe();

    public void InitInstance();

    public BaseUnit GetResult();

    public void CardBuilderUpdate();
}
