using System.Collections.Generic;
using SAIN.Attributes;
using UnityEngine;

namespace SAIN.Preset.GlobalSettings;

/// <summary>
/// Puts Lega Medals back into circulation as boss loot.
/// <para>
/// Lega Medals are a barter token that vanilla only hands out for PvP kills, which makes them
/// close to unobtainable in SPT. These sliders let harder bots carry them, scaled by how hard
/// the bot is to kill.
/// </para>
/// <para>
/// Applied server-side: the values here are posted to the SAIN server mod, which adds the medals
/// while a bot's loot is being generated. Changes take effect on the next bot spawned, so they
/// will not retroactively arm bots already in the raid.
/// </para>
/// </summary>
public class LegaMedalSettings : SAINSettingsBase<LegaMedalSettings>, ISAINSettings
{
    [Name("Enable Lega Medal Drops")]
    [Description(
        "Master switch. When off, SAIN leaves bot loot completely alone and vanilla Lega Medal behaviour applies."
    )]
    [Category("General")]
    public bool Enabled = true;

    [Name("Guaranteed Boss Medals")]
    [Description(
        "Medals every boss is guaranteed to carry, before any chance rolls. Set to 0 to make bosses roll for medals like everyone else."
    )]
    [Category("General")]
    [MinMax(0, 5, 1)]
    public float GuaranteedBossMedals = 1;

    [Name("Guarantee Cultist Priests Too")]
    [Description("Extends the guaranteed drop above to cultist priests, who are boss-tier in practice but not named like one.")]
    [Category("General")]
    public bool GuaranteeCultistPriests = false;

    [Name("Max Medals Per Bot")]
    [Description("Hard cap on how many medals a single bot can carry, counting the guaranteed ones.")]
    [Category("General")]
    [MinMax(1, 10, 1)]
    public float MaxMedalsPerBot = 3;

    [Name("Bonus Rolls Per Bot")]
    [Description(
        "How many independent chance rolls each bot gets. Raising this widens the payout spread rather than changing the odds of any single roll."
    )]
    [Category("General")]
    [Advanced]
    [MinMax(1, 5, 1)]
    public float BonusRolls = 1;

    [Name("Boss Chance")]
    [Description("Percent chance of a bonus medal on a boss, before the difficulty multiplier. This is on top of the guaranteed drop.")]
    [Category("Chance By Bot Type")]
    [MinMax(0f, 100f, 10f)]
    public float BossChance = 12f;

    [Name("Cultist Priest Chance")]
    [Description("Percent chance of a medal on a cultist priest, before the difficulty multiplier.")]
    [Category("Chance By Bot Type")]
    [MinMax(0f, 100f, 10f)]
    public float CultistPriestChance = 10f;

    [Name("Boss Guard Chance")]
    [Description("Percent chance of a medal on a boss follower or guard, before the difficulty multiplier.")]
    [Category("Chance By Bot Type")]
    [MinMax(0f, 100f, 10f)]
    public float FollowerChance = 6f;

    [Name("Raider / Rogue Chance")]
    [Description(
        "Percent chance of a medal on raiders, rogues, cultist warriors and arena fighters, before the difficulty multiplier."
    )]
    [Category("Chance By Bot Type")]
    [MinMax(0f, 100f, 10f)]
    public float RaiderChance = 3f;

    [Name("PMC Chance")]
    [Description("Percent chance of a medal on a PMC, before the difficulty multiplier.")]
    [Category("Chance By Bot Type")]
    [MinMax(0f, 100f, 10f)]
    public float PmcChance = 1.5f;

    [Name("Scav Chance")]
    [Description("Percent chance of a medal on a regular scav, before the difficulty multiplier. Zero by default.")]
    [Category("Chance By Bot Type")]
    [MinMax(0f, 100f, 10f)]
    public float ScavChance = 0f;

    [Name("Easy Bot Multiplier")]
    [Description("Chances above are multiplied by this for bots spawned on Easy difficulty.")]
    [Category("Multiplier By Difficulty")]
    [MinMax(0f, 5f, 100f)]
    public float EasyMultiplier = 0.5f;

    [Name("Normal Bot Multiplier")]
    [Description("Chances above are multiplied by this for bots spawned on Normal difficulty.")]
    [Category("Multiplier By Difficulty")]
    [MinMax(0f, 5f, 100f)]
    public float NormalMultiplier = 1f;

    [Name("Hard Bot Multiplier")]
    [Description("Chances above are multiplied by this for bots spawned on Hard difficulty.")]
    [Category("Multiplier By Difficulty")]
    [MinMax(0f, 5f, 100f)]
    public float HardMultiplier = 1.75f;

    [Name("Impossible Bot Multiplier")]
    [Description("Chances above are multiplied by this for bots spawned on Impossible difficulty.")]
    [Category("Multiplier By Difficulty")]
    [MinMax(0f, 5f, 100f)]
    public float ImpossibleMultiplier = 3f;

    public override void Init(List<ISAINSettings> list)
    {
        list.Add(this);
    }

    /// <summary>
    /// Builds the payload the SAIN server mod expects. Key names must match
    /// <c>ELegaMedalTier</c> and the SPT difficulty names on the server side.
    /// </summary>
    public LegaMedalServerPayload ToPayload()
    {
        return new LegaMedalServerPayload
        {
            Enabled = Enabled,
            GuaranteedBossMedals = Mathf.RoundToInt(GuaranteedBossMedals),
            GuaranteeCultistPriests = GuaranteeCultistPriests,
            MaxMedalsPerBot = Mathf.RoundToInt(MaxMedalsPerBot),
            BonusRolls = Mathf.RoundToInt(BonusRolls),
            TierChance = new Dictionary<string, double>
            {
                { "Boss", BossChance },
                { "CultistPriest", CultistPriestChance },
                { "Follower", FollowerChance },
                { "Raider", RaiderChance },
                { "Pmc", PmcChance },
                { "Scav", ScavChance },
            },
            DifficultyMultiplier = new Dictionary<string, double>
            {
                { "easy", EasyMultiplier },
                { "normal", NormalMultiplier },
                { "hard", HardMultiplier },
                { "impossible", ImpossibleMultiplier },
            },
        };
    }
}

/// <summary>
/// Wire format for /sain/legamedals/update. Mirrors LegaMedalConfig on the server.
/// </summary>
public class LegaMedalServerPayload
{
    public bool Enabled;
    public int GuaranteedBossMedals;
    public bool GuaranteeCultistPriests;
    public int MaxMedalsPerBot;
    public int BonusRolls;
    public Dictionary<string, double> TierChance;
    public Dictionary<string, double> DifficultyMultiplier;
}
