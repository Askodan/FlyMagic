using UnityEngine;
using System.Collections;
public interface Shooter{
	void Shoot (GameObject projectile, Vector3 projectileSpawnPoint, float force);
	void Aim (Vector3 target_pos);
}
