using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using TestApp.Managers;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

namespace TestApp.UI.Elements
{
    public class SimpleButton : MonoBehaviour, IPointerDownHandler,
        IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IDisposable
    {
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _innerScaler;

        public bool NeedAnimateOnClick = true;
        public bool NeedAnimateOnUnPress = true;

        [SerializeField, ShowIf("NeedAnimateOnClick")]
        private float scaleX = .95f;

        [SerializeField, ShowIf("NeedAnimateOnClick")]
        private float scaleY = .95f;

        [Space]
        [SerializeField, FoldoutGroup("Button Lock Graphics", expanded: true)]
        private GameObject _enabledGraphics;

        [SerializeField, FoldoutGroup("Button Lock Graphics")]
        private GameObject _disabledGraphics;

        public Button Button => _button;

        private readonly Button.ButtonClickedEvent _onClick = new Button.ButtonClickedEvent();
        public Button.ButtonClickedEvent onClick => _button != null ? _button.onClick : _onClick;

        private Vector3 _baseScale;
        protected Sequence _selectTween;

        private Sound2DManager _sound2D;

        private bool _wasPointerDown = false;
        private bool _wasPointerExit = false;

        private event Action OnLongClickCallback;
        private bool _longClickBlockClick = false;
        private IDisposable _longClickSubscription;

        private bool _wasClickOnLongClickBlocked = false;

        protected const double LONG_CLICK_TIME = .6d;

        /** Возоможно ли нажатие */
        public bool Locked { get; private set; } = false;

        /** Кликабельность по кнопке */
        private bool _touchable = true;
        public bool Touchable
        {
            get => _touchable;
            set
            {
                _touchable = value;

                if (_button)
                    _button.interactable = _touchable;
            }
        }

        /// <summary>
        /// Установить состояние кнопки
        /// </summary>
        /// <param name="locked"><see langword="true"/> - кнопка выключена, <see langword="false"/> - кнопка включена</param>
        /// <param name="touchable">Разрешить нажатие кнопки. Только для выключенного состояния. При включении, кнопка всегда кликабельна.</param>
        public virtual void SetLock(bool locked, bool touchable = false)
        {
            //if (Locked == locked)
            //    return;

            Locked = locked;
            Touchable = locked ? touchable : true; // При снятии лока всегда возвращаем кликабельность кнопки

            if (_enabledGraphics && _disabledGraphics)
            {
                _enabledGraphics.SetActive(!locked);
                _disabledGraphics.SetActive(locked);
            }
        }

        [Inject]
        private void Inject(Sound2DManager sound2D)
        {
            _sound2D = sound2D;
        }

        public void Awake()
        {
            if (!_button)
                _button = GetComponent<Button>();

            if (_innerScaler)
                _baseScale = _innerScaler.localScale;

            OnAwake();
        }

        public virtual void OnAwake() { }

        public virtual void OnSelectAnimation()
        {
            if (!this || !gameObject || !_innerScaler)
                return;

            _selectTween?.Kill();
            _selectTween = DOTween.Sequence()
                .SetLink(gameObject)
                .Append(_innerScaler.DOScale(new Vector3(_baseScale.x * 0.95f, _baseScale.y * 1.01f, _baseScale.z), 0.05f))
                .Append(_innerScaler.DOScale(new Vector3(_baseScale.x * 1.05f, _baseScale.y * 0.9f, _baseScale.z), 0.05f))
                .Append(_innerScaler.DOScale(new Vector3(_baseScale.x * 0.95f, _baseScale.y * 1.01f, _baseScale.z), 0.05f))
                .Append(_innerScaler.DOScale(_baseScale, 0.05f))
                .OnComplete(() => _selectTween = null)
                .OnKill(() => _innerScaler.localScale = _baseScale);

            _selectTween.Play();
        }

        private Sequence _pressTween;
        public virtual void OnPressAnimationStart()
        {
            if (!this || !gameObject || !_innerScaler)
                return;

            if (_pressTween != null)
            {
                _pressTween.Rewind();
                _pressTween.Play();
                return;
            }

            _pressTween = DOTween.Sequence()
                .Append(_innerScaler.DOScale(new Vector3(_baseScale.x * scaleX, _baseScale.y * scaleY, _baseScale.z), 0.05f));

            _pressTween.SetLink(gameObject);
            _pressTween.SetAutoKill(false);

            _pressTween.Play();
        }

        public virtual void OnPressAnimationFinished()
        {
            if (!gameObject)
                return;

            if (_pressTween != null)
            {
                _pressTween.Rewind();
                return;
            }
        }

        public void StopAnim()
        {
            _selectTween?.Kill();
            _selectTween = null;
        }

        public void SetBaseScale()
        {
            if (!this || !gameObject || !_innerScaler)
                return;

            _innerScaler.localScale = _baseScale;
        }

        internal void PlaySoundClick()
        {
            _sound2D.PlaySoundAsync("button_click").Forget();
        }

        public void SetOnClick(Action onClick)
        {
            this.onClick.RemoveAllListeners();
            this.onClick.AddListener(() => onClick?.Invoke());
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Touchable)
                return;

            _wasPointerDown = true;
            _wasPointerExit = false;

            if (NeedAnimateOnClick)
                OnPressAnimationStart();

            if (OnLongClickCallback != null)
            {
                _longClickSubscription?.Dispose();
                _longClickSubscription = Observable.Timer(TimeSpan.FromSeconds(LONG_CLICK_TIME))
                    .Subscribe(_ => OnLongClick())
                    .AddTo(this);
            }
        }

        private void OnLongClick()
        {
            OnLongClickCallback?.Invoke();

            if (NeedAnimateOnClick)
                OnPressAnimationFinished();

            if (_longClickBlockClick)
                _wasClickOnLongClickBlocked = true;
        }

        public void SetOnLongClick(Action onLongClick, bool blockClickOnLongClick)
        {
            OnLongClickCallback = onLongClick;
            _longClickBlockClick = blockClickOnLongClick;

            _wasClickOnLongClickBlocked = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _wasPointerDown = false;

            _longClickSubscription?.Dispose();
            _longClickSubscription = null;

            if (_wasPointerExit)
            {
                _wasPointerExit = false;
                if (NeedAnimateOnClick)
                    OnPressAnimationFinished();

                _wasClickOnLongClickBlocked = false;

                return;
            }

            if (_wasClickOnLongClickBlocked)
            {
                if (NeedAnimateOnClick)
                    OnPressAnimationFinished();

                _wasClickOnLongClickBlocked = false;

                return;
            }

            //Брать приходится каждый раз, т.к. парент у кнопок может поменятся
            var parentCanvas = GetComponentInParent<CanvasGroup>();
            if (parentCanvas != null && !parentCanvas.interactable)
                return;

            if (!Touchable)
            {
                if (NeedAnimateOnClick)
                    OnPressAnimationFinished();

                return;
            }

            var isDrag = eventData.dragging;

            if (isDrag)
            {
                if (NeedAnimateOnClick)
                    OnPressAnimationFinished();

                return;
            }

            if (NeedAnimateOnClick)
            {
                if (NeedAnimateOnUnPress)
                    OnSelectAnimation();
                else
                    OnPressAnimationFinished();
            }

            if (!_button)
                _onClick?.Invoke();

            PlaySoundClick();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_wasPointerDown)
                return;

            if (_wasClickOnLongClickBlocked)
                return;

            _wasPointerExit = false;
            if (NeedAnimateOnClick)
                OnPressAnimationStart();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_wasPointerDown)
                return;

            if (_wasClickOnLongClickBlocked)
                return;

            _wasPointerExit = true;
            if (NeedAnimateOnClick)
                OnPressAnimationFinished();
        }

        public void DisposeClickListeners()
        {
            onClick.RemoveAllListeners();
        }

        public void DisposeLongClickListeners()
        {
            _longClickBlockClick = false;
            OnLongClickCallback = null;

            _longClickSubscription?.Dispose();
            _longClickSubscription = null;

            _wasClickOnLongClickBlocked = false;
        }

        public virtual void Dispose()
        {
            DisposeClickListeners();
            DisposeLongClickListeners();

            StopAnim();
            SetBaseScale();
        }

        protected virtual void OnDestroy()
        {
            StopAnim();
        }
    }

    public static class RXBasicButtonExtension
    {
        public static IObservable<Unit> OnClickAsObservable(this SimpleButton button)
        {
            return button.onClick.AsObservable();
        }
    }
}
