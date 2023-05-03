using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts
{
	[Serializable]
	public class BalloonSpawnEvent
	{
		[AssetsOnly] public BalloonScript balloonPrefab;
		[Min(0), MaxValue(1000)] public int count = 1;

		[Space]
		[Min(0f)] public float eventStartDelay;
		[ShowIf("@count > 1")]
		[Min(0f)] public float delayBetweenSpawns = 0.33f;
	}
}