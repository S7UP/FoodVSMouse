namespace ResourceLoading.Food
{
    public class HotDog : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/41/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/41/Bullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/41");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/41/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/41/Bullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/41");
        }
    }
}