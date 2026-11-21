using UnityEngine;

[CreateAssetMenu(menuName = "Starberry Patrol/Level")]
public class LevelDataSO : ScriptableObject
{
    public int index;
    public string scene;
    public string theme;
    public Sprite backdrop;
    public AudioClip music;
    public WaveDataSO[] waves;
    public Boss boss;
}
