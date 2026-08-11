/// <summary>
/// Singleton state of the multiplayer menu: which provider window is currently open.
/// SteamLauncher and RoomSelector update it when opening, closing or switching between
/// the Steam lobby panel and the Photon room list, so both screens stay in sync and
/// the player can always switch back from one provider to the other.
/// </summary>
public static class MultiplayerMenuState
{
    /// <summary>The multiplayer window currently shown.</summary>
    public enum Menu
    {
        /// <summary>No multiplayer window is open; the campaign selection is idle.</summary>
        None,

        /// <summary>The Steam lobby panel is open.</summary>
        Steam,

        /// <summary>The Photon room list is open.</summary>
        Photon
    }

    private static Menu _current = Menu.None;

    /// <summary>Gets the multiplayer window currently shown.</summary>
    public static Menu Current
    {
        get { return _current; }
    }

    /// <summary>Marks the given window as the one currently shown.</summary>
    public static void Open(Menu menu)
    {
        _current = menu;
    }

    /// <summary>Marks the multiplayer menu as closed.</summary>
    public static void Close()
    {
        _current = Menu.None;
    }
}
