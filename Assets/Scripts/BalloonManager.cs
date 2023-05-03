using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEngine;

namespace Scripts
{
	public class BalloonManager : Singleton<BalloonManager>
	{
		#region fields/settings
		[SerializeField] private List<BalloonSpawnEvent> spawnEvents = new List<BalloonSpawnEvent>();
		[SerializeField, Required] private Transform spawnParent;
		[SerializeField] private List<Transform> transformPath = new List<Transform>();
		#endregion

		private float _lastSpawnTime = Mathf.NegativeInfinity;

		private int _eventSpawnCount;
		private BalloonSpawnEvent _currentEvent;

		private void Start()
		{
			spawnParent ??= transform; // ignore immediate error
			StartCoroutine(SpawnRoutine());
		}

		private IEnumerator SpawnRoutine()
		{
			foreach (BalloonSpawnEvent spawnEvent in spawnEvents)
			{
				// delay some amount
				if (spawnEvent.eventStartDelay > 0f)
				{
					yield return new WaitForSeconds(spawnEvent.eventStartDelay);
				}

				// spawn all the balloons for this event
				for (int c = 0; c < spawnEvent.count; c++)
				{
					_lastSpawnTime = Time.time;
					_eventSpawnCount++;

					BalloonScript spawnedBalloon = Instantiate(_currentEvent.balloonPrefab, transformPath.First().position.WithZ(0f), Quaternion.identity);
					spawnedBalloon.Init(transformPath); // OBS: will include spawning point

					// delay next spawn in event by some amount
					if (spawnEvent.delayBetweenSpawns > 0f)
					{
						yield return new WaitForSeconds(spawnEvent.delayBetweenSpawns);
					}
				}
			}
		}
	}
}