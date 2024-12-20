using System.Diagnostics;

namespace KIBAEMON2024_CSharp.Service;

public record VideoInfo(string Title, string Author, string AuthorUrl, long Duration, string Url);

public interface IStreamProvider
{
    bool IsMine(string url);

    /// <summary>
    /// 입력 URL에 대해 오디오 스트림을 제공하는 프로세스를 시작하고,
    /// StandardOutput (원시 오디오 스트림)을 반환한다.
    /// </summary>
    /// <param name="url">음원 URL 또는 플랫폼 별 식별자</param>
    /// <returns>해당 URL로부터 오디오 스트림을 제공하는 Process</returns>
    Process StartStream(string url);

    Task WaitForStreamAsync();

    Task<string> GetPreviewUrl(string url);

    Task<VideoInfo> GetInfo(string url);
}