namespace ResourceLoading.Food
{
    public class CoffeePowder : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/3/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/3");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/3/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/3");
        }
    }
}
