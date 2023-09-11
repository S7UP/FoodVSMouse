namespace ResourceLoading.Food
{
    public class IceCream : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/5/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/5");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/5/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/5");
        }
    }
}