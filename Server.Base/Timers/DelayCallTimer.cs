﻿using Server.Base.Timers.Extensions;
using Server.Base.Timers.Services;

namespace Server.Base.Timers;

public class DelayCallTimer(TimeSpan delay, TimeSpan interval, int count, Timer.TimerCallback callback, TimerThread tThread) : Timer(delay, interval, count, tThread)
{
    public TimerCallback Callback { get; } = callback;

    public override void OnTick() => Callback?.Invoke();

    public override string ToString() => $"DelayCallTimer [{Callback.FormatDelegate()}]";
}
