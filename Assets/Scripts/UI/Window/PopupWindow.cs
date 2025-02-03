using DG.Tweening;
using System.Linq;
using TestApp.UI.Elements;
using TestApp.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TestApp.UI.Window
{
    public class PopupWindow : MonoBehaviour
    {
        [SerializeField] private CanvasGroup _windowCanvas;
        [SerializeField] private RectTransform _windowInnerContainer;
        [Space]
        [SerializeField] private TMP_Text _windowTitle;
        [SerializeField] private TMP_Text _windowDesc;
        [SerializeField] private SimpleButton _okButton;
        [Space]
        [SerializeField] private Image _closeTint;
        [SerializeField] private SimpleButton _closeTintButton;

        public const float OPEN_ANIM_DURATION = .25f;
        public const float CLOSE_ANIM_DURATION = .15f;

        private float _startTintAlpha;

        public void Init()
        {
            _startTintAlpha = _closeTint.color.a;
            _windowCanvas.blocksRaycasts = false;

            _okButton.SetOnClick(Close);
            _closeTintButton.SetOnClick(Close);

            gameObject.SetActive(false);
        }

        public void Open(string titleText, string descText)
        {
            ShowOpenAnim();

            _windowTitle.text = titleText;
            _windowDesc.text = descText;
        }

        private void ShowOpenAnim()
        {
            _closeTint.SetColor(a: 0f);
            _windowCanvas.alpha = 1f;
            _windowCanvas.blocksRaycasts = false;

            _windowInnerContainer.localScale = Vector3.zero;

            gameObject.SetActive(true);

            DOTween.Sequence()
                .Append(_windowInnerContainer.DOScale(Vector3.one, OPEN_ANIM_DURATION).SetEase(Ease.OutBack))
                .Join(_closeTint.DOFade(_startTintAlpha, OPEN_ANIM_DURATION))
                .OnComplete(() => _windowCanvas.blocksRaycasts = true)
                .SetLink(gameObject);
        }

        public void Close()
        {
            _windowCanvas.blocksRaycasts = false;

            _windowCanvas.DOFade(0f, CLOSE_ANIM_DURATION)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    gameObject.SetActive(false);
                });
        }
    }
}
