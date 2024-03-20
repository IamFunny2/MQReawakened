﻿using Server.Reawakened.Entities.AIBehavior;
using Server.Reawakened.Entities.Components;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Helpers;
using Server.Reawakened.Rooms;
using Server.Reawakened.Rooms.Extensions;
using Server.Reawakened.Rooms.Models.Entities;

namespace Server.Reawakened.Entities.Entity.Enemies.BehaviorEnemies;
public class EnemySpiderling(Room room, string entityId, string prefabName, EnemyControllerComp enemyController, IServiceProvider services) : AIStateEnemy(room, entityId, prefabName, enemyController, services)
{
    public override void Initialize()
    {
        base.Initialize();

        //Temporarily here until I figure out how these dudes work
        var aiInit = new AIInit_SyncEvent(Id, Room.Time, Position.x, Position.y, Position.z, Position.x, Position.y, 0,
        Health, MaxHealth, 1, 1, 1, Status.Stars, Level, EnemyGlobalProps.ToString(), "");

        aiInit.EventDataList[2] = Position.x;
        aiInit.EventDataList[3] = Position.y;
        aiInit.EventDataList[4] = Position.z;
    }
}
