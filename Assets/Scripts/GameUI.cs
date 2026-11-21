using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [SerializeField] Stage stage;
    [SerializeField] Text round;
    [SerializeField] Text theme;
    [SerializeField] Text score;
    [SerializeField] Text missiles;
    [SerializeField] Text power;
    [SerializeField] Image weapon;
    [SerializeField] Image[] lives;
    [SerializeField] Text star;
    [SerializeField] Image starCard;
    [SerializeField] GameObject target;
    [SerializeField] Text targetText;
    [SerializeField] GameObject banner;
    [SerializeField] Text bannerText;

    GameManager game;

    void OnEnable()
    {
        game = GameManager.I;
        game.Changed += Stats;
        stage.Changed += ShowStage;
    }

    void OnDisable()
    {
        if (game != null)
            game.Changed -= Stats;
        stage.Changed -= ShowStage;
    }

    void ShowStage()
    {
        LevelDataSO data = stage.Level;
        theme.text = data.theme;
        round.text = $"Level {data.index} / Round {stage.Round} of {data.waves.Length}";
        banner.SetActive(!string.IsNullOrEmpty(stage.Message));
        bannerText.text = stage.Message;

        Boss boss = stage.Target;
        target.SetActive(boss != null);
        if (boss != null)
            targetText.text = $"{data.boss.gameObject.name}   {boss.Hp}/{boss.MaxHp}";

        Stats();
    }

    void Stats()
    {
        score.text = game.Score.ToString("D6");
        missiles.text = game.Ammo.ToString();
        power.text = game.Power.ToString();
        weapon.sprite = stage.Weapon.icon;
        for (int i = 0; i < lives.Length; i++)
            lives[i].color = i < game.Lives ? Color.white : new Color(0.8f, 0.68f, 0.77f, 0.35f);

        if (game.Eggs == GameManager.EggGoal)
        {
            star.text = "STAR VOLLEY READY - C";
            starCard.color = new Color32(255, 218, 115, 255);
        }
        else
        {
            star.text = $"STAR VOLLEY   {game.Eggs}/{GameManager.EggGoal}";
            starCard.color = new Color32(255, 243, 211, 255);
        }
    }
}
