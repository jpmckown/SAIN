namespace SAINServerMod.Models.LegaMedals;

/// <summary>
/// Difficulty tiers a bot role can fall into, used to scale Lega Medal drops.
/// Ordered roughly from hardest to easiest.
/// </summary>
public enum ELegaMedalTier
{
    /// <summary>Bots that should never carry medals (gifter, peacemaker, spirits, test bots).</summary>
    Excluded,
    Scav,
    Pmc,
    Raider,
    Follower,
    CultistPriest,
    Boss,
}
