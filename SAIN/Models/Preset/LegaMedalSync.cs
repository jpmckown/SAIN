using System;
using SAIN.Preset.GlobalSettings;
using SPT.Common.Http;
using SPT.Common.Utils;

namespace SAIN.Models.Preset;

/// <summary>
/// Pushes the Lega Medal sliders to the SAIN server mod.
/// <para>
/// The medals themselves are added server-side while a bot's loot is generated, so the client
/// only ever ships the numbers across. Bot loot pools are baked at server startup and cached per
/// bot role, which is why this cannot be done client-side or by editing the bot database.
/// </para>
/// </summary>
internal static class LegaMedalSync
{
    private const string UpdateRoute = "/sain/legamedals/update";

    private static string _lastSent;

    /// <summary>
    /// Sends the current preset's settings if they differ from whatever was last sent.
    /// Safe to call on every preset update — identical payloads are skipped.
    /// </summary>
    public static void Send(GlobalSettingsClass globalSettings)
    {
        if (globalSettings?.LegaMedals == null)
        {
            return;
        }

        try
        {
            string json = Json.Serialize(globalSettings.LegaMedals.ToPayload());
            if (json == _lastSent)
            {
                return;
            }

            RequestHandler.PostJson(UpdateRoute, json);
            _lastSent = json;
            Logger.LogDebug("[SAIN] Sent Lega Medal settings to server.");
        }
        catch (Exception ex)
        {
            // An older SAIN server mod will not know this route. That is not worth breaking a
            // preset load over - the server simply keeps using its own LegaMedals.json defaults.
            _lastSent = null;
            Logger.LogWarning($"[SAIN] Could not send Lega Medal settings to the server, using its defaults instead: {ex.Message}");
        }
    }
}
