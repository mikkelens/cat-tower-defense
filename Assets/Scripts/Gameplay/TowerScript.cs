using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Gameplay
{
	public class TowerScript : MonoBehaviour
	{
		[SerializeField] private float range = 2.75f;
		[SerializeField, Min(0.01f)] private float projectilesPerSecond = 1f;
		[SerializeField, AssetsOnly, Required] private ProjectileScript projectilePrefab;
		[SerializeField, Required] private Transform projectileSourcePoint;

		private void Start()
		{
			StartCoroutine(SpawnRoutine());
		}

		private IEnumerator SpawnRoutine()
		{
			float shootDelay = 1f / projectilesPerSecond;
			while (true)
			{
				YarnScript newTarget = FindTargetInRange();
				if (newTarget != null)
				{
					SpawnProjectile(newTarget);
					yield return new WaitForSeconds(shootDelay);
				}
				else
				{
					yield return new WaitForSeconds(Time.deltaTime);
				}
			}
		}

		[CanBeNull] private YarnScript FindTargetInRange()
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position.V2FromV3(), range);
			return transform.ClosestScript(colliders
				.Select(x => x.GetComponent<YarnScript>())
				.Where(x => x != null));
		}

		private void SpawnProjectile(YarnScript targetYarn)
		{
			Vector2 targetPos = targetYarn.transform.position.V2FromV3();
			Vector2 lookDir = (targetPos - transform.position.V2FromV3()).normalized;
			transform.rotation = Quaternion.FromToRotation(Vector3.down, lookDir.V3FromV2()); // taken from yarn script follow rotation

			Vector3 projectileSourcePos = projectileSourcePoint.position;
			Vector2 projectileThrowDirection = (targetPos - projectileSourcePos.V2FromV3()).normalized;
			ProjectileScript projectile = Instantiate(projectilePrefab, projectileSourcePos, Quaternion.identity, ProjectileManager.Instance.projectileParent);
			projectile.Init(projectileThrowDirection);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(transform.position, range);
		}
	}
}