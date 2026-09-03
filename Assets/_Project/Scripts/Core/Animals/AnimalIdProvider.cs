namespace ZooWorld.Core.Animals
{
    public sealed class AnimalIdProvider
    {
        private long _lastId;

        public long Next()
        {
            _lastId = checked(_lastId + 1);
            return _lastId;
        }
    }
}
