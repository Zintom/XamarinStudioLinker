using System.Diagnostics;

namespace XamarinStudioLinkerVSIX
{
    internal static class IdeExecutor
    {

        internal static void RunInAndroidStudio(string projectPath)
        {
            Process.Start(@"C:\Program Files\Android\Android Studio\bin\studio64.exe", projectPath);
        }

    }
}
