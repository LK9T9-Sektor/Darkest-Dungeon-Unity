using System.IO;

namespace Sektor.DarkestDungeon.Core.Save
{
    /// <summary>
    /// File-backed storage contract for save slots and dungeon maps. The presentation layer
    /// provides a concrete implementation (e.g. over the engine's persistent data path); the
    /// core consumes only this abstraction so binary serialization stays engine-free.
    /// </summary>
    public interface ISaveStorage
    {
        /// <summary>Gets the file name of the given save slot.</summary>
        /// <param name="slotId">The save slot id.</param>
        /// <returns>The fully qualified slot file name.</returns>
        string GetSaveFileName(int slotId);

        /// <summary>Gets the file name of the given dungeon map.</summary>
        /// <param name="mapName">The map name.</param>
        /// <returns>The fully qualified map file name.</returns>
        string GetMapFileName(string mapName);

        /// <summary>Creates the save directory when missing.</summary>
        void EnsureSaveDirectory();

        /// <summary>Creates the map directory when missing.</summary>
        void EnsureMapDirectory();

        /// <summary>Reports whether the given slot file exists.</summary>
        /// <param name="slotId">The save slot id.</param>
        /// <returns>True when the slot file exists.</returns>
        bool SaveExists(int slotId);

        /// <summary>Deletes the given slot file when present.</summary>
        /// <param name="slotId">The save slot id.</param>
        void DeleteSave(int slotId);

        /// <summary>Opens the slot file for writing (created when missing).</summary>
        /// <param name="slotId">The save slot id.</param>
        /// <returns>The writable stream.</returns>
        Stream OpenSaveForWrite(int slotId);

        /// <summary>Opens the slot file for reading.</summary>
        /// <param name="slotId">The save slot id.</param>
        /// <returns>The readable stream, or null when the file is absent.</returns>
        Stream OpenSaveForRead(int slotId);

        /// <summary>Opens the map file for writing (created when missing).</summary>
        /// <param name="mapName">The map name.</param>
        /// <returns>The writable stream.</returns>
        Stream OpenMapForWrite(string mapName);

        /// <summary>Opens the map file for reading.</summary>
        /// <param name="mapName">The map name.</param>
        /// <returns>The readable stream, or null when the file is absent.</returns>
        Stream OpenMapForRead(string mapName);
    }
}