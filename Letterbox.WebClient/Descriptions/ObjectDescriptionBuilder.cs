using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Letterbox.WebClient.Clients
{
    public class ObjectDescriptionBuilder
    {
        private const string _atomXmlNamespace = "http://www.w3.org/2005/Atom";
        private const string _serviceBusXmlNamespace = "http://schemas.microsoft.com/netservices/2010/10/servicebus/connect";
        private const string _xmlSchemaInstanceNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        private const string _nameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        private static readonly XName _contentXName = XName.Get("content", _atomXmlNamespace);
        private static readonly XName _authorizationRulesXName = XName.Get("AuthorizationRules", _serviceBusXmlNamespace);
        private static readonly XName _authorizationRuleXName = XName.Get("AuthorizationRule", _serviceBusXmlNamespace);

        public static XElement CreateSubscriptionDescription(List<AccessRule> accessRules)
        {
            XElement atomEntry = CreateEntityDescription("SubscriptionDescription", accessRules);
            return atomEntry;
        }

        public static XElement CreateTopicDescription(List<AccessRule> accessRules)
        {
            XElement atomEntry = CreateEntityDescription("TopicDescription", accessRules);
            return atomEntry;
        }

        public static XElement CreateQueueDescription(List<AccessRule> accessRules)
        {
            XElement atomEntry = CreateEntityDescription("QueueDescription", accessRules);
            return atomEntry;
        }

        private static XElement CreateEntityDescription(string descriptionTypeName, List<AccessRule> accessRules)
        {
            var atomEntry = new XElement(XName.Get("entry", _atomXmlNamespace));
            atomEntry.Add(
                new XElement(_contentXName, new XAttribute("type", "application/xml"),
                    new XElement(XName.Get(descriptionTypeName, _serviceBusXmlNamespace), new XAttribute(XNamespace.Xmlns + "i", _xmlSchemaInstanceNamespace))));

            if (accessRules != null && accessRules.Count > 0)
            {
                AddAccessRules(atomEntry, accessRules);
            }

            return atomEntry;
        }

        private static void AddAccessRules(XElement atomEntry, List<AccessRule> accessRules)
        {
            XElement descriptionElement = atomEntry.Element(_contentXName).Elements().First();
            XElement authorizationElement = new XElement(_authorizationRulesXName);

            foreach (AccessRule accessRule in accessRules)
            {
                var xmlRule = CreateXmlAccessRule(accessRule);
                authorizationElement.Add(xmlRule);
            }

            descriptionElement.Add(authorizationElement);
        }

        private static XElement CreateXmlAccessRule(AccessRule accessRule)
        {
            var xmlRule = new XElement(_authorizationRuleXName, new XAttribute(XName.Get("type", _xmlSchemaInstanceNamespace), "AllowRule"));
            xmlRule.Add(new XElement(XName.Get("IssuerName", _serviceBusXmlNamespace), accessRule.ServiceBusNamespace));
            xmlRule.Add(new XElement(XName.Get("ClaimType", _serviceBusXmlNamespace), _nameClaimType));
            xmlRule.Add(new XElement(XName.Get("ClaimValue", _serviceBusXmlNamespace), accessRule.UserName));

            var xmlAccessRights = new XElement(XName.Get("Rights", _serviceBusXmlNamespace));
            foreach (AccessRight right in accessRule.Permissions)
            {
                xmlAccessRights.Add(new XElement(XName.Get("AccessRights", _serviceBusXmlNamespace), right.ToString()));
            }

            xmlRule.Add(xmlAccessRights);

            return xmlRule;
        }
    }
}
