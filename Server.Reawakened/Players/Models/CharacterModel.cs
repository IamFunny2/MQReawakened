﻿using Server.Base.Core.Models;
using Server.Reawakened.Players.Models.Character;
using Server.Reawakened.Players.Models.Pets;
using Server.Reawakened.Players.Models.System;

namespace Server.Reawakened.Players.Models;

public class CharacterModel : PersistantData
{
    public CharacterDataModel Data { get; set; }
    public LevelData LevelData { get; set; }
    public Dictionary<int, List<int>> CollectedIdols { get; set; }
    public List<EmailHeaderModel> Emails { get; set; }
    public List<EmailMessageModel> EmailMessages { get; set; }
    public List<int> Events { get; set; }
    public Dictionary<int, Dictionary<string, int>> AchievementObjectives { get; set; }
    public Dictionary<string, float> BestMinigameTimes { get; set; }
    public Dictionary<string, DailiesModel> CurrentCollectedDailies { get; set; }
    public Dictionary<string, DailiesModel> CurrentQuestDailies { get; set; }
    public Dictionary<string, PetModel> Pets { get; set; }

    public CharacterModel()
    {
        CollectedIdols = [];
        Emails = [];
        EmailMessages = [];
        Events = [];
        AchievementObjectives = [];
        BestMinigameTimes = [];
        CurrentCollectedDailies = [];
        CurrentQuestDailies = [];
        Pets = [];

        Data = new CharacterDataModel();
        LevelData = new LevelData();
    }

    public override string ToString() => throw new InvalidOperationException();
}
