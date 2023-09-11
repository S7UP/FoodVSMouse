namespace ResourceLoading.Food
{
    public class SpinCoffee : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/20/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/20");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/20/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/20");
        }
    }
}