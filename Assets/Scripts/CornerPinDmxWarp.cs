using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CornerPinDmxWarp : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] [Range(0.01f, 10f)] private float maxOffset = 0.5f;

    private Mesh _runtimeMesh;
    private readonly Vector3[] _baseVertices = new Vector3[4];
    private readonly Vector3[] _warpedVertices = new Vector3[4];
    private Vector3[] _meshVertices;

    private static readonly int[] QuadCornerVertexIndices = { 0, 2, 3, 1 }; // BL, TL, TR, BR

    private void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.sharedMesh == null)
        {
            return;
        }

        _runtimeMesh = Instantiate(meshFilter.sharedMesh);
        _runtimeMesh.MarkDynamic();
        meshFilter.mesh = _runtimeMesh;

        _meshVertices = _runtimeMesh.vertices;
        CacheBaseVertices(_meshVertices);
    }

    private void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || _runtimeMesh == null)
        {
            return;
        }

        byte[] dmx = artNetReceiver.DmxBuffer.GetRawBuffer();

        for (int corner = 0; corner < 4; corner++)
        {
            int xChannel = Mathf.Clamp(artNetReceiver.StartChannel + 8 + (corner * 2), 1, 512);
            int yChannel = Mathf.Clamp(artNetReceiver.StartChannel + 8 + (corner * 2) + 1, 1, 512);

            float offsetX = DmxToOffset(dmx[xChannel - 1]);
            float offsetY = DmxToOffset(dmx[yChannel - 1]);

            Vector3 baseVertex = _baseVertices[corner];
            _warpedVertices[corner] = new Vector3(baseVertex.x + offsetX, baseVertex.y + offsetY, baseVertex.z);
        }

        ApplyVertices();
    }

    private float DmxToOffset(byte value)
    {
        return ((value / 255f) * 2f - 1f) * maxOffset;
    }

    private void CacheBaseVertices(Vector3[] meshVertices)
    {
        for (int i = 0; i < QuadCornerVertexIndices.Length; i++)
        {
            int vertexIndex = QuadCornerVertexIndices[i];
            _baseVertices[i] = meshVertices[vertexIndex];
            _warpedVertices[i] = meshVertices[vertexIndex];
        }
    }

    private void ApplyVertices()
    {
        for (int i = 0; i < QuadCornerVertexIndices.Length; i++)
        {
            _meshVertices[QuadCornerVertexIndices[i]] = _warpedVertices[i];
        }

        _runtimeMesh.vertices = _meshVertices;
        _runtimeMesh.RecalculateBounds();
    }
}
