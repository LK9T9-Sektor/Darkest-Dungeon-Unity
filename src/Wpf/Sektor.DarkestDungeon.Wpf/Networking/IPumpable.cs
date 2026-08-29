namespace Sektor.DarkestDungeon.Wpf.Networking
{
    /// <summary>A screen whose background work must be pumped regularly by the UI loop.</summary>
    public interface IPumpable
    {
        /// <summary>Pumps pending transport callbacks and messages.</summary>
        void Pump();
    }
}
