namespace Entropy.Perks
{
    public static class GameEvents
    {
        public static event System.Action<ActionEvent> OnActionTriggered;
        public static void Trigger(ActionEvent evt) => OnActionTriggered?.Invoke(evt);
    }
}