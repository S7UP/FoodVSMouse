namespace ResourceLoading.Food
{
    public class PineappleBreadBoom : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/19/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/19");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/19/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/19");
        }
    }
}