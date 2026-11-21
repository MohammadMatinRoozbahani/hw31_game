using UnityEngine;

[CreateAssetMenu(menuName = "Starberry Patrol/Weapon")]
public class WeaponDataSO : ScriptableObject
{
    public string weaponName;
    public Sprite icon;
    public Pooled bullet;
    [Min(1)] public int damage = 2;
    [Min(0.1f)] public float speed = 17;
    [Min(0.05f)] public float delay = 0.18f;
    public Vector2[] barrels;
    public float[] angles;
}
