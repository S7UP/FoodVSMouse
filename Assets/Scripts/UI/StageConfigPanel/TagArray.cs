using UnityEngine;
namespace UIPanel.StageConfigPanel
{
    public class TagArray : MonoBehaviour, IGameControllerMember
    {
        private Tag[] arr = new Tag[4];

        private void Awake()
        {

        }

        public void MInit()
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = Tag.GetInstance();
                arr[i].transform.SetParent(transform);
                arr[i].transform.localScale = Vector2.one;
            }
        }

        public void MUpdate()
        {
            
        }
        public void MPause()
        {
            
        }

        public void MPauseUpdate()
        {
            
        }

        public void MResume()
        {
            
        }
        public void MDestory()
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i].MDestory();
            }
            ExecuteRecycle();
        }

        public Tag GetTag(int index)
        {
            return arr[index];
        }

        public static TagArray GetInstance()
        {
            TagArray t = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "StageConfigPanel/TagArray").GetComponent<TagArray>();
            t.MInit();
            return t;
        }

        private void ExecuteRecycle()
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "StageConfigPanel/TagArray", gameObject);
        }
    }

}
