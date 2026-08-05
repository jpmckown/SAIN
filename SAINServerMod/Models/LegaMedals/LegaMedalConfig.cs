using SPTarkov.Server.Core.Models.Utils;

namespace SAINServerMod.Models.LegaMedals;

/// <summary>
/// Controls how Lega Medals are seeded into bot pockets.
/// <para>
/// Lega Medals are normally a PvP-only barter token, so in SPT they are close to unobtainable.
/// This puts them back into circulation, weighted towards bots that are actually hard to kill.
/// </para>
/// <para>
/// Tier and difficulty keys are plain strings on purpose — they are hand-edited in
/// LegaMedals.json and posted from the client, so they stay forgiving about casing
/// and never depend on how enums happen to serialize as dictionary keys.
/// </para>
/// </summary>
/// <remarks>
/// Implements <see cref="IRequestData"/> so the same shape can be posted straight back from the
/// client's sliders to /sain/legamedals/update without a parallel DTO to keep in sync.
/// </remarks>
public sealed class LegaMedalConfig : IRequestData
{
    private Dictionary<ELegaMedalTier, double>? _resolvedTierChance;

    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Bosses always drop at least this many medals, regardless of difficulty.
    /// This is the floor that makes a boss kill always worth something.
    /// </summary>
    public int GuaranteedBossMedals { get; set; } = 1;

    /// <summary>
    /// Whether <see cref="GuaranteedBossMedals"/> also applies to cultist priests,
    /// who are boss-tier in practice but are not named "boss".
    /// </summary>
    public bool GuaranteeCultistPriests { get; set; }

    /// <summary>
    /// Percent chance (0-100) of a <em>bonus</em> medal, before the difficulty multiplier.
    /// For bosses this is on top of <see cref="GuaranteedBossMedals"/>; for every other
    /// tier it is the only chance they get. Keys are <see cref="ELegaMedalTier"/> names.
    /// </summary>
    public Dictionary<string, double> TierChance { get; set; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { nameof(ELegaMedalTier.Boss), 12.0 },
            { nameof(ELegaMedalTier.CultistPriest), 10.0 },
            { nameof(ELegaMedalTier.Follower), 6.0 },
            { nameof(ELegaMedalTier.Raider), 3.0 },
            { nameof(ELegaMedalTier.Pmc), 1.5 },
            { nameof(ELegaMedalTier.Scav), 0.0 },
        };

    /// <summary>
    /// Multiplier applied to <see cref="TierChance"/> based on the bot's spawned difficulty.
    /// Keys are the SPT difficulty names (easy / normal / hard / impossible) — the same value
    /// SAIN itself reads client-side from the bot's profile settings.
    /// </summary>
    public Dictionary<string, double> DifficultyMultiplier { get; set; } =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "easy", 0.5 },
            { "normal", 1.0 },
            { "hard", 1.75 },
            { "impossible", 3.0 },
        };

    /// <summary>
    /// Upper bound on medals handed to a single bot, counting the guaranteed ones.
    /// Stops a stacked multiplier from turning a boss into a piñata.
    /// </summary>
    public int MaxMedalsPerBot { get; set; } = 3;

    /// <summary>
    /// How many independent bonus rolls a bot gets. Each roll uses the same computed chance,
    /// so raising this widens the payout spread rather than shifting the per-roll odds.
    /// </summary>
    public int BonusRolls { get; set; } = 1;

    /// <summary>
    /// Rebuilds the typed lookup. Must be called after loading from JSON or applying an
    /// override, since both write the string-keyed dictionaries directly.
    /// </summary>
    public void Normalize()
    {
        // Deserialization replaces the dictionary instances, dropping the case-insensitive
        // comparers set up above, so rebuild them before anything reads a key.
        TierChance = new Dictionary<string, double>(TierChance, StringComparer.OrdinalIgnoreCase);
        DifficultyMultiplier = new Dictionary<string, double>(DifficultyMultiplier, StringComparer.OrdinalIgnoreCase);

        var resolved = new Dictionary<ELegaMedalTier, double>();
        foreach (var (key, chance) in TierChance)
        {
            if (Enum.TryParse<ELegaMedalTier>(key, ignoreCase: true, out var tier))
            {
                resolved[tier] = chance;
            }
        }

        _resolvedTierChance = resolved;
    }

    public double GetChanceFor(ELegaMedalTier tier, string? difficulty)
    {
        _resolvedTierChance ??= BuildFallbackLookup();

        if (!_resolvedTierChance.TryGetValue(tier, out var baseChance) || baseChance <= 0d)
        {
            return 0d;
        }

        var multiplier = 1d;
        if (!string.IsNullOrEmpty(difficulty) && DifficultyMultiplier.TryGetValue(difficulty, out var found))
        {
            multiplier = found;
        }

        return Math.Clamp(baseChance * multiplier, 0d, 100d);
    }

    /// <summary>
    /// Medals this bot gets before any rolling — the "bosses are always in the rolls" guarantee.
    /// </summary>
    public int GetGuaranteedFor(ELegaMedalTier tier)
    {
        if (GuaranteedBossMedals <= 0)
        {
            return 0;
        }

        return tier switch
        {
            ELegaMedalTier.Boss => GuaranteedBossMedals,
            ELegaMedalTier.CultistPriest when GuaranteeCultistPriests => GuaranteedBossMedals,
            _ => 0,
        };
    }

    /// <summary>
    /// Copies tunable values across from another config without swapping the instance,
    /// so anything holding a reference (the loot patch) picks the change up immediately.
    /// </summary>
    public void CopyFrom(LegaMedalConfig other)
    {
        Enabled = other.Enabled;
        GuaranteedBossMedals = other.GuaranteedBossMedals;
        GuaranteeCultistPriests = other.GuaranteeCultistPriests;
        MaxMedalsPerBot = other.MaxMedalsPerBot;
        BonusRolls = other.BonusRolls;

        foreach (var (tier, chance) in other.TierChance)
        {
            TierChance[tier] = chance;
        }

        foreach (var (difficulty, multiplier) in other.DifficultyMultiplier)
        {
            DifficultyMultiplier[difficulty] = multiplier;
        }

        Normalize();
    }

    private Dictionary<ELegaMedalTier, double> BuildFallbackLookup()
    {
        Normalize();
        return _resolvedTierChance!;
    }
}
