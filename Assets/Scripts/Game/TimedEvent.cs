using UnityEngine;
using System.Collections;

public class TimedEvent : IEqualityComparer
{
	private event TimedEventListener _listeners;
	
	private string _name;
	private float _timer;
	private float _target;
	
	public TimedEvent(string name, float target)
	{
		_name = name;
		_target = target;
		_timer = 0.0f;
	}
	
	public void Increment(float dt)
	{
		_timer+=dt;
		if(_timer >= _target)
		{
			FireEvent();
			Reset();
		}
	}
	
	public void FireEvent()
	{
		if(_listeners != null)
			_listeners(_timer);
	}
	
	public void Reset()
	{
		_timer = 0.0f;
	}
	
	public void RegisterListener(TimedEventListener listener)
	{
		_listeners += listener;
	}
	
	public void DeregisterListener(TimedEventListener listener)
	{
		_listeners -= listener;
	}
	
	/// <summary>
	/// Equals the specified obj1 and obj2.
	/// </summary>
	/// <param name='obj1'>
	/// If set to <c>true</c> obj1.
	/// </param>
	/// <param name='obj2'>
	/// If set to <c>true</c> obj2.
	/// </param>
	public new bool Equals(object obj1, object obj2)
	{
		return obj1.GetHashCode() == obj2.GetHashCode();
	}
	
	/// <summary>
	/// Serves as a hash function for a <see cref="TimedEvent"/> object.
	/// </summary>
	/// <returns>
	/// A hash code for this instance that is suitable for use in hashing
	/// algorithms and data structures such as a hash table.
	/// </returns>
	public new int GetHashCode()
	{
		const int FNVprime = 16777619;
		uint offset = 2166136261;
		int ret = (int)offset;
		char[] octets = _name.ToCharArray();
		foreach(char octet in octets)
		{
			ret ^= (int)octet;
			ret *= FNVprime;
		}
		return ret;
	}
	
	/// <summary>
	/// Gets the hash code.
	/// </summary>
	/// <returns>
	/// A hash code for this instance that is suitable for use in hashing
	/// algorithms and data structures such as a hash table.
	/// </returns>
	/// <param name='obj'>
	/// Object.
	/// </param>
	public int GetHashCode(object obj)
	{
		if((TimedEvent)obj != null)
		{
			const int FNVprime = 16777619;
			uint offset = 2166136261;
			int ret = (int)offset;
			char[] octets = ((TimedEvent)obj).Name.ToCharArray();
			foreach(char octet in octets)
			{
				ret ^= (int)octet;
				ret *= FNVprime;
			}
			return ret;
		}
		else
			return obj.GetHashCode();
	}
	
	
	public string Name { get { return _name; } }
	
	public float Target
	{
		get { return _target; }
		set { _target = value; }
	}
	
	public float Timer { get { return _timer; } }
}