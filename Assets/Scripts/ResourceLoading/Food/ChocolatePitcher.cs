namespace ResourceLoading.Food
{
    public class ChocolatePitcher : IFood
    {
        protected override void O_Load(int shape)
        {
            GameManager.Instance.GetRuntimeAnimatorController("Food/35/" + shape);
            GameManager.Instance.GetRuntimeAnimatorController("Food/35/BigBullet");
            GameManager.Instance.GetRuntimeAnimatorController("Food/35/SmallBullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Food/35");
        }

        protected override void O_UnLoad(int shape)
        {
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/35/" + shape);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/35/BigBullet");
            GameManager.Instance.UnLoadRuntimeAnimatorController("Food/35/SmallBullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Food/35");
        }
    }
}