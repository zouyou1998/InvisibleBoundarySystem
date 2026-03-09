using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class BalloonManager : MonoBehaviour
{
    public GameObject balloonPrefab;
    public TextMeshProUGUI levelText;       // 显示关卡
    public TextMeshProUGUI timeText;        // 显示时间
    public TextMeshProUGUI messageText;     // 显示倒计时和提示
    private List<GameObject> balloons = new List<GameObject>();
    private float levelTimer = 0f;
    private float totalTimer = 0f;
    private float levelTime = 60f;
    private float totalGameTime = 90f; // 2.5 minutes total game time
    private int currentLevel = 1;
    private int currentScore = 0;
    private bool gameOver = false;
    private bool gameStarted = false;
    private float startCountdown = 3f;
    private bool levelTransition = false;
    private float transitionCountdown = 5f;
    private readonly int[] levelGoals = { 6, 8, 12, 14, 16, 18, 20, 24 };
    private Vector3[][] levelBalloonPositions; // 每个关卡的固定气球位置

    void Start()
    {
        // 初始化每个关卡的固定气球位置
        InitializeLevelPositions();
        UpdateLevelUI();
    }

    void InitializeLevelPositions()
    {
        levelBalloonPositions = new Vector3[8][]; // 8 个关卡
        float minDistance = 0.5f; // 气球间最小间距，避免重叠

        for (int level = 0; level < 8; level++)
        {
            levelBalloonPositions[level] = new Vector3[30]; // 每个关卡 30 个气球
            List<Vector3> positions = new List<Vector3>();
            Random.InitState(level + 1); // 为每个关卡设置不同种子，确保位置固定且不同

            for (int i = 0; i < 30; i++)
            {
                int attempts = 0;
                const int maxAttempts = 30; // 防止无限循环
                bool validPosition = false;
                Vector3 newPos = Vector3.zero;

                // 尝试生成有效位置
                while (!validPosition && attempts < maxAttempts)
                {
                    float x = Random.Range(-3f, 3f);
                    float z = Random.Range(-3f, 3f);
                    newPos = new Vector3(x, 0.7f, z);
                    validPosition = true;

                    // 检查与已有位置的间距
                    foreach (Vector3 pos in positions)
                    {
                        if (Vector3.Distance(newPos, pos) < minDistance)
                        {
                            validPosition = false;
                            break;
                        }
                    }
                    attempts++;
                }

                if (validPosition)
                {
                    positions.Add(newPos);
                }
                else
                {
                    // 回退：使用网格位置
                    int row = i / 6; // 调整为 6 列网格以适应 30 个气球
                    int col = i % 6;
                    float x = -3f + (col + 0.5f) * (6f / 6) + (level - 2) * 0.2f;
                    float z = -3f + (row + 0.5f) * (6f / 5) + (level - 2) * 0.2f;
                    positions.Add(new Vector3(
                        Mathf.Clamp(x, -3f, 3f),
                        0.3f,
                        Mathf.Clamp(z, -3f, 3f)
                    ));
                }
            }

            levelBalloonPositions[level] = positions.ToArray();
        }
    }

    void Update()
    {
        if (gameOver) return;

        if (!gameStarted) // 开始倒计时
        {
            startCountdown -= Time.deltaTime;
            if (messageText != null)
            {
                messageText.text = "Get Ready: " + Mathf.Ceil(startCountdown).ToString();
            }
            if (startCountdown <= 0f)
            {
                gameStarted = true;
                SpawnBalloons();
            }
            return;
        }

        if (levelTransition) // 通关后倒计时
        {
            transitionCountdown -= Time.deltaTime;
            if (messageText != null)
            {
                messageText.text = "Next Level in: " + Mathf.Ceil(transitionCountdown).ToString();
            }
            if (transitionCountdown <= 0f)
            {
                levelTransition = false;
                NextLevel();
            }
            return;
        }

        levelTimer += Time.deltaTime;
        totalTimer += Time.deltaTime;
        UpdateTimeUI();

        if (totalTimer >= totalGameTime)
        {
            FailGame();
            return;
        }

        if (currentScore >= levelGoals[currentLevel - 1])
        {
            PassLevel();
        }
        else if (levelTimer >= levelTime)
        {
            FailLevel();
        }
    }

    void SpawnBalloons()
    {
        // 使用当前关卡的固定位置生成气球
        Vector3[] positions = levelBalloonPositions[currentLevel - 1];
        for (int i = 0; i < positions.Length; i++)
        {
            GameObject balloon = Instantiate(balloonPrefab, positions[i], Quaternion.identity);
            balloons.Add(balloon);
        }
        if (messageText != null) messageText.text = ""; // 清空中部消息
        Debug.Log("关卡 " + currentLevel + " 开始，目标得分: " + levelGoals[currentLevel - 1]);
    }

    void ClearBalloons()
    {
        foreach (GameObject balloon in balloons)
        {
            Destroy(balloon);
        }
        balloons.Clear();
    }

    public void RemoveBalloon(GameObject balloon)
    {
        if (!gameOver && gameStarted && !levelTransition)
        {
            balloons.Remove(balloon);
            Destroy(balloon);
            currentScore++;
        }
    }

    void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
        }
    }

    void UpdateTimeUI()
    {
        if (timeText != null)
        {
            float levelTimeLeft = levelTime - levelTimer;
            float totalTimeLeft = totalGameTime - totalTimer;
            timeText.text = "Level Time: " + Mathf.Ceil(levelTimeLeft).ToString() + "\n" + "Total Time: " + Mathf.Ceil(totalTimeLeft).ToString();
        }
    }

    void PassLevel()
    {
        Debug.Log("Level " + currentLevel + " finished，current score for this level: " + currentScore);
        ClearBalloons();
        if (currentLevel == 8)
        {
            gameOver = true;
            EndGame("You Win! All Levels Cleared!");
        }
        else
        {
            if (messageText != null)
            {
                messageText.text = "Level " + currentLevel + " Passed!";
            }
            levelTransition = true;
            transitionCountdown = 5f;
        }
    }

    void NextLevel()
    {
        currentScore = 0;
        currentLevel++;
        levelTimer = 0f;
        UpdateLevelUI();
        SpawnBalloons();
    }

    void FailLevel()
    {
        gameOver = true;
        EndGame("Failed! Score: " + currentScore + " < Goal: " + levelGoals[currentLevel - 1]);
    }

    void FailGame()
    {
        gameOver = true;
        EndGame("Time's Up! Score: " + currentScore);
    }

    void EndGame(string message)
    {
        if (messageText != null)
        {
            messageText.text = "Game Over\n" + message;
        }
        Debug.Log("Game Over, total ballon hit in this level: " + currentScore);
        Time.timeScale = 0f;
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }
}