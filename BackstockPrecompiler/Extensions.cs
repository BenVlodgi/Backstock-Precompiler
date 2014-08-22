using System.Collections.Generic;
using System.Linq;
using VMFParser;

namespace BackstockPrecompiler
{

    public static class Extensions
    {
        /// <summary>Searches through each node and subnode of the list and find the highest valued id.</summary>
        public static int GetHighestID(this IList<IVNode> nodes)
        {
            int max = 0;
            int tempMax;
            foreach (var node in nodes)
            {
                if (node.GetType() == typeof(VBlock))
                {
                    if ((tempMax = GetHighestID((node as VBlock).Body)) > max)
                    {
                        max = tempMax;
                    }
                }
                else
                {
                    if (node.GetType() == typeof(VProperty) && node.Name == "id" &&
                        int.TryParse((node as VProperty).Value, out tempMax) && tempMax > max)
                    {
                        max = tempMax;
                    }
                }
            }
            return max;
        }

        private static bool ContainsID(IList<IVNode> nodes, string id)
        {
            foreach (var node in nodes)
            {
                if ((node.GetType() == typeof(VBlock) && ContainsID((node as VBlock).Body, id)) ||
                    (node.GetType() == typeof(VProperty) && node.Name == "id" && (node as VProperty).Value == id))
                    return true;
            }
            return false;
        }

        /// <summary>Performs a deep clone of this block. </summary>
        /// DeepClone should be added to the IVNode interface, and these methods moved into that library
        public static VBlock DeepClone(this VBlock vBlock)
        {
            return new VBlock(vBlock.Name, vBlock.Body == null ? null :
                vBlock.Body.Select(node => node.GetType() == typeof(VBlock) ? ((VBlock)node).DeepClone() : (IVNode)((VProperty)node).DeepClone()).ToList());
        }

        /// <summary>Performs a deep clone of this property. </summary>
        /// DeepClone should be added to the IVNode interface, and these methods moved into that library
        public static VProperty DeepClone(this VProperty vProperty)
        {
            return new VProperty(vProperty.Name, vProperty.Value);
        }

        public static void ReID(this VProperty vProperty, ref int highID)
        {
            if (vProperty.Name == "id")
            {
                vProperty.Value = (highID++).ToString();
            }
        }
        public static void ReID(this VBlock vBlock, ref int highID)
        {
            foreach (var node in vBlock.Body)
            {
                if (node.GetType() == typeof(VBlock))
                {
                    (node as VBlock).ReID(ref highID);
                }
                else if (node.GetType() == typeof(VProperty))
                {
                    (node as VProperty).ReID(ref highID);
                }
            }
        }
    }
}
