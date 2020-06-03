using System;
using System.Security.Cryptography;
using System.Xml;
using Highwind.Helpers.Interfaces;

namespace Highwind.Helpers
{
    public class XmlHelper : IXmlHelper
    {
        public RSA FromXmlString(string xml)
        {
            var parameters = new RSAParameters();
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);

            if (xmlDoc.DocumentElement.Name.Equals("RSAKeyValue"))
            {
                foreach (XmlNode node in xmlDoc.DocumentElement.ChildNodes)
                {
                    switch (node.Name)
                    {
                        case "Modulus":     parameters.Modulus =    Convert.FromBase64String(node.InnerText); break;
                        case "Exponent":    parameters.Exponent =   Convert.FromBase64String(node.InnerText); break;
                        case "P":           parameters.P =          Convert.FromBase64String(node.InnerText); break;
                        case "Q":           parameters.Q =          Convert.FromBase64String(node.InnerText); break;
                        case "DP":          parameters.DP =         Convert.FromBase64String(node.InnerText); break;
                        case "DQ":          parameters.DQ =         Convert.FromBase64String(node.InnerText); break;
                        case "InverseQ":    parameters.InverseQ =   Convert.FromBase64String(node.InnerText); break;
                        case "D":           parameters.D =          Convert.FromBase64String(node.InnerText); break;
                    }
                }
            } else
            {
                throw new Exception("Invalid XML RSA key.");
            }

            RSA rsa = RSA.Create();
            rsa.ImportParameters(parameters);
            
            return rsa;
        }
    }
}