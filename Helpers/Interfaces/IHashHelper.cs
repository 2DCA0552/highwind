namespace Highwind.Helpers.Interfaces
{
    public interface IHashHelper {
        string GenerateClientSecret();
        string GenerateSHA256String(string input);
    }
}