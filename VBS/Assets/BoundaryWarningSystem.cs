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
    [Tooltip("拖入创建的WarningIndicator预制体（可选）")]
    public GameObject warningPrefab; // 保留但设为可选

    [Header("参数设置")]
    [Range(0.2f, 2f)]
    public float warningDistance = 0.42f;
    [Tooltip("腰部相对于头部的垂直偏移")]
    public float waistOffset = 0.8f;
    [Tooltip("脚部相对于腰部的垂直偏移")]
    public float feetOffset = 0.5f;
    [Tooltip("裁剪网格的大小（米）")]
    [Range(0.2f, 2f)]
    public float clipMeshSize = 3f; // 新增：网格大小，默认为 1m×1m

    [Header("犯规检测设置")]
    [Tooltip("腰部检测球体的半径（米）")]
    public float waistDetectionRadius = 0.12f;

    private GameObject currentWarning;
    private Collider[] boundaryColliders;
    private Material warningMaterial;
    private int foulCount = 0;
    private bool isColliding = false;

    void Start()
    {
        if (!ValidateComponents())
        {
            enabled = false;
            return;
        }

        // 创建红色半透明材质，使用 Unlit/Transparent 以确保 VR 兼容
        warningMaterial = new Material(Shader.Find("Unlit/Transparent"));
        warningMaterial.color = new Color(1f, 0f, 0f, 0.5f); // 红色，半透明
        warningMaterial.SetFloat("_Cull", (float)UnityEngine.Rendering.CullMode.Off); // 双面渲染

        Physics.queriesHitBackfaces = true;
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

        // warningPrefab 是可选的
        if (warningPrefab == null)
        {
            Debug.LogWarning("警告预制体未绑定！将仅显示动态生成的网格警告。");
        }

        return isValid;
    }

    void Update()
    {
        if (boundaryColliders == null || boundaryColliders.Length == 0) return;

        UpdateReferencePositions();
        Vector3 dangerPoint;
        Collider closestCollider;
        float minDistance = FindClosestDangerPoint(out dangerPoint, out closestCollider);
        //UpdateWarningVisuals(dangerPoint, minDistance, closestCollider);
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

    float FindClosestDangerPoint(out Vector3 closestPoint, out Collider closestCollider)
    {
        closestPoint = Vector3.zero;
        closestCollider = null;
        float minDistance = Mathf.Infinity;

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
                    closestCollider = col;
                }
            }
        }

        // 将加权距离转换回实际距离
        return minDistance * priorityWeights[0]; // 以脚部权重为基准
    }

    void UpdateWarningVisuals(Vector3 warnPosition, float distance, Collider closestCollider)
    {
        if (distance < warningDistance && closestCollider != null)
        {
            // 创建或更新裁剪网格
            if (currentWarning == null)
            {
                currentWarning = new GameObject("ClippedWarningMesh");
                currentWarning.layer = LayerMask.NameToLayer("Default"); // 设置 Layer
                MeshFilter meshFilter = currentWarning.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = currentWarning.AddComponent<MeshRenderer>();
                meshRenderer.material = warningMaterial;
                meshFilter.mesh = CreateClippedMesh();
            }
            currentWarning.SetActive(true); // 显示网格

            // 确保幕布不会太靠近玩家
            Vector3 toPlayer = head.position - warnPosition;
            float distanceToPlayer = toPlayer.magnitude;
            //if (distanceToPlayer < 0.5f)
            //{
                //warnPosition = head.position - toPlayer.normalized * 0.5f; // 推远到 0.5 米
            //}

            // 放置网格在碰撞点
            currentWarning.transform.position = warnPosition;
            currentWarning.transform.localScale = Vector3.one; // 确保 1:1 缩放

            // 计算朝向玩家的水平方向
            toPlayer.y = 0; // 限制在水平面
            toPlayer.Normalize();

            // 如果方向有效，设置网格垂直于地面，面向玩家
            if (toPlayer.magnitude > 0.01f)
            {
                currentWarning.transform.rotation = Quaternion.LookRotation(toPlayer, Vector3.up);
            }
            else
            {
                currentWarning.transform.rotation = Quaternion.identity;
            }
        }
        else
        {
            if (currentWarning != null)
            {
                currentWarning.SetActive(false); // 隐藏网格
            }
        }
    }

    Mesh CreateClippedMesh()
    {
        Mesh mesh = new Mesh();

        // 创建一个垂直于地面的矩形网格（大小由 clipMeshSize 控制）
        float size = clipMeshSize / 2f;
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-size, -size, 0),
            new Vector3(size, -size, 0),
            new Vector3(size, size, 0),
            new Vector3(-size, size, 0)
        };

        int[] triangles = new int[]
        {
            0, 1, 2,
            2, 3, 0
        };

        Vector2[] uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds(); // 确保边界框正确

        return mesh;
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
            Debug.Log($"fault, total brenches is：{foulCount}");
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
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(waist.position, waistDetectionRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(waist.position, 0.1f);
        }
        if (feet != null) Gizmos.DrawSphere(feet.position, 0.1f);
    }

    void OnDestroy()
    {
        if (warningMaterial != null)
        {
            Destroy(warningMaterial);
        }
    }
}