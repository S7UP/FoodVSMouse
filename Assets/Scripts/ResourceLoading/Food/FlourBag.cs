namespace ResourceLoading.Food
{
    public class FlourBag : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/14/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/14");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/14/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/14");
        }
    }
}