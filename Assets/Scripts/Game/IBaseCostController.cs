public interface IBaseCostController
{
    public float GetCost(string name);
    public void SetCost(string name, float val);
    public void AddCost(string name, float val);
}
