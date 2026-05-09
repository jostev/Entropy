namespace Entropy.Perks
{
    public interface IPerk
    {
        string ID { get; }
        string Title { get; }
        string Description { get; }
        void OnEquip(IModdableStats target);
        void OnRemove(IModdableStats target);
    }
}
