using System.IO;
using System.Xml;
using System.Xml.XPath;

/// <summary>
/// Helper class for markup related chores
/// </summary>
public static class MarkupHelper
{
    #region excerpt generation

    public static string TruncateHtml(this string input, int length = 300, string ommission = "...")
    {
        if (input == null || input.Length < length)
            return input;
        int nextSpace = input.LastIndexOf(" ", length);
        return string.Format("{0}" + ommission, 
                              input.Substring(0, (nextSpace > 0) ? nextSpace : length).Trim());
    }

    public static string StripTags(this string markup)
    {
        try
        {
            StringReader stringReader = new StringReader(markup);
            XPathDocument xPathdocument;
            using (XmlReader xmlReader = XmlReader.Create(stringReader,
                    new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Fragment }))
            {
                xPathdocument = new XPathDocument(xmlReader);
            }

            return xPathdocument.CreateNavigator().Value;
        }
        catch
        {
            return string.Empty;
        }
    }

    #endregion
}