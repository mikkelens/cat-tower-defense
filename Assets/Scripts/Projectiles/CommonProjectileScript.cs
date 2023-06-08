using System.Collections;
using Scripts.Yarn;
using Sirenix.OdinInspector;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Projectiles
{
	[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
	public class CommonProjectileScript : MonoBehaviour
	{
		private Rigidbody2D _rb;
		private CircleCollider2D _col;
		private SpriteRenderer _spriteRenderer;

		[ShowInInspector, ReadOnly] private Projectile _projectile;

		public void Init(Projectile projectile, Vector2 throwDirection) // only called once
		{
			_rb = GetComponent<Rigidbody2D>();
			_col = GetComponent<CircleCollider2D>();
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

			_throwDirection = throwDirection;
			_startTime = Time.time;
			_projectileAlive = true;

			ApplyProjectile(projectile);
		}

		private Vector2 _throwDirection;
		private float _startTime;
		private int _projectileDamageLeft;
		private bool _projectileAlive;

		private void ApplyProjectile(Projectile projectile) // possibly used multiple times
		{
			if (projectile == null) Debug.LogError("Projectile missing when trying to assign it at instantiation Init()");

			_projectile = projectile;
			_projectileDamageLeft = _projectile.MaxTotalDamage.Enabled ? _projectile.MaxTotalDamage.Value : int.MaxValue;

			_rb.velocity = _throwDirection * _projectile.TravelSpeed;
			_col.radius = _projectile.ColliderRadius;
			_spriteRenderer.sprite = _projectile.Sprite;
			_spriteRenderer.color = _projectile.Color;
		}

		private void FixedUpdate()
		{
			// cull if out of bounds
			const float cullClearance = 3f;
			if (_projectileAlive && (_projectile.MaxLifetime.Enabled && _startTime.TimeSince() > _projectile.MaxLifetime.Value
			                         || Camera.main.PointOutsideViewArea2D(transform.position.V2FromV3(), cullClearance)))
			{
				Cull();
			}
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (!_projectileAlive) return;
			CommonYarnScript target = collision.collider.GetComponent<CommonYarnScript>();
			if (target == null) return;

			if (!target.CanBeDamaged) return;

			DealProjectileDamageToYarn(target);
		}

		private void DealProjectileDamageToYarn(CommonYarnScript target) // returns true if all damage absorbed
		{
			bool hasCompletedImpact = false;
			while (true)
			{
				int damage;
				if (!_projectile.MaxTotalDamage.Enabled)
				{
					damage = int.MaxValue;
				}
				else
				{
					damage = Mathf.Min(_projectile.MaxTotalDamage.Value, _projectileDamageLeft); // cap under remaining damage points

					if (_projectile.ShellDurabilityType == Projectile.ShellDurability.Rigid && _projectile.MaxDamagePerCollision.Enabled) // check if rigid first bc fragility means maxdamage should be off
					{
						damage = Mathf.Min(damage, _projectile.MaxDamagePerCollision.Value); // cap under max damaage per hit
					}
				}


				if (_projectile.impactAreaOfEffect.Enabled &&
				    (_projectile.impactAreaOfEffect.Value.TriggerType == Projectile.AreaOfEffect.Trigger.AllImpacts ||
				     (!hasCompletedImpact && _projectile.impactAreaOfEffect.Value.TriggerType == Projectile.AreaOfEffect.Trigger.FirstImpact)))
				{
					// first impact under condition or any impact otherwise
					DealAreaOfEffectDamage(_projectile.impactAreaOfEffect.Value);
				}

				_projectileDamageLeft -= target.Damage(damage, _projectile.SurfaceImpactType);
				hasCompletedImpact = true;


				if (_projectileDamageLeft <= 0 || _projectile.ShellDurabilityType == Projectile.ShellDurability.Fragile)
				{
					if (_projectile.impactAreaOfEffect.Enabled &&
					    _projectile.impactAreaOfEffect.Value.TriggerType == Projectile.AreaOfEffect.Trigger.LastImpact)
					{
						DealAreaOfEffectDamage(_projectile.impactAreaOfEffect.Value);
					}

					if (_projectile.BelowProjectile.Enabled)
					{
						ApplyProjectile(_projectile.BelowProjectile.Value);

						if (_projectile.BelowDamageStack == Projectile.DamageStack.CanDamageWithBoth)
						{
							continue;
						}
					}
					else
					{
						StartCoroutine(KillProjectile());
					}
				}

				break;
			}
		}

		private void DealAreaOfEffectDamage(Projectile.AreaOfEffect areaOfEffect)
		{
			Vector3 pos = transform.position;

			// spawn effect
			Debug.Log($"Spawning area of effect from {name}", this);
			Debug.Log($"areaOfEffect BasePrefab: {areaOfEffect.EffectBasePrefab}", areaOfEffect.EffectBasePrefab);
			BasicEffectScript effectScript = Instantiate(areaOfEffect.EffectBasePrefab, pos, Quaternion.identity, ProjectileManager.Instance.projectileParent);
			effectScript.Init(areaOfEffect.Effect);

			// deal damage
			int totalDamage = areaOfEffect.MaxTotalDamage.Enabled ? areaOfEffect.MaxTotalDamage.Value : int.MaxValue;
			Collider2D[] yarnColliders = Physics2D.OverlapCircleAll(pos.V2FromV3(), areaOfEffect.Radius, LayerMask.GetMask("Yarn"));
			foreach (Collider2D yarnCollider in yarnColliders)
			{
				CommonYarnScript yarnScript = yarnCollider.GetComponent<CommonYarnScript>();
				if (yarnScript == null)
				{
					Debug.LogWarning($"Collider {yarnCollider} did not have a yarn script!");
					continue;
				}

				int damage = totalDamage;

				if (areaOfEffect.MaxDamagePerCollider.Enabled) damage = Mathf.Min(damage, areaOfEffect.MaxDamagePerCollider.Value);

				totalDamage -= yarnScript.Damage(damage, areaOfEffect.ImpactType);
				if (totalDamage <= 0) break;
			}
		}

		private IEnumerator KillProjectile()
		{
			_projectileAlive = false;

			_col.enabled = false;
			_rb.constraints = RigidbodyConstraints2D.FreezeAll;

			if (_projectile.DeathEffect.Enabled)
			{
				yield return _projectile.DeathEffect.Value.ApplyEffectToSpriteRenderer(_spriteRenderer);
			}
			Cull();
		}

		private void Cull()
		{
			Destroy(gameObject);
		}
	}
}