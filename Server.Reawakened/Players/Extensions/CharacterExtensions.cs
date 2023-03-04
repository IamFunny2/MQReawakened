﻿using A2m.Server;
using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Network.Extensions;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Players.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.Protocol;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Services;
using WorldGraphDefines;

namespace Server.Reawakened.Players.Extensions;

public static class CharacterExtensions
{
    public static bool HasDiscoveredTribe(this CharacterModel characterData, TribeType tribe)
    {
        if (characterData == null) return false;

        // ReSharper disable once InvertIf
        if (characterData.Data.TribesDiscovered.TryGetValue(tribe, out var discovered))
            if (discovered)
                return true;

        return characterData.Data.Allegiance == tribe;
    }

    public static CharacterModel GetCharacterFromName(this Player player, string characterName)
        => player.UserInfo.Characters.Values
            .FirstOrDefault(c => c.Data.CharacterName == characterName);

    public static void SetCharacterSelected(this Player player, int characterId)
    {
        player.Character = player.UserInfo.Characters[characterId];
        player.UserInfo.LastCharacterSelected = player.Character.Data.CharacterName;
    }

    public static void AddCharacter(this Player player, CharacterModel character) =>
        player.UserInfo.Characters.Add(character.Data.CharacterId, character);

    public static void DeleteCharacter(this Player player, int id)
    {
        player.UserInfo.Characters.Remove(id);

        player.UserInfo.LastCharacterSelected = player.UserInfo.Characters.Count > 0
            ? player.UserInfo.Characters.First().Value.Data.CharacterName
            : string.Empty;
    }

    public static void SetLevelXp(this CharacterModel characterData, int level)
    {
        characterData.Data.GlobalLevel = level;

        characterData.Data.ReputationForCurrentLevel = GetReputationForLevel(level - 1);
        characterData.Data.ReputationForNextLevel = GetReputationForLevel(level);
        characterData.Data.Reputation = 0;

        characterData.Data.MaxLife = GetHealthForLevel(level);
        characterData.Data.CurrentLife = characterData.Data.MaxLife;
    }

    public static void LevelUp(this Player player, int level, Microsoft.Extensions.Logging.ILogger logger)
    {
        var levelUpData = new LevelUpDataModel
        {
            Level = level
        };

        SetLevelXp(player.Character, level);

        player.SendLevelUp(levelUpData);

        logger.LogTrace("{Name} leveled up to {Level}", player.Character.Data.CharacterName, level);
    }

    private static int GetHealthForLevel(int level) => (level - 1) * 270 + 81;

    private static int GetReputationForLevel(int level) => (Convert.ToInt32(Math.Pow(level, 2)) - (level - 1)) * 500;

    public static void DiscoverTribe(this NetState state, TribeType tribe)
    {
        var player = state.Get<Player>();

        if (HasAddedDiscoveredTribe(player.Character, tribe))
            state.SendXt("cB", (int)tribe);
    }

    public static bool HasAddedDiscoveredTribe(this CharacterModel characterData, TribeType tribe)
    {
        if (characterData.Data.TribesDiscovered.ContainsKey(tribe))
        {
            if (characterData.Data.TribesDiscovered[tribe])
                return false;

            characterData.Data.TribesDiscovered[tribe] = true;
        }
        else
        {
            characterData.Data.TribesDiscovered.Add(tribe, true);
        }

        return true;
    }

    public static void AddBananas(this Player player, NetState state, int collectedBananas)
    {
        var charData = player.Character.Data;
        charData.Cash += collectedBananas;
        player.SendCashUpdate(state);
    }

    public static void SendCashUpdate(this Player player, NetState state)
    {
        var charData = player.Character.Data;
        state.SendXt("ca", charData.Cash, charData.NCash);
    }

    public static void SendLevelChange(this Player player, NetState netState, WorldHandler worldHandler,
        WorldGraphXML worldGraph)
    {
        var error = string.Empty;
        var levelName = string.Empty;
        var surroundingLevels = string.Empty;

        try
        {
            var levelInfo = worldHandler.GetLevelInfo(player.GetLevelId());
            levelName = levelInfo.Name;
            surroundingLevels = GetSurroundingLevels(levelInfo, worldGraph);
        }
        catch (Exception e)
        {
            error = e.Message;
        }

        netState.SendXt("lw", error, levelName, surroundingLevels);
    }

    private static string GetSurroundingLevels(LevelInfo levelInfo, WorldGraphXML worldGraph)
    {
        var sb = new SeparatedStringBuilder('!');

        var levels = worldGraph.GetLevelWorldGraphNodes(levelInfo.LevelId)
            .Where(x => x.ToLevelID != x.LevelID)
            .Select(x => worldGraph.GetInfoLevel(x.ToLevelID).Name)
            .Distinct();

        foreach (var level in levels)
            sb.Append(level);

        return sb.ToString();
    }

    public static void SetLevel(this CharacterModel character, int levelId,
        Microsoft.Extensions.Logging.ILogger logger) =>
        character.SetLevel(levelId, 0, 0, logger);

    public static void SetLevel(this CharacterModel character, int levelId, int portalId, int spawnId,
        Microsoft.Extensions.Logging.ILogger logger)
    {
        character.LevelData.LevelId = levelId;
        character.LevelData.PortalId = portalId;
        character.LevelData.SpawnPointId = spawnId;

        logger.LogDebug("Set spawn of '{CharacterName}' to portal {PortalId} spawn {SpawnId}",
            character.Data.CharacterName,
            portalId, spawnId);
    }

    public static void AddQuest(this CharacterModel character, QuestDescription quest, bool setActive)
    {
        if (character.HasQuest(quest.Id)) return;

        character.Data.QuestLog.Add(new QuestStatusModel
        {
            QuestStatus = QuestStatus.QuestState.IN_PROCESSING,
            Id = quest.Id,
            Objectives = quest.Objectives.ToDictionary(
                x => x.Key,
                x => new ObjectiveModel
                {
                    Completed = false,
                    CountLeft = x.Value.TotalCount
                }
            )
        });

        if (setActive)
            character.Data.ActiveQuestId = quest.Id;
    }

    public static void RemoveFromGroup(this NetState state)
    {
        var player = state.Get<Player>();

        if (player.Group == null)
            return;

        player.Group.GroupMembers.Remove(state);

        if (player.Group.GroupMembers.Count > 0)
        {
            if (player.Group.LeaderCharacter == player.Character.Data.CharacterName)
            {
                var newLeader = player.Group.GroupMembers.First();
                var newLeaderPlayer = newLeader.Get<Player>();
                player.Group.LeaderCharacter = newLeaderPlayer.Character.Data.CharacterName;

                foreach (var member in player.Group.GroupMembers)
                    member.SendXt("pp", player.Group.LeaderCharacter);
            }

            foreach (var member in player.Group.GroupMembers)
                member.SendXt("pl", player.Character.Data.CharacterName);
        }

        player.Group = null;
    }

    public static bool HasQuest(this CharacterModel character, int questId) =>
        character.Data.QuestLog.Count != 0 && character.Data.QuestLog.Any(quest => quest.Id == questId);

    public static bool TryGetQuest(this CharacterModel model, int questId, out QuestStatusModel outQuest)
    {
        outQuest = null;

        if (model.Data.QuestLog.Count == 0)
            return false;

        foreach (var quest in model.Data.QuestLog.Where(quest => quest.Id == questId))
        {
            outQuest = quest;
            return true;
        }

        return false;
    }

    public static bool HasPreviousQuests(this CharacterModel model, QuestDescription quest) =>
        model.Data.CompletedQuests.Count != 0 && quest.PreviousQuests.Where(prevId => prevId.Key != 0)
            .All(prevId => model.Data.CompletedQuests.Contains(prevId.Key));
}
