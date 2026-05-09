using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public class BulletTimePerk : AdvancedPerk
    {
        [SerializeField] private KeyCode _toggleKey = KeyCode.T;
        [SerializeField] private float _timeScale = 0.12f;
        [SerializeField] private float _cooldown = 10f;

        private bool _isActive;
        private bool _onCooldown;
        private float _originalFixedDeltaTime;

        public override void OnEquip(IModdableStats target)
        {
            GameEvents.OnActionTriggered += OnActionTriggered;
            _originalFixedDeltaTime = Time.fixedDeltaTime;
        }

        public override void OnRemove(IModdableStats target)
        {
            GameEvents.OnActionTriggered -= OnActionTriggered;
            StopAllCoroutines();
            if (_isActive)
                Deactivate();
        }

        void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                TryToggle();
            }
        }

        private void OnActionTriggered(ActionEvent evt)
        {
            TryToggle();
        }

        private void TryToggle()
        {
            if (_onCooldown) return;

            if (_isActive)
            {
                Deactivate();
                StartCoroutine(CooldownRoutine());
            }
            else
            {
                Activate();
            }
        }

        private void Activate()
        {
            _isActive = true;
            Time.timeScale = _timeScale;
            Time.fixedDeltaTime = _originalFixedDeltaTime * _timeScale;
        }

        private void Deactivate()
        {
            _isActive = false;
            Time.timeScale = 1f;
            Time.fixedDeltaTime = _originalFixedDeltaTime;
        }

        private IEnumerator CooldownRoutine()
        {
            _onCooldown = true;
            yield return new WaitForSecondsRealtime(_cooldown);
            _onCooldown = false;
        }
    }
}
