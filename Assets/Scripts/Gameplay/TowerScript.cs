using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using Scripts.TowerUpgradeSystem;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Gameplay
{
	public class TowerScript : MonoBehaviour
	{
		[Header("Tower")]
		[SerializeField, InlineProperty] private TowerStat<float> range = new TowerStat<float>(2.1f);
		[Space]
		[SerializeField, InlineProperty] private TowerStat<float> fireSpeed = new TowerStat<float>(1f);

		[Header("Projectile")]
		[SerializeField, AssetsOnly, Required] private ProjectileScript projectilePrefab;
		[SerializeField, Required] private Transform projectileSourcePoint;

		[HorizontalGroup("LevelGroup")]
		[SerializeField, ReadOnly] private int level = -1; // 0 by default
		[ButtonGroup("LevelGroup/LevelButtons"), UsedImplicitly] private void IncreaseLevel()
		{
			level++;
			fireSpeed.Level = range.Level = level;
		}
		[ButtonGroup("LevelGroup/LevelButtons"), UsedImplicitly] private void DecreaseLevel()
		{
			level--;
			fireSpeed.Level = range.Level = level;
		}

		private void Start()
		{
			StartCoroutine(SpawnRoutine());
		}

		private float ShootDelay => 1f / fireSpeed.UpdatedValue;
		private IEnumerator SpawnRoutine()
		{
			while (true)
			{
				YarnScript newTarget = FindTargetInRange();
				if (newTarget != null)
				{
					SpawnProjectile(newTarget);
					yield return new WaitForSeconds(ShootDelay);
				}
				else
				{
					yield return new WaitForSeconds(Time.deltaTime);
				}
			}
		}

		[CanBeNull] private YarnScript FindTargetInRange()
		{
			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position.V2FromV3(), range.UpdatedValue);
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
			Gizmos.DrawWireSphere(transform.position, range.UpdatedValue);
		}
	}
}