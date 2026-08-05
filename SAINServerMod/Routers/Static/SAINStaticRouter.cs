using SAINServerMod.Models.LegaMedals;
using SAINServerMod.Services;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Routers.Static;

[Injectable]
public sealed class SAINStaticRouter(JsonUtil jsonUtil, ConfigService configService)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/sain/namepersonalities",
                async (url, info, sessionID, output, cancellationToken) =>
                    jsonUtil.Serialize(configService.NicknamesModel)
                    ?? throw new InvalidOperationException("Could not serialize personalities!")
            ),
            // Lets the client seed its sliders from whatever LegaMedals.json currently holds.
            new RouteAction<EmptyRequestData>(
                "/sain/legamedals",
                async (url, info, sessionID, output, cancellationToken) =>
                    jsonUtil.Serialize(configService.LegaMedalConfig)
                    ?? throw new InvalidOperationException("Could not serialize Lega Medal config!")
            ),
            // Applied in place rather than swapped, so the loot patch's reference stays live and
            // slider changes land on the next bot generated instead of needing a server restart.
            new RouteAction<LegaMedalConfig>(
                "/sain/legamedals/update",
                async (url, info, sessionID, output, cancellationToken) =>
                {
                    if (info is not null)
                    {
                        configService.LegaMedalConfig.CopyFrom(info);
                    }

                    return jsonUtil.Serialize(configService.LegaMedalConfig)
                        ?? throw new InvalidOperationException("Could not serialize Lega Medal config!");
                }
            ),
        ]
    ) { }
