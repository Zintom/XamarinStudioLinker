using System.IO;
using System.IO.Compression;
using System.Runtime.InteropServices;

namespace XamarinStudioLinkerVSIX
{
    internal class ProjectLinker
    {

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.I1)]
        private static extern bool CreateSymbolicLinkW(string lpSymlinkFileName, string lpTargetFileName, SymbolicLinkFlags dwFlags);

        private enum SymbolicLinkFlags : uint
        {
            File = 0,
            Directory = 1
        }

        /// <summary>
        /// Extracts the template project to a temporary location.
        /// </summary>
        /// <param name="projectName">This will be the name given to the temporary project.</param>
        /// <param name="tempProjectLocation">The on-disk location of the temporary project.</param>
        /// <param name="tempResourcesLocation">The on-disk location of the temporary project's "res" (Resources) folder, <b>The folder is not created by this method.</b></param>
        internal static void SetupTemplateProject(string projectName, out string tempProjectLocation, out string tempResourcesLocation, ILogger logger)
        {
            tempProjectLocation = string.Format("{0}XamarinStudioLinkerTemporaryProject({1})", Path.GetTempPath(), projectName);

            if (!Directory.Exists(tempProjectLocation))
            {
                // Read the template.
                using (var zipStream = new MemoryStream(Properties.Resources.XamarinStudioLinkerTemplateV2))
                using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Read))
                {
                    logger.Log("Extracting the project template to the temporary location.");

                    // Extract the template project to the temporary project location.
                    archive.ExtractToDirectory(tempProjectLocation);

                    logger.Log("Temporary project extracted.");
                }
            }
            else
            {
                logger.Log("The temporary project already exists so re-using.");
            }

            tempResourcesLocation = string.Concat(tempProjectLocation, "\\app\\src\\main\\res");
            logger.Log(string.Format("The location for the temporary resources folder is '{0}'", tempResourcesLocation));
        }

        /// <summary>
        /// Creates a symbolic link, creates <paramref name="newDirectory"/> as a link to <paramref name="sourceDirectory"/>.
        /// </summary>
        internal static void CreateSymbolicLink(string sourceDirectory, string newDirectory, out bool success, out int errorCode)
        {
            success = CreateSymbolicLinkW(newDirectory, sourceDirectory, SymbolicLinkFlags.Directory);
            errorCode = success ? -1 : Marshal.GetLastWin32Error();
        }

        internal interface ILogger
        {
            void Log(string message);
        }
    }

}
