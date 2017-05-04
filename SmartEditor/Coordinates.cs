using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartEditor
{
    class Coordinates : IEquatable<Coordinates>
    {
        public int start { get; set; }
        public int end { get; set; }

        public bool Equals(Coordinates other)
        {
            throw new NotImplementedException();
        }
    }
}
