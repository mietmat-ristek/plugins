using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RistekPluginSample
{
    public static class MainController
    {
        static string __PLUGIN_PREFIX = "RSTSam";

        #region document serialze

        //public static SerializableDocument SerializableDocumentCurrent { get; private set; }

        public static int DocVersion { get { return 1; } }

        internal static bool ckeckDocumentVersionIsOk(int version)
        {
            return DocVersion == version;
        }

        private static string DefaulDirCurrent;

        private static string RppSubdir = __PLUGIN_PREFIX + "_data";

        internal static string getDefaultFileDialogueDir()
        {
            if (DefaulDirCurrent == null)
            {
                string _DefaulDirCurrentBase = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                DefaulDirCurrent = Path.Combine(_DefaulDirCurrentBase, RppSubdir);
            }
            return DefaulDirCurrent;
        }

        internal static void saveCurrentFileDialogueDir(string fileName)
        {
            DefaulDirCurrent = fileName;
        }

        private static string SaveFileNameStart = __PLUGIN_PREFIX + "_save.xml";
        private static string m_SaveFileNameCurrent = "";
        public static string getDefaultSaveFileNameFinal()
        {
            if (!String.IsNullOrEmpty(m_SaveFileNameCurrent))
            {
                return m_SaveFileNameCurrent;
            }
            else
            {
                return Path.Combine(MainController.getDefaultFileDialogueDir(), SaveFileNameStart);
            }
        }

        public static void resetSaveFileName()
        {
            DefaulDirCurrent = null;
            m_SaveFileNameCurrent = "";
        }

        public static bool OpenFile(out SerializableDocument SerializableDocumentCurrent, bool defaultFile = false, string filename = "")
        {
            bool res = false;

            if (defaultFile)
            {
                /*
                filename = Path.Combine(ApplicationInfo.UserAppDataDir, "default.ezp");
                */
                filename = getDefaultSaveFileNameFinal();
                //m_fileName = "";
            }

            SerializableDocument _serializableDocument = null;
            if (DocumentSerializer<SerializableDocument>.Load(ref _serializableDocument, ref filename))
            {
                //if (!defaultFile)
                //    m_SaveFileNameCurrent = filename;
                //else
                //    m_options = Options.Load();
                m_SaveFileNameCurrent = filename;
                res = true;
            }
            if (_serializableDocument != null)
            {
                SerializableDocumentCurrent = _serializableDocument;
                res &= true;
            }
            else
            {
                SerializableDocumentCurrent = null;
                // when no file at startup?
                m_SaveFileNameCurrent = getDefaultSaveFileNameFinal();
                res = false;
            }

            //mainForm.SavedDataLoadedBeforeUpdate();
            //mainForm.Reset();

            /*
            Converter.Convert(ref m_document);

            AfterDeserialize();

            mainForm.SavedDataLoadedAfterUpdate();
            */
            //AfterDocumentLoad();

            return res;
        }

        public static void SaveFile(SerializableDocument SerializableDocumentCurrent)
        {
            m_SaveFileNameCurrent = getDefaultSaveFileNameFinal();
            DocumentSerializer<SerializableDocument>.Save(SerializableDocumentCurrent, ref m_SaveFileNameCurrent);
        }

        public static void SaveAsFile(SerializableDocument SerializableDocumentCurrent)
        {
            string fileName = "";
            if (DocumentSerializer<SerializableDocument>.Save(SerializableDocumentCurrent, ref fileName))
                m_SaveFileNameCurrent = fileName;
        }

        #endregion
    }
}
