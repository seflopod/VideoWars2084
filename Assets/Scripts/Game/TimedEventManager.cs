using UnityEngine;
using System.Collections.Generic;

public delegate void TimedEventListener(float timerValue);

public class TimedEventManager
{
	private static Dictionary<string, TimedEvent> _timedEvents = new Dictionary<string, TimedEvent>();
	
	public static void AddEvent(string eventName, float targetTime)
	{
		if(!_timedEvents.ContainsKey(eventName))
			_timedEvents.Add(eventName, (new TimedEvent(eventName, targetTime)));
	}
	
	public static void Register(string eventName, TimedEventListener listener)
	{
		_timedEvents[eventName].RegisterListener(listener);
	}
	
	public static void IncrementTimers(float dt)
	{
		foreach(KeyValuePair<string, TimedEvent> kvp in _timedEvents)
			kvp.Value.Increment(dt);
	}
	
	public static TimedEvent GetEvent(string name)
	{
		return _timedEvents[name];
	}
}
