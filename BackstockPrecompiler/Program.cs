﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using VMFParser;

namespace BackstockPrecompiler
{
    class Program
    {
        static int Main(string[] args)
        {
#if DEBUG
            //args = new string[] { "-file", "dev_room.vmf" };
            args = new string[] { "-file", "1_instance.vmf" };

            Utils.DirectoryCopy(@"../../instances", "instances", true, true);
#endif

#if !DEBUG
            try
            {
#endif
            #region Get execution parameters
            string filePath = null;
            string gamePath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-file" && i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    filePath = args[++i];
                }
                else if (args[i] == "-game" && i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    gamePath = args[++i]; //does this matter, or should I use the vmfs path to find the instances
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

            CollapseInstances(vmf);

            #region Save Altered VMF
            string newFilePath = "";
            newFilePath = filePath.EndsWith(".vmf")
                ? filePath.Insert(filePath.LastIndexOf("."), "_new")
                : filePath + "_new";

            File.WriteAllLines(newFilePath, vmf.ToVMFStrings());
            #endregion

#if !DEBUG
            }
            #region catch - display exception
            catch (Exception ex)
            {
                WriteLine("Exception:", ConsoleColor.Yellow);
                WriteLine(ex.ToString(), ConsoleColor.Red);
                return 1;
            }
            #endregion
#endif

            return 0;
        }

        static int CollapseInstances(VMF vmf)
        {
            #region Identify useable IDs
            int usableID = vmf.Body.GetHighestID() + 1;
            #endregion

            var world = vmf.Body.Where(node => node.Name == "world").Where(node => node is VBlock).Cast<VBlock>().FirstOrDefault();
            if (world == null)
            {
                Console.WriteLine("Can't find \"world\"");
                return -1;
            }

            int lastEntityLocation = 0;
            lastEntityLocation = vmf.Body.IndexOf(world) + vmf.Body.Where(node => node.Name == "entity").Count();

            int autoInstance = 0;

            var entities = vmf.Body.Where(item => item.Name == "entity").Select(item => item as VBlock).ToList();
            var instances = entities.Where(entity => entity.Body.Where(item => item.Name == "classname" && (item as VProperty).Value == "func_instance").Count() > 0).ToList();
            foreach (var instance in instances)
            {
                // Get instance targetname
                string instanceTargetName = "";
                var instanceTargetNameProperty = instance.Body.Where(node => node.Name == "targetname" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault();
                if ((instanceTargetNameProperty?.Value ?? "") == "")
                    instanceTargetName = String.Format("AutoInstance{0}", autoInstance++);


                // TODO: Give it a default name if unnamed (or not?).

                // Load the instance vmf
                var fileProp = instance.Body.Where(node => node.Name == "file" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault();
                if (fileProp == null || string.IsNullOrEmpty(fileProp.Value.Trim()))
                {
                    // Looks like there is no file property, or its empty
                    continue;
                }
                if (!File.Exists(fileProp.Value))
                {
                    // TODO: What do you do when the file doesn't exist? Probably should throw up some sort of error.
                    // Just skip it for now.
                    continue;
                }
                var instanceVMF = new VMF(File.ReadAllLines(fileProp.Value));

                // Clone the important parts
                // TODO: think about collapsing groups
                // TODO: only grab visible entities
                var instanceVisibleEntities = instanceVMF.Body.Where(node => node.Name == "entity").Where(node => node is VBlock).Cast<VBlock>();
                var instanceVisibleSolids = instanceVMF.Body.Where(node => node.Name == "world").Where(node => node is VBlock).Cast<VBlock>()
                    .SelectMany(node => node.Body.Where(worldNode => worldNode.Name == "solid").Where(worldNode => worldNode is VBlock).Cast<VBlock>()
                    );

                // Update each entity into the map with relative offsets and angles from the instance point, and the instance origin (defaults at 0 0 0)
                // angles and origin
                var instanceOriginProperty = instance.Body.Where(node => node.Name == "origin" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault();
                var instanceAnglesProperty = instance.Body.Where(node => node.Name == "angles" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault();

                var instanceOrigin = new Vector3(instanceOriginProperty?.Value ?? "0 0 0");
                var instanceAngles = new Vector3(instanceAnglesProperty?.Value ?? "0 0 0");

                var fixupStyle = int.Parse(instance.Body.Where(node => node.Name == "fixup_style" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault()?.Value ?? "0");


                foreach (var entity in instanceVisibleEntities)
                {
                    VBlock collapsedEntity = CollapseEntity(entity, fixupStyle, instanceTargetName, instanceOrigin, instanceAngles, ref usableID);
                    vmf.Body.Insert(lastEntityLocation++, collapsedEntity);
                }

                foreach (var solid in instanceVisibleSolids)
                {
                    VBlock collapsedSolid = CollapseSolid(solid, ref usableID);
                    world.Body.Add(collapsedSolid);
                }


                // Rename all internal communication
                // Link external IO
                // Replace replaceable parameters
                // Replace replaceable materials
                // Insert into actual map

                // Remove instance from map
                vmf.Body.Remove(instance);
            }


            return 0;
        }

        static VBlock CollapseEntity(VBlock entity, int fixupStyle, string instanceName, Vector3 instanceOrigin, Vector3 instanceAngles, ref int highID)
        {
            VBlock collapsedEntity = entity.DeepClone();
            var targetNameProperty = collapsedEntity.Body.Where(node => node.Name == "targetname" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault();

            var solids = entity.Body.Where(node => node.Name == "solid" && node is VBlock).Cast<VBlock>();

            var relativeEntityPositionProperty = collapsedEntity.Body.Where(node => node.Name == "origin" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault();
            var relativeEntityAngleProperty = collapsedEntity.Body.Where(node => node.Name == "angles" && node.GetType() == typeof(VProperty)).Select(node => node as VProperty).FirstOrDefault();

            var relativeEntityPosition = new Vector3(relativeEntityPositionProperty?.Value ?? "0 0 0");
            var relativeEntityAngle = new Vector3(relativeEntityAngleProperty?.Value ?? "0 0 0");

            if (relativeEntityPositionProperty != null)
            {
                var newEntityPosition = relativeEntityPosition.Clone();
                newEntityPosition.Rotate(instanceAngles);
                newEntityPosition.Offset(instanceOrigin);
                relativeEntityPositionProperty.Value = newEntityPosition.ToString();
            }

            if (relativeEntityAngleProperty != null)
            {
                var newEntityAngle = relativeEntityAngle.Clone();
                newEntityAngle.AddAngles(instanceAngles);
                relativeEntityAngleProperty.Value = newEntityAngle.ToString();
            }

            collapsedEntity.ReID(ref highID);

            // Collapse the entity's sub solids
            foreach (var solid in solids)
            {
                entity.Body[entity.Body.IndexOf(solid)] = CollapseSolid(solid, ref highID);
            }


            // Rename Entity
            if (targetNameProperty != null && !targetNameProperty.Value.Contains("@"))
            {
                if (fixupStyle == 0)
                    targetNameProperty.Value = instanceName + "-" + targetNameProperty.Value; // Prefix
                else if (fixupStyle == 1)
                    targetNameProperty.Value = targetNameProperty.Value + "-" + instanceName; // Postfix
            }

            return collapsedEntity;
        }

        static VBlock CollapseSolid(VBlock solid, ref int highID)
        {
            // TODO: transform solid

            return solid.DeepClone();
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