namespace InvestmentApp.Application.Services;

public interface IVpnService
{
    void ConnectToVPN(string countryName);
    void DisconnectFromVPN();
}
