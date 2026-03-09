using UnityEngine;

public class BoundaryGenerator : MonoBehaviour
{
    [Header("必填参数")]
    [Tooltip("墙体预制体")]
    public GameObject wallPrefab;
    [Tooltip("墙体透明材质（留空则使用默认完全透明材质）")]
    public Material transparentWallMaterial;
    [Tooltip("是否禁用墙体Renderer（完全不可见，仅保留碰撞）")]
    public bool disableWallRenderer = false;

    [Header("参数设置")]
    [Tooltip("墙体高度（米）")]
    public float wallHeight = 2f;
    [Tooltip("墙体厚度（米）")]
    public float wallThickness = 0.1f;
    [Tooltip("线条宽度（米）")]
    public float lineWidth = 0.05f;
    [Tooltip("线条材质（留空则使用默认白色材质）")]
    public Material lineMaterial;

    private OVRBoundary boundary;
    private Collider[] boundaryColliders;

    void Start()
    {
        boundary = new OVRBoundary();
        Debug.Log($"Oculus XR Plugin 启用: {UnityEngine.XR.XRSettings.enabled}");
        Debug.Log($"Guardian 配置状态: {boundary.GetConfigured()}");
        if (!boundary.GetConfigured())
        {
            Debug.LogError("Guardian边界未配置！请在Quest 3上设置Roomscale边界。");
            return;
        }
        // 创建默认完全透明材质（如果未分配）
        if (transparentWallMaterial == null)
        {
            transparentWallMaterial = new Material(Shader.Find("Unlit/Transparent"));
            transparentWallMaterial.color = new Color(1f, 1f, 1f, 0f); // 完全透明
        }
        GenerateBoundaryWalls();
        //GenerateBoundaryLine();
    }

    void GenerateBoundaryWalls()
    {
        // 尝试获取PlayArea边界点
        Vector3[] boundaryPoints = boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
        Vector3 dimensions = boundary.GetDimensions(OVRBoundary.BoundaryType.PlayArea);
        if (dimensions.y != 0)
        {
            Debug.LogWarning($"PlayArea 尺寸 y 值异常: {dimensions.y}，强制修正为 0");
            dimensions.y = 0;
        }
        Debug.Log($"PlayArea 尺寸: {dimensions}");
        if (boundaryPoints == null || boundaryPoints.Length == 0)
        {
            Debug.LogError($"无法获取PlayArea边界点！点数: {boundaryPoints?.Length}");
            // 回退到默认6x6米矩形边界
            boundaryPoints = GenerateDefaultBoundaryPoints();
            Debug.LogWarning("使用默认6x6米矩形边界");
        }

        Debug.Log($"获取到 {boundaryPoints.Length} 个边界点");
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            Debug.Log($"边界点 {i}: {boundaryPoints[i]}");
        }

        boundaryColliders = new Collider[boundaryPoints.Length];
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            Vector3 startPoint = boundaryPoints[i];
            Vector3 endPoint = boundaryPoints[(i + 1) % boundaryPoints.Length];

            Vector3 wallCenter = (startPoint + endPoint) / 2f;
            wallCenter.y = wallHeight / 2f;

            float wallLength = Vector3.Distance(startPoint, endPoint);
            Vector3 direction = (endPoint - startPoint).normalized;
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);

            GameObject wall = Instantiate(wallPrefab, wallCenter, rotation);
            wall.tag = "Boundary";

            wall.transform.localScale = new Vector3(wallThickness, wallHeight, wallLength);

            MeshCollider collider = wall.GetComponent<MeshCollider>();
            if (collider == null)
            {
                collider = wall.AddComponent<MeshCollider>();
            }
            boundaryColliders[i] = collider;

            Renderer renderer = wall.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = transparentWallMaterial; // 应用完全透明材质
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; // 禁用阴影投射
                renderer.receiveShadows = false; // 禁用阴影接收
                if (disableWallRenderer)
                {
                    renderer.enabled = false; // 禁用Renderer
                }
            }
        }

        BoundaryWarningSystem warningSystem = FindObjectOfType<BoundaryWarningSystem>();
        if (warningSystem != null)
        {
            warningSystem.SetBoundaryColliders(boundaryColliders);
        }
        else
        {
            Debug.LogWarning("未找到BoundaryWarningSystem组件！");
        }
    }

    void GenerateBoundaryLine()
    {
        // 获取PlayArea边界点
        Vector3[] boundaryPoints = boundary.GetGeometry(OVRBoundary.BoundaryType.PlayArea);
        if (boundaryPoints == null || boundaryPoints.Length == 0)
        {
            Debug.LogWarning("无法获取PlayArea边界点用于线条绘制，使用默认6x6米边界");
            boundaryPoints = GenerateDefaultBoundaryPoints();
        }

        // 创建LineRenderer
        GameObject lineObject = new GameObject("BoundaryLine");
        lineObject.transform.SetParent(transform, false);
        LineRenderer lineRenderer = lineObject.AddComponent<LineRenderer>();

        // 配置LineRenderer
        lineRenderer.positionCount = boundaryPoints.Length + 1; // 闭合线
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.loop = true;
        lineRenderer.useWorldSpace = true;

        // 设置材质
        if (lineMaterial == null)
        {
            lineMaterial = new Material(Shader.Find("Unlit/Color"));
            lineMaterial.color = Color.white;
        }
        lineRenderer.material = lineMaterial;

        // 设置线条点（y=0.01避免Z-fighting）
        for (int i = 0; i < boundaryPoints.Length; i++)
        {
            lineRenderer.SetPosition(i, new Vector3(boundaryPoints[i].x, 0.01f, boundaryPoints[i].z));
        }
        lineRenderer.SetPosition(boundaryPoints.Length, new Vector3(boundaryPoints[0].x, 0.01f, boundaryPoints[0].z));
    }

    // 默认边界点（6x6米矩形）
    private Vector3[] GenerateDefaultBoundaryPoints()
    {
        float size = 6f; // 默认6x6米
        return new Vector3[]
        {
            new Vector3(-size / 2, 0, -size / 2),
            new Vector3(-size / 2, 0, size / 2),
            new Vector3(size / 2, 0, size / 2),
            new Vector3(size / 2, 0, -size / 2)
        };
    }

    public Collider[] GetBoundaryColliders()
    {
        return boundaryColliders;
    }
}