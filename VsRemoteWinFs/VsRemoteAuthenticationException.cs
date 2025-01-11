namespace VsRemoteWinFs;

[Serializable]
internal class VsRemoteAuthenticationException(string message) : Exception(message)
{
}