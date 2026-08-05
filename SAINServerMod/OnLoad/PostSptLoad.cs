using SAINServerMod.Patches;
using SAINServerMod.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Common.Models.Logging;

namespace SAINServerMod.OnLoad;

/// <summary>
/// Enables the bot loot patch once the database and every other mod have finished loading.
/// </summary>
[Injectable(TypePriority = OnLoadOrder.PostLoad + 1)]
public sealed class PostSptLoad(
    ISptLogger<BotLootGeneratorPatch> logger,
    ConfigService configService,
    LegaMedalService legaMedalService
) : IOnLoad
{
    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        if (!configService.LegaMedalConfig.Enabled)
        {
            return Task.CompletedTask;
        }

        // Harmony patch methods are static, and 4.1 dropped ServiceLocator, so hand the
        // patch its dependencies here.
        BotLootGeneratorPatch.LegaMedalService = legaMedalService;
        BotLootGeneratorPatch.Logger = logger;

        new BotLootGeneratorPatch().Enable();

        return Task.CompletedTask;
    }
}
