namespace ResourceLoading.Mouse
{
    public class MagicianMouse : IMouse
    {
        private int type = 44;

        protected override void O_Load(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.GetRuntimeAnimatorController("Mouse/44/Retinue");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
        }

        protected override void O_UnLoad(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.UnLoadRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Mouse/44/Retinue");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
        }
    }
}