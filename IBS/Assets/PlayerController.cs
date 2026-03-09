using UnityEngine;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public BalloonManager balloonManager;
    public Transform feetPosition;
    public Transform mainCamera;         // 主相机（玩家头部位置）
    public float detectionRadius = 0.5f; // 初始值，仅用于调试
    public float minDetectionRadius = 0.1f; // 最小检测半径
    public TextMeshProUGUI scoreText;

    void Update()
    {
        // 动态计算 detectionRadius
        if (mainCamera != null)
        {
            detectionRadius = Mathf.Max(minDetectionRadius, mainCamera.position.y - 0.2f);
            detectionRadius = 0.3f;
            //Debug.Log("动态检测半径: " + detectionRadius);
            //Debug.Log("当前高度: " + mainCamera.position.y);
        }
        else
        {
            Debug.LogWarning("Main Camera 未分配，使用默认 detectionRadius");
        }

        CheckBalloonCollision();
        UpdateScoreUI();
    }

    void CheckBalloonCollision()
    {
        if (feetPosition == null || balloonManager == null) return;

        Collider[] hitColliders = Physics.OverlapSphere(feetPosition.position, detectionRadius);
        foreach (Collider hit in hitColliders)
        {
            if (hit.CompareTag("Balloon"))
            {
                //Debug.Log("检测到气球: " + hit.gameObject.name);
                balloonManager.RemoveBalloon(hit.gameObject);
            }
        }
    }

    void UpdateScoreUI()
    {
        if (scoreText != null && balloonManager != null)
        {
            scoreText.text = "Score: " + balloonManager.GetCurrentScore();
        }
    }

    void OnDrawGizmos()
    {
        if (feetPosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(feetPosition.position, detectionRadius);
        }
    }
}