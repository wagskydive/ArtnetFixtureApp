using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CornerPinDmxWarp : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField][Range(0.01f, 10f)] private float maxOffset = 0.5f;
    [SerializeField][Range(1, 64)] private int subdivisionAmount = 8;
    [SerializeField] private DmxModeManager dmxModeManager;

    private Mesh _runtimeMesh;
    private Vector3[] _expandedCorners = new Vector3[4];
    private Vector3[] _warpedCorners = new Vector3[4];
    private Vector3[] _meshVertices;
    private float _minX;
    private float _maxX;
    private float _minY;
    private float _maxY;

    private int _gridResolution;

    bool isInMode;

    private void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            return;
        }

        _gridResolution = Mathf.Max(1, subdivisionAmount);
        _runtimeMesh = CreateSubdividedQuad(_gridResolution);
        _runtimeMesh.MarkDynamic();
        meshFilter.mesh = _runtimeMesh;

        _meshVertices = _runtimeMesh.vertices;
        CacheExpandedCorners();
        for (int i = 0; i < _warpedCorners.Length; i++)
        {
            _warpedCorners[i] = new Vector3(_minX, _minY, 0f);
        }

        ApplyWarpedGrid(updateMesh: false);
    }

    private void Start()
    {
        DmxModeManager.OnModeChanged += HandleModeChange;
        isInMode = DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.PixelMapping || DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.Standard;
    }

    void HandleModeChange(DmxModeManager.FixtureMode mode)
    {
        isInMode = DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.PixelMapping || DmxModeManager.Instance.CurrentMode == DmxModeManager.FixtureMode.Standard;

    }

    bool fullScreen;

    void SetFullScreen()
    {

        fullScreen = true;
    }

    private void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || _runtimeMesh == null)
        {
            return;
        }
        if (!isInMode)
        {
            if (!fullScreen)
            {

                _warpedCorners[0] = new Vector3(_minX, _minY, 0);
                _warpedCorners[1] = new Vector3(_minX, _maxY, 0);
                _warpedCorners[2] = new Vector3(_maxX, _maxY, 0);
                _warpedCorners[3] = new Vector3(_maxX, _minY, 0);


                SetFullScreen();
                ApplyWarpedGrid();
            }
            return;
        }
        else
        {
            if (fullScreen)
            {
                fullScreen = false;
            }
        }

        byte[] dmx = artNetReceiver.DmxBuffer.GetRawBuffer();

        int cornerStartChannel = ResolveCornerPinStartChannel();

        for (int corner = 0; corner < 4; corner++)
        {
            int xChannel = Mathf.Clamp(artNetReceiver.StartChannel + cornerStartChannel - 1 + (corner * 2), 1, 512);
            int yChannel = Mathf.Clamp(xChannel + 1, 1, 512);

            float xLerp = dmx[xChannel - 1] / 255f;
            float yLerp = dmx[yChannel - 1] / 255f;

            _warpedCorners[corner] = new Vector3(
                Mathf.Lerp(_minX, _maxX, xLerp),
                Mathf.Lerp(_minY, _maxY, yLerp),
                0f);
        }

        ApplyWarpedGrid();
    }


    private int ResolveCornerPinStartChannel()
    {
        if (dmxModeManager != null && dmxModeManager.CurrentMode == DmxModeManager.FixtureMode.PixelMapping)
        {
            return PixelMappingDmxPersonality.CornerPinStartChannel;
        }

        return SurfaceProjectionDmxPersonality.CornerPinStartChannel;
    }

    private void CacheExpandedCorners()
    {
        _minX = -0.5f - maxOffset;
        _maxX = 0.5f + maxOffset;
        _minY = -0.5f - maxOffset;
        _maxY = 0.5f + maxOffset;

        _expandedCorners[0] = new Vector3(_minX, _minY, 0f); // BL
        _expandedCorners[1] = new Vector3(_minX, _maxY, 0f); // TL
        _expandedCorners[2] = new Vector3(_maxX, _maxY, 0f); // TR
        _expandedCorners[3] = new Vector3(_maxX, _minY, 0f); // BR
    }

    private void ApplyWarpedGrid()
    {
        ApplyWarpedGrid(updateMesh: true);
    }

    private void ApplyWarpedGrid(bool updateMesh)
    {
        int rowLength = _gridResolution + 1;

        for (int y = 0; y <= _gridResolution; y++)
        {
            float v = y / (float)_gridResolution;
            for (int x = 0; x <= _gridResolution; x++)
            {
                float u = x / (float)_gridResolution;
                int index = y * rowLength + x;

                Vector3 bottom = Vector3.Lerp(_warpedCorners[0], _warpedCorners[3], u);
                Vector3 top = Vector3.Lerp(_warpedCorners[1], _warpedCorners[2], u);
                _meshVertices[index] = Vector3.Lerp(bottom, top, v);
            }
        }

        if (!updateMesh)
        {
            return;
        }

        _runtimeMesh.vertices = _meshVertices;
        _runtimeMesh.RecalculateBounds();
    }

    private static Mesh CreateSubdividedQuad(int subdivisions)
    {
        int rowLength = subdivisions + 1;
        int vertexCount = rowLength * rowLength;
        int quadCount = subdivisions * subdivisions;

        var vertices = new Vector3[vertexCount];
        var uvs = new Vector2[vertexCount];
        var triangles = new int[quadCount * 6];

        int vertexIndex = 0;
        for (int y = 0; y <= subdivisions; y++)
        {
            float v = y / (float)subdivisions;
            float posY = Mathf.Lerp(-0.5f, 0.5f, v);
            for (int x = 0; x <= subdivisions; x++)
            {
                float u = x / (float)subdivisions;
                float posX = Mathf.Lerp(-0.5f, 0.5f, u);

                vertices[vertexIndex] = new Vector3(posX, posY, 0f);
                uvs[vertexIndex] = new Vector2(u, v);
                vertexIndex++;
            }
        }

        int triIndex = 0;
        for (int y = 0; y < subdivisions; y++)
        {
            for (int x = 0; x < subdivisions; x++)
            {
                int bottomLeft = y * rowLength + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + rowLength;
                int topRight = topLeft + 1;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topLeft;
                triangles[triIndex++] = topRight;

                triangles[triIndex++] = bottomLeft;
                triangles[triIndex++] = topRight;
                triangles[triIndex++] = bottomRight;
            }
        }

        var mesh = new Mesh
        {
            name = "CornerPinSubdividedQuad"
        };

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}
