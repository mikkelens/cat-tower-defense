using System.Collections;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;

namespace Scripts
{
	public class TowerScript : MonoBehaviour
	{
		[SerializeField, Min(0.01f)] private float projectilesPerSecond = 1f;
		[SerializeField, AssetsOnly, Required] private ProjectileScript projectilePrefab;
		[SerializeField] private float spawnOffset;

		private void Start()
		{
			StartCoroutine(SpawnRoutine());
		}

		private IEnumerator SpawnRoutine()
		{
			float shootDelay = 1f / projectilesPerSecond;
			while (true)
			{
				yield return new WaitForSeconds(shootDelay);
				SpawnProjectile();
			}
		}

		private Vector2 FindTarget()
		{
			return Vector2.zero; // todo: implement
		}

		private void SpawnProjectile()
		{
			Transform towerTransform = transform;
			Vector3 towerPosition = towerTransform.position;

			Vector2 target = FindTarget();
			towerTransform.LookAt(target.V3FromV2()); // todo: test/fix?

			Vector2 projectileThrowDirection = (target - towerPosition.V2FromV3()).normalized;
			Vector2 projectileSpawnPosition = towerPosition.V2FromV3() + towerTransform.up.V2FromV3() * spawnOffset;
			ProjectileScript projectile = Instantiate(projectilePrefab, projectileSpawnPosition, Quaternion.identity, towerTransform);
			projectile.Init(projectileThrowDirection);
		}
	}
}