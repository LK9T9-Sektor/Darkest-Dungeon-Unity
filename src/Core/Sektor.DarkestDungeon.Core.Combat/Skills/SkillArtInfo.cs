using System.Collections.Generic;
using Sektor.DarkestDungeon.Core.Combat.Enums;

namespace Sektor.DarkestDungeon.Core.Combat.Skills
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
