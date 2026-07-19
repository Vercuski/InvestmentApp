using InvestmentApp.Application.Services;
using System.Diagnostics;

namespace InvestmentApp.Infrastructure.Services;

public class VPNService : IVpnService
{
    public void ConnectToVPN(string countryName)
    {
        try
        {
            // The command to connect to a specific country's best server
            string cliArguments = $"-c \"{countryName}\"";

            // Configure the process start information
            ProcessStartInfo startInfo = new()
            {
                FileName = "C:\\Program Files\\NordVPN\\NordVPN.exe",
                Arguments = cliArguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            // Start the process
            using Process? process = Process.Start(startInfo);
            // Optionally read the output
            //string output = process!.StandardOutput.ReadToEnd();
            //process.WaitForExit();
            //Console.WriteLine(output);
            Console.WriteLine($"Attempting to connect to {countryName}...");
            Task.Delay(20000).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public void DisconnectFromVPN()
    {
        try
        {
            // The command to disconnect
            string cliArguments = "-d";

            ProcessStartInfo startInfo = new()
            {
                FileName = "C:\\Program Files\\NordVPN\\NordVPN.exe",
                Arguments = cliArguments,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(startInfo);
            process!.WaitForExit();
            Console.WriteLine("Attempting to disconnect...");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }
}
