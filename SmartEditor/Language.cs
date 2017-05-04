using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SmartEditor
{
    class Language
    {
        public string sessionId;
        public string type;
        public string apiVersion;
        public string path;
        public string visibleText;
        public int visibleTextStartPos;
        public int cursorPos;
        public string applicationKey;
        public string application;
        public int timestamp;
        public JArray items;
        
    }
}
