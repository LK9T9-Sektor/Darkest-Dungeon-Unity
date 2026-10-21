using System.Collections.Generic;
using System.Globalization;

using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Combat.Raid.Battle;

namespace Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills
{
    /// <summary>Visual/animation data for a combat skill.</summary>
    public class SkillArtInfo
    {
        /// <summary>Gets the skill identifier.</summary>
        public string SkillId { get; private set; }

        /// <summary>Gets the animation identifier.</summary>
        public string AnimationId { get; private set; }

        /// <summary>Gets the art identifier.</summary>
        public string ArtId { get; private set; }

        /// <summary>Gets the icon identifier.</summary>
        public string IconId { get; private set; }

        /// <summary>Gets a value indicating whether selection display is allowed.</summary>
        public bool? CanDisplaySelection { get; private set; }

        /// <summary>Gets the target effect identifier.</summary>
        public string TargetFx { get; private set; }

        /// <summary>Gets the target chest effect identifier.</summary>
        public string TargetChestFx { get; private set; }

        /// <summary>Gets the target head effect identifier.</summary>
        public string TargetHeadFx { get; private set; }

        /// <summary>Gets the area offset X coordinate.</summary>
        public float AreaOffsetX { get; private set; }

        /// <summary>Gets the area offset Y coordinate.</summary>
        public float AreaOffsetY { get; private set; }

        /// <summary>Gets the target area offset X coordinate.</summary>
        public float TargetAreaOffsetX { get; private set; }

        /// <summary>Gets the target area offset Y coordinate.</summary>
        public float TargetAreaOffsetY { get; private set; }

        /// <summary>Initializes a new instance of the <see cref="SkillArtInfo"/> class.</summary>
        /// <param name="data">The data to load from.</param>
        /// <param name="isMonster">Whether this is a monster skill.</param>
        public SkillArtInfo(List<string> data, bool isMonster)
        {
            LoadData(data, isMonster);
        }

        /// <summary>Initializes a new instance of the <see cref="SkillArtInfo"/> class from parsed content tokens.</summary>
        /// <param name="tokens">The parsed <c>.key value</c> tokens (keys without the leading dot).</param>
        public SkillArtInfo(IReadOnlyDictionary<string, string> tokens)
        {
            SkillId = GetText(tokens, "id");
            AnimationId = GetText(tokens, "anim");
            ArtId = GetText(tokens, "fx");
            TargetFx = GetText(tokens, "targfx");
            TargetChestFx = GetText(tokens, "targchestfx");
            TargetHeadFx = GetText(tokens, "targheadfx");
            IconId = GetText(tokens, "icon");
            CanDisplaySelection = GetFlag(tokens, "can_display_selection");
            AreaOffsetX = GetNumber(tokens, "area_pos_offset");
            AreaOffsetY = GetNumber(tokens, "area_pos_offset#2");
            TargetAreaOffsetX = GetNumber(tokens, "target_area_pos_offset");
            TargetAreaOffsetY = GetNumber(tokens, "target_area_pos_offset#2");
        }

        private static string GetText(IReadOnlyDictionary<string, string> tokens, string key)
        {
            string value;
            return tokens.TryGetValue(key, out value) ? value : null;
        }

        private static bool? GetFlag(IReadOnlyDictionary<string, string> tokens, string key)
        {
            string value;
            if (!tokens.TryGetValue(key, out value))
                return null;

            bool parsed;
            return bool.TryParse(value, out parsed) ? parsed : (bool?)null;
        }

        private static float GetNumber(IReadOnlyDictionary<string, string> tokens, string key)
        {
            string value;
            if (!tokens.TryGetValue(key, out value))
                return 0f;

            int parsed;
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out parsed)
                ? parsed
                : 0f;
        }

        private void LoadData(List<string> data, bool isMonster)
        {
            for (int i = 1; i < data.Count; i++)
            {
                switch (data[i])
                {
                    case ".id":
                        SkillId = data[++i];
                        break;
                    case ".anim":
                        AnimationId = data[++i];
                        break;
                    case ".fx":
                        ArtId = data[++i];
                        break;
                    case ".targfx":
                        TargetFx = data[++i];
                        break;
                    case ".targchestfx":
                        TargetChestFx = data[++i];
                        break;
                    case ".targheadfx":
                        TargetHeadFx = data[++i];
                        break;
                    case ".area_pos_offset":
                        if (isMonster)
                        {
                            AreaOffsetX = -int.Parse(data[++i]);
                            AreaOffsetY = int.Parse(data[++i]);
                        }
                        else
                        {
                            AreaOffsetX = int.Parse(data[++i]);
                            AreaOffsetY = int.Parse(data[++i]);
                        }
                        break;
                    case ".target_area_pos_offset":
                        TargetAreaOffsetX = int.Parse(data[++i]);
                        TargetAreaOffsetY = int.Parse(data[++i]);
                        break;
                    case ".misstargfx":
                    case ".reset_source_stance":
                    case ".reset_target_stance":
                        ++i;
                        break;
                    case ".can_display_selection":
                        CanDisplaySelection = bool.Parse(data[++i]);
                        break;
                    case ".icon":
                        IconId = data[++i];
                        break;
                }
            }
        }
    }
}
