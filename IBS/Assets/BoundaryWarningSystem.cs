using UnityEngine;

public class BoundaryWarningSystem : MonoBehaviour
{
    [Header("必填参数")]
    [Tooltip("将Main Camera拖入此处")]
    public Transform head;
    [Tooltip("在XR Origin下创建的WaistTracker空物体")]
    public Transform waist;
    [Tooltip("在XR Origin下创建的FeetTracker空物体")]
    public Transform feet;
    [Tooltip("拖入创建的WarningIndicator预制体")]
    public GameObject warningPrefab;
    [Tooltip("拖入BoundaryGenerator组件")]
    public BoundaryGenerator boundaryGenerator;
    [Tooltip("拖入AudioRoom（含MetaXRAudioRoomAcoustics）")]
    public MetaXRAudioRoomAcousticProperties roomAcoustics;

    [Header("参数设置")]
    [Range(0.1f, 2f)]
    public float warningDistance = 0.42f;
    [Tooltip("腰部相对于头部的垂直偏移")]
    public float waistOffset = 0.8f;
    [Tooltip("脚部相对于腰部的垂直偏移")]
    public float feetOffset = 0.5f;
    [Tooltip("警告音效的最大音量")]
    [Range(0f, 1f)]
    public float maxVolume = 1f;

    [Header("犯规检测设置")]
    [Tooltip("腰部检测球体的半径（米）")]
    public float waistDetectionRadius = 0.05f;

    private GameObject currentWarning;
    private Collider[] boundaryColliders;
    private int foulCount = 0;
    private bool isColliding = false;
    private MetaXRAudioSource warningAudioSource;

    void Start()
    {
        if (!ValidateComponents())
        {
            enabled = false;
            return;
        }

        // 等待BoundaryGenerator设置碰撞体
        // boundaryColliders 将通过SetBoundaryColliders设置
        // 设置房间声学
        SetupRoomAcoustics();
    }

    public void SetBoundaryColliders(Collider[] colliders)
    {
        boundaryColliders = colliders;
        if (boundaryColliders == null || boundaryColliders.Length == 0)
        {
            Debug.LogError("未找到边界碰撞体！请确保BoundaryGenerator已正确生成墙体。");
            enabled = false;
            return;
        }

        foreach (Collider col in boundaryColliders)
        {
            Renderer renderer = col.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }
        }
    }

    bool ValidateComponents()
    {
        bool isValid = true;

        if (head == null)
        {
            Debug.LogError("头部参考点未绑定！请拖入Main Camera");
            isValid = false;
        }

        if (waist == null)
        {
            Debug.LogError("腰部参考点未绑定！请拖入WaistTracker");
            isValid = false;
        }

        if (feet == null)
        {
            Debug.LogError("脚部参考点未绑定！请拖入FeetTracker");
            isValid = false;
        }

        if (warningPrefab == null)
        {
            Debug.LogError("警告预制体未绑定！请拖入WarningIndicator预制体");
            isValid = false;
        }

        return isValid;
    }

    void SetupRoomAcoustics()
    {
        if (roomAcoustics == null || boundaryGenerator == null) return;

        // 获取 PlayArea 尺寸
        var (boundaryPoints, dimensions) = boundaryGenerator.GetBoundaryData();
        if (boundaryPoints == null || boundaryPoints.Length < 4)
        {
            Debug.LogError("无法获取有效的 PlayArea 边界点，使用默认尺寸 6x2x6 米");
            dimensions = new Vector3(6f, 2f, 6f);
        }
        else
        {
            dimensions.y = 2f; // 墙体高度
            Debug.Log($"设置房间声学尺寸: {dimensions}");
        }

        // 配置房间声学
        roomAcoustics.width = dimensions.x;
        roomAcoustics.height = dimensions.y;
        roomAcoustics.depth = dimensions.z;
    }

    void Update()
    {
        if (boundaryColliders == null || boundaryColliders.Length == 0) return;

        UpdateReferencePositions();
        Vector3 dangerPoint;
        float minDistance = FindClosestDangerPoint(out dangerPoint);
        UpdateWarningVisuals(dangerPoint, minDistance);
        CheckWaistCollision();
    }

    void UpdateReferencePositions()
    {
        if (head != null && waist != null && feet != null)
        {
            waist.position = head.position + Vector3.down * waistOffset;
            feet.position = waist.position + Vector3.down * feetOffset;
        }
    }

    float FindClosestDangerPoint(out Vector3 closestPoint)
    {
        closestPoint = Vector3.zero;
        float minDistance = Mathf.Infinity;

        // 优先级：脚部 > 腰部 > 头部
        Vector3[] checkPoints = new Vector3[]
        {
            feet.position,   // 优先级最高
            waist.position,
            head.position
        };
        float[] priorityWeights = new float[] { 1f, 0.8f, 0.6f }; // 优先级权重

        for (int i = 0; i < checkPoints.Length; i++)
        {
            Vector3 point = checkPoints[i];
            foreach (Collider col in boundaryColliders)
            {
                Vector3 surfacePoint = col.ClosestPoint(point);
                float currentDistance = Vector3.Distance(point, surfacePoint);

                // 调整距离以反映优先级
                float weightedDistance = currentDistance / priorityWeights[i];

                if (weightedDistance < minDistance)
                {
                    minDistance = weightedDistance;
                    closestPoint = surfacePoint;
                }
            }
        }

        // 将加权距离转换回实际距离
        return minDistance * priorityWeights[0]; // 以脚部权重为基准
    }

    void UpdateWarningVisuals(Vector3 warnPosition, float distance)
    {
        if (distance < warningDistance)
        {
            if (currentWarning == null)
            {
                currentWarning = Instantiate(warningPrefab);
                AudioSource audioSource = currentWarning.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Play();
                }
                else
                {
                    Debug.LogWarning("WarningIndicator 预制体缺少 AudioSource 组件，无法播放空间化音频。");
                }
            }
            currentWarning.transform.position = warnPosition;

            AudioSource audio = currentWarning.GetComponent<AudioSource>();
            if (audio != null)
            {
                float volume = maxVolume * (1f - (distance / warningDistance));
                audio.volume = Mathf.Clamp(volume, 0f, maxVolume);
            }
        }
        else
        {
            if (currentWarning != null)
            {
                AudioSource audioSource = currentWarning.GetComponent<AudioSource>();
                if (audioSource != null)
                {
                    audioSource.Stop();
                }
                Destroy(currentWarning);
                currentWarning = null;
            }
        }
    }

    void CheckWaistCollision()
    {
        if (waist == null || boundaryColliders == null) return;

        Collider[] hits = Physics.OverlapSphere(waist.position, waistDetectionRadius);
        bool foundBoundary = false;

        foreach (Collider hit in hits)
        {
            if (hit.CompareTag("Boundary"))
            {
                foundBoundary = true;
                break;
            }
        }

        if (foundBoundary && !isColliding)
        {
            foulCount++;
            isColliding = true;
            Debug.Log($"fault！current brenches time：{foulCount}");
        }
        else if (!foundBoundary && isColliding)
        {
            isColliding = false;
        }
    }

    public int GetFoulCount()
    {
        return foulCount;
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        if (head != null) Gizmos.DrawSphere(head.position, 0.1f);
        if (waist != null)
        {
            Gizmos.DrawSphere(waist.position, 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(waist.position, waistDetectionRadius);
        }
        if (feet != null) Gizmos.DrawSphere(feet.position, 0.1f);
    }
}



