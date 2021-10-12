using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace XamarinStudioLinkerVSIX
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class OpenInStudioCommand : ProjectLinker.ILogger
    {

        private static readonly string[] _validFileExtensions = new string[] { ".xml" };

        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("3fd946e8-0693-4208-89b4-35822a519235");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenInStudioCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private OpenInStudioCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new OleMenuCommand(this.Execute, menuCommandID);
            menuItem.BeforeQueryStatus += MenuItem_BeforeQueryStatus;

            commandService.AddCommand(menuItem);
        }

        private void MenuItem_BeforeQueryStatus(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (menuCommand == null) { return; }

            ProjectItem selectedProjectItem = GetSingleSelectedProjectItemOrNull();
            if (selectedProjectItem == null) { return; }

            bool isVisible = false;

            if (Guid.TryParse(selectedProjectItem.Kind, out Guid kindGuid))
            {
                if (kindGuid == VSConstants.ItemTypeGuid.PhysicalFile_guid)
                {
                    // The file extension must be an xml file.
                    if (_validFileExtensions.Contains(Path.GetExtension(selectedProjectItem.Name)))
                    {
                        isVisible = true;
                    }
                }
            }

            menuCommand.Visible = isVisible;
        }

        /// <summary>
        /// Gets the selected project item, or <see langword="null"/> if no item is selected or more than one item is selected.
        /// </summary>
        private static ProjectItem GetSingleSelectedProjectItemOrNull()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = Package.GetGlobalService(typeof(DTE)) as DTE;
            if (dte == null) { return null; }

            SelectedItem[] selectedItems = dte.SelectedItems.OfType<SelectedItem>().ToArray();
            if (selectedItems.Length != 1) { return null; }

            return selectedItems[0].ProjectItem;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static OpenInStudioCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in OpenInStudioCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenInStudioCommand(package, commandService);
        }


        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD110:Observe result of async calls", Justification = "Not necessary as is the last call the method makes.")]
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ProjectItem selectedProjectItem = GetSingleSelectedProjectItemOrNull();
            if (selectedProjectItem == null) { return; }

            Project project = selectedProjectItem.ContainingProject;

            // This is the "Resources" folder that will need mirroring
            string resourcesFolder = Path.Combine(Directory.GetParent(project.FileName).FullName, "Resources");

            if (!Directory.Exists(resourcesFolder))
            {
                VsShellUtilities.ShowMessageBox(
                    this.package,
                    $"The 'Resources' folder is not present.",
                    "Problem with project.",
                    OLEMSGICON.OLEMSGICON_CRITICAL,
                    OLEMSGBUTTON.OLEMSGBUTTON_OK,
                    OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                return;
            }

            string projectName = project.Name;
            Task.Run(new Func<Task>(async () =>
            {
                ProjectLinker.SetupTemplateProject(projectName, out string tempProjectLocation, out string tempResourcesLocation, this);

                // If the temp resource folder does not exist yet, we need to create it.
                if (!Directory.Exists(tempResourcesLocation))
                {
                    ProjectLinker.CreateSymbolicLink(resourcesFolder, tempResourcesLocation, out bool symLinkSuccess, out int symLinkErrorCode);

                    // Come back to the UI thread as all subsequent work is minimal impact and relies on the UI thread.
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                    if (!symLinkSuccess && symLinkErrorCode == 1314) // ERROR_PRIVILEGE_NOT_HELD
                    {
                        VsShellUtilities.ShowMessageBox(
                            this.package,
                            $"This functionality relies on the CreateSymbolicLinkW Kernel32.dll API, which requires elevated access. Run Visual Studio as Administrator to fix this.",
                            "1314 ERROR_PRIVILEGE_NOT_HELD",
                            OLEMSGICON.OLEMSGICON_INFO,
                            OLEMSGBUTTON.OLEMSGBUTTON_OK,
                            OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);

                        return;
                    }
                }

                // Launch the IDE using the temporary project.
                IdeExecutor.RunInAndroidStudio(tempProjectLocation);
            }));
        }

        public void Log(string message) { }
    }
}
