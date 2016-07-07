using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackstockPrecompiler
{
    public class MatrixMath
    {
        public static decimal[,] Multiply3v3(decimal[,] matrix1, decimal[,] matrix2)
        {
            decimal[,] newMatrix = new decimal[3, 3];

            for (int r = 0; r < 3; ++r)
                for (int c = 0; c < 3; ++c)
                    for (int i = 0; i < 3; ++i)
                        newMatrix[r, c] += matrix1[r, i] * matrix2[i, c];

            return newMatrix;
        }

        public static decimal[,] GetXRotation(decimal angle)
        {
            double angleRad = Convert.ToDouble(angle) * Math.PI / 180;
            decimal cos = Convert.ToDecimal(Math.Cos(angleRad));
            decimal sin = Convert.ToDecimal(Math.Sin(angleRad));

            return new decimal[,] {
                {  1M,  0M,  0M    }
               ,{  0M,  cos, -sin  }
               ,{  0M,  sin,  cos  }
            };
        }

        public static decimal[,] GetYRotation(decimal angle)
        {
            double angleRad = Convert.ToDouble(angle) * Math.PI / 180;
            decimal cos = Convert.ToDecimal(Math.Cos(angleRad));
            decimal sin = Convert.ToDecimal(Math.Sin(angleRad));

            return new decimal[,] {
                {  cos,  0M,  sin  }
               ,{  0M,   1M,  0M   }
               ,{ -sin,  0M,  cos  }
            };
        }

        public static decimal[,] GetZRotation(decimal angle)
        {
            double angleRad = Convert.ToDouble(angle) * Math.PI / 180;
            decimal cos = Convert.ToDecimal(Math.Cos(angleRad));
            decimal sin = Convert.ToDecimal(Math.Sin(angleRad));

            return new decimal[,] {
                {  cos, -sin,  0M  }
               ,{  sin,  cos,  0M  }
               ,{  0M,   0M,   1M  }
            };
        }

        public static decimal[,] GetRotationMatrix(decimal Pitch, decimal Yaw, decimal Roll)
        {
            return MatrixMath.Multiply3v3(MatrixMath.GetZRotation(Yaw), MatrixMath.Multiply3v3(MatrixMath.GetYRotation(Pitch), MatrixMath.GetXRotation(Roll)));
        }
    }
}
