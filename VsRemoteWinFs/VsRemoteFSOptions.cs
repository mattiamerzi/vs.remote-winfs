using DokanNet.Logging;

namespace VsRemoteWinFs;

public record struct VsRemoteFSOptions
(
    string Uri,
    string? UserName = null,
    string? Password = null,
    ILogger? Logger = null,
    bool UseStatCache = true
);
