﻿// Copyright © 2021 Ravahn - All Rights Reserved
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY. without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see<http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.Diagnostics;

namespace Machina.Tests.Utility
{
    public class MemoryTraceListener : TraceListener
    {
        public List<string> Messages = new List<string>();

        public override void Write(string message)
        {
            Messages.Add(message);
        }

        public override void WriteLine(string message)
        {
            Messages.Add(message);
        }
    }
}
