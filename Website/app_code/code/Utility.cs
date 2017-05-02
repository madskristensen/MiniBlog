using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

public static class Utility
{
    private static NameValueCollection _emoticons;
    private const string EmoticonsSymbols = @"[adhopsADHOPS8+\-*|^()\:;@#$']{2,4}";

    static Utility()
    {
        //Emotional graphics ASCII's inspired from MSN Messenger emoticons, for more information please check the following URL http://messenger.msn.com/Resource/Emoticons.aspx
        _emoticons = new NameValueCollection() { { "Smile", ":)" }, { "Smile", ":-)" }, { "Surprised", ":-O" }, { "Surprised", ":o" }, { "Wink", ";-)" }, { "Wink", ";)" }, { "Confused", ":-S" }, { "Confused", ":s" }, { "Crying", ":'(" }, { "Hot", "(H)" }, { "Hot", "(h)" }, { "Don't tell anyone", ":-#" }, { "Nerd", "8-|" }, { "	I don't know", ":^)" }, { "Party", "<:o)" }, { "Sleepy", "|-)" }, { "Open-mouthed", ":-D" }, { "Open-mouthed", ":d" }, { "Tongue out", ":-P" }, { "Tongue out", ":p" }, { "Sad", ":-(" }, { "Sad", ":(" }, { "Disappointed", ":-|" }, { "Disappointed", ":|" }, { "Embarrassed", ":-$" }, { "Embarrassed", ":$" }, { "Angry", ":-@" }, { "Angry", ":@" }, { "Baring teeth", "8o|" }, { "Sarcastic", "^o)" }, { "Sick", "+o(" }, { "Thinking", "*-)" }, { "Eye-rolling", "8-)" } };
    }

    public static string ContentWithEmoticons(string content)
    {
        return Regex.Replace(content, EmoticonsSymbols, match => _emoticons.AllKeys.Where(key => _emoticons.GetValues(key).Where(value => value == match.Value).Any()).Select(key => string.Format("<img src=\"/posts/emoticons/{0}.gif\"/>", key)).SingleOrDefault() ?? match.Value);
    }
}