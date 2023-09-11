namespace ResourceLoading.Food
{
    public class IceBucketFoodUnit : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/11/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/11/BoomEffect");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/11");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/11/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/11/BoomEffect");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/11");
        }
    }
}