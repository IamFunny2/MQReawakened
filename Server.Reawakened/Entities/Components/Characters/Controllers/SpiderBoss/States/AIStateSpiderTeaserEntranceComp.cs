﻿namespace Server.Reawakened.Entities.Components.Characters.Controllers.SpiderBoss.States;

public class AIStateSpiderTeaserEntranceComp : BaseAIState<AIStateSpiderTeaserEntrance>
{
    public float DelayBeforeEntranceDuration => ComponentData.DelayBeforeEntranceDuration;
    public float EntranceDuration => ComponentData.EntranceDuration;
    public float IntroDuration => ComponentData.IntroDuration;
    public float MinimumLifeRatioAccepted => ComponentData.MinimumLifeRatioAccepted;
    public float LifeRatioAtHeal => ComponentData.LifeRatioAtHeal;
}
