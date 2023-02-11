using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class Reporting
{
	public static string ReportingPath = "H:/UnityStationDumps/dump.json";

	[System.Serializable]
	public class ObjectInfo
	{
		[SerializeField]
		public string ObjectString;
		[SerializeField]
		public WeakReference<MonoBehaviour> weakref;

		public int scene_build_index = -9999;
	}

	[System.Serializable]
	public struct FinalInfo
	{
		[SerializeField]
		public string ObjectString;
		[SerializeField]
		public string lifecycle_status;
	}

	static System.Collections.Concurrent.ConcurrentQueue<ObjectInfo> object_queue = new System.Collections.Concurrent.ConcurrentQueue<ObjectInfo>();
	public static void AddObject(MonoBehaviour @object)
	{
		object_queue.Enqueue(new ObjectInfo() { ObjectString = @object.GetType() + " :: " +  @object.ToString(), weakref = new WeakReference<MonoBehaviour>(@object), scene_build_index = @object.gameObject.scene.buildIndex });
	}

	public static void DumpAll()
	{
		using (var writer = System.IO.File.AppendText(ReportingPath))
		{
			foreach (var a in object_queue)
			{
				FinalInfo fi = new FinalInfo();
				fi.ObjectString = a.ObjectString;
				MonoBehaviour t;

				if (a.weakref.TryGetTarget(out t))
				{
					if (t != null)
					{
						fi.lifecycle_status = "alive, build index : " + t.gameObject.scene.buildIndex;
					}
					else
					{
						fi.lifecycle_status = "leaked, build index : " + a.scene_build_index;
					}

					if (t is IHasDestructionInfo)
					{
						fi.lifecycle_status += ", destruction info : " + (t as IHasDestructionInfo).GetInfo();
					}

					writer.WriteLine(UnityEngine.JsonUtility.ToJson(fi));
				}
			}

			writer.Flush();
		}

		Debug.Log("Dumped " + object_queue.Count + " objects");
		object_queue.Clear();
	}
}

public static class CleanupUtil
{

	public static int RidListOfSoonToBeDeadElements<T>(IList<T> list_in_question, Func<T, UnityEngine.MonoBehaviour> target_extractor)
	{
		if (list_in_question == null)
		{
			return -1;
		}

		List<T> survivor_list = new List<T>();

		for (int i = 0, max = list_in_question.Count; i < max; i++)
		{
			MonoBehaviour target = target_extractor(list_in_question[i]);

			if (list_in_question[i] != null && (target == null || target.gameObject.scene.buildIndex == -1))
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

	public static int RidListOfDeadElements<T>(IList<T> list_in_question, Func<T, UnityEngine.MonoBehaviour> target_extractor)
	{
		if (list_in_question == null)
		{
			return -1;
		}

		List<T> survivor_list = new List<T>();

		for (int i = 0, max = list_in_question.Count; i < max; i++)
		{
			MonoBehaviour target = target_extractor(list_in_question[i]);

			if (list_in_question[i] != null && target == null)
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

	public static int RidListOfDeadElements(IList<Action> list_in_question)
	{
		if (list_in_question == null)
		{
			return -1;
		}

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
		if (list_in_question == null)
		{
			return -1;
		}
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

	public static int RidDictionaryOfDeadElements<TKey, TValue>(Dictionary<TKey, TValue> dict_in_question, Func<TKey, TValue, bool> condition )
	{
		if (dict_in_question == null)
		{
			return -1;
		}

		List<KeyValuePair<TKey, TValue>> survivor_list = new List<KeyValuePair<TKey, TValue>>();

		foreach (var a in dict_in_question)
		{
			if (condition(a.Key, a.Value))
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

	public static int RidDictionaryOfDeadElements<TKey, TValue>(IDictionary<TKey, TValue> dict_in_question)
	{
		if (dict_in_question == null)
		{
			return -1;
		}

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

	public static void EndRoundCleanup()
	{
		PlayerManager.Reset();
		GameManager.Instance.CentComm.Clear();
		Items.Weapons.ExplosiveBase.ExplosionEvent = new UnityEngine.Events.UnityEvent<Vector3Int, Items.Weapons.BlastData>();
		Items.TrackingBeacon.Clear();
		SoundManager.Instance.Clear();
		SpriteHandlerManager.Instance.OnRoundRestart(default(UnityEngine.SceneManagement.Scene), default(UnityEngine.SceneManagement.Scene));
		UpdateManager.Instance.Clear();
	}

	public static void CleanupInbetweenScenes()
	{
		MatrixManager.Instance.ResetMatrixManager();
		MatrixManager.IsInitialized = false;
		GameManager.Instance.ResetStaticsOnNewRound();
		Systems.Cargo.CargoManager.Instance.OnRoundRestart();
		Systems.Scenes.LavaLandManager.Instance.Clean();
		ClientSynchronisedEffectsManager.Instance.ClearData();
		TileManager.Instance.Cleanup_between_rounds();
	}

	public static void RoundStartCleanup()
	{
		Initialisation.LoadManager.RegisterActionDelayed(()=> { Debug.Log("Delayed cleanup started");
			ComponentManager.ObjectToPhysics.Clear();
			Spawn.Clean();
			MatrixManager.Instance.PostRoundStartCleanup();
			Managers.SignalsManager.Instance.Clear();
			//SpriteHandlerManager.Instance.ClearAllDirtyBits();
			//UpdateManager.Instance.Clear();
			AdminTools.AdminOverlay.Instance?.Clear();
			//
			TileManager.Instance.DeepCleanupTiles();
			UI.Core.Action.UIActionManager.Instance.Clear();//maybe it'l work second time?
			EventManager.Instance.Clear();
			//PlayerList.Instance.AllPlayers.ForEach(u => u.GameObject = u.GameObject == null ? null : u.GameObject);
			//CustomNetworkManager.Instance.Clear();
			//DynamicItemStorage.Clear();
			//
			//
			//
			//
			//Systems.Scenes.LavaLandManager.ClearBetweenRounds();
			////
			//foreach (var a in UnityEngine.GameObject.FindObjectsOfType<TileManager>())
			//{
			//	a.Cleanup_between_rounds();
			//}
			//
			//
			//TileManager.Instance.DeepCleanupTiles();
			//Spawn.Clean();
			//Systems.Scenes.LavaLandManager.ClearBetweenRounds();
			//CustomNetworkManager.Instance.Clear();
			//
			//foreach (var a in UnityEngine.GameObject.FindObjectsOfType<TileManager>())
			//{
			//	a.Cleanup_between_rounds();
			//}
			//Debug.Log("Delayed cleanup finished");
		}, 30);

		//
	}
}
