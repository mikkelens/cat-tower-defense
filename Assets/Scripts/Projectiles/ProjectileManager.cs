using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.Projectiles
{
	public class ProjectileManager : Singleton<ProjectileManager>
	{
		[SerializeField, Required, SceneObjectsOnly]
		public Transform projectileParent;
	}
}