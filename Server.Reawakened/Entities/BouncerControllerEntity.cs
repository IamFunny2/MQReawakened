﻿using Server.Base.Network;
using Server.Reawakened.Levels.Models.Entities;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;

namespace Server.Reawakened.Entities;

public class BouncerControllerModel : SyncedEntity<BouncerController>
{
    public override void RunSyncedEvent(SyncEvent syncEvent, NetState netState)
    {
        var player = netState.Get<Player>();

        var bouncer = new Bouncer_SyncEvent(syncEvent);
        Level.SendSyncEvent(new Bouncer_SyncEvent(bouncer.TargetID, bouncer.TriggerTime, false), player);
        
        netState.SendSyncEventToPlayer(bouncer);
    }
}