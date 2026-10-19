using System;
using System.Collections.Generic;

/// <summary>
/// One battle test slot: a hero or monster class plus the optional hero configuration (skills, quirks
/// and trinkets). Monster slots ignore the hero-only fields.
/// </summary>
[Serializable]
public class BattleTestSlotSpec
{
    /// <summary>The hero or monster class id from the campaign content.</summary>
    public string ClassId;

    /// <summary>Whether the slot is a hero; otherwise a monster.</summary>
    public bool IsHero;

    /// <summary>The combat skills the hero takes; empty means the class defaults.</summary>
    public List<string> SkillIds = new List<string>();

    /// <summary>The quirk ids applied to the hero; empty means none.</summary>
    public List<string> QuirkIds = new List<string>();

    /// <summary>The trinket ids equipped on the hero; empty means none.</summary>
    public List<string> TrinketIds = new List<string>();
}