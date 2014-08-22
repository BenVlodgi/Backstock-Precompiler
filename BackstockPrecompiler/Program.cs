using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using VMFParser;

namespace BackstockPrecompiler
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            args = new string[] { "-file", "dev_room.vmf" };
#endif
            try
            {
                #region Get execution parameters
                string filePath = null;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "-file" && i + 1 < args.Length)
                    {
                        filePath = args[++i];
                    }
                    // else if // Add additional parameters here
                }
                if (filePath == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    return 1;
                }
                #endregion

                #region Load VMF
                Console.Write("Loading file contents from \"" + filePath + "\"...");
                string[] fileContents = File.ReadAllLines(filePath);
                Console.WriteLine("Complete.");

                Console.Write("Parsing vmf ");
                VMF vmf = new VMF(fileContents);
                #endregion

                #region Identify useable IDs
                int usableID = vmf.Body.GetHighestID() + 1;
                #endregion

                var entities = vmf.Body.Where(item => item.Name == "entity").Select(item => item as VBlock).ToList();
                var instances = entities.Where(entity => entity.Body.Where(item => item.Name == "classname" && (item as VProperty).Value == "func_instance").Count() > 0).ToList();
                foreach(var instance in instances)
                {
                    // Load the instance vmf
                    // Clone the important parts
                    // ReID the clone
                    // Update each entity into the map with relative offsets and angles from the instance point, and the instance origin (defaults at 0,0,0)
                    // Rename all entities
                    // Rename all internal communication
                    // Link exteneral IO
                    // Replace replacable parameters
                    // Replace replacable materials
                    // Insert into actual map
                }

                File.WriteAllLines(filePath, vmf.ToVMFStrings());
            }
            #region catch - display exception
            catch (Exception ex)
            {
                WriteLine("Exception:", ConsoleColor.Yellow);
                WriteLine(ex.ToString(), ConsoleColor.Red);
                return 1;
            }
            #endregion

            return 0;
        }


        private static void WriteLine(string format, ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black, params object[] args)
        {
            ConsoleColor f = Console.ForegroundColor;
            ConsoleColor b = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.WriteLine(format, args);
            Console.ForegroundColor = f;
            Console.BackgroundColor = b;
        }

        private static void Write(string format, ConsoleColor foreground = ConsoleColor.Gray, ConsoleColor background = ConsoleColor.Black, params object[] args)
        {
            ConsoleColor f = Console.ForegroundColor;
            ConsoleColor b = Console.BackgroundColor;
            Console.ForegroundColor = foreground;
            Console.BackgroundColor = background;
            Console.Write(format, args);
            Console.ForegroundColor = f;
            Console.BackgroundColor = b;
        }
    }
}