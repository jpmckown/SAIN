namespace SAINServerMod.Models.LegaMedals;

/// <summary>
/// Maps a bot role onto a <see cref="ELegaMedalTier"/>.
/// <para>
/// Deliberately an explicit key table rather than substring matching on "boss" / "follower":
/// substring matching picks up the <c>bossTest</c> and <c>followerTest</c> debug roles while
/// missing genuinely hard bots whose names carry no tell — <c>sectantPriest</c>, <c>exUsec</c>
/// (rogue) and <c>pmcBot</c> (raider) among them.
/// </para>
/// </summary>
public static class LegaMedalTiers
{
    private static readonly Dictionary<string, ELegaMedalTier> _tiersByRole = new(StringComparer.OrdinalIgnoreCase)
    {
        // --- Bosses ---
        ["bossBully"] = ELegaMedalTier.Boss, // Reshala
        ["bossKilla"] = ELegaMedalTier.Boss,
        ["bossKillaAgro"] = ELegaMedalTier.Boss,
        ["bossKojaniy"] = ELegaMedalTier.Boss, // Shturman
        ["bossGluhar"] = ELegaMedalTier.Boss,
        ["bossSanitar"] = ELegaMedalTier.Boss,
        ["bossTagilla"] = ELegaMedalTier.Boss,
        ["bossTagillaAgro"] = ELegaMedalTier.Boss,
        ["bossKnight"] = ELegaMedalTier.Boss,
        ["bossZryachiy"] = ELegaMedalTier.Boss,
        ["bossBoar"] = ELegaMedalTier.Boss, // Kaban
        ["bossBoarSniper"] = ELegaMedalTier.Boss,
        ["bossKolontay"] = ELegaMedalTier.Boss,
        ["bossPartisan"] = ELegaMedalTier.Boss,

        // --- Cultist priests: boss-tier in practice, but not named like one ---
        ["sectantPriest"] = ELegaMedalTier.CultistPriest,
        ["sectactPriestEvent"] = ELegaMedalTier.CultistPriest, // BSG's typo, kept verbatim

        // --- Boss guards / followers ---
        ["followerBully"] = ELegaMedalTier.Follower,
        ["followerKojaniy"] = ELegaMedalTier.Follower,
        ["followerGluharAssault"] = ELegaMedalTier.Follower,
        ["followerGluharSecurity"] = ELegaMedalTier.Follower,
        ["followerGluharScout"] = ELegaMedalTier.Follower,
        ["followerGluharSnipe"] = ELegaMedalTier.Follower,
        ["followerSanitar"] = ELegaMedalTier.Follower,
        ["followerTagilla"] = ELegaMedalTier.Follower,
        ["tagillaHelperAgro"] = ELegaMedalTier.Follower,
        ["followerBigPipe"] = ELegaMedalTier.Follower,
        ["followerBirdEye"] = ELegaMedalTier.Follower,
        ["followerZryachiy"] = ELegaMedalTier.Follower,
        ["followerBoar"] = ELegaMedalTier.Follower,
        ["followerBoarClose1"] = ELegaMedalTier.Follower,
        ["followerBoarClose2"] = ELegaMedalTier.Follower,
        ["followerKolontayAssault"] = ELegaMedalTier.Follower,
        ["followerKolontaySecurity"] = ELegaMedalTier.Follower,

        // --- Raiders, rogues, cultist warriors, arena fighters ---
        ["pmcBot"] = ELegaMedalTier.Raider, // Raider
        ["exUsec"] = ELegaMedalTier.Raider, // Rogue
        ["sectantWarrior"] = ELegaMedalTier.Raider,
        ["sectantPredvestnik"] = ELegaMedalTier.Raider,
        ["sectantPrizrak"] = ELegaMedalTier.Raider,
        ["sectantOni"] = ELegaMedalTier.Raider,
        ["arenaFighter"] = ELegaMedalTier.Raider,
        ["arenaFighterEvent"] = ELegaMedalTier.Raider,
        ["shooterBTR"] = ELegaMedalTier.Raider,

        // --- PMCs ---
        ["pmcBEAR"] = ELegaMedalTier.Pmc,
        ["pmcUSEC"] = ELegaMedalTier.Pmc,
        ["sptBear"] = ELegaMedalTier.Pmc, // legacy SPT role spellings
        ["sptUsec"] = ELegaMedalTier.Pmc,

        // --- Scavs ---
        ["assault"] = ELegaMedalTier.Scav,
        ["marksman"] = ELegaMedalTier.Scav,
        ["cursedAssault"] = ELegaMedalTier.Scav,
        ["assaultGroup"] = ELegaMedalTier.Scav,
        ["crazyAssaultEvent"] = ELegaMedalTier.Scav,
        ["infectedAssault"] = ELegaMedalTier.Scav,
        ["infectedPmc"] = ELegaMedalTier.Scav,
        ["infectedCivil"] = ELegaMedalTier.Scav,
        ["infectedLaborant"] = ELegaMedalTier.Scav,
        ["infectedTagilla"] = ELegaMedalTier.Scav,

        // --- Never carry medals: non-combatants, event NPCs and debug roles ---
        ["gifter"] = ELegaMedalTier.Excluded, // Santa
        ["peacemaker"] = ELegaMedalTier.Excluded,
        ["skier"] = ELegaMedalTier.Excluded,
        ["spiritWinter"] = ELegaMedalTier.Excluded,
        ["spiritSpring"] = ELegaMedalTier.Excluded,
        ["peacefullZryachiyEvent"] = ELegaMedalTier.Excluded,
        ["ravangeZryachiyEvent"] = ELegaMedalTier.Excluded,
        ["test"] = ELegaMedalTier.Excluded,
        ["bossTest"] = ELegaMedalTier.Excluded,
        ["followerTest"] = ELegaMedalTier.Excluded,
    };

    /// <summary>
    /// Resolves a bot role to its tier. Unrecognised roles — modded bots, or roles BSG adds in a
    /// future wipe — fall back to <see cref="ELegaMedalTier.Excluded"/> so that new content never
    /// silently starts printing medals.
    /// </summary>
    public static ELegaMedalTier GetTier(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return ELegaMedalTier.Excluded;
        }

        return _tiersByRole.TryGetValue(role, out var tier) ? tier : ELegaMedalTier.Excluded;
    }
}
