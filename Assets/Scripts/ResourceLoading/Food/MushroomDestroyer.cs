namespace ResourceLoading.Food
{
    public class MushroomDestroyer : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/31/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/31/BoomEffect");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/31");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/31/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/31/BoomEffect");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/31");
        }
    }
}