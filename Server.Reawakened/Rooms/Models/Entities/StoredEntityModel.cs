﻿using Server.Base.Logging;
using Server.Reawakened.Rooms.Models.Planes;

namespace Server.Reawakened.Rooms.Models.Entities;

public class StoredEntityModel(GameObjectModel gameObject, Room room, FileLogger logger)
{
    public readonly GameObjectModel GameObject = gameObject;
    public readonly Room Room = room;
    public readonly FileLogger Logger = logger;
}
