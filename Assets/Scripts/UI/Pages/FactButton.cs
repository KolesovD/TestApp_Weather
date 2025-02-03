using Cysharp.Threading.Tasks;
using TestApp.Data;
using TestApp.Network;
using TestApp.UI.Elements;
using TestApp.UI.Window;
using TMPro;
using UnityEngine;
using Zenject;

namespace TestApp.UI.Pages
{
    public class FactButton : MonoBehaviour
    {
        [SerializeField] private TMP_Text _factNumberText;
        [SerializeField] private TMP_Text _factName;
        [SerializeField] private GameObject _loaderImage;
        [SerializeField] private SimpleButton _buttonSelf;

        private int _factNumber;
        private FactData _factData;

        private ICommandSender _commandSender;
        private PopupWindow _popupWindow;

        [Inject]
        private void Inject(ICommandSender commandSender, PopupWindow popupWindow)
        {
            _commandSender = commandSender;
            _popupWindow = popupWindow;
        }

        public void Init(int factNumber, FactData factData)
        {
            _factNumber = factNumber;
            _factData = factData;

            _factNumberText.text = _factNumber.ToString();
            _factName.text = _factData.Name;

            _loaderImage.SetActive(false);

            _buttonSelf.SetOnClick(() => OnButtonClick().Forget());
        }

        private async UniTask OnButtonClick()
        {
            _loaderImage.SetActive(true);

            FactData factData = await _commandSender.GetOneFactData(_factData.Id, default);

            _loaderImage.SetActive(false);

            _popupWindow.Open(factData.Name, factData.Description);
        }

        public void Dispose()
        {
            _buttonSelf.Dispose();
        }

        public class Factory : PlaceholderFactory<FactButton, Transform, FactButton>
        {
            //
        }

        public class CustomFactory : IFactory<FactButton, Transform, FactButton>
        {
            private DiContainer _container;

            public CustomFactory(DiContainer container)
            {
                _container = container;
            }

            public FactButton Create(FactButton prefab, Transform parent)
            {
                return _container.InstantiatePrefabForComponent<FactButton>(prefab, parent);
            }
        }
    }
}
