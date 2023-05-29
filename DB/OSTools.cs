using System.Runtime.InteropServices;

namespace AngelDB
{
    public static class OSTools
    {
        public static bool IsLinux()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
               return true;
            } 

            return false;

        }

        public static bool IsOsx()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
               return true;
            } 

            return false;

        }

        public static bool IsWindows()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
               return true;
            } 

            return false;

        }


        public static string OSName() 
        {

            string architecture = "";
            string os_name = "";  

            if( System.Environment.Is64BitOperatingSystem ) 
            {
                architecture = "-x64";
            } 
            else 
            {
                architecture = "-x86";
            }

            if (IsLinux())
            {
               os_name = "linux";
            } 

            if (IsWindows())
            {
               os_name = "win";
            } 

            if ( IsOsx())
            {
               os_name = "osx";
            } 

            return os_name + architecture;
        }

    }

}
