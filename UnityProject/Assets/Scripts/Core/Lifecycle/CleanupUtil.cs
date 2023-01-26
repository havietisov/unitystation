using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public static class CleanupUtil
{
	public static int RidListOfDeadElements(IList<Action> list_in_question)
	{ 
		List<Action> survivor_list = new List<Action>();

		for (int i = 0, max = list_in_question.Count; i < max; i++)
		{
			if (list_in_question[i] != null && list_in_question[i].Target == null)
			{
				survivor_list.Add(list_in_question[i]);
			}
		}

		int res = list_in_question.Count - survivor_list.Count;
		list_in_question.Clear();

		for (int i = 0, max = survivor_list.Count; i < max; i++)
		{
			list_in_question.Add(survivor_list[i]);
		}

		return res;
	}

	public static int RidListOfDeadElements<T>(IList<T> list_in_question) where T: MonoBehaviour
	{
		List<T> survivor_list = new List<T>();
		
		for (int i = 0, max = list_in_question.Count; i < max; i++)
		{
			if (list_in_question[i] != null)
			{
				survivor_list.Add(list_in_question[i]);
			}
		}

		int res = list_in_question.Count - survivor_list.Count;
		list_in_question.Clear();

		for (int i = 0, max = survivor_list.Count; i < max; i++)
		{
			list_in_question.Add(survivor_list[i]);
		}

		return res;
	}

	public static int RidDictionaryOfDeadElements<TKey, TValue>(IDictionary<TKey, TValue> dict_in_question)
	{
		List<KeyValuePair<TKey, TValue>> survivor_list = new List<KeyValuePair<TKey, TValue>>();
		
		foreach(var a in dict_in_question)
		{
			if (a.Key != null && a.Value != null)
			{
				survivor_list.Add(a);
			}
		}

		int res = dict_in_question.Count - survivor_list.Count;
		dict_in_question.Clear();

		for (int i = 0, max = survivor_list.Count; i < max; i++)
		{
			dict_in_question.Add(survivor_list[i].Key, survivor_list[i].Value);
		}

		return res;
	}

	public static void PerformCleanup()
	{
		ComponentManager.ObjectToPhysics.Clear();
		Spawn.Clean();
	}
}
