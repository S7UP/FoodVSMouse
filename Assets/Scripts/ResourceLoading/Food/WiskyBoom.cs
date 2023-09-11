namespace ResourceLoading.Food
{
    public class WiskyBoom : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/18/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/18");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/18/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/18");
        }
    }
}