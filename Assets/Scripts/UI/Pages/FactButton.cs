using System;
using TestApp.Data;
using TestApp.UI.Elements;
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
        private Action<string, FactButton> _onButtonClick;

        //private IRequestSender _commandSender;

        //[Inject]
        //private void Inject(IRequestSender commandSender)
        //{
        //    _commandSender = commandSender;
        //}

        public void Init(int factNumber, FactData factData, Action<string, FactButton> onButtonClick)
        {
            _factNumber = factNumber;
            _factData = factData;
            _onButtonClick = onButtonClick;

            _factNumberText.text = _factNumber.ToString();
            _factName.text = _factData.Name;

            TurnLoader(false);

            _buttonSelf.SetOnClick(() => OnButtonClick());
        }

        private void OnButtonClick()
        {
            _onButtonClick?.Invoke(_factData.Id, this);
        }

        public void TurnLoader(bool value)
        {
            _loaderImage.SetActive(value);
        }

        public void Dispose()
        {
            _buttonSelf.Dispose();
            TurnLoader(false);
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
