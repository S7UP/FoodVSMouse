namespace ResourceLoading.Food
{
    public class CherryPuddingFoodUnit : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/4/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/4");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/4/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/4");
        }
    }
}