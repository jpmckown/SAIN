using System.Reflection;
using SAINServerMod.Models.LegaMedals;
using SAINServerMod.Models.Preset.Personalities;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Server;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Services;

[Injectable(InjectionType.Singleton)]
public sealed class ConfigService(ModHelper modHelper, JsonUtil jsonUtil)
{
    public string ModPath { get; init; } = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
    public NicknamesModel NicknamesModel { get; private set; } = default!;

    /// <summary>
    /// Held as a single mutable instance for the lifetime of the server. The loot patch keeps a
    /// reference to it, so client slider overrides posted to /sain/legamedals take effect on the
    /// next bot generated rather than needing a restart.
    /// </summary>
    public LegaMedalConfig LegaMedalConfig { get; } = new();

    public async Task LoadAsync()
    {
        NicknamesModel nicknamesModel =
            await jsonUtil.DeserializeFromFileAsync<NicknamesModel>(Path.Combine(ModPath, "Data", "NicknamePersonalities.json"))
            ?? throw new InvalidOperationException("Could not load nicknames, is the mod installed correctly?");

        NicknamesModel = nicknamesModel;

        // Defaults are baked into LegaMedalConfig, so a missing or unreadable file is survivable.
        LegaMedalConfig? legaMedals = await jsonUtil.DeserializeFromFileAsync<LegaMedalConfig>(
            Path.Combine(ModPath, "Data", "LegaMedals.json")
        );

        if (legaMedals is not null)
        {
            LegaMedalConfig.CopyFrom(legaMedals);
        }
        else
        {
            LegaMedalConfig.Normalize();
        }
    }
}
