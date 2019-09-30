﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Appio.ObjectModel
{
    public class OpcuaServerApp : IOpcuaServerApp
    {
        public OpcuaServerApp()
        {
        }

        public OpcuaServerApp(string name, string url, string port)
        {
            Name = name;
            Url = url;
			Port = port;
        }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

		[JsonProperty("type")]
		public string Type { get; set;  } = Constants.ApplicationType.Server;

        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

		[JsonProperty("port")]
		public string Port { get; set; } = string.Empty;

		[JsonProperty("models")]
		[JsonConverter(typeof(OpcuaappConverter<IModelData, ModelData>))]
		public List<IModelData> Models { get; set; } = new List<IModelData>();
	}
}