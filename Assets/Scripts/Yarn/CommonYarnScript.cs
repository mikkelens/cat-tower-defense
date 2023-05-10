using System.Collections;
using System.Collections.Generic;
using Scripts.Player;
using Scripts.Projectiles;
using Sirenix.OdinInspector;
using Tools.Utils;
using Unity.Properties;
using UnityEngine;

namespace Scripts.Yarn
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class CommonYarnScript : MonoBehaviour
	{
		[SerializeField] private float turnTime = 0.45f;
		[SerializeField] private AnimationCurve turnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

		[ShowInInspector, ReadOnly] private List<Transform> _pathTargets;
		[ShowInInspector, ReadOnly] private YarnLayer _layer;
		[ShowInInspector, ReadOnly] private int _layerHealth;

		public void Init(YarnLayer layer, List<Transform> targets) // called by manager that spawns it
		{
			_spriteRenderer = GetComponentInChildren<SpriteRenderer>();

			SetNewLayer(layer);

			_pathTargets = targets;
			StartCoroutine(PathFollowRoutine());
		}

		private void SetNewLayer(YarnLayer layer)
		{
			if (layer == null) Debug.LogError($"Yarn '{name}' missing source layer!");
			_layer = layer;
			UpdateValues();
		}
		private void UpdateValues()
		{
			Values = _layer.GetLayerValuesRecursively();
			_layerHealth = Values.health;

			_spriteRenderer.sprite = Values.sprite;
			_spriteRenderer.color = Values.color;
		}

		private YarnValues Values { get; set; }

		public bool CanBeDamaged => _layerHealth > 0;
		public int Damage(int fullProjectileDamage, Projectile.SurfaceImpact surfaceImpactType)
		{
			int maxPossibleDamage = Mathf.Min(fullProjectileDamage, _layerHealth); // can never deal more damage than our total health
			int layerDamage = Values.damageAbsorptionCap.Enabled ? Mathf.Min(maxPossibleDamage, Values.damageAbsorptionCap.Value) : maxPossibleDamage;
			_layerHealth -= layerDamage;
			if (_layerHealth > 0)
				return layerDamage; // damage that was dealt at layer, which didn't kill layer
			// layer died, more damage left

			YarnLayer.Surface surface = Values.surface; // will change when we kill the layer/change layer
			bool yarnDiedCompletely = KillCurrentLayer();
			if (yarnDiedCompletely || surfaceImpactType == Projectile.SurfaceImpact.SurfaceOnly || surface == YarnLayer.Surface.Impenetrable)
				return layerDamage; // damage that was dealt at layer, which *did* kill layer
			// yarn alive, remaining damage should be dealt to new layer

			int damageLeft = fullProjectileDamage - layerDamage;
			int belowLayersDamage = Damage(damageLeft, surfaceImpactType); // give rest (dropped damage) to below layer (recursive), receive how much was actually dealt
			return layerDamage + belowLayersDamage; // damage dealt at layer (and killed it) + damage that was done below this layer
		}
		private bool KillCurrentLayer()
		{
			switch (_layer)
			{
				case YarnBaseLayer:
					StopAllCoroutines(); // stops movement
					StartCoroutine(KillYarn());
					return true;
				case YarnOverrideLayer overrideLayer:
					SetNewLayer(overrideLayer.belowLayer);
					return false;
				default:
					throw new InvalidContainerTypeException(_layer.GetType());
			}
		}

		private SpriteRenderer _spriteRenderer;
		private SpriteRenderer SP => _spriteRenderer ??= GetComponentInChildren<SpriteRenderer>();

		private IEnumerator KillYarn()
		{
			if (Values.deathEffect.Enabled)
			{
				yield return Values.deathEffect.Value.ApplyEffectToSpriteRenderer(SP);
			}
			Cull();
		}

		private Rigidbody2D RB => _rb ??= GetComponent<Rigidbody2D>();
		private Rigidbody2D _rb;

		private IEnumerator PathFollowRoutine()
		{
			foreach (Transform pathTarget in _pathTargets)
			{
				float pathTargetFollowStartTime = Time.time;

				Vector2 targetPos = pathTarget.position.V2FromV3();
				Vector2 targetDir = (targetPos - transform.position.V2FromV3()).normalized;
				Vector2 awayDir = -targetDir;

				Quaternion pathTargetFollowStartRotation = transform.rotation;
				Quaternion pathTargetFollowTargetRotation = Quaternion.FromToRotation(Vector3.up, awayDir.V3FromV2());

				const float minDistance = 0.0001f;
				while (Vector2.Distance(RB.position, targetPos) > minDistance)
				{
					float turnT = pathTargetFollowStartTime.TimeSince() / turnTime;
					float smoothedTurnT = turnCurve.Evaluate(turnT);
					transform.rotation = Quaternion.LerpUnclamped(pathTargetFollowStartRotation, pathTargetFollowTargetRotation, smoothedTurnT);
					Vector2 newPos = Vector2.MoveTowards(RB.position, targetPos, Values.speed * Time.fixedDeltaTime);
					RB.MovePosition(newPos);
					yield return new WaitForSeconds(Time.fixedDeltaTime); // waits one frame: https://forum.unity.com/threads/coroutine-wait-x-frames-not-seconds.550168/
				}
			}

			int yarnDamage = _layer.GetStackedHealthRecursively();
			PlayerHealthManager.Instance.Damage(yarnDamage);
			Cull();
		}

		private void Cull()
		{
			Destroy(gameObject);
		}
	}
}