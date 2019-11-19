using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CM
{
    public struct Position
    {
        public Position(int phase, int resistance)
        {
            Phase = phase;
            Resistance = resistance;
        }

        public int Phase { get; set; }
        public int Resistance { get; set; }

        public override string ToString()
        {
            return $"{Phase:00}.{Resistance}";
        }

        public static Position FromString(string s)
        {
            var parts = s.Split('.');
            return parts.Length != 2 ? default(Position) : new Position(int.Parse(parts[0]), int.Parse(parts[1]));
        }
    }
}
