﻿using Server.Reawakened.Entities.Components.AI.Stats;
using Server.Reawakened.Entities.Enemies.Behaviors.Abstractions;
using Server.Reawakened.Entities.Enemies.EnemyTypes;
using Server.Reawakened.XMLs.Data.Enemy.States;

namespace Server.Reawakened.Entities.Enemies.Behaviors;

public class AIBehaviorPatrol(PatrolState patrolState, AIStatsGlobalComp globalComp, AIStatsGenericComp genericComp) : AIBaseBehavior
{
    public float MoveSpeed => globalComp.Patrol_MoveSpeed != globalComp.Default.Patrol_MoveSpeed ? globalComp.Patrol_MoveSpeed : patrolState.Speed;
    public bool SmoothMove => globalComp.Patrol_SmoothMove != globalComp.Default.Patrol_SmoothMove ? globalComp.Patrol_SmoothMove : patrolState.SmoothMove;
    public float EndPathWaitTime => globalComp.Patrol_EndPathWaitTime != globalComp.Default.Patrol_EndPathWaitTime ? globalComp.Patrol_EndPathWaitTime : patrolState.EndPathWaitTime;
    public float PatrolX => genericComp.PatrolX;
    public float PatrolY => genericComp.PatrolY;
    public int ForceDirectionX => genericComp.Patrol_ForceDirectionX;
    public float InitialProgressRatio => genericComp.Patrol_InitialProgressRatio;

    public override bool ShouldDetectPlayers => true;

    protected override AI_Behavior GetBehaviour() => new AI_Behavior_Patrol(MoveSpeed, EndPathWaitTime, PatrolX, PatrolY, ForceDirectionX, InitialProgressRatio);

    public override object[] GetData() => [
        MoveSpeed, SmoothMove, EndPathWaitTime,
        PatrolX, PatrolY, ForceDirectionX, InitialProgressRatio
    ];

    public override void NextState(BehaviorEnemy enemy) { }
}