public interface IBaseCardBuilder
{
    public bool CanSelect();

    public bool CanConstructe();

    public void Constructe();

    public void InitInstance();

    public BaseUnit GetResult();
}
