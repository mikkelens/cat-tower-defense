using Sirenix.OdinInspector;
using Tools.Types;
using UnityEngine;

namespace Scripts.Gameplay
{
	public class ProjectileManager : Singleton<ProjectileManager>
	{
		[SerializeField, Required, SceneObjectsOnly]
		public Transform projectileParent;
	}
}