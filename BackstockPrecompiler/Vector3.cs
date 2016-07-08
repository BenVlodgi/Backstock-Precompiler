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

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", X, Y, Z);
        }

        public Vector3 Clone()
        {
            return new Vector3() { X = X, Y = Y, Z = Z };
        }


        public Vector3 GetTransformedPosition(Vector3 translation, Vector3 rotation)
        {
            var vec = Clone();
            vec.Rotate(rotation);
            vec.Offset(translation);
            return vec;
        }

        public Vector3 GetTransformedRotation(Vector3 rotation)
        {
            var vec = Clone();
            vec.Rotate(rotation);
            return vec;
        }

        public Vector3 GetTransformedAngles(Vector3 rotation)
        {
            var vec = Clone();
            vec.AddAngles(rotation);
            return vec;
        }

        public void Rotate(Vector3 angles)
        {
            decimal[,] rotationMatrix = MatrixMath.GetRotationMatrix(angles.Y, angles.X, angles.Z);
            decimal x = X, y = Y, z = Z;

            X = x * rotationMatrix[0, 0] + y * rotationMatrix[0, 1] + z * rotationMatrix[0, 2];
            Y = x * rotationMatrix[1, 0] + y * rotationMatrix[1, 1] + z * rotationMatrix[1, 2];
            Z = x * rotationMatrix[2, 0] + y * rotationMatrix[2, 1] + z * rotationMatrix[2, 2];
        }

        public void AddAngles(Vector3 angles)
        {
            var a = MatrixMath.GetRotationMatrix(X, Y, Z);
            var b = MatrixMath.GetRotationMatrix(angles.X, angles.Y, angles.Z);
            
            decimal[,] angleMatrix = MatrixMath.Multiply3v3(b, a);
            double yaw = Math.Atan2((double)angleMatrix[1, 0], (double)angleMatrix[0, 0]) / Math.PI * 180.0;

            angleMatrix = MatrixMath.Multiply3v3(MatrixMath.GetZRotation(Convert.ToDecimal(-yaw)), angleMatrix);
            double pitch = Math.Atan2((double)-angleMatrix[2, 0], (double)angleMatrix[0, 0]) / Math.PI * 180.0;
            double roll = Math.Atan2((double)-angleMatrix[1, 2], (double)angleMatrix[1, 1]) / Math.PI * 180.0;
            
            X = Convert.ToDecimal(pitch - Math.Floor(pitch / 360.0) * 360.0);
            Y = Convert.ToDecimal(yaw - Math.Floor(yaw / 360.0) * 360.0);
            Z = Convert.ToDecimal(roll - Math.Floor(roll / 360.0) * 360.0);
        }

        public void Offset(Vector3 vector)
        {
            X += vector.X;
            Y += vector.Y;
            Z += vector.Z;
        }
    }
}
