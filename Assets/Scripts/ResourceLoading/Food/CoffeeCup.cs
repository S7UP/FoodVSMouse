namespace ResourceLoading.Food
{
    public class CoffeeCup : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/25/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/25/Bullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/25");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/25/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/25/Bullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/25");
        }
    }
}