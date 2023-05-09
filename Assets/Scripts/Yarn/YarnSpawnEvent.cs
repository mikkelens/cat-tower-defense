using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Scripts.Yarn
{
	[Serializable]
	public class YarnSpawnEvent
	{
		[Min(0f)] public float eventStartDelay;
		[Space(1f)]
		[AssetsOnly] public YarnLayer yarnLayer;
		[Min(0), MaxValue(1000)] public int count = 1;
		[ShowIf("@count > 1")]
		[Min(0f)] public float delayBetweenSpawns = 0.33f;
	}
}