﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LostArkWebsocket
{
    public class Message
    {
        public string Type { get; set; }

        public JObject Data { get; set; }

    }
}
