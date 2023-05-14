using UnityEngine;
using UnityEngine.UI;
using S7P.State;

namespace UIPanel.StageConfigPanel
{
    public class Tag : MonoBehaviour, IGameControllerMember
    {
        public Button Btn;
        public Image BackImg;
        public Image Img;
        public StateController mStateController = new StateController();

        private void Awake()
        {
            BackImg = GetComponent<Image>();
            Btn = transform.Find("Button").GetComponent<Button>();
            Img = transform.Find("Button").GetComponent<Image>();
        }

        public void MInit()
        {
            Btn.interactable = true;
            Btn.onClick.RemoveAllListeners();
            Img.sprite = null;
            BackImg.color = new Color(0, 32f / 255, 63f / 255, 1);

            // 初始化状态
            {
                mStateController.Initial();
                // 未选中的状态
                mStateController.AddCreateStateFunc("Unselected", delegate {
                    BaseState s = new BaseState();
                    s.AddOnEnterAction(delegate {
                        Img.color = new Color(73f / 255, 121f / 255, 168f / 255, 1);
                    });
                    s.AddOnExitAction(delegate {
                    });
                    return s;
                });
                // 已选中状态
                mStateController.AddCreateStateFunc("Selected", delegate {
                    BaseState s = new BaseState();
                    s.AddOnEnterAction(delegate {
                        Img.color = new Color(172f / 255, 213f / 255, 255f / 255, 1);
                    });
                    s.AddOnExitAction(delegate {
                    });
                    return s;
                });
                // 隐藏状态
                mStateController.AddCreateStateFunc("Hide", delegate {
                    BaseState s = new BaseState();
                    s.AddOnEnterAction(delegate {
                        Img.color = new Color(1, 1, 1, 0);
                        BackImg.color = new Color(0, 32f / 255, 63f / 255, 0);
                        Btn.interactable = false;
                    });
                    s.AddOnExitAction(delegate {
                        BackImg.color = new Color(0, 32f / 255, 63f / 255, 1);
                        Btn.interactable = true;
                    });
                    return s;
                });
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
            Btn.onClick.RemoveAllListeners();
            Img.sprite = null;
            ExecuteRecycle();
        }

        #region 以下方法供外界调用


        public void ChangeSprite(Sprite sprite)
        {
            Img.sprite = sprite;
        }
        #endregion

        public static Tag GetInstance()
        {
            Tag t = GameManager.Instance.GetGameObjectResource(FactoryType.UIFactory, "StageConfigPanel/Tag").GetComponent<Tag>();
            t.MInit();
            return t;
        }

        private void ExecuteRecycle()
        {
            GameManager.Instance.PushGameObjectToFactory(FactoryType.UIFactory, "StageConfigPanel/Tag", gameObject);
        }
    }
}

