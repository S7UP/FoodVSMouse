namespace ResourceLoading.Mouse
{
    public class CatapultMouse : IMouse
    {
        private int type = 15;

        protected override void O_Load(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.GetRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.GetRuntimeAnimatorController("Mouse/"+type+"/" + shape + "/Bullet");
            GameManager.Instance.LoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
        }

        protected override void O_UnLoad(int shape)
        {
            for (int i = 0; i < MouseManager.GetAttribute(type, shape).mHertRateList.Count; i++)
                GameManager.Instance.UnLoadRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/" + i);
            GameManager.Instance.UnLoadRuntimeAnimatorController("Mouse/" + type + "/" + shape + "/Bullet");
            GameManager.Instance.UnLoadGameObjectResource(FactoryType.GameFactory, "Mouse/" + type);
        }
    }
}