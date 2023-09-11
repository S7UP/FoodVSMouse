namespace ResourceLoading.Food
{
    public class HeaterFoodUnit : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/8/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/8/Bullet"+shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/8/SpBullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/8");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/8/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/8/Bullet" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/8/SpBullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/8");
        }
    }
}