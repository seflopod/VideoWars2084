using System.Collections.Generic;

/// <summary>
/// Priority queue class.
/// </summary>
/// <description>
/// C# doesn't have a built-in priority queue, so I found an article online with
/// an implementation of one.  This is a slightly modified version of the class
/// found at
/// http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
/// </description>
public class PriorityQueue<T> where T : System.IComparable<T>
{
	private List<T> _data;
	
	public PriorityQueue()
	{
		_data = new List<T>();
	}
	
	/// <summary>
	/// Push the specified item.
	/// </summary>
	/// <param name='item'>
	/// Item.
	/// </param>
	public void Push(T item)
	{
		_data.Add (item);
		int c_idx = _data.Count - 1;
		while(c_idx > 0)
		{
			int p_idx = (c_idx - 1) / 2;
			
			//if the value at the child index is greater than the value at the
			//parent index, we're in order.  This could be changed/generalized
			//for using > or < for priority comparisons, but we're probably
			//going to use < most of the time anyway.
			if(_data[c_idx].CompareTo(_data[p_idx]) > 0)
				break;
			T tmp = _data[c_idx];
			_data[c_idx] = _data[p_idx];
			_data[p_idx] = tmp;
			c_idx = p_idx;
		}
	}
	
	/// <summary>
	/// Pop this instance.
	/// </summary>
	public T Pop()
	{
		//could throw an exception, for now assume that the list is not empty
		int li = _data.Count - 1;
		T frontItem = _data[0];
		_data[0] = _data[li];
		_data.RemoveAt(li);
		
		//now resort so the queue is in order
		--li;
		int p_idx = 0;
		while(true)
		{
			int c_idx = p_idx * 2 + 1;
			
			if(c_idx > li)
				break;
			
			int rc = c_idx + 1;
			
			if(rc <= li && _data[rc].CompareTo(_data[c_idx]) < 0)
				c_idx = rc;
			
			if(_data[p_idx].CompareTo(_data[c_idx]) <= 0)
				break;
			
			T tmp = _data[c_idx];
			_data[c_idx] = _data[p_idx];
			_data[p_idx] = tmp;
			c_idx = p_idx;
		}
		return frontItem;
	}
	
	/// <summary>
	/// View the front-most item in the PriorityQueue.
	/// </summary>
	public T Peek()
	{
		return _data[0];
	}
	
	public override string ToString ()
	{
		return string.Format ("[PriorityQueue: Count={0}]", Count);
	}
	
	public int Count { get { return _data.Count; } }
	public bool IsEmpty { get { return _data.Count == 0; } }
		
}
