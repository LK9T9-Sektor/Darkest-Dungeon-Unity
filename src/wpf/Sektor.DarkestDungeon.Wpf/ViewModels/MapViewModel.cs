using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Sektor.DarkestDungeon.Wpf.ViewModels
{
    /// <summary>Dungeon map with rooms and the current position.</summary>
    public partial class MapViewModel : ObservableObject
    {
        /// <summary>Gets the room nodes of the map.</summary>
        public ObservableCollection<MapRoomViewModel> Rooms { get; } = new ObservableCollection<MapRoomViewModel>();

        /// <summary>Initializes a new instance of the <see cref="MapViewModel"/> class with a placeholder corridor.</summary>
        public MapViewModel()
        {
            Rooms.Add(new MapRoomViewModel("Start", 24, 250, true));
            Rooms.Add(new MapRoomViewModel("Room", 128, 120, true));
            Rooms.Add(new MapRoomViewModel("Room", 236, 208));
            Rooms.Add(new MapRoomViewModel("Room", 168, 44));
            Rooms.Add(new MapRoomViewModel("Boss", 268, 60));
        }
    }
}
