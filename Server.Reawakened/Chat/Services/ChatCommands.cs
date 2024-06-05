using A2m.Server;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Server.Base.Accounts.Services;
using Server.Base.Core.Abstractions;
using Server.Base.Core.Services;
using Server.Base.Logging;
using Server.Base.Worlds.Services;
using Server.Reawakened.Chat.Models;
using Server.Reawakened.Configs;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Core.Services;
using Server.Reawakened.Entities.Components.GameObjects.MonkeyGadgets;
using Server.Reawakened.Entities.Components.GameObjects.Trigger;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Services;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Planes;
using Server.Reawakened.Rooms.Services;
using Server.Reawakened.XMLs.Bundles.Base;
using Server.Reawakened.XMLs.Bundles.Internal;
using System.Text.RegularExpressions;

namespace Server.Reawakened.Chat.Services;

public partial class ChatCommands(
    ItemCatalog itemCatalog, ServerRConfig config, ItemRConfig itemConfig, ILogger<ServerConsole> logger,
    InternalAchievement internalAchievement, IHostApplicationLifetime appLifetime, QuestCatalog questCatalog,
    CharacterHandler characterHandler, GetServerAddress getSA) : IService
{
    private readonly Dictionary<string, ChatCommand> commands = [];

    [GeneratedRegex("[^A-Za-z0-9]+")]
    private static partial Regex MyRegex();

    public void Initialize() => appLifetime.ApplicationStarted.Register(RunChatListener);

    public void RunChatListener()
    {
        logger.LogDebug("Setting up chat commands");

        AddCommand(new ChatCommand("maxHP", string.Empty, MaxHealth));

        AddCommand(new ChatCommand("hotbar", "[hotbarNum] [itemId]", Hotbar));
        AddCommand(new ChatCommand("itemKit", "[itemKit]", ItemKit));
        AddCommand(new ChatCommand("cashKit", "[cashKit]", CashKit));

        AddCommand(new ChatCommand("badgePoints", "[badgePoints]", BadgePoints));
        AddCommand(new ChatCommand("discoverTribes", string.Empty, DiscoverTribes));

        AddCommand(new ChatCommand("changeName", "[first] [middle] [last]", ChangeName));
        AddCommand(new ChatCommand("tp", "[X] [Y]", Teleport));
        AddCommand(new ChatCommand("openDoors", string.Empty, OpenDoors));

        AddCommand(new ChatCommand("closestEntity", string.Empty, ClosestEntity));

        AddCommand(new ChatCommand("getPlayerId", "[id]", GetPlayerId));

        AddCommand(new ChatCommand("findQuest", "[name]", GetQuestByName));

        AddCommand(new ChatCommand("updateNpcs", string.Empty, UpdateLevelNpcs));

        logger.LogInformation("See chat commands by running {ChatCharStart}help", config.ChatCommandStart);
    }

    public void RunCommand(Player player, string[] args)
    {
        var name = args.FirstOrDefault();

        if (name != null && commands.TryGetValue(name, out var value))
        {
            Log(
                !value.CommandMethod(player, args)
                    ? $"Usage: {config.ChatCommandStart}{value.Name} {value.Arguments}"
                    : "Successfully run command!", player
            );
        }
        else
        {
            DisplayHelp(player);
        }
    }

    private static void Log(string logMessage, Player player) =>
        player.Chat(CannedChatChannel.Tell, "Console", logMessage);

    public void DisplayHelp(Player player)
    {
        Log("Chat Commands:", player);

        foreach (var command in commands.Values)
        {
            var padding = config.ChatCommandPadding - command.Name.Length;
            if (padding < 0) padding = 0;

            Log(
                $"  {config.ChatCommandStart}{command.Name.PadRight(padding)}" +
                $"{(command.Arguments.Length > 0 ? $" - {command.Arguments}" : string.Empty)}",
                player
            );
        }

        Log($"Note, these commands will soon be DEPRECATED! Please become familiar with using our website: {getSA.ServerAddress}/commands", player);
        Log("These new commands can be accessed by pressing shift + enter.", player);
    }

    public void AddCommand(ChatCommand command) => commands.Add(command.Name, command);

    public static bool MaxHealth(Player player, string[] args)
    {
        player.Room.SendSyncEvent(new Health_SyncEvent(player.GameObjectId, player.Room.Time,
            player.Character.Data.CurrentLife = player.Character.Data.MaxLife, player.Character.Data.MaxLife, player.GameObjectId));

        return true;
    }

    public bool Hotbar(Player player, string[] args)
    {
        player.AddSlots(true, itemConfig);

        if (args.Length <= 2)
            return true;

        if (!int.TryParse(args[1], out var hotbarId) || !int.TryParse(args[2], out var itemId) || hotbarId is < 1 or > 5)
        {
            Log("Please enter a hotbar number from 1-5 and an item Id.", player);
            return false;
        }

        var item = itemCatalog.GetItemFromId(itemId);

        if (item == null)
        {
            Log($"No item with id '{itemId}' could be found.", player);
            return false;
        }

        if (hotbarId == 5 && item.InventoryCategoryID != ItemFilterCategory.Pets)
        {
            Log("Please enter the item Id of a pet for the 5th hotbar slot.", player);
            return false;
        }

        if (item.InventoryCategoryID is
            ItemFilterCategory.WeaponAndAbilities or
            ItemFilterCategory.Consumables or
            ItemFilterCategory.NestedSuperPack)
        {
            var itemModel = new ItemModel()
            {
                ItemId = item.ItemId,
                Count = 0,
                BindingCount = 0,
                DelayUseExpiry = DateTime.Now
            };

            player.Character.Data.Inventory.Items.TryAdd(item.ItemId, itemModel);

            player.SetHotbarSlot(hotbarId - 1, itemModel, itemConfig);

            player.SendXt("hs", player.Character.Data.Hotbar);

            return true;
        }
        else
        {
            Log("Please enter the item id of a weapon, consumable, or pack.", player);
            return false;
        }
    }

    private void AddKit(Player player, int amount)
    {
        var items = itemConfig.SingleItemKit
            .Select(itemCatalog.GetItemFromId)
            .ToList();

        foreach (var itemId in itemConfig.StackedItemKit)
        {
            var stackedItem = itemCatalog.GetItemFromId(itemId);

            if (stackedItem == null)
            {
                logger.LogError("Unknown item with id {itemId}", itemId);
                continue;
            }

            for (var i = 0; i < itemConfig.AmountToStack; i++)
                items.Add(stackedItem);
        }

        player.Character.AddKit(items, amount);

        player.SendUpdatedInventory();
    }

    private bool Teleport(Player player, string[] args)
    {
        if (args.Length < 3 || !int.TryParse(args[1], out var x) || !int.TryParse(args[2], out var y))
        {
            Log("Please enter valid coordinates.", player);
            return false;
        }

        var zPosition = player.TempData.Position.z;

        if (args.Length > 3 && int.TryParse(args[3], out var givenZPosition))
            zPosition = givenZPosition;

        player.TeleportPlayer(player.TempData.Position.x + x, player.TempData.Position.y + y, zPosition is > 0 and not 10);

        return true;
    }

    private bool DiscoverTribes(Player player, string[] args)
    {
        var character = player.Character;

        player.DiscoverAllTribes();

        Log($"{character.Data.CharacterName} has discovered all tribes!", player);

        return true;
    }

    private bool ItemKit(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length > 2)
            Log($"Unknown kit amount, defaulting to 1", player);

        var amount = 1;

        if (args.Length == 2)
        {
            if (!int.TryParse(args[1], out var kitAmount))
                Log($"Invalid kit amount, defaulting to 1", player);

            amount = kitAmount;

            if (amount <= 0)
                amount = 1;
        }

        AddKit(player, amount);

        Log($"{character.Data.CharacterName} received {amount} item kit{(amount > 1 ? "s" : string.Empty)}!", player);

        return true;
    }

    private bool BadgePoints(Player player, string[] args)
    {
        player.AddPoints();
        return true;
    }

    private bool CashKit(Player player, string[] args)
    {
        var character = player.Character;

        player.AddBananas(config.CashKitAmount, internalAchievement, logger);
        player.AddNCash(config.CashKitAmount);

        Log($"{character.Data.CharacterName} received {config.CashKitAmount} " +
            $"banana{(config.CashKitAmount > 1 ? "s" : string.Empty)} & monkey cash!", player);

        return true;
    }

    private bool ChangeName(Player player, string[] args)
    {
        var character = player.Character;

        if (args.Length < 3)
        {
            Log("Please specify a valid name.", player);
            return false;
        }

        var names = args.Select(name =>
            MyRegex().Replace(name.ToLower(), string.Empty)
        ).ToList();

        var firstName = names[1];
        var secondName = names[2];

        var thirdName = names.Count > 3 ? names[3] : string.Empty;

        if (firstName.Length > 0)
            firstName = char.ToUpper(firstName[0]) + firstName[1..];

        if (secondName.Length > 0)
            secondName = char.ToUpper(secondName[0]) + secondName[1..];

        var newName = $"{firstName} {secondName}{thirdName}";

        if (characterHandler.GetCharacterFromName(newName) != null)
        {
            Log("Please specify a name that is not in use by another _player.", player);
            return false;
        }

        character.Data.CharacterName = $"{firstName} {secondName}{thirdName}";

        Log($"You have changed your monkey's name to {character.Data.CharacterName}!", player);
        Log("This change will apply only once you've logged out.", player);

        return true;
    }

    private bool OpenDoors(Player player, string[] args)
    {
        foreach (var triggerEntity in player.Room.GetEntitiesFromType<TriggerReceiverComp>())
        {
            if (config.IgnoredDoors.Contains(triggerEntity.PrefabName))
                continue;

            triggerEntity.Trigger(true);
        }

        foreach (var vineEntity in player.Room.GetEntitiesFromType<MysticCharmTargetComp>())
            vineEntity.Charm(player);

        return true;
    }

    private bool UpdateLevelNpcs(Player player, string[] args)
    {
        player.UpdateAllNpcsInLevel();
        Log($"All NPCs updated for {player.CharacterName}.", player);
        return true;
    }

    private bool GetPlayerId(Player player, string[] args)
    {
        Log($"{player.CharacterName} has id of {player.GameObjectId}", player);
        return true;
    }

    private bool ClosestEntity(Player player, string[] args)
    {
        var plane = player.GetPlaneEntities();

        var closestGameObjects = plane.Select(gameObject =>
        {
            var x = gameObject.ObjectInfo.Position.X - player.TempData.Position.x;
            var y = gameObject.ObjectInfo.Position.Y - player.TempData.Position.y;

            var distance = Math.Round(Math.Sqrt(Math.Pow(Math.Abs(x), 2) + Math.Pow(Math.Abs(y), 2)));

            return new Tuple<double, GameObjectModel>(distance, gameObject);
        }).OrderBy(x => x.Item1).ToList();

        if (closestGameObjects.Count == 0)
        {
            Log("No game objects found close to _player!", player);
            return false;
        }

        Log("Closest Game Objects:", player);

        var count = 0;

        if (closestGameObjects.Count > config.MaximumEntitiesToReturnLog)
            closestGameObjects = closestGameObjects.Take(config.MaximumEntitiesToReturnLog).ToList();

        closestGameObjects.Reverse();

        foreach (var item in closestGameObjects)
        {
            if (count > config.MaximumEntitiesToReturnLog)
                break;

            Log($"{item.Item1} units: " +
                $"{item.Item2.ObjectInfo.PrefabName} " +
                $"({item.Item2.ObjectInfo.ObjectId})",
                player);

            count++;
        }

        return true;
    }

    private bool GetQuestByName(Player player, string[] args)
    {
        var name = string.Join(" ", args.Skip(1)).ToLower();

        var closestQuest = questCatalog.QuestCatalogs.FirstOrDefault(q => q.Value.Title.Equals(name, StringComparison.OrdinalIgnoreCase)).Value;

        if (closestQuest == null)
        {
            Log($"Could not find quest with name '{name}'.", player);
            return false;
        }

        Log($"Found quest: '{name}' with ID: '{closestQuest.Id}'.", player);
        return true;
    }
}
