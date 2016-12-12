using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public static class GameObjectExtensions
{
	public static ForwardPointerEvent<TEventData> ForwardPointerEvent<TEventData>(this GameObject gameObject, TEventData eventData)
		where TEventData : PointerEventData
	{
		return new ForwardPointerEvent<TEventData>(gameObject, eventData);
	}
}

public class ForwardPointerEvent<TData> where TData : PointerEventData
{
	private readonly GameObject _root;
	private readonly TData _data;

	public ForwardPointerEvent(GameObject root, TData data)
	{
		_root = root;
		_data = data;
	}

	public void To<THandler>(Func<THandler, Action<TData>> selectMethod) where THandler : IEventSystemHandler
	{
		if(_root == null)
		{
			return;
		}

		var results = new List<RaycastResult>();
		EventSystem.GetSystem(0).RaycastAll(_data, results);

		var foundRoot = false;
		foreach(var result in results)
		{
			if(foundRoot && result.gameObject != null)
			{
				if(ExecuteEvents.Execute(result.gameObject, _data, new ExecuteEvents.EventFunction<THandler>((h, b) => selectMethod(h)(ExecuteEvents.ValidateEventData<TData>(b)))))
				{
					return;
				}

			}
			else if(result.gameObject == _root)
			{
				foundRoot = true;
			}
		}
	}
}