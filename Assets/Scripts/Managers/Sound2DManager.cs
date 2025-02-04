using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UniRx;
using UnityEngine;

namespace TestApp.Managers
{
    public class Sound2DManager : MonoBehaviour, IDisposable
    {
        private const string TAG = "[Sound2DManager]";

        public const float MIN_VOLUME = 0f;
        public const float MAX_VOLUME = 1f;

        private AudioSource _soundSource;

        private BoolReactiveProperty _isSoundOn;

        private IDisposable _soundChangeStateSub;

        private bool _isInited = false;

        private readonly Dictionary<string, Tween> _repeatSoundTweens = new();
        private readonly Dictionary<string, (AudioSource source, int times)> _loopedSoundAudioSources = new();
        private readonly List<string> _loopedSoundLoading = new();
        private readonly Dictionary<string, float> _playingSoundsEndTime = new();

        private bool IsSoundOff => !_isSoundOn.Value;
        private string GetSoundPath(string soundClip) => $"Sounds/{soundClip}";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Init(BoolReactiveProperty isSoundOn)
        {
            _isSoundOn = isSoundOn;

            _soundChangeStateSub?.Dispose();
            _soundChangeStateSub = _isSoundOn
                .Subscribe(enabled =>
                {
                    if (!enabled)
                        TurnOffSound();
                })
                .AddTo(this);

            if (!_soundSource)
                _soundSource = CreateAudioSource(MAX_VOLUME, false);

            _isInited = true;
        }

        private AudioSource CreateAudioSource(float volume, bool loop)
        {
            var source = gameObject.AddComponent<AudioSource>();

            //source.spatialize = false;
            source.spatialBlend = 0f;

            source.volume = volume;
            source.minDistance = 100f;
            source.maxDistance = 100f;
            source.playOnAwake = false;
            source.loop = loop;

            return source;
        }

        private async UniTask LoadClipToSource(string path, AudioSource source, CancellationToken cancellationToken)
        {
            AudioClip clip = await AssetsManager.Instance.LoadAsync<AudioClip>(path);
            cancellationToken.ThrowIfCancellationRequested();

            source.clip = clip;
        }

        public async UniTask PlaySoundAsync(string soundClip, int repeats = 1, float volumeScale = 1f, bool playOnlyIfNotPlaying = false,
            CancellationToken cancellationToken = default)
        {
            if (!_isInited)
                return;

            bool isSoundOff = IsSoundOff;
            Debug.Log($"{TAG} Try play sound \"{soundClip}\". Repeats {repeats} times. Enabled = {!isSoundOff}");

            if (isSoundOff)
                return;

            if (repeats <= 0)
                return;

            if (string.IsNullOrEmpty(soundClip))
                return;

            cancellationToken.ThrowIfCancellationRequested();

            await PlaySoundInternalAsync(soundClip, repeats, volumeScale, playOnlyIfNotPlaying, cancellationToken);
        }

        private async UniTask PlaySoundInternalAsync(string soundClip, int repeats = 1, float volumeScale = 1f, bool playOnlyIfNotPlaying = false,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (playOnlyIfNotPlaying && IsCurrentlyPlaying(soundClip))
                return;

            string path = GetSoundPath(soundClip);
            AudioClip audioClip = await AssetsManager.Instance.LoadAsync<AudioClip>(path);

            cancellationToken.ThrowIfCancellationRequested();

            if (IsSoundOff || (playOnlyIfNotPlaying && IsCurrentlyPlaying(soundClip)))
                return;

            await PlaySoundClipAsync(soundClip, audioClip, repeats, volumeScale, cancellationToken);
        }

        private bool IsCurrentlyPlaying(string soundClip)
        {
            if (_playingSoundsEndTime.ContainsKey(soundClip) && _playingSoundsEndTime[soundClip] > Time.time)
                return true;

            return false;
        }

        private async UniTask PlaySoundClipAsync(string soundClip, AudioClip audioClip, int repeats, float volumeScale = 1f,
            CancellationToken cancellationToken = default)
        {
            if (audioClip == null)
                return;

            float delay = audioClip.length;
            float blockPlayTime = delay * repeats;
            float blockEndTime = Time.time + blockPlayTime;

            if (_playingSoundsEndTime.ContainsKey(soundClip))
                _playingSoundsEndTime[soundClip] = blockEndTime;
            else
                _playingSoundsEndTime.Add(soundClip, blockEndTime);

            PlaySoundClipOneShot(audioClip, volumeScale);

            if (repeats > 1)
            {
                if (_repeatSoundTweens.ContainsKey(soundClip))
                    _repeatSoundTweens[soundClip].Kill();

                _repeatSoundTweens[soundClip] = DOTween.Sequence()
                    .AppendCallback(() => PlaySoundClipOneShot(audioClip, volumeScale))
                    .AppendInterval(delay)
                    .AppendCallback(() => cancellationToken.ThrowIfCancellationRequested())
                    .SetLoops(repeats - 1, LoopType.Restart)
                    .SetLink(gameObject);
            }

            await UniTask.WaitForSeconds(blockPlayTime, cancellationToken: cancellationToken);
        }

        private void PlaySoundClipOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (IsSoundOff)
                return;

            _soundSource.PlayOneShot(clip, volumeScale);
        }

        public void PlayLoopSound(string soundClip, float volumeScale = 1f)
        {
            if (!_isInited)
                return;

            bool isSoundOff = IsSoundOff;
            Debug.Log($"{TAG} Try play loop sound \"{soundClip}\". Enabled = {!isSoundOff}");

            if (isSoundOff)
                return;

            if (_loopedSoundAudioSources.TryGetValue(soundClip, out (AudioSource source, int times) pair))
            {
                pair.source.volume = volumeScale;
                pair.times++;

                if (!pair.source.isPlaying)
                    pair.source.Play();

                _loopedSoundAudioSources[soundClip] = pair;

                return;
            }

            if (string.IsNullOrEmpty(soundClip))
                return;

            StartLoopPlayAsync(soundClip, volumeScale).Forget();
        }

        private async UniTask StartLoopPlayAsync(string soundClip, float volumeScale)
        {
            OnLoopClipStartLoading(soundClip);

            string path = GetSoundPath(soundClip);
            AudioClip audioClip = await AssetsManager.Instance.LoadAsync<AudioClip>(path);

            if (soundClip == null || audioClip == null)
                return;

            //Если звук перестал быть нужен
            if (!IsWaitingForLoopClipLoading(soundClip))
                return;

            OnLoopClipStopLoading(soundClip);

            if (_loopedSoundAudioSources.TryGetValue(soundClip, out (AudioSource source, int times) pair))
            {
                pair.source.volume = volumeScale;
                pair.times++;

                if (!pair.source.isPlaying)
                    pair.source.Play();

                _loopedSoundAudioSources[soundClip] = pair;

                return;
            }

            var source = CreateAudioSource(MAX_VOLUME, true);
            source.clip = audioClip;
            source.volume = volumeScale;
            source.Play();

            _loopedSoundAudioSources[soundClip] = (source, 1);
        }

        public bool StopLoopSound(string soundClip, bool completely = false)
        {
            if (_loopedSoundAudioSources.TryGetValue(soundClip, out (AudioSource source, int times) pair))
            {
                int currentTimes = completely ? 0 : Mathf.Max(0, pair.times - 1);
                pair.times = currentTimes;

                if (currentTimes <= 0)
                    if (pair.source) //От краша при закрытии игры
                        pair.source.Stop();

                _loopedSoundAudioSources[soundClip] = pair;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Остановить проигрывание всех зацикленных звуков.
        /// Не останавливает обычные звуки, для этого используйте <see cref="StopAllPlayingSounds"/>.
        /// </summary>
        public void StopAllLoopedSounds()
        {
            foreach (var key in _loopedSoundAudioSources.Keys.ToArray())
                StopLoopSound(key, completely: true);

            Debug.Log("All looped sounds stopped.");
        }

        /// <summary>
        /// Остановить проигрывание всех звуков.
        /// Не останавливает LoopSounds, для этого используйте <see cref="StopAllLoopedSounds"/>.
        /// </summary>
        public void StopAllPlayingSounds()
        {
            if (_soundSource)
                _soundSource.Stop();

            foreach (var tween in _repeatSoundTweens.Values)
                tween.Kill();

            _repeatSoundTweens.Clear();
        }

        private void OnLoopClipStartLoading(string soundClip)
        {
            if (!_loopedSoundLoading.Contains(soundClip))
                _loopedSoundLoading.Add(soundClip);
        }

        private void OnLoopClipStopLoading(string soundClip)
        {
            if (_loopedSoundLoading.Contains(soundClip))
                _loopedSoundLoading.Remove(soundClip);
        }

        private bool IsWaitingForLoopClipLoading(string soundClip) => _loopedSoundLoading.Contains(soundClip);

        public void TurnOffSound()
        {
            StopAllLoopedSounds();
            StopAllPlayingSounds();
        }

        public async UniTask PreloadSoundClip(string soundClip, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string path = GetSoundPath(soundClip);

            if (!AssetsManager.Instance.IsCached(path))
                await AssetsManager.Instance.LoadAsync<AudioClip>(path);
        }

        public void Dispose()
        {
            StopAllPlayingSounds();
            StopAllLoopedSounds();

            _soundChangeStateSub?.Dispose();
            _soundChangeStateSub = null;

            _isInited = false;
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}
