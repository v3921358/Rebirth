using Common.Server;

namespace WvsRebirth
{
    /// <summary>
    /// Tiny wrapper for easy insertion into a windows service planned for the future.
    /// </summary>
    public sealed class MapleService
    {
        public WvsCenter WvsCenter { get; }

        public MapleService()
        {
            WvsCenter = new WvsCenter(1);
        }

        public void Start() => WvsCenter.Start();
        public void Stop() => WvsCenter.Stop();
    }
}
