using UnityEngine;

namespace Entropy.Perks
{
    public class ActionEvent
    {
        public string EventName;
        public GameObject Source;
        public GameObject Target;
        public Vector3 Position;
        public float Value;
        public bool Handled;
    }
}