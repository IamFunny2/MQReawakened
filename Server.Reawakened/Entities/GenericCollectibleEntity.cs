﻿using Microsoft.Extensions.Logging;
using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class GenericCollectibleModel : SyncedEntity<GenericCollectible>
{
    public ILogger<GenericCollectibleModel> Logger { get; set; }
    
    public int Value;

    public override void InitializeEntity()
    {
        switch (PrefabName)
        {
            case "BananaGrapCollectible":
                Value = 5;
                break;
            case "BananeCollectible":
                Value = 1;
                break;
            default:
                Logger.LogInformation("Collectible not implemented for {PrefabName}", PrefabName);
                break;
        }
    }

    public override object[] GetInitData(NetState netState) =>
        !IsActive ? new object[] { 0 } : Array.Empty<object>();

    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        IsActive = false;
        var collectedValue = Value * Level.Clients.Count;

        var player = netState.Get<Player>();
        Level.SentEntityTriggered(Id, player, true, true);

        var effectName = string.Empty;

        switch (PrefabName)
        {
            case "BananaGrapCollectible":
                effectName = "PF_FX_Banana_Level_01";
                break;
            case "BananeCollectible":
                effectName = "PF_FX_Banana_Level_02";
                break;
            default:
                Logger.LogInformation("Collectible not implemented for {PrefabName}", PrefabName);
                break;
        }

        var effectEvent = new FX_SyncEvent(Id.ToString(), Level.Time, effectName,
            Position.X, Position.Y, FX_SyncEvent.FXState.Play);

        Level.SendSyncEvent(effectEvent);

        if (collectedValue <= 0)
            return;

        foreach (var client in Level.Clients.Values)
            client.Get<Player>().AddBananas(client, collectedValue);
    }
}
