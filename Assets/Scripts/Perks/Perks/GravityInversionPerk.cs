using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Entropy.Perks
{
    public class GravityInversionPerk : AdvancedPerk
    {
        [SerializeField] private KeyCode _activationKey = KeyCode.G;
        [SerializeField] private float _duration = 8f;
        [SerializeField] private float _enemyRecalibrationDelay = 1.5f;
        [SerializeField] private float _cooldown = 15f;

        private bool _isActive;
        private float _lastActivationTime = -999f;
        private Vector3 _originalGravity;
        private Coroutine _activeRoutine;

        public override void OnEquip(IModdableStats target)
        {
            _originalGravity = Physics.gravity;
        }

        public override void OnRemove(IModdableStats target)
        {
            if (_activeRoutine != null)
                StopCoroutine(_activeRoutine);

            Physics.gravity = _originalGravity;

            EnemyController[] enemies = FindObjectsByType<EnemyController>();
            foreach (var ec in enemies)
            {
                if (ec != null)
                    ec.SetGravity(_originalGravity, 0.5f);
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(_activationKey) && !_isActive && Time.time >= _lastActivationTime + _cooldown)
            {
                _activeRoutine = StartCoroutine(ActivateInversion());
            }
        }

        private IEnumerator ActivateInversion()
        {
            _isActive = true;
            _lastActivationTime = Time.time;

            Vector3 invertedGravity = new Vector3(_originalGravity.x, -_originalGravity.y, _originalGravity.z);
            Physics.gravity = invertedGravity;

            EnemyController[] enemies = FindObjectsByType<EnemyController>();
            foreach (var ec in enemies)
            {
                if (ec != null)
                    ec.SetGravity(invertedGravity, _enemyRecalibrationDelay);
            }

            yield return new WaitForSeconds(_duration);

            Physics.gravity = _originalGravity;

            enemies = FindObjectsByType<EnemyController>();
            foreach (var ec in enemies)
            {
                if (ec != null)
                    ec.SetGravity(_originalGravity, _enemyRecalibrationDelay);
            }

            _isActive = false;
        }
    }
}
