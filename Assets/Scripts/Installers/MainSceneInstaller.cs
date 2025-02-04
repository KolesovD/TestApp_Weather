using TestApp.Managers;
using TestApp.Network;
using TestApp.UI;
using TestApp.UI.Pages;
using TestApp.UI.Window;
using UnityEngine;
using Zenject;

namespace TestApp.Installers
{
    public class MainSceneInstaller : MonoInstaller
    {
        [SerializeField] private MainHUDContent _HUDContent;
        [SerializeField] private PopupWindow _popupWindow;

        public override void InstallBindings()
        {
            InstallHUD();

            Container.Bind<Sound2DManager>().FromNewComponentOnNewGameObject().WithGameObjectName("Sound2D").AsSingle();

            Container.Bind<IRequestSender>().To<RequestSender>().FromNew().AsSingle();
        }

        private void InstallHUD()
        {
            Container.Bind<MainHUDContent>().FromInstance(_HUDContent).AsSingle();
            Container.Bind<PopupWindow>().FromInstance(_popupWindow).AsSingle();

            Container.BindFactory<FactButton, Transform, FactButton, FactButton.Factory>()
                .FromIFactory(b => b.To<FactButton.CustomFactory>().AsCached());
        }
    }
}
