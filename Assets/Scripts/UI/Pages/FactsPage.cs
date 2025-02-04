using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TestApp.Data;
using TestApp.Network;
using TestApp.ObjectPool;
using TestApp.UI.Window;
using TestApp.Utils;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace TestApp.UI.Pages
{
    public class FactsPage : AbstractMenuPage
    {
        [SerializeField] private GameObject _factsLoaderContainer;
        [Space]
        [SerializeField] private GameObject _factsDataContainer;
        [SerializeField] private ScrollRect _factsScroll;
        [SerializeField] private RectTransform _factsDataScrollContainer;
        [SerializeField] private FactButton _factButtonPrefab;

        private FactButton.Factory _factButtonsFactory;
        private IRequestSender _commandSender;
        private PopupWindow _popupWindow;

        private PoolObjectDirectList<FactButton, FactButton> _factButtonsPool;

        private const int MAX_BUTTONS = 10;

        private List<FactButton> _factButtonsList = new(MAX_BUTTONS);
        private FactButton _buttonWaitingForResponse;

        [Inject]
        private void Inject(FactButton.Factory factButtonsFactory, IRequestSender commandSender, PopupWindow popupWindow)
        {
            _factButtonsFactory = factButtonsFactory;
            _commandSender = commandSender;
            _popupWindow = popupWindow;
        }

        public override void Init()
        {
            base.Init();

            _factButtonsPool = new PoolObjectDirectList<FactButton, FactButton>(_factsDataScrollContainer,
                prefab => _factButtonsFactory.Create(prefab, _factsDataScrollContainer));
        }

        protected override void OnShow()
        {
            SetLoader();

            SendFactsRequestAndShowResults().Forget();
        }

        protected override void OnHide()
        {
            ClearButtons();

            _commandSender.CancellFactsDataRequest();
            _commandSender.CancellFactInfoRequest();
        }

        private async UniTask SendFactsRequestAndShowResults()
        {
            Debug.Log("Try send facts request");

            try
            {
                var factsData = await _commandSender.GetFactsData();

                ShowResults(factsData);
            }
            catch (OperationCanceledException ex)
            {
                //
            }
        }

        private void ShowResults(FactsArrayData factsArray)
        {
            if (factsArray.Data == null || factsArray.Data.Length == 0)
                return;

            _factsLoaderContainer.SetActive(false);
            _factsDataContainer.SetActive(true);

            int maxLength = Mathf.Min(factsArray.Data.Length, MAX_BUTTONS);

            for (int i = 0; i < maxLength; i++)
            {
                var nextButton = _factButtonsPool.Rent(_factButtonPrefab, _factsDataScrollContainer);
                nextButton.Init(i + 1, factsArray.Data[i], (factId, button) => OnButtonClick(factId, button).Forget());

                _factButtonsList.Add(nextButton);
            }

            LayoutRebuilder.ForceRebuildLayoutImmediate(_factsDataScrollContainer);
            _factsScroll.normalizedPosition = _factsScroll.normalizedPosition.Set(y: 1f);
        }

        private async UniTask OnButtonClick(string factId, FactButton button)
        {
            if (_buttonWaitingForResponse == button)
                return;

            Debug.Log("Try send fact info request");

            try
            {
                if (_buttonWaitingForResponse != null)
                {
                    _buttonWaitingForResponse.TurnLoader(false);
                    _commandSender.CancellFactInfoRequest();
                }

                _buttonWaitingForResponse = button;

                button.TurnLoader(true);

                FactsInfoData factInfo = await _commandSender.GetFactInfoData(factId);

                button.TurnLoader(false);

                if (_buttonWaitingForResponse == button)
                    _buttonWaitingForResponse = null;

                _popupWindow.Open(factInfo.Data.Name, factInfo.Data.Description);
            }
            catch (OperationCanceledException ex)
            {
                button.TurnLoader(false);

                if (_buttonWaitingForResponse == button)
                    _buttonWaitingForResponse = null;
            }
        }

        private void ClearButtons()
        {
            foreach (var button in _factButtonsList)
            {
                button.Dispose();
                _factButtonsPool.Return(_factButtonPrefab, button);
            }

            _factButtonsList.Clear();

            _buttonWaitingForResponse = null;
        }

        private void SetLoader()
        {
            _factsLoaderContainer.SetActive(true);
            _factsDataContainer.SetActive(false);
        }
    }
}
