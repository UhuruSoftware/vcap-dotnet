using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Security;

namespace Uhuru.CloudFoundry.Server.MsSqlNode
{
    class Util
    {
        public static string generate_credential(int length = 12)
        {
            // as per msdn (http://msdn.microsoft.com/en-us/library/system.web.security.membership.generatepassword.aspx)
            // the characters that are non-alphanumeric will be replaced with letters/numbers
            Dictionary<char, char> unwantedCharacterMap = new Dictionary<char, char>() {
                {'!', '0'}, {'@', '1'}, {'#', '2'}, {'$', '3'},
                {'%', '4'}, {'^', '5'}, {'&', '6'}, {'*', '7'},
                {'(', '8'}, {')', '9'}, {'_', 'a'}, {'-', 'b'},
                {'+', 'c'}, {'=', 'd'}, {'[', 'e'}, {'{', 'f'},
                {']', 'g'}, {'}', 'h'}, {';', 'i'}, {':', 'j'},
                {'<', 'k'}, {'>', 'l'}, {'|', 'm'}, {'.', 'n'},
                {'/', 'o'}, {'?', 'p'}};

            string credential = Membership.GeneratePassword(length, 0);
            string result = "";
            for (int i = 0; i < credential.Length; i++)
            {
                if (unwantedCharacterMap.ContainsKey(credential[i]))
                {
                    result += unwantedCharacterMap[credential[i]];
                }
                else
                {
                    result += credential[i];
                }
            }
            return result;
        }
    }
}
