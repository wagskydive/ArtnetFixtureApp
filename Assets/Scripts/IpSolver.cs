using System.Net;
using System.Net.Sockets;

public static class IpSolver
{
    public static string ResolveLocalIpv4Address()
    {
        try
        {
            string host = Dns.GetHostName();
            IPAddress[] addresses = Dns.GetHostAddresses(host);
            for (int i = 0; i < addresses.Length; i++)
            {
                IPAddress candidate = addresses[i];
                if (candidate.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(candidate))
                {
                    return candidate.ToString();
                }
            }
        }
        catch (System.Exception)
        {
            // Ignore network lookup issues and fallback to localhost.
        }

        return "127.0.0.1";
    }
}
