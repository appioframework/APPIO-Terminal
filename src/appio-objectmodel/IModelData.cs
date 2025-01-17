﻿/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 *    Copyright 2019 (c) talsen team GmbH, http://talsen.team
 */

using System.Collections.Generic;

namespace Appio.ObjectModel
{
    public interface IModelData
    {
		string Name { get; set; }
		string Uri { get; set; }
		string NamespaceVariable { get; set; }
		List<string> RequiredModelUris { get; set; }
    }
}