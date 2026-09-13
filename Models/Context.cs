using System;
using System.Collections.Generic;

namespace AssassinBullet.Models
{
    public class Context
    {
        public Dictionary<string, object> Variables { get; set; } = new Dictionary<string, object>();
        public BlockType BlockType { get; set; }
        public HttpResponseData LastResponseData { get; set; } 
        public ScanStatus Status { get; set; }

    }
}
