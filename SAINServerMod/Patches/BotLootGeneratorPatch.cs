using System.Reflection;
using HarmonyLib;
using SAINServerMod.Services;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Generators.Loot;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Common.Models.Logging;

namespace SAINServerMod.Patches;

/// <summary>
/// Runs after SPT has finished filling a bot's containers and tops it up with Lega Medals.
/// <para>
/// A postfix is used rather than a prefix so the medals are added to an inventory that already
/// has its normal loot — that way the pocket space check reflects reality, and vanilla loot
/// generation is left completely untouched.
/// </para>
/// </summary>
public sealed class BotLootGeneratorPatch : AbstractPatch
{
    /// <summary>
    /// Assigned by <see cref="OnLoad.PostSptLoad"/>. Harmony patch methods must be static, and
    /// SPT 4.1 removed ServiceLocator, so the dependencies are handed over during OnLoad instead.
    /// </summary>
    public static LegaMedalService? LegaMedalService;

    public static ISptLogger<BotLootGeneratorPatch>? Logger;

    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotLootGenerator), nameof(BotLootGenerator.GenerateLoot));
    }

    [PatchPostfix]
    public static void Postfix(MongoId botId, BotGenerationDetails botGenerationDetails, BotBaseInventory botInventory)
    {
        // Never let a loot tweak take a raid down with it.
        try
        {
            LegaMedalService?.TryAddMedals(botId, botGenerationDetails, botInventory);
        }
        catch (Exception ex)
        {
            Logger?.Error($"[SAIN] Lega Medal generation failed for '{botGenerationDetails?.Role}': {ex}");
        }
    }
}
