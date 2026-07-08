namespace InvestmentApp.Application.Services;

public interface IVPNService
{
    void ConnectToVPN(string countryName);
    void DisconnectFromVPN();
}
