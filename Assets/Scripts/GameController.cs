using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    //Static Variable
    static GameController gameController;
    static int nb_controllers = 0;

    //Non-static attributes
    [SerializeField] Wallet wallet;
    [SerializeField] PlayerHealthController health;
    [SerializeField] MapV2 map;
    [SerializeField] WaveController waveController;

    public static void StartGame(int seed)
    {
        gameController.wallet.ResetWallet();


        //gameController.wallet.AddMoney(10000);

        gameController.health.ResetHealth();

        gameController.waveController.ResetAll();

        TowerController[] towers = FindObjectsOfType<TowerController>();
        for (int i = 0; i < towers.Length; i++)
        {
            Destroy(towers[i].gameObject);
        }

        gameController.map.m_mapSeed = seed;
        gameController.map.GenerateMapFromSeed(seed);
    }

    // called 
    public static void EndGame(bool showScreen = true)
    {
        gameController.waveController.ResetAll();

        TowerController[] towers = FindObjectsOfType<TowerController>();
        for (int i = 0; i < towers.Length; i++)
        {
            Destroy(towers[i].gameObject);
        }

        //TODO: Game Over UI
        if (showScreen) { 
            GameOverController goc = FindObjectOfType<GameOverController>();
            goc.GameOver();
        }
    }

    public static void EndReached(int dmg)
    {
        bool gameover = gameController.health.SubtractHealth(dmg);
        if (gameover)
            EndGame();
    }

    //
    public static void EnemyKilled(int reward)
    {
        gameController.wallet.EnemyKillReward(reward);
    }

    public static bool Purchase(int cost)
    {
        bool purchasable = gameController.wallet.CheckMoney(cost);
        if (!purchasable) return false;
        gameController.wallet.SpendMoney(cost);
        return true;
    }

    public static void WaveCompleted(int reward)
    {
        gameController.wallet.WaveCompletionReward(reward);
    }

    public static void SellTowerGetMoney(int amount)
    {
        gameController.wallet.AddMoney(amount);
    }

    void Start()
    {
        if (nb_controllers > 0 && gameController != null)
        {
            Destroy(gameObject);
            return;
        }
        //DontDestroyOnLoad(gameObject);
        gameController = GetComponent<GameController>();
        nb_controllers += 1;

        if (wallet == null)
            wallet = FindObjectOfType<Wallet>();
        if (health == null)
            health = FindObjectOfType<PlayerHealthController>();
        if (map == null)
            map = FindObjectOfType<MapV2>();

        //GET SEED
        MainMenuManagement mmm = FindObjectOfType<MainMenuManagement>();
        if (mmm != null)
        {
            StartGame(mmm.seed);
        }
        else
        {
            StartGame(15243);
        }
    }

    public void RestartApplication()
    {
        EndGame(false);
        SceneManager.LoadScene("MainMenuV2");
    }

    public void QuitApplication()
    {
        Application.Quit();
    }
}
