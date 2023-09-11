namespace ResourceLoading.Food
{
    public class MouseCatcher : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/22/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/22");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/22/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/22");
        }
    }
}