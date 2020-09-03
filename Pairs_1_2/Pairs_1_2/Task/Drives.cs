using System;
using System.IO;
using Pairs_1_2.Task;

namespace Pairs_1_2
{
    class Drives
    {
        private const int ByteToGb = 1073741824;
        public static void Func()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                Console.WriteLine($"{drive.Name} drive config:\n");
                Console.WriteLine($"avail free space: {drive.AvailableFreeSpace / ByteToGb} GB (approx. {drive.AvailableFreeSpace} bytes)");
                Console.WriteLine($"file system: {drive.DriveFormat}");
                Console.WriteLine($"drive type: {drive.DriveType}");
                if (drive.IsReady)
                {
                    Console.WriteLine($"total size: {drive.TotalSize / ByteToGb} GB (approx. {drive.TotalSize} bytes)");
                    Console.WriteLine($"total free space: {drive.TotalFreeSpace / ByteToGb} GB (approx. {drive.TotalFreeSpace} bytes)");
                    Console.WriteLine($"volume label: {drive.VolumeLabel}");
                }
                Console.WriteLine("\n---------------------------\n");
            }
        }

    }
}
