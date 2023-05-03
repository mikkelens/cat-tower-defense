using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts
{
	[Serializable]
	public class BalloonSpawnEvent
	{
		[Min(0f)] public float eventStartDelay;
		[Space(1f)]
		[AssetsOnly] public BalloonScript balloonPrefab;
		[Min(0), MaxValue(1000)] public int count = 1;
		[ShowIf("@count > 1")]
		[Min(0f)] public float delayBetweenSpawns = 0.33f;
	}
}