using Cysharp.Threading.Tasks;
using DG.Tweening;
using TestApp.Managers;
using TestApp.UI;
using TestApp.UI.Window;
using UnityEngine;
using UniRx;
using Zenject;

namespace TestApp
{
    public class Initer : MonoBehaviour
    {
        private Sound2DManager _sound2DManager;
        private MainHUDContent _HUDContent;
        private PopupWindow _popupWindow;

        [Inject]
        private void Inject(Sound2DManager sound2DManager, MainHUDContent HUDContent, PopupWindow popupWindow)
        {
            _sound2DManager = sound2DManager;
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

            _sound2DManager.Init(new BoolReactiveProperty(true));

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