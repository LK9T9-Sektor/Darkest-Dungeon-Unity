using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Sektor.DarkestDungeon.Clients.Content;
using Sektor.DarkestDungeon.Core.Combat.Character;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.AI;
using Sektor.DarkestDungeon.Core.Combat.Mechanics.Skills;
using Sektor.DarkestDungeon.Core.Content.Character;
using Sektor.DarkestDungeon.Core.Duel.Fight;

/// <summary>
/// Loads the campaign content (heroes, monsters, brains, buffs, quirks, effects, traits) from the
/// Unity Resources folder into the pure core catalogs and exposes a cached <see cref="TextFightContent"/>
/// for the fight tester overlay.
/// </summary>
public static class FightContentLoader
{
    private const string BuffsPath = "Data/JsonBuffs";
    private const string QuirksPath = "Data/JsonQuirks";
    private const string TraitsPath = "Data/JsonTraits";
    private const string BrainsPath = "Data/JsonAI";
    private const string EffectsPath = "Data/Mechanics/Effects";
    private const string HeroesPath = "Data/Heroes/Info";
    private const string MonstersPath = "Data/Monsters";

    private static TextFightContent _content;
    private static HeroCatalog _heroes;
    private static MonsterCatalog _monsters;

    /// <summary>Gets the loaded campaign content source for the duel fight runner.</summary>
    public static TextFightContent Content
    {
        get
        {
            if (_content == null)
                Load();
            return _content;
        }
    }

    /// <summary>Gets the loaded hero class catalog.</summary>
    public static HeroCatalog Heroes
    {
        get
        {
            if (_content == null)
                Load();
            return _heroes;
        }
    }

    /// <summary>Gets the loaded monster class catalog.</summary>
    public static MonsterCatalog Monsters
    {
        get
        {
            if (_content == null)
                Load();
            return _monsters;
        }
    }

    private static void Load()
    {
        EffectCatalog effects = EffectCatalog.Load(LoadText(EffectsPath));

        _heroes = HeroCatalog.Load(LoadTexts(HeroesPath), effects);
        _monsters = MonsterCatalog.Load(LoadTexts(MonstersPath), effects);

        string brainsText = LoadText(BrainsPath);
        MonsterBrainCatalog brains = string.IsNullOrEmpty(brainsText)
            ? new MonsterBrainCatalog(null)
            : GameDataReader.ReadBrains(brainsText);

        string buffsText = LoadText(BuffsPath);
        BuffCatalog buffs = string.IsNullOrEmpty(buffsText) ? BuffCatalog.Empty : GameDataReader.ReadBuffs(buffsText);

        string quirksText = LoadText(QuirksPath);
        QuirkCatalog quirks = string.IsNullOrEmpty(quirksText) ? QuirkCatalog.Empty : GameDataReader.ReadQuirks(quirksText);

        string traitsText = LoadText(TraitsPath);
        List<Sektor.DarkestDungeon.Core.Combat.Character.Trait> traits = string.IsNullOrEmpty(traitsText)
            ? new List<Sektor.DarkestDungeon.Core.Combat.Character.Trait>()
            : GameDataReader.ReadTraits(traitsText);

        _content = new TextFightContent(
            _heroes,
            _monsters,
            brains,
            buffs,
            quirks,
            effects,
            traits.Where(trait => trait.IsAffliction).ToList(),
            traits.Where(trait => trait.IsVirtue).ToList());
    }

    private static string LoadText(string path)
    {
        TextAsset asset = Resources.Load<TextAsset>(path);
        return asset != null ? asset.text : string.Empty;
    }

    private static IEnumerable<string> LoadTexts(string path)
    {
        foreach (TextAsset asset in Resources.LoadAll<TextAsset>(path))
            yield return asset.text;
    }
}