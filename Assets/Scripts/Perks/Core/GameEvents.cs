namespace Entropy.Perks
{
    /// <summary>
    /// Static event bus for perk-triggered game actions.
    /// </summary>
    public static class GameEvents
    {
        public static event System.Action<ActionEvent> OnActionTriggered;
        public static void Trigger(ActionEvent evt) => OnActionTriggered?.Invoke(evt);
    }
}