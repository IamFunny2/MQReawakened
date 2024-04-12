﻿using A2m.Server;
using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;
using Server.Reawakened.Core.Configs;
using Server.Reawakened.Players;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;
using Server.Reawakened.XMLs.Bundles.Base;
using UnityEngine;

namespace Server.Reawakened.Entities.Components.GameObjects.Hazards;
public class DroppingsControllerComp : Component<DroppingsController>
{
    public float DropRate => ComponentData.DropRate;

    public TimerThread TimerThread { get; set; }
    public ItemCatalog ItemCatalog { get; set; }
    public ServerRConfig ServerRConfig { get; set; }

    private Vector3 _startPosition;

    public override void InitializeComponent()
    {
        _startPosition = new Vector3(Position.X, Position.Y, Position.Z);
        WaitDrop();
    }

    public void WaitDrop() =>
        TimerThread.DelayCall(SendDrop, null, TimeSpan.FromSeconds(DropRate), TimeSpan.FromSeconds(1), 1);

    public void SendDrop(object _)
    {
        if (Room.IsObjectKilled(Id))
            return;

        Position.SetPosition(_startPosition);

        var speed = new Vector2
        {
            x = -5,
            y = 3
        };

        var damage = 0;
        var effect = ItemEffectType.Freezing;

        Room.AddRangedProjectile(Id, _startPosition, speed, 3, damage, effect, false);

        WaitDrop();
    }

    public void FreezePlayer(Player player)
    {
        SendFreezeEvent(player, ItemEffectType.IceDamage, 1, 5, true, false);
        SendFreezeEvent(player, ItemEffectType.Freezing, 1, 5, true, false);
        SendFreezeEvent(player, ItemEffectType.FreezingStatusEffect, 1, 5, true, false);
    }

    private void SendFreezeEvent(Player player, ItemEffectType effect, int amount,
            int duration, bool start, bool isPremium) =>
        Room.SendSyncEvent(
            new StatusEffect_SyncEvent(
                player.GameObjectId, Room.Time,
                (int)effect, amount, duration,
                start, Id, isPremium
            )
        );
}
