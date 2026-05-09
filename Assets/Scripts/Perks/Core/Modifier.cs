namespace Entropy.Perks
{
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
