using System.Collections.Generic;
using System.Globalization;

using Sektor.DarkestDungeon.Core.Raid;

namespace Sektor.DarkestDungeon.Core.Raid.Database
{
    /// <summary>Parses the curio content file (CSV) into curio definitions.</summary>
    public static class CurioCsvParser
    {
        /// <summary>
        /// Parses the raw curio CSV text into a dictionary of curios keyed by their string id.
        /// The expected layout matches the legacy Curios.csv content file.
        /// </summary>
        /// <param name="csvText">The raw curio CSV text.</param>
        /// <returns>The parsed curios keyed by string id.</returns>
        public static Dictionary<string, Curio> Parse(string csvText)
        {
            var curios = new Dictionary<string, Curio>();
            string[,] curioGrid = CsvReader.SplitCsvGrid(csvText);

            for (int i = 2; i < curioGrid.GetLength(0); i += 15)
            {
                Curio curio = new Curio(curioGrid[i + 2, 2]);
                curio.ResultTypes = curioGrid[i, 4].ToLower();
                curio.RegionFound = curioGrid[i + 4, 2].ToLower();
                curio.IsFullCurio = curioGrid[i + 6, 2] == "Yes";
                if (curioGrid[i + 8, 2] != "")
                    curio.Tags.Add(curioGrid[i + 8, 2].ToLower());
                if (curioGrid[i + 8, 3] != "")
                    curio.Tags.Add(curioGrid[i + 8, 3].ToLower());
                if (curioGrid[i + 9, 2] != "")
                    curio.Tags.Add(curioGrid[i + 9, 2].ToLower());
                if (curioGrid[i + 9, 3] != "")
                    curio.Tags.Add(curioGrid[i + 9, 3].ToLower());

                for (int resultIndex = 0; resultIndex < 8; resultIndex++)
                {
                    if (curioGrid[i + 2 + resultIndex, 5] != null
                        && curioGrid[i + 2 + resultIndex, 5] != "")
                    {
                        CurioInteraction interaction = new CurioInteraction();
                        interaction.ResultType = curioGrid[i + 2 + resultIndex, 4].ToLower();
                        interaction.Chance = ParseInt(curioGrid[i + 2 + resultIndex, 5]);

                        for (int typeIndex = 0; typeIndex < 3; typeIndex++)
                        {
                            if (curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3] != null
                                && curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3] != ""
                                && curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3] != "N/A")
                            {
                                CurioResult curioResult = new CurioResult();
                                curioResult.Item = curioGrid[i + 2 + resultIndex, 7 + typeIndex * 3];
                                curioResult.Chance = ParseInt(curioGrid[i + 2 + resultIndex, 8 + typeIndex * 3]);
                                if (curioGrid[i + 2 + resultIndex, 9 + typeIndex * 3] == "<- # Draws")
                                {
                                    curioResult.Draws = curioResult.Chance;
                                    curioResult.IsCombined = true;
                                }
                                else
                                    curioResult.Draws = 1;
                                interaction.Results.Add(curioResult);
                            }
                        }

                        curio.Results.Add(interaction);
                    }
                }

                for (int interactIndex = 0; interactIndex < 3 && i + 11 + interactIndex < curioGrid.GetLength(0); interactIndex++)
                {
                    if (curioGrid[i + 11 + interactIndex, 4] != null && curioGrid[i + 11 + interactIndex, 4] != "")
                    {
                        ItemInteraction itemInteraction = new ItemInteraction();
                        itemInteraction.ItemId = curioGrid[i + 11 + interactIndex, 4];
                        itemInteraction.ResultType = curioGrid[i + 11 + interactIndex, 5].ToLower();

                        for (int itemIndex = 0; itemIndex < 3; itemIndex++)
                        {
                            if (curioGrid[i + 11 + interactIndex, 7 + itemIndex * 3] != null &&
                                curioGrid[i + 11 + interactIndex, 7 + itemIndex * 3] != "")
                            {
                                CurioResult curioResult = new CurioResult();
                                curioResult.Item = curioGrid[i + 11 + interactIndex, 7 + itemIndex * 3];
                                curioResult.Chance = ParseInt(curioGrid[i + 11 + interactIndex, 8 + itemIndex * 3]);
                                if (curioGrid[i + 11 + interactIndex, 9 + itemIndex * 3] == "<- # Draws")
                                {
                                    curioResult.Draws = curioResult.Chance;
                                    curioResult.IsCombined = true;
                                }
                                else
                                    curioResult.Draws = 1;

                                itemInteraction.Results.Add(curioResult);
                            }
                        }
                        curio.ItemInteractions.Add(itemInteraction);
                    }
                }

                curios.Add(curio.StringId, curio);
            }

            return curios;
        }

        private static int ParseInt(string value)
        {
            return int.Parse(value, CultureInfo.InvariantCulture);
        }
    }
}
