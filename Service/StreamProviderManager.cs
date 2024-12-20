using System.Diagnostics;

namespace KIBAEMON2024_CSharp.Service;

public class StreamProviderManager
{
    protected List<IStreamProvider> Providers { get; } = [new YoutubeStreamProvider(), new SoundCloudStreamProvider()];

    public IStreamProvider GetProvider(string url)
    {
        return Providers.FirstOrDefault(p => p.IsMine(url)) ?? throw new Exception("지원하지 않는 URL입니다.");
    }
}

public class SoundCloudStreamProvider : IStreamProvider
{
    public bool IsMine(string url)
    {
        return url.Contains("soundcloud.com");
    }

    public Process StartStream(string url)
    {
        throw new NotImplementedException();
    }

    public Task WaitForStreamAsync()
    {
        throw new NotImplementedException();
    }

    public Task<string> GetPreviewUrl(string url)
    {
        throw new NotImplementedException();
    }

    public Task<VideoInfo> GetInfo(string url)
    {
        throw new NotImplementedException();
    }
}