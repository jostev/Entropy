namespace Entropy.Perks
{
    /// <summary>
    /// A single stat change from a perk, trackable by source for cleanup.
    /// </summary>
    public class Modifier
    {
        public float Value;
        public ModifierType Type;
        public object Source;

        public Modifier(float value, ModifierType type, object source)
        {
            Value = value;
            Type = type;
            Source = source;
        }
    }
}
