using System.Security.Cryptography;

namespace Highwind.Helpers.Interfaces
{
    public interface IXmlHelper
    {
        RSA FromXmlString(string xml);
    }
}
