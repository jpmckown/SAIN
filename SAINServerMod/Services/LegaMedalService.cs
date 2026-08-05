using SAINServerMod.Models.LegaMedals;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers.Bot;
using SPTarkov.Server.Core.Helpers.Items;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Bots;
using SPTarkov.Common.Models.Logging;
using SPTarkov.Server.Core.Utils;

namespace SAINServerMod.Services;

/// <summary>
/// Decides whether a freshly generated bot carries Lega Medals, and puts them in its pockets.
/// <para>
/// This runs at bot-generation time rather than editing the bot database at startup, for two
/// reasons. First, pocket loot pools live on the bot <em>type</em> and carry no notion of
/// difficulty, and <c>BotLootCacheService</c> caches them per role — so a difficulty-scaled
/// weight baked into the database would be served to every difficulty alike. Second, adding the
/// item directly gives exact percentages instead of the approximation you get from nudging a
/// weight in a shared pool.
/// </para>
/// </summary>
[Injectable(InjectionType.Singleton)]
public sealed class LegaMedalService(
    ISptLogger<LegaMedalService> logger,
    ConfigService configService,
    ItemHelper itemHelper,
    BotGeneratorHelper botGeneratorHelper,
    RandomUtil randomUtil
)
{
    private static readonly HashSet<EquipmentSlots> _pockets = [EquipmentSlots.Pockets];

    /// <summary>
    /// Fallback containers, tried only if pockets are full. This postfix runs after SPT has
    /// already filled the bot's containers, so without a fallback a guaranteed boss medal could
    /// silently vanish into a NO_SPACE result. Secure containers are deliberately excluded —
    /// the player cannot loot those off a corpse, so a medal in one may as well not exist.
    /// </summary>
    private static readonly HashSet<EquipmentSlots> _fallbackContainers = [EquipmentSlots.TacticalVest, EquipmentSlots.Backpack];

    public void TryAddMedals(MongoId botId, BotGenerationDetails details, BotBaseInventory inventory)
    {
        var config = configService.LegaMedalConfig;
        if (!config.Enabled || config.MaxMedalsPerBot <= 0)
        {
            return;
        }

        var role = details.Role;
        var tier = LegaMedalTiers.GetTier(role);
        if (tier == ELegaMedalTier.Excluded)
        {
            return;
        }

        var medals = config.GetGuaranteedFor(tier);

        var chance = config.GetChanceFor(tier, details.BotDifficulty);
        if (chance > 0d)
        {
            for (var roll = 0; roll < config.BonusRolls; roll++)
            {
                if (randomUtil.GetChance100(chance))
                {
                    medals++;
                }
            }
        }

        medals = Math.Min(medals, config.MaxMedalsPerBot);
        if (medals <= 0)
        {
            return;
        }

        AddMedalStack(botId, role, medals, inventory);
    }

    private void AddMedalStack(MongoId botId, string? role, int medals, BotBaseInventory inventory)
    {
        var (found, template) = itemHelper.GetItem(ItemTpl.BARTER_LEGA_MEDAL);
        if (!found || template is null)
        {
            logger.Warning($"[SAIN] Lega Medal template {ItemTpl.BARTER_LEGA_MEDAL} is missing from the item database; skipping.");
            return;
        }

        var stackMax = template.Properties?.StackMaxSize;
        if (stackMax is > 0)
        {
            medals = Math.Min(medals, stackMax.Value);
        }

        var result = TryPlace(botId, medals, inventory, _pockets);
        if (result != ItemAddedResult.SUCCESS)
        {
            result = TryPlace(botId, medals, inventory, _fallbackContainers);
        }

        if (result != ItemAddedResult.SUCCESS)
        {
            // Some roles spawn with no lootable containers at all, so this is only ever debug noise.
            logger.Debug($"[SAIN] Could not give {medals} Lega Medal(s) to '{role}': {result}");
        }
    }

    private ItemAddedResult TryPlace(MongoId botId, int medals, BotBaseInventory inventory, HashSet<EquipmentSlots> slots)
    {
        // A fresh id per attempt — a failed add may still have left the previous one referenced.
        var itemId = new MongoId();
        var medalStack = new Item
        {
            Id = itemId,
            Template = ItemTpl.BARTER_LEGA_MEDAL,
            Upd = new Upd { StackObjectsCount = medals, SpawnedInSession = true },
        };

        return botGeneratorHelper.AddItemWithChildrenToEquipmentSlot(
            botId,
            slots,
            itemId,
            ItemTpl.BARTER_LEGA_MEDAL,
            [medalStack],
            inventory
        );
    }
}
