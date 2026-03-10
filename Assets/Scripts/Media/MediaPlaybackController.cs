using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

public class MediaPlaybackController : MonoBehaviour
{
    [SerializeField] private ArtNetReceiver artNetReceiver;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private int dmxStartChannel = 9;
    [SerializeField] private string usbMediaDirectory = "/storage/emulated/0/ArtnetFixture/media";
    [SerializeField] [Min(1)] private int maxMediaFileSizeMb = 50;
    [SerializeField] private List<string> mediaFiles = new List<string>();

    private IVideoPlaybackBackend _playbackBackend;
    private int _lastSelectedIndex = -1;
    private TransportCommand _lastTransportCommand = TransportCommand.None;
    private bool? _lastLooping;

    private enum TransportCommand
    {
        None,
        Stop,
        Pause,
        Play,
    }

    public IVideoPlaybackBackend PlaybackBackend
    {
        get => _playbackBackend;
        set => _playbackBackend = value;
    }

    private void Awake()
    {
        if (artNetReceiver == null)
        {
            artNetReceiver = FindFirstObjectByType<ArtNetReceiver>();
        }

        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }

        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        if (_playbackBackend == null && videoPlayer != null)
        {
            _playbackBackend = new UnityVideoPlaybackBackend(videoPlayer);
        }
    }

    private void Update()
    {
        if (artNetReceiver == null || artNetReceiver.DmxBuffer == null || _playbackBackend == null)
        {
            return;
        }

        DmxBuffer dmxBuffer = artNetReceiver.DmxBuffer;

        int selectedIndex = ResolveMediaIndex(dmxBuffer.GetChannel1Based(dmxStartChannel));
        if (selectedIndex != _lastSelectedIndex)
        {
            _lastSelectedIndex = selectedIndex;
            TryLoadSelectedMedia(selectedIndex);
        }

        TransportCommand transportCommand = ResolveTransport(dmxBuffer.GetChannel1Based(dmxStartChannel + 1));
        if (transportCommand != _lastTransportCommand)
        {
            _lastTransportCommand = transportCommand;
            ApplyTransport(transportCommand);
        }

        bool shouldLoop = dmxBuffer.GetChannel1Based(dmxStartChannel + 2) >= 127;
        if (!_lastLooping.HasValue || shouldLoop != _lastLooping.Value)
        {
            _lastLooping = shouldLoop;
            _playbackBackend.SetLooping(shouldLoop);
        }
    }

    public int ResolveMediaIndex(byte dmxValue)
    {
        if (mediaFiles == null || mediaFiles.Count == 0)
        {
            return -1;
        }

        return Mathf.Clamp(dmxValue * mediaFiles.Count / 256, 0, mediaFiles.Count - 1);
    }

    private void TryLoadSelectedMedia(int index)
    {
        if (index < 0 || mediaFiles == null || index >= mediaFiles.Count)
        {
            return;
        }

        string fileName = mediaFiles[index];
        string usbPath = Path.Combine(usbMediaDirectory, fileName);

        if (File.Exists(usbPath))
        {
            if (IsMediaWithinBudget(usbPath))
            {
                _playbackBackend.SetUrl(usbPath);
            }

            return;
        }

        string streamingPath = Path.Combine(Application.streamingAssetsPath, fileName);
        if (File.Exists(streamingPath))
        {
            if (IsMediaWithinBudget(streamingPath))
            {
                _playbackBackend.SetUrl(streamingPath);
            }

            return;
        }

        Debug.LogWarning($"Media file not found in USB or StreamingAssets: {fileName}");
    }

    public bool IsMediaWithinBudget(string mediaPath)
    {
        if (string.IsNullOrWhiteSpace(mediaPath) || !File.Exists(mediaPath))
        {
            return false;
        }

        var fileInfo = new FileInfo(mediaPath);
        long maxBytes = (long)maxMediaFileSizeMb * 1024L * 1024L;
        if (fileInfo.Length <= maxBytes)
        {
            return true;
        }

        Debug.LogWarning($"Media file exceeds configured budget ({maxMediaFileSizeMb} MB): {mediaPath}");
        return false;
    }

    private static TransportCommand ResolveTransport(byte dmxValue)
    {
        if (dmxValue <= 84)
        {
            return TransportCommand.Stop;
        }

        if (dmxValue <= 169)
        {
            return TransportCommand.Pause;
        }

        return TransportCommand.Play;
    }

    private void ApplyTransport(TransportCommand command)
    {
        switch (command)
        {
            case TransportCommand.Stop:
                _playbackBackend.Stop();
                break;
            case TransportCommand.Pause:
                _playbackBackend.Pause();
                break;
            case TransportCommand.Play:
                _playbackBackend.Play();
                break;
        }
    }
}

public interface IVideoPlaybackBackend
{
    void SetUrl(string url);
    void SetLooping(bool shouldLoop);
    void Play();
    void Pause();
    void Stop();
}

public class UnityVideoPlaybackBackend : IVideoPlaybackBackend
{
    private readonly VideoPlayer _videoPlayer;

    public UnityVideoPlaybackBackend(VideoPlayer videoPlayer)
    {
        _videoPlayer = videoPlayer;
        _videoPlayer.playOnAwake = false;
        _videoPlayer.source = VideoSource.Url;
    }

    public void SetUrl(string url)
    {
        _videoPlayer.url = url;
        _videoPlayer.Prepare();
    }

    public void SetLooping(bool shouldLoop)
    {
        _videoPlayer.isLooping = shouldLoop;
    }

    public void Play()
    {
        _videoPlayer.Play();
    }

    public void Pause()
    {
        _videoPlayer.Pause();
    }

    public void Stop()
    {
        _videoPlayer.Stop();
    }
}
