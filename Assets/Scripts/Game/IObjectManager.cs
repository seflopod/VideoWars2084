using UnityEngine;
using System.Collections;

public interface IObjectManager
{
	void Disable();
	void Enable();
	int Id { get; }
}
