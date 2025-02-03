using UnityEngine;

namespace TestApp.UI.Pages
{
    public abstract class AbstractMenuPage : MonoBehaviour
    {
        public virtual void Init()
        {
            //
        }

        public void Show()
        {
            if (IsShowing())
                return;

            gameObject.SetActive(true);
            OnShow();
        }

        public void Hide()
        {
            if (!IsShowing())
                return;

            OnHide();
            gameObject.SetActive(false);
        }

        public bool IsShowing()
        {
            return gameObject.activeSelf;
        }

        protected abstract void OnShow();
        protected abstract void OnHide();
    }
}