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

    public static byte[] ResolveLocalIpv4AddressBytes()
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
                    return candidate.GetAddressBytes();
                }
            }
        }
        catch (System.Exception)
        {
            // Ignore network lookup issues and fallback to localhost.
        }
        byte[] local = new byte[4];
        local[0] = 127;
        local[1] = 0;
        local[2] = 0;
        local[3] = 1;
        return local;
         
    }
}
