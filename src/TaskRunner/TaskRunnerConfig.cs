using System;
using System.IO;
using System.Text;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TaskRunnerExplorer;

namespace BrunchTaskRunner
{
    class TaskRunnerConfig : ITaskRunnerConfig
    {
        private readonly ImageSource _icon;
        private readonly ITaskRunnerCommandContext _context;
        readonly ITaskRunnerNode _hierarchy;

        public TaskRunnerConfig(ITaskRunnerCommandContext context, ITaskRunnerNode hierarchy, ImageSource icon)
        {
            _context = context;
            _hierarchy = hierarchy;
            _icon = icon;
        }

        public ImageSource Icon
        {
            get { return _icon; }
        }

        public ITaskRunnerNode TaskHierarchy
        {
            get { return _hierarchy; }
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        public string LoadBindings(string configPath)
        {
            string bindingPath = configPath + ".bindings";

            if (File.Exists(bindingPath))
                return File.ReadAllText(bindingPath).Replace("///", string.Empty);

            return "<binding />";
        }

        public bool SaveBindings(string configPath, string bindingsXml)
        {
            string bindingPath = configPath + ".bindings";

            try
            {
                ProjectHelpers.CheckFileOutOfSourceControl(bindingPath);

                if (bindingsXml == "<binding />" && File.Exists(bindingPath))
                {
                    ProjectHelpers.DeleteFileFromProject(bindingPath);
                }
                else
                {
                    File.WriteAllText(bindingPath, "///" + bindingsXml, Encoding.UTF8);
                    ProjectHelpers.AddNestedFile(configPath, bindingPath);
                }

                if (!BrunchPackage.IsDocumentDirty(configPath, out IVsPersistDocData persistDocData) && persistDocData != null)
                {
                    persistDocData.SaveDocData(VSSAVEFLAGS.VSSAVE_SilentSave, out string newName, out int cancelled);
                }
                else if (persistDocData == null)
                {
                    new FileInfo(configPath).LastWriteTime = DateTime.Now;
                }

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex);
                return false;
            }
        }
    }
}
