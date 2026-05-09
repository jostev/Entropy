using System.Collections;
using UnityEngine;

namespace Entropy.Perks
{
    public class EntropyFieldPerk : AdvancedPerk
    {
        [SerializeField] private float _changeInterval = 3f;
        [SerializeField] private float _maxGravityDeviation = 4f;
        [SerializeField] private float _maxFrictionDeviation = 0.4f;
        [SerializeField] private float _maxDragDeviation = 2f;

        private Vector3 _originalGravity;
        private Coroutine _entropyRoutine;

        public override void OnEquip(IModdableStats target)
        {
            _originalGravity = Physics.gravity;
            _entropyRoutine = StartCoroutine(EntropyLoop());
        }

        public override void OnRemove(IModdableStats target)
        {
            if (_entropyRoutine != null)
                StopCoroutine(_entropyRoutine);

            Physics.gravity = _originalGravity;
        }

        private IEnumerator EntropyLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(_changeInterval);

                Vector3 randomGravity = new Vector3(
                    Random.Range(-_maxGravityDeviation, _maxGravityDeviation),
                    -9.81f + Random.Range(-_maxGravityDeviation, _maxGravityDeviation),
                    Random.Range(-_maxGravityDeviation, _maxGravityDeviation)
                );
                Physics.gravity = randomGravity;

                float randomDrag = Random.Range(-_maxDragDeviation, _maxDragDeviation);
                float randomFriction = Random.Range(-_maxFrictionDeviation, _maxFrictionDeviation);
            }
        }
    }
}
