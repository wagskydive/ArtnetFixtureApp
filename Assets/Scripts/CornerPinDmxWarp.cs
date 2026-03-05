using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class CornerPinDmxWarp : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] [Range(0.01f, 10f)] private float maxOffset = 0.5f;

    private Mesh _runtimeMesh;
    private readonly Vector3[] _expandedVertices = new Vector3[4];
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

            float xLerp = dmx[xChannel - 1] / 255f;
            float yLerp = dmx[yChannel - 1] / 255f;

            Vector3 expandedVertex = _expandedVertices[corner];
            _warpedVertices[corner] = new Vector3(
                Mathf.Lerp(0f, expandedVertex.x, xLerp),
                Mathf.Lerp(0f, expandedVertex.y, yLerp),
                expandedVertex.z);
        }

        ApplyVertices();
    }

    private void CacheBaseVertices(Vector3[] meshVertices)
    {
        for (int i = 0; i < QuadCornerVertexIndices.Length; i++)
        {
            int vertexIndex = QuadCornerVertexIndices[i];
            Vector3 baseVertex = meshVertices[vertexIndex];
            _expandedVertices[i] = new Vector3(
                baseVertex.x + Mathf.Sign(baseVertex.x) * maxOffset,
                baseVertex.y + Mathf.Sign(baseVertex.y) * maxOffset,
                baseVertex.z);
            _warpedVertices[i] = baseVertex;
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
