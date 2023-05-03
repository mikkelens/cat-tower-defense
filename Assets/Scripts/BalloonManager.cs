using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts
{
	public class BalloonManager : Singleton<BalloonManager>
	{
		#region fields/settings
		[SerializeField, Required] private Transform spawnParent;
		[SerializeField] private List<BalloonSpawnEvent> spawnEvents = new List<BalloonSpawnEvent>();
		[Space]
		[SerializeField] private List<Transform> transformPath = new List<Transform>();
		#endregion

		[Header("Debug")]
		[SerializeField] private bool showInfoAlways = true;

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
					BalloonScript spawnedBalloon = Instantiate(spawnEvent.balloonPrefab, transformPath.First().position.WithZ(0f), Quaternion.identity, spawnParent);
					spawnedBalloon.Init(transformPath);

					// delay next spawn in event by some amount
					if (spawnEvent.delayBetweenSpawns > 0f)
					{
						yield return new WaitForSeconds(spawnEvent.delayBetweenSpawns);
					}
				}
			}
		}

		#if UNITY_EDITOR
		private void OnDrawGizmos()
		{
			bool drawEnds;
			bool drawPath;

			if (showInfoAlways)
			{
				drawEnds = true;
				drawPath = true;
			}
			else
			{
				Object[] selectedObjects = Selection.objects;
				drawEnds = selectedObjects.Contains(gameObject);
				drawPath = transformPath.Exists(point =>
				{
					Transform parent;
					return selectedObjects.Contains(point.gameObject)
					       || ((parent = point.parent) != null && selectedObjects.Contains(parent.gameObject));
				});
			}

			if (!drawEnds && !drawPath) return;

			// draw lines
			Gizmos.color = Color.grey;
			Gizmos.DrawLineStrip(transformPath.Select(x => x.position).ToArray(), false);

			if (drawPath)
			{
				// draw points
				Gizmos.color = Color.white;
				Vector3 pointCube = 0.15f.ToCubeV3();
				foreach (Transform point in transformPath)
				{
					Gizmos.DrawCube(point.position, pointCube);
				}
			}
			if (drawEnds)
			{
				const float pointRadius = 0.125f;
				// draw start and end
				Transform first = transformPath.FirstOrDefault();
				Transform last = transformPath.LastOrDefault();
				if (first != null)
				{
					Gizmos.color = Color.green;
					Gizmos.DrawSphere(first.position, pointRadius);
				}
				if (last != null)
				{
					Gizmos.color = Color.red;
					Gizmos.DrawSphere(last.position, pointRadius);
				}
			}
		}
		#endif
	}
}