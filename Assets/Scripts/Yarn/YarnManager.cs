using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Scripts.Yarn
{
	public class YarnManager : Singleton<YarnManager>
	{
		[Header("Yarn Spawning")]
		[SerializeField, Required] private Transform spawnParent;
		[SerializeField, Required] private YarnScript yarnScriptBasePrefab;
		[SerializeField] private List<YarnSpawnEvent> spawnEvents = new List<YarnSpawnEvent>();
		[Space]
		[SerializeField] private List<Transform> transformPath = new List<Transform>();



		[Header("Debug")]
		[SerializeField] private bool showPathAlways = true;

		private void Start()
		{
			spawnParent ??= transform; // ignore immediate error
			StartCoroutine(SpawnRoutine());
		}

		private IEnumerator SpawnRoutine()
		{
			foreach (YarnSpawnEvent spawnEvent in spawnEvents)
			{
				// delay some amount
				if (spawnEvent.eventStartDelay > 0f)
				{
					yield return new WaitForSeconds(spawnEvent.eventStartDelay);
				}

				// spawn all the balloons for this event
				for (int c = 0; c < spawnEvent.count; c++)
				{
					YarnScript spawnedYarn = Instantiate(yarnScriptBasePrefab, transformPath.First().position.WithZ(0f), Quaternion.identity, spawnParent);
					spawnedYarn.Init(spawnEvent.yarnLayer, transformPath);

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
			bool drawLinesAndPoints;

			if (showPathAlways)
			{
				drawEnds = true;
				drawLinesAndPoints = true;
			}
			else
			{
				Object[] selectedObjects = Selection.objects;
				drawEnds = selectedObjects.Contains(gameObject);
				drawLinesAndPoints = transformPath.Exists(point =>
				{
					Transform parent;
					return selectedObjects.Contains(point.gameObject)
					       || ((parent = point.parent) != null && selectedObjects.Contains(parent.gameObject));
				});
			}

			if (!drawEnds && !drawLinesAndPoints) return;

			// draw lines
			Gizmos.color = Color.grey;
			Gizmos.DrawLineStrip(transformPath.Select(x => x.position).ToArray(), false);

			if (drawLinesAndPoints)
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