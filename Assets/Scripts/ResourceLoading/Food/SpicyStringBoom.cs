namespace ResourceLoading.Food
{
    public class SpicyStringBoom : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/23/" + shape);
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/23");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/23/" + shape);
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/23");
        }
    }
}