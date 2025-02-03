using Cysharp.Threading.Tasks;
using DG.Tweening;
using TestApp.Managers;
using TestApp.UI;
using TestApp.UI.Window;
using UnityEngine;
using Zenject;

namespace TestApp
{
    public class Initer : MonoBehaviour
    {
        private MainHUDContent _HUDContent;
        private PopupWindow _popupWindow;

        [Inject]
        private void Inject(MainHUDContent HUDContent, PopupWindow popupWindow)
        {
            _HUDContent = HUDContent;
            _popupWindow = popupWindow;
        }

        private void Awake()
        {
            Init().Forget();
        }

        private async UniTask Init()
        {
            DOTween.SetTweensCapacity(500, 250);

            _HUDContent.Init();
            _popupWindow.Init();

            await LoadAddressableCatalogs();
        }

        private async UniTask LoadAddressableCatalogs()
        {
            await AssetsManager.Instance.InitializeAddressables();
            await AssetsManager.Instance.LoadAddressables();
        }
    }
}