using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Tools.Utils;
using Unity.Properties;
using UnityEngine;

namespace Scripts.Yarn
{
	[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
	public class YarnScript : MonoBehaviour
	{
		[SerializeField, Required] private YarnLayer sourceLayer;

		// buttons for refreshing values after editing
		[ButtonGroup("LayerUpdateButtons"), PropertyOrder(-5)]
		[EnableIf("@_layer != null")]
		[Button("Force Source Update")] private void ForceCurrentUpdate() => UpdateValues();
		[ButtonGroup("LayerUpdateButtons")]
		[EnableIf("@sourceLayer != null && _layer == null")]
		[Button("Force Current Update")] private void ForceSourceUpdate() => UpdateValues();

		private YarnLayer _layer;
		private int _layerHealth;

		[ShowInInspector, EnableIf("@_pathTargets != null")] private List<Transform> _pathTargets;
		public void Init(List<Transform> targets) // called by manager that spawns it
		{
			_pathTargets = targets;

			if (sourceLayer == null) Debug.LogError($"Yarn '{name}' missing source layer!");
			SetNewLayer(sourceLayer);
			StartCoroutine(PathFollowRoutine());
		}

		private void SetNewLayer(YarnLayer layer)
		{
			_layer = layer;
			UpdateValues();
		}
		private void UpdateValues()
		{
			Values = _layer.GetValuesRecursively();
			_layerHealth = Values.health;
		}

		private YarnLayer.YarnValues Values { get; set; }

		public int Damage(int fullProjectileDamage)
		{
			if (_layerHealth == 0) return 0; // projectile can hit a dead thing if 2 projectiles hit it on the same physics update

			int maxPossibleDamage = Mathf.Min(fullProjectileDamage, _layerHealth); // can never deal more damage than our total health
			int layerDamage = Values.damageAbsorptionCap.Enabled ? Mathf.Min(maxPossibleDamage, Values.damageAbsorptionCap.Value) : maxPossibleDamage;
			_layerHealth -= layerDamage;
			if (_layerHealth > 0)
				return layerDamage; // damage that was dealt at layer, which didn't kill layer
			// layer died, more damage left

			YarnLayer.DamagePassthroughType passthroughType = Values.damagePassthroughType; // will change when we kill the layer/change layer
			bool yarnDiedCompletely = KillCurrentLayer();
			if (yarnDiedCompletely || passthroughType != YarnLayer.DamagePassthroughType.Penetrable)
				return layerDamage; // damage that was dealt at layer, which *did* kill layer
			// yarn alive, remaining damage should be dealt to new layer

			int belowLayersDamage = Damage(fullProjectileDamage - layerDamage); // give rest (dropped damage) to below layer (recursive), receive how much was actually dealt
			return layerDamage + belowLayersDamage; // damage dealt at layer (and killed it) + damage that was done below this layer
		}
		private bool KillCurrentLayer()
		{
			switch (_layer)
			{
				case YarnBaseLayer:
					Kill();
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

		private void Kill()
		{
			StopAllCoroutines();
			SP.sprite = Values.deathSprite;
			StartCoroutine(DeathRoutine());
		}

		private IEnumerator DeathRoutine()
		{
			yield return new WaitForSeconds(Values.deathTime);
			Destroy(gameObject);
		}

		private Rigidbody2D RB => _rb ??= GetComponent<Rigidbody2D>();
		private Rigidbody2D _rb;

		private IEnumerator PathFollowRoutine()
		{
			foreach (Transform pathTarget in _pathTargets)
			{
				Vector2 targetPos = pathTarget.position.V2FromV3();
				Vector2 targetDir = (targetPos - transform.position.V2FromV3()).normalized;
				Vector2 awayDir = -targetDir;
				transform.rotation = Quaternion.FromToRotation(Vector3.up, awayDir.V3FromV2()); // look away from direction, assuming default sprite dir is down (away from up)
				const float minDistance = 0.0001f;
				while (Vector2.Distance(RB.position, targetPos) > minDistance)
				{
					float deltaTime = Time.deltaTime;
					Vector2 newPos = Vector2.MoveTowards(RB.position, targetPos, Values.speed * deltaTime);
					RB.MovePosition(newPos);
					yield return new WaitForSeconds(deltaTime); // waits one frame: https://forum.unity.com/threads/coroutine-wait-x-frames-not-seconds.550168/
				}
			}
		}
	}
}