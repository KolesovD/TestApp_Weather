using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestApp.Data;
using TestApp.Network;
using TestApp.ObjectPool;
using UnityEngine;
using Zenject;

namespace TestApp.UI.Pages
{
    public class FactsPage : AbstractMenuPage
    {
        [SerializeField] private GameObject _factsLoaderContainer;
        [Space]
        [SerializeField] private GameObject _factsDataContainer;
        [SerializeField] private RectTransform _factsDataScrollContainer;
        [SerializeField] private FactButton _factButtonPrefab;
        //[SerializeField] private TMP_Text _weatherText;

        private FactButton.Factory _factButtonsFactory;
        private ICommandSender _commandSender;

        private PoolObjectDirectList<FactButton, FactButton> _factButtonsPool;
        private CancellationTokenSource _cts;

        private List<FactButton> _factButtonsList = new(10);

        [Inject]
        private void Inject(FactButton.Factory factButtonsFactory, ICommandSender commandSender)
        {
            _factButtonsFactory = factButtonsFactory;
            _commandSender = commandSender;
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
            DisposeCancellationToken();
        }

        private async UniTask SendFactsRequestAndShowResults()
        {
            Debug.Log("Try send facts request");

            try
            {
                if (_cts == null)
                    CreateNewCancellationToken();

                var factsData = await _commandSender.GetFactsData(_cts.Token);

                ShowResults(factsData);
            }
            catch (OperationCanceledException ex)
            {
                //
            }
        }

        private void ShowResults(FactsArrayData factsArray)
        {
            _factsLoaderContainer.SetActive(false);
            _factsDataContainer.SetActive(true);

            for (int i = 0; i < factsArray.Data.Length; i++)
            {
                var nextButton = _factButtonsPool.Rent(_factButtonPrefab, _factsDataScrollContainer);
                nextButton.Init(i + 1, factsArray.Data[i]);

                _factButtonsList.Add(nextButton);
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
        }

        private void SetLoader()
        {
            _factsLoaderContainer.SetActive(true);
            _factsDataContainer.SetActive(false);
        }

        private void CreateNewCancellationToken()
        {
            _cts = new CancellationTokenSource();
        }

        private void DisposeCancellationToken()
        {
            if (_cts != null)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
            }
        }
    }
}
