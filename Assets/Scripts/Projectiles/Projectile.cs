using System;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Tools.Types;
using Tools.Utils;
using UnityEngine;

namespace Scripts.Projectiles
{
	[CreateAssetMenu(menuName = "Scriptable/Projectile", fileName = "New Projectile")]
	public class Projectile : ScriptableObject
	{
		// movement and lifetime
		[field: Header("Movement, Size & Lifetime")]
		[field: Min(0f)]
		[field: SerializeField] public float TravelSpeed { get; private set; } = 4f;

		[field: Min(0f)]
		[field: SerializeField] public float ColliderRadius { get; private set; } = 0.25f;

		private bool MaxLifetimeValidation => MaxLifetime.Enabled == false || MaxLifetime.Value > 0f;
		[field: ValidateInput(nameof(MaxLifetimeValidation))]
		[field: SerializeField] public Optional<float> MaxLifetime { get; private set; } = new Optional<float>(4f);

		// damage
		private bool MaxTotalDamageValidation => MaxTotalDamage.Enabled == false || MaxTotalDamage.Value > 0;
		[field: Header("Damage")]
		[field: ValidateInput(nameof(MaxTotalDamageValidation), "Max Total Damage cannot be 0 or less.")]
		[field: SerializeField] public Optional<int> MaxTotalDamage { get; private set; } = new Optional<int>(3, true);

		// durability
		[field: ShowIf(nameof(SurfaceImpactShow))] // reuse
		[field: SerializeField] public ShellDurability ShellDurabilityType { get; private set; } = ShellDurability.Rigid;
		[Serializable]
		public enum ShellDurability
		{
			Rigid, // keeps going if more damage left
			Fragile // always breaks on impact
		}

		// impact
		[field: ShowIf(nameof(SurfaceImpactShow))]
		[field: SerializeField] public SurfaceImpact SurfaceImpactType { get; private set; } = SurfaceImpact.SurfaceOnly;
		private bool SurfaceImpactShow => !MaxTotalDamage.Enabled || MaxTotalDamage.Value > 1; // would never be used if maxdamage was enabled as 1
		[Serializable]
		public enum SurfaceImpact
		{
			Penetrating, // can damage more layers (assuming yarn hasnt disallowed it, see its surface type)
			SurfaceOnly, // only damage outermost layer
		}

		// rigidness optional specification
		private bool MaxDamagePerCollisionShow => !MaxTotalDamageValidation || MaxTotalDamage is { Enabled: true, Value: > 1 } && ShellDurabilityType == ShellDurability.Rigid;
		private bool MaxDamagePerCollisionValidation => !MaxDamagePerCollisionShow || !MaxDamagePerCollision.Enabled ||
		                                                (MaxDamagePerCollision.Value > 0 && (!MaxTotalDamage.Enabled || MaxDamagePerCollision.Value < MaxTotalDamage.Value));
		[field: ShowIf(nameof(MaxDamagePerCollisionShow))]
		[field: ValidateInput(nameof(MaxDamagePerCollisionValidation), "Piercing Damage Per Hit must be bigger than zero and less than Max Total Damage.")]
		[field: SerializeField] public Optional<int> MaxDamagePerCollision { get; private set; } = new Optional<int>(2);

		// optional below projectile
		private bool BelowProjectileShow => ShellDurabilityType == ShellDurability.Rigid;
		private bool BelowProjectileValidation => !BelowProjectileShow || BelowProjectile.Enabled == false || BelowProjectile.Value != null;
		[field: Header("Nested Projectile")]
		[field: ShowIf(nameof(BelowProjectileShow))]
		[field: ValidateInput(nameof(BelowProjectileValidation), "Below Projectile cannot be null.")]
		[field: SerializeField] public Optional<Projectile> BelowProjectile { get; private set; }

		private bool BelowProjectileStackShow => BelowProjectileShow && BelowProjectile.Enabled && BelowProjectileValidation;
		[field: ShowIf(nameof(BelowProjectileStackShow))]
		[field: SerializeField] public DamageStack BelowDamageStack { get; private set; }
		[Serializable]
		public enum DamageStack
		{
			[UsedImplicitly] // selectable by editor
			OnlyOutermost,
			CanDamageWithBoth,
		}

		private bool ValidAOE => !impactAreaOfEffect.Enabled || impactAreaOfEffect.Value.EffectBasePrefab != null;
		[field: ValidateInput(nameof(ValidAOE), "If you want this enabled, give it a prefab!")]
		[field: SerializeField] public Optional<AreaOfEffect> impactAreaOfEffect; // todo: implement in CommonProjectileScript

		[Serializable, InlineProperty]
		public class AreaOfEffect
		{
			[field: SerializeField] public float Radius { get; private set; } = 1.25f;
			[field: SerializeField] public Optional<int> MaxTotalDamage { get; private set; } = new Optional<int>(3, true);
			[field: SerializeField] public Optional<int> MaxDamagePerCollider { get; private set; } = new Optional<int>(2);
			[field: SerializeField] public SurfaceImpact ImpactType { get; private set; } = SurfaceImpact.Penetrating;
			[field: SerializeField] public Trigger TriggerType {get; private set; } = Trigger.LastImpact;
			public enum Trigger
			{
				FirstImpact, // first impact for this projectile layer
				AllImpacts, // any impact
				LastImpact, // last impact for this projectile layer
			}
			[field: SerializeField, Required, AssetsOnly] public BasicEffectScript EffectBasePrefab { get; private set; }
			[field: SerializeField] public Effect Effect { get; private set; } = Effect.LinearFade();
		}

		// visuals
		[field: Header("Visuals")]
		[field: SerializeField, Required] public Sprite Sprite { get; private set; }
		[field: SerializeField] public Color Color { get; private set; } = Color.white;

		// death visuals
		private bool DeathEffectNullValidation => !DeathEffect.Enabled || DeathEffect.Value.Sprite != null;
		[field: Header("Death Visuals")]
		[field: ValidateInput(nameof(DeathEffectNullValidation), "If enabled, Death Effect should have a sprite!")]
		[field: SerializeField] public Optional<Effect> DeathEffect { get; private set; } = Effect.LinearFade().AsDisabled();
	}
}