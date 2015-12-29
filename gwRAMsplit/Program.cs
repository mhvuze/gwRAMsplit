using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace gwRAMsplit
{
    class Program
    {
        static void Main(string[] args)
        {
            // Print header
            Console.WriteLine("gwRAMsplit by MHVuze\n");

            // Handle argument count
            if (args.Length > 1 || args.Length < 1)
            {
                Console.WriteLine("ERROR: Invalid amount of arguments specified.");
                Console.WriteLine("Use gwRAMsplit dump.bin");
            }

            // Check input
            string input = args[0];
            if (!File.Exists(input))
            {
                Console.WriteLine("ERROR: Input file not found.");
                return;
            }

            // Prepare input
            byte[] input_ = File.ReadAllBytes(input);
            MemoryStream input_ms = new MemoryStream(input_);
            BinaryReader reader = new BinaryReader(input_ms);

            // Prepare output
            if (File.Exists("dump_info.txt"))
                File.Delete("dump_info.txt");

            if (!Directory.Exists("mapdata"))
                Directory.CreateDirectory("mapdata");
            else
            {
                Directory.Delete("mapdata", true);
                Directory.CreateDirectory("mapdata");
            }

            StreamWriter txt = new StreamWriter("dump_info.txt", false, Encoding.UTF8);

            // Parse header
            // Format:
            // uint32 mapcount
            // uint32 unknown, GW didn't mention it lol
            // mapinfo: uint32 vaddr, uint32 paddr, uint32 size
            // mapdata, stored consecutively
            string entry = "vaddr\tpaddr\tsize\toffset\r\n";
            UInt32 mapcount = reader.ReadUInt32();
            reader.ReadInt32();
            UInt32 offset = (mapcount * 12) + 8;

            for (int i = 0; i < mapcount; i++)
            {
                // Read entry info
                UInt32 vaddr = reader.ReadUInt32();
                UInt32 paddr = reader.ReadUInt32();
                UInt32 size = reader.ReadUInt32();

                entry += vaddr.ToString("X8") + "\t" + paddr.ToString("X8") + "\t" + size.ToString("X8") + "\t" + offset.ToString("X8") + "\r\n";

                // Calculate next offset
                offset = offset + size;

                // Dump mapdata to output directory
                byte[] data = new byte[size];
                long pos = input_ms.Position;
                input_ms.Read(data, 0, Convert.ToInt32(size));
                input_ms.Seek(pos, SeekOrigin.Begin);
                File.WriteAllBytes("mapdata\\" + (i + 1) + "_" + vaddr.ToString("X8") + ".bin", data);
            }

            // Write txt
            txt.Write(entry);
         
            // Cleaning
            txt.Close();
            reader.Close();
            input_ms.Close();

            // Finished text
            Console.WriteLine("INFO: Finished processing input.");
        }
    }
}
