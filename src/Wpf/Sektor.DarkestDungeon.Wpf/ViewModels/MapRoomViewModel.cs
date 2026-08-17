using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>A map room node placed on the map canvas.</summary>
    public partial class MapRoomViewModel : ObservableObject
    {
        /// <summary>Gets the room type label.</summary>
        public string Type { get; }

        /// <summary>Gets the horizontal position on the map canvas.</summary>
        public double X { get; }

        /// <summary>Gets the vertical position on the map canvas.</summary>
        public double Y { get; }

        /// <summary>Gets a value indicating whether the room has been visited.</summary>
        public bool IsVisited { get; }

        /// <summary>Gets a value indicating whether this is the current position.</summary>
        public bool IsCurrent { get; }

        /// <summary>Initializes a new instance of the <see cref="MapRoomViewModel"/> class.</summary>
        public MapRoomViewModel(string type, double x, double y, bool isCurrent = false)
        {
            Type = type;
            X = x;
            Y = y;
            IsVisited = isCurrent;
            IsCurrent = isCurrent;
        }
    }
}
