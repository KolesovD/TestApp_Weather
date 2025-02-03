using DG.Tweening;
using UnityEngine;

namespace TestApp.Utils
{
    public class SelfRotateObject : MonoBehaviour
    {
        [SerializeField] private float _fullRotateTime = 10f;
        [SerializeField] private bool _clockwise = false;
        [Space]
        [SerializeField] private Ease _easing = Ease.Linear;

        private Tween _rotateTween;

        private void Awake()
        {
            _rotateTween = transform.DORotate(new Vector3(0f, 0f, _clockwise ? -360f : 360f), _fullRotateTime, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(_easing)
                .SetLink(gameObject);
        }

        private void OnEnable()
        {
            _rotateTween.Play();
        }

        private void OnDisable()
        {
            _rotateTween.Pause();
        }
    }
}
