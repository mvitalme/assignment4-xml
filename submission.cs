using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Net;
using System.Collections.Generic;

/**
 * This template file is created for ASU CSE445 Distributed SW Dev Assignment 4.
 * Please do not modify or delete any existing class/variable/method names. However, you can add more variables and functions.
 * Uploading this file directly will not pass the autograder's compilation check, resulting in a grade of 0.
 **/

namespace ConsoleApp1
{
    public class Program
    {
        public static string xmlURL = "https://mvitalme.github.io/assignment4-xml/Hotels.xml";
        public static string xmlErrorURL = "https://mvitalme.github.io/assignment4-xml/HotelsErrors.xml";
        public static string xsdURL = "https://mvitalme.github.io/assignment4-xml/Hotels.xsd";

        public static void Main(string[] args)
        {
            try
            {
                // Validates XML against schema Hotels.xsd
                string result = Verification(xmlURL, xsdURL);
                Console.WriteLine("Valid XML: \n" + result);

                // Validates incorrect XML against schema Hotels.xsd
                result = Verification(xmlErrorURL, xsdURL);
                Console.WriteLine("\nInvalid XML:\nException! " + result.Replace("Exception: ", ""));

                // Converts valid XML to JSON via Newtonsoft
                result = Xml2Json(xmlURL);
                Console.WriteLine("\nJSON Result: \n" + result);
            }
            catch (Exception ex)
            {
                // Catch unhandled exceptions
                Console.WriteLine($"Unhandled Exception: {ex.Message}");
            }

            // Pauses console unless key is pressed
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            // Return "No Error" if XML is valid
            try
            {
                XmlSchemaSet schema = new XmlSchemaSet();
                using (WebClient client = new WebClient())
                {
                    string xsdContent = client.DownloadString(xsdUrl);
                    // XSD schema
                    using (StringReader xsdReader = new StringReader(xsdContent))
                    {
                        schema.Add("", XmlReader.Create(xsdReader));
                    }
                    // XML hotels content
                    string xmlContent = client.DownloadString(xmlUrl);
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.Schemas = schema;
                    settings.ValidationType = ValidationType.Schema;

                    // Collects any errors during validation
                    string errors = "";
                    settings.ValidationEventHandler += (s, e) => errors += e.Message + "\n";

                    // Validates XML via XmlReader
                    using (StringReader xmlReader = new StringReader(xmlContent))
                    using (XmlReader reader = XmlReader.Create(xmlReader, settings))
                    {
                        while (reader.Read()) { }
                    }

                    return string.IsNullOrWhiteSpace(errors) ? "No Error" : errors; // XML is valid
                }
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}"; // Returns exception message
            }
        }

        // Q2.2
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    string xmlContent = client.DownloadString(xmlUrl);
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xmlContent);

                    var hotelsList = new List<Dictionary<string, object>>();
                    XmlNodeList hotelNodes = doc.DocumentElement.SelectNodes("Hotel");

                    foreach (XmlNode hotel in hotelNodes)
                    {
                        var hotelDict = new Dictionary<string, object>();

                        // Hotel name
                        hotelDict["Name"] = hotel.SelectSingleNode("Name")?.InnerText;

                        // Phone(s)
                        var phones = hotel.SelectNodes("Phone");
                        var phoneList = new List<string>();
                        foreach (XmlNode phone in phones)
                        {
                            phoneList.Add(phone.InnerText);
                        }
                        hotelDict["Phone"] = phoneList;

                        // Address
                        var address = hotel.SelectSingleNode("Address");
                        if (address != null)
                        {
                            var addressDict = new Dictionary<string, object>();

                            // Gets <Number> as an element (new structure)
                            var numberNode = address.SelectSingleNode("Number");
                            if (numberNode != null)
                                addressDict["Number"] = numberNode.InnerText;

                            addressDict["Street"] = address.SelectSingleNode("Street")?.InnerText ?? "";
                            addressDict["City"] = address.SelectSingleNode("City")?.InnerText ?? "";
                            addressDict["State"] = address.SelectSingleNode("State")?.InnerText ?? "";
                            addressDict["Zip"] = address.SelectSingleNode("Zip")?.InnerText ?? "";

                            // REQUIRED attribute _NearestAirport
                            if (address.Attributes["NearestAirport"] != null)
                                addressDict["_NearestAirport"] = address.Attributes["NearestAirport"].Value;
                            else
                                addressDict["_NearestAirport"] = "MISSING"; // Defensive fallback if schema fails

                            hotelDict["Address"] = addressDict;
                        }

                        // OPTIONAL attribute _Rating
                        if (hotel.Attributes["Rating"] != null)
                            hotelDict["_Rating"] = hotel.Attributes["Rating"].Value;

                        hotelsList.Add(hotelDict);
                    }

                    var root = new Dictionary<string, object>
                    {
                        ["Hotels"] = new Dictionary<string, object>
                        {
                            ["Hotel"] = hotelsList
                        }
                    };

                    return JsonConvert.SerializeObject(root, Newtonsoft.Json.Formatting.Indented);
                }
            }
            catch (Exception ex)
            {
                return $"Conversion Error: {ex.Message}";
            }
        }
    }
}
