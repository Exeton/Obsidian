namespace Obsidian.API
{
    public interface IWorld
    {
        public string Name { get; }
        public bool Loaded { get; }

        public long Time { get; }
        public Gamemode GameType { get; }
        public bool TryAddEntity(IEntity entity);
    }
}
