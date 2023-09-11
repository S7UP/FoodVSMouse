namespace ResourceLoading.Food
{
    public class WoodenDisk : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/9/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/9");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/9/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/9");
        }
    }
}