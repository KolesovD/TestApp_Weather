using TestApp.Managers;
using TestApp.Network;
using Zenject;
using UnityEngine;
using TestApp.UI;
using TestApp.UI.Window;
using TestApp.UI.Pages;

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

            Container.Bind<ICommandSender>().To<FakeCommandSender>().FromNew().AsSingle();
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
