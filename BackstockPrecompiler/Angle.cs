using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackstockPrecompiler
{
    public class Angle
    {
        public decimal Yaw;
        public decimal Pitch;
        public decimal Roll;

        public Angle()
        {
            Yaw = 0;
            Pitch = 0;
            Roll = 0;
        }

        public Angle(string val)
        {
            var parts = val.Split(' ');
            Yaw = decimal.Parse(parts[0]);
            Pitch = decimal.Parse(parts[1]);
            Roll = decimal.Parse(parts[2]);
        }
    }
}
