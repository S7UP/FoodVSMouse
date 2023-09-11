namespace ResourceLoading.Food
{
    public class TofuPitcher : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + shape + "/GreenBullet");
            GameManager.Instance.GetRuntimeAnimatorController("Food/36/" + shape + "/RedBullet");
            GameManager.Instance.GetRuntimeAnimatorController("Food/36/Poison");
            GameManager.Instance.GetRuntimeAnimatorController("Food/36/PoisonEffect");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/36");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/36/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/36/" + shape + "/GreenBullet");
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/36/" + shape + "/RedBullet");
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/36/Poison");
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/36/PoisonEffect");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/36");
        }
    }
}