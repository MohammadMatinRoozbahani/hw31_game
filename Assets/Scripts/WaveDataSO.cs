using UnityEngine;

[CreateAssetMenu(menuName = "Starberry Patrol/Wave")]
public class WaveDataSO : ScriptableObject
{
    public enum Kind { Chickens, Asteroids }

    public Kind kind;
    public Chicken chicken;
    public Pooled rock;
    [Min(1)] public int count = 6;
    [Min(0.1f)] public float speed = 4;
    [Min(0.1f)] public float eggDelay = 5;
    [Min(0.1f)] public float eggSpeed = 4;
    [Min(0)] public float gap = 0.15f;
    [Min(0)] public int diveEvery;
    [Min(1)] public int rockHp = 4;
    public Vector2[] slots;
    public Vector2[] path;
}
