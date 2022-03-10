using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IBaseCostController
{
    public float GetCost(string name);
    public void SetCost(string name, float val);
    public void AddCost(string name, float val);
}
