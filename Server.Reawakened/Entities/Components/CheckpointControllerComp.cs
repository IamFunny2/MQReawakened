﻿using Microsoft.Extensions.Logging;
using Server.Base.Logging;
using Server.Reawakened.Entities.AbstractComponents;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using System.Text;

namespace Server.Reawakened.Entities.Components;

public class CheckpointControllerComp : TriggerCoopControllerComp<CheckpointController>
{
    public int SpawnPoint => ComponentData.SpawnpointID;

    public new ILogger<CheckpointControllerComp> Logger { get; set; }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (!isActive)
            return;

        Logger.LogInformation("Checkpoint 1: {SpawnPoint}", SpawnPoint);

        if (Room.LastCheckpoint != null)
            if (Room.LastCheckpoint.Id == Id)
            {
                Logger.LogTrace("Skipped checkpoint: {SpawnPoint}", SpawnPoint);
                return;
            }

        if (player.Room.LastCheckpoint != null)
        {
            var checkpoints = Room.GetComponentsOfType<CheckpointControllerComp>().Values;
            var possibleLastCheckpoint = checkpoints.FirstOrDefault(c => c.Id == player.Room.LastCheckpoint.Id);
            possibleLastCheckpoint?.Trigger(player, false);
        }

        player.Room.LastCheckpoint = this;
    }
}
