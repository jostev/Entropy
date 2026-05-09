namespace Entropy.Perks
{
    /// <summary>
    /// Blueprint for every upgrade in the game.
    /// </summary>
    public interface IPerk
    {
        string ID { get; }
        string Title { get; }
        string Description { get; }
        void OnEquip(IModdableStats target);
        void OnRemove(IModdableStats target);
    }
}
