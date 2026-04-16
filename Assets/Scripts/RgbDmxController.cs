using System.Collections;
using UnityEngine;

public class RgbDmxController : BaseDmxMaterialConsumer
{

    protected override bool IsActiveMode()
    {
                return DmxModeManager.Instance != null &&
               DmxModeManager.Instance.CurrentMode != FixtureMode.PixelMapping;
    }


    protected override void OnDmxFrame(DmxFrame frame)
    {
        float dimmer = _fixture.GetChannelValue(frame, 1) / 255f;
        float red = _fixture.GetChannelValue(frame, 2) / 255f;
        float green = _fixture.GetChannelValue(frame, 3) / 255f;
        float blue = _fixture.GetChannelValue(frame, 4) / 255f;

        _material.SetColor("_Color", new Color(red, green, blue, 1f));
        _material.SetFloat("_Intensity", dimmer);
    }
}
