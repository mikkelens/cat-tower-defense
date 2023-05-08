using System.Collections;
using System.Linq;
using JetBrains.Annotations;
using Scripts.Projectiles;
using Scripts.Yarn;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Scripts.Towers
{
	[DisallowMultipleComponent] // ensures editor things work as expected
	#if UNITY_EDITOR
	[ExecuteInEditMode] // required to make undo-ing work properly for stats
	#endif
	public class TowerScript : MonoBehaviour
	{
		[SerializeField] private LeveledStats stats;
		[SerializeField, Required] private Transform projectileSourcePoint;

		#if UNITY_EDITOR
		private void OnEnable()
		{
			Undo.undoRedoPerformed += stats.UpdateStats; // ensure undos actually refresh everything, and nicely
		}
		private void OnDisable()
		{
			Undo.undoRedoPerformed -= stats.UpdateStats;
		}
		private void OnValidate()
		{
			stats.AssignSpriteRendererIfNecessary();
			stats.UpdateStats();
		}
		#endif

		private void Start()
		{
			stats.Init(GetComponentInChildren<SpriteRenderer>());
			StartCoroutine(SpawnRoutine());
		}

		private float ShootDelay => 1f / stats.AttackSpeed;
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
			Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position.V2FromV3(), stats.Range);
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
			OldProjectileScript oldProjectile = Instantiate(stats.Projectile, projectileSourcePos, Quaternion.identity, ProjectileManager.Instance.projectileParent);
			oldProjectile.Init(projectileThrowDirection);
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.grey;
			Gizmos.DrawWireSphere(transform.position, stats.Range);
		}
	}
}