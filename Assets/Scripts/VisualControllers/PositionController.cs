using UnityEngine;

public class PositionController : BaseRawDmxConsumer
{


    private Vector3 _startPostiion;

    [SerializeField]
    private float _positionOffestScaling = 10;


    void Start()
    {
        _startPostiion = transform.localPosition;
    }

    void Update()
    {
        if (!IsActiveMode() || _fixture == null)
            return;


        var snapshot = MovingHeadDmxPersonality.Parse(_fixture, DmxDataService.LatestFrame, Time.time);
        ApplyPositionOffset(new Vector2(Mathf.Lerp(-1f, 1f, snapshot.PanNormalized), Mathf.Lerp(-1f, 1f, snapshot.TiltNormalized)));

    }

    private void ApplyPositionOffset(Vector2 offset)
    {
        Vector3 offsetFull = new Vector3(offset.x, offset.y, 0);
        transform.SetLocalPositionAndRotation(_startPostiion + offsetFull * _positionOffestScaling, Quaternion.identity);
    }


    protected override void HandleFrameChange(DmxFrame frame)
    {
        IsActiveMode();
    }

    protected override bool IsActiveMode()
    {
        bool isMovingHead = DmxModeManager.Instance != null && DmxModeManager.Instance.CurrentMode == FixtureMode.MovingHead;
        if (!isMovingHead)
        {
            ApplyPositionOffset(Vector2.zero);
        }
        return isMovingHead;
    }
}
