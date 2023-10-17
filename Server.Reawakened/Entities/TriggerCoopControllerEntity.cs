﻿using A2m.Server;
using Server.Reawakened.Configs;
using Server.Reawakened.Entities.Abstractions;
using Server.Reawakened.Players;
using Server.Reawakened.Players.Extensions;
using Server.Reawakened.XMLs.Bundles;

namespace Server.Reawakened.Entities;

public class TriggerCoopControllerEntity : AbstractTriggerCoop<TriggerCoopController>
{
    public ItemCatalog ItemCatalog { get; set; }
    public QuestCatalog QuestCatalog { get; set; }
    public ServerRConfig ServerConfig { get; set; }

    public override void Triggered(Player player, bool isSuccess, bool isActive)
    {
        if (isActive)
            if (PrefabName == "GoToTrigger")
                player.CheckObjective(QuestCatalog, ObjectiveEnum.Goto, Id, ServerConfig.QuestItemId, 1);
    }
}
