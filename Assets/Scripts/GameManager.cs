using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public class GameManager : MonoBehaviour
{
    public const int EggGoal = 3;
    public static GameManager I { get; private set; }

    [SerializeField] LevelDataSO[] levels;

    public int Level { get; private set; }
    public int Lives { get; private set; }
    public int Score { get; private set; }
    public int Ammo { get; private set; }
    public int Power { get; private set; }
    public int Eggs { get; private set; }
    public event Action Changed;

    void Awake()
    {
        if (I != null && I != this)
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        if (I == this)
            I = null;
    }

    public void StartGame()
    {
        Reset(1);
        Load(levels[0].scene);
    }

    public void Enter(LevelDataSO level)
    {
        if (Level != level.index)
            Reset(level.index);
    }

    public bool HasNext => Level < levels.Length;

    public void Next()
    {
        if (!HasNext)
            return;

        Level++;
        Load(levels[Level - 1].scene);
    }

    public void Retry()
    {
        Reset(Level);
        Load(levels[Level - 1].scene);
    }

    public void Menu()
    {
        Load("MainMenu");
    }

    public void Quit()
    {
        Application.Quit();
    }

    void Reset(int level)
    {
        Level = level;
        Lives = 3;
        Score = 0;
        Ammo = 2;
        Power = 1;
        Eggs = 0;
    }

    void Load(string scene)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(scene);
    }

    public void AddScore(int points)
    {
        Score += points;
        Changed?.Invoke();
    }

    public void Gift()
    {
        Power = Mathf.Min(3, Power + 1);
        Changed?.Invoke();
    }

    public void Hit()
    {
        Lives--;
        Power = Mathf.Max(1, Power - 1);
        Changed?.Invoke();
    }

    public bool UseMissile()
    {
        if (Ammo <= 0)
            return false;

        Ammo--;
        Changed?.Invoke();
        return true;
    }

    public void EggShot()
    {
        if (Eggs >= EggGoal)
            return;

        Eggs++;
        Changed?.Invoke();
    }

    public bool UseStar()
    {
        if (Eggs < EggGoal)
            return false;

        Eggs = 0;
        Changed?.Invoke();
        return true;
    }
}
