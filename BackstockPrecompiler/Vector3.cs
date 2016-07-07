using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackstockPrecompiler
{
    public class Vector3
    {
        public decimal X;
        public decimal Y;
        public decimal Z;

        public Vector3()
        {
            X = 0; //Pitch
            Y = 0; //Yaw
            Z = 0; //Roll
        }

        public Vector3(string val)
        {
            var parts = val.Split(' ');
            X = decimal.Parse(parts[0]);
            Y = decimal.Parse(parts[1]);
            Z = decimal.Parse(parts[2]);
        }

        public Vector3 Clone()
        {
            return new Vector3() { X = X, Y = Y, Z = Z };
        }
    }
}
