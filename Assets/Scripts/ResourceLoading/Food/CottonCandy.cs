namespace ResourceLoading.Food
{
    public class CottonCandy : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/10/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/10");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/10/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/10");
        }
    }
}