namespace Entropy.Perks
{
    public abstract class EnemyState
    {
        protected readonly EnemyController enemy;

        public EnemyState(EnemyController enemy)
        {
            this.enemy = enemy;
        }

        public virtual void Enter() { }
        public virtual void Tick() { }
        public virtual void Exit() { }
    }
}
