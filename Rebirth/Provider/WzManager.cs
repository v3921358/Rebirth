using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reWZ;
using Tools;

namespace Provider
{
    public class WzManager : IDisposable
    {
        private readonly Dictionary<string, WZFile> m_wzFiles;

        public WzManager()
        {
            m_wzFiles = new Dictionary<string, WZFile>();
        }

        public WZFile this[string name] => m_wzFiles[name];

        public void LoadAll()
        {
            WzConstant.Files.ForEach(LoadFile);
        }
        public void LoadFile(string fileName)
        {
            var path = WzConstant.GetFilePath(fileName);
            var file = new WZFile(path,WZVariant.GMS,true,WzConstant.WZReadSelection);

            m_wzFiles.Add(fileName, file);
        }

        public void Dispose()
        {
            //TODO: One Day
        }
    }
}
