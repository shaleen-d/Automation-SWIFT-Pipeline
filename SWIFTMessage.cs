using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WpfApplication1
{
    // Class that describes the template of a generic SWIFT Message.
    class SWIFTMessage
    {
        // Properties
        public int Type { get; set; } // Type of SWIFT Message. Ex - 202
        public string Sender { get; set; } // Bank that sands the message. Ex - SBPP, BBOB
        public float Amount { get; set; } // Amount if the transaction. Just the total aount for 203. 0 for others.
        public int NPTR { get; set; } // Relevant only for BB, CB, MM. Just the first NPTR for SBP. 0 for others.
        public string CompleteMessage { get; set; } // String of the Complete message
        
        // Constructor
        public SWIFTMessage() { }
        public SWIFTMessage(Match pageMatch)
        {
            CompleteMessage = pageMatch.Value; // Stores the Complete Message
            Type = ParseType(CompleteMessage); // Regex required = Swift\s*Output\s*:\s*FIN\s*(\d{3})
            Sender = ParseSender(CompleteMessage); // Regex required = [S|s]ender.*:\s*(\w{4})
            Amount = ParseAmount(CompleteMessage); // Regex required = Amount.*?:\s*?#(\d+)\W(\d*)# (Check for CBCE)
            NPTR = ParseNPTR(CompleteMessage); // Regex required = NPTR\s*2[\s-]*(NO)?\s*(\d*)\b

        }

        // Static Functions
        // Parse NPTR No. from the SWIFT message
        public static int ParseNPTR(string MessageString)
        {
            Regex nptrMatchHelper = new Regex(@"NPTR\s*2[\s-]*(NO)?\s*(\d*)\b", RegexOptions.IgnoreCase);
            Match typeMatch = nptrMatchHelper.Match(MessageString);
            int tempNPTR;
            return int.TryParse(typeMatch.Groups[2].Value, out tempNPTR) ? tempNPTR : 0;
        }

        // Parse Amount from the SWIFT message
        public static float ParseAmount(string MessageString)
        {
            Regex amountMatchHelper = new Regex(@"Amount.*?:\s*?#(\d+)\W(\d*)#", RegexOptions.IgnoreCase);
            Match typeMatch = amountMatchHelper.Match(MessageString);
            long tempInteger, tempDecimal;
            string tempString = (long.TryParse(typeMatch.Groups[1].Value, out tempInteger) ? tempInteger : 0).ToString()+"."+ (long.TryParse(typeMatch.Groups[2].Value, out tempDecimal) ? tempDecimal : 0).ToString();
            float tempAmount;
            return float.TryParse(tempString, out tempAmount) ? tempAmount : 0;
        }

        // Parse Sender information from the SWIFT message
        public static string ParseSender(string MessageString)
        {
            Regex senderMatchHelper = new Regex(@"Sender.*:\s*(\w{4})", RegexOptions.IgnoreCase);
            Match senderMatch = senderMatchHelper.Match(MessageString);
            return senderMatch.Groups[1].Value;
        }

        // Parse Type from the SWIFT message
        public static int ParseType(string MessageString)
        {
            Regex typeMatchHelper = new Regex(@"Swift\s*Output\s*:\s*FIN\s*(\d{3})", RegexOptions.IgnoreCase);
            Match typeMatch = typeMatchHelper.Match(MessageString);
            int tempType;
            return int.TryParse(typeMatch.Groups[1].Value, out tempType) ? tempType : 0; 
        }
    }
}
