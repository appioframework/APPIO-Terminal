/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using Appio.ObjectModel;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

namespace Appio.Terminal
{
    [ExcludeFromCodeCoverage]
    public class ConsoleWriter : IWriter
    {     

        public void Write(MessageLines messagesToWrite)
        {
			var multipleColumnMessages = messagesToWrite.Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value));
			var longestKey = GetLongestKey(multipleColumnMessages);
            foreach (var message in messagesToWrite)
            {
				if(string.IsNullOrWhiteSpace(message.Key) || string.IsNullOrWhiteSpace(message.Value))
				{
					WriteSingleTextLine(message);
				}
				else
				{
					WriteParameterWithDescription(message, longestKey);
				}
            }
        }

        private static int GetLongestKey(IEnumerable<KeyValuePair<string,string>> messagesToWrite)
		{
			var longestKey = 0;
			foreach (var message in messagesToWrite)
            {
				if (message.Key.Length > longestKey)
                {
					longestKey = message.Key.Length;
                }
            }
            return longestKey;
        }

		private static void WriteParameterWithDescription(KeyValuePair<string, string> message, int longestKey)
		{
			System.Console.WriteLine(string.Format("{0}{1}", message.Key.PadRight(longestKey + Constants.NumericValues.HelpAlignmentSpace, ' '), message.Value));
		}
		private static void WriteSingleTextLine(KeyValuePair<string, string> message)
		{
			System.Console.WriteLine(string.Format("{0}{1}", message.Key, message.Value));
		}

	}
}