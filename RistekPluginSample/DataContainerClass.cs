using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace RistekPluginSample
{
    [Serializable()]
    public abstract class SerializableDocAbst
    {
        protected SerializableDocAbst()
        {
            m_Build = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        protected int m_Version;
        [System.Xml.Serialization.XmlAttribute]
        //public int Version { get; set; }
        public int Version
        {
            get
            {
                return m_Version;
            }
            set
            {
                // do nothing
            }
        }

        private string m_Build;
        [System.Xml.Serialization.XmlAttribute]
        public string Build
        {
            get
            {
                return m_Build;
            }
            set
            {
                // do nothing
            }
        }

    }

    [Serializable()]
    public class SerializableDocument : SerializableDocAbst
    {
        public SerializableDocument()
            : base()
        {
            //m_Build = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            //Version = MainController.DocVersion;
            m_Version = MainController.DocVersion;
            DataFieldsRawContainerObject = new DataFieldsRawContainer();
        }

        public DataFieldsRawContainer DataFieldsRawContainerObject { get; set; }
    }

    public static class DocumentSerializer<T> where T : SerializableDocAbst
    {
        static DocumentSerializer()
        {
            //DefaulDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
        }

        //private static string DefaulDir;

        private static Encoding m_encoding = Encoding.UTF8;
        //private static string m_fileFilter = TranslationManager.Translate("General.ProjectFileDescription") + "(*.xml)|*.xml";
        private static string m_fileFilter = "(*.xml)|*.xml";

        /// <summary>
        /// save document to xml file
        /// </summary>
        public static bool Save(T document, ref string fileName)
        {
            //if (fileName != "")
            if (!String.IsNullOrEmpty(fileName)
                && File.Exists(fileName)
                )
            {
                FileInfo fInfo = new FileInfo(fileName);
                if (fInfo.IsReadOnly)
                {
                    fileName = "";
                }
            }

            if (fileName == "")
            {
                //fileName = MainController.GetProjectProperties().ProjectDescTitle
                //    + " - " + MainController.GetProjectProperties().ProjectDescSubtitle;
                //fileName = MainController.getDefaultSaveFileNameFinal();

                /*
                if (Settings.Default.OpenDir == "")
                    Settings.Default.OpenDir = ApplicationInfo.UserDocumentsDir;
                */
                SaveFileDialog dlg = new SaveFileDialog();
                dlg.Filter = m_fileFilter;
                dlg.FileName = fileName;
                /*
                dlg.InitialDirectory = Settings.Default.OpenDir;
                */
                //dlg.InitialDirectory = MainController.getDefaultFileDialogueDir();
                dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                //if (dlg.ShowDialog() != DialogResult.OK)
                if (dlg.ShowDialog().GetValueOrDefault(false) != true)
                    return false;

                fileName = dlg.FileName;

                /*
                Settings.Default.OpenDir = Path.GetDirectoryName(fileName);
                */
                MainController.saveCurrentFileDialogueDir(dlg.FileName);
            }

            TextWriter textWriter = null;
            bool result = true;

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                textWriter = new StreamWriter(fileName);
                serializer.Serialize(textWriter, document);
            }
            catch //(Exception ex)
            {
                result = false;
                MyUtils.ShowErrorMessage(String.Format(Strings.Strings._SavingFileError, fileName));
            }
            finally
            {
                textWriter.Close();
            }

            return result;
        }

        /// <summary>
        /// load document from xml file
        /// </summary>
        public static bool Load(ref T document, ref string fileName)
        {
            if (fileName == "")
            {
                /*
                if (Settings.Default.OpenDir == "")
                    Settings.Default.OpenDir = ApplicationInfo.UserDocumentsDir;
                */

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = m_fileFilter;
                //openFileDialog.InitialDirectory = Settings.Default.OpenDir;
                openFileDialog.InitialDirectory = MainController.getDefaultFileDialogueDir();
                //if (openFileDialog.ShowDialog() != DialogResult.OK)
                if (openFileDialog.ShowDialog().GetValueOrDefault(false) != true)
                    return false;

                fileName = openFileDialog.FileName;

                /*
                Settings.Default.OpenDir = Path.GetDirectoryName(fileName);
                */
            }

            FileStream fileStream = null;
            /*
            bool result = true;
            */
            bool result = true;
            int version = 0;
            try
            {
                XDocument xdoc = XDocument.Load(fileName);
                version = int.Parse(xdoc.Root.Attribute(XName.Get("Version")).Value);
            }
            catch //(Exception ex)
            {
                result = false;
            }
            finally
            {
                result = result && PerformPreCheckOnLoad(version);
            }
            T newDoc = null;
            if (result)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                    //document = (T)serializer.Deserialize(fileStream);
                    newDoc = (T)serializer.Deserialize(fileStream);

                }
                catch (Exception ex)
                {
                    MyUtils.ShowErrorMessage(String.Format(Strings.Strings._LoadingFileError, fileName));
                    //MyUtils.ShowErrorMessage(ex.Message);
                    result = false;
                }
                finally
                {
                    if (result)
                    {
                        document = newDoc;
                    }
                    fileStream.Close();
                }
            }

            return result;
        }

        /// <summary>
        /// load document from xmlNode
        /// </summary>
        public static bool Load(ref T document, XmlNode documentNode)
        {
            /*
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Document));
                document = serializer.Deserialize(new XmlNodeReader(documentNode)) as Document;
            }    
            catch //(Exception ex)
            {
                MyUtils.ShowErrorMessage(TranslationManager.Translate("Messages.LoadingFileError_DocumentSerializer"));
                return false;
            }

            return true;
            */
            bool result = true;

            int version = 0;
            try
            {
                version = int.Parse(documentNode.Attributes["Version"].Value);
            }
            catch //(Exception ex)
            {
            }
            finally
            {
                PerformPreCheckOnLoad(version);
            }

            if (result)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    document = serializer.Deserialize(new XmlNodeReader(documentNode)) as T;
                    result = true;
                }
                catch //(Exception ex)
                {
                    MyUtils.ShowErrorMessage(string.Format(Strings.Strings._LoadingFileError_DocumentSerializer, "code: documentNode"));
                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// save document to xmlNode
        /// </summary>
        public static bool Save(T document, ref XmlNode documentNode)
        {

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                using (XmlWriter writer = xmlDocument.CreateNavigator().AppendChild())
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(writer, document);
                }
                documentNode.AppendChild(documentNode.OwnerDocument.ImportNode(xmlDocument.DocumentElement, true));
            }
            catch (Exception)
            {
                MyUtils.ShowErrorMessage(Strings.Strings._SavingFileError_DocumentSerializer);
                return false;
            }

            return true;
        }

        private static bool PerformPreCheckOnLoad(int version)
        {
            bool result = MainController.ckeckDocumentVersionIsOk(version);

            return result;
        }
    }

    // adm
    // see https://www.codeproject.com/Articles/4491/Load-and-save-objects-to-XML-using-serialization
    //[XmlInclude(typeof(RppModuleSnowDataFieldsRaw))]
    [Serializable()]
    public class DataFieldsRawContainer : DataFieldsRawAbst
    {
        #region base

        //[System.Xml.Serialization.XmlIgnore]
        //public AdmSamplePlugin RistekPluginMaster { get; set; }

        public DataFieldsRawContainer()
        {
            //this.PropertyChanged += ThisPropertyChangedListener;

            PlaneAlignementListFull = PlaneAlignementWrapper.getDefaultList();

            // default vals
            m_BeamTrussOffsetLengwiseBottom_mm = 0;
            m_BeamTrussOffsetLengwiseTop_mm = 0;
            m_BeamTrussOutreachPerpPrimary_mm = 0;
            m_BeamTrussOutreachPerpSeccondary_mm = 0;
            m_BeamTrussOffsetPerpendicular_mm = 0;
            m_PlaneAlignement = PlaneAlignementEnum.toAxis;

            // todo tmp?
            // cofnac sie do poprzedniego zapisanewgo projektu? tak chyba prawidlowo bylo powiazane PlaneAlignementWrapperObj, chociaz nadal nie wyxwietlane w combo box
            PlaneAlignementWrapper _PlaneAlignementWrapperTmp = new PlaneAlignementWrapper();
            _PlaneAlignementWrapperTmp.PlaneAlignement = PlaneAlignementEnum.toBottom;
            PlaneAlignementWrapperObj = PlaneAlignementWrapper.getEqualByEnumFromList(_PlaneAlignementWrapperTmp, PlaneAlignementListFull);
        }

        #endregion

        #region fields

        private double m_BeamTrussOffsetLengwiseBottom_mm;
        public double BeamTrussOffsetLengwiseBottom_mm
        {
            get
            {
                return m_BeamTrussOffsetLengwiseBottom_mm
                    ;
            }
            set
            {
                if (this.m_BeamTrussOffsetLengwiseBottom_mm == value) return;
                this.m_BeamTrussOffsetLengwiseBottom_mm = value;
                RaisePropertyChanged(() => BeamTrussOffsetLengwiseBottom_mm);
            }
        }
        // 20240328 Knaga
        internal double getModeAdjusted_BeamTrussOffsetLengwiseBottom_mm()
        {
            if (!this.IsKnagaMode_technicalExternallySettedFlag)
            {
                return this.BeamTrussOffsetLengwiseBottom_mm;
            }
            else
            {
                return -this.BeamTrussOutreachPerpPrimary_mm;
            }
        }

        private double m_BeamTrussOffsetLengwiseTop_mm;
        public double BeamTrussOffsetLengwiseTop_mm
        {
            get
            {
                return m_BeamTrussOffsetLengwiseTop_mm
                    ;
            }
            set
            {
                if (this.m_BeamTrussOffsetLengwiseTop_mm == value) return;
                this.m_BeamTrussOffsetLengwiseTop_mm = value;
                RaisePropertyChanged(() => BeamTrussOffsetLengwiseTop_mm);
            }
        }
        // 20240328 Knaga
        internal double getModeAdjusted_BeamTrussOffsetLengwiseTop_mm()
        {
            if (!this.IsKnagaMode_technicalExternallySettedFlag)
            {
                return this.BeamTrussOffsetLengwiseTop_mm;
            }
            else
            {
                return -this.BeamTrussOutreachPerpSeccondary_mm;
            }
        }

        private double m_BeamTrussOutreachPerpPrimary_mm;
        public double BeamTrussOutreachPerpPrimary_mm
        {
            get
            {
                return m_BeamTrussOutreachPerpPrimary_mm
                    ;
            }
            set
            {
                if (this.m_BeamTrussOutreachPerpPrimary_mm == value) return;
                this.m_BeamTrussOutreachPerpPrimary_mm = value;
                RaisePropertyChanged(() => BeamTrussOutreachPerpPrimary_mm);
            }
        }
        internal double getModeAdjusted_BeamTrussOutreachPerpPrimary_mm()
        {
            if (!this.IsKnagaMode_technicalExternallySettedFlag)
            {
                return this.BeamTrussOutreachPerpPrimary_mm;
            }
            else
            {
                return -this.BeamTrussOffsetLengwiseBottom_mm;
            }
        }

        private double m_BeamTrussOutreachPerpSeccondary_mm;
        public double BeamTrussOutreachPerpSeccondary_mm
        {
            get
            {
                return m_BeamTrussOutreachPerpSeccondary_mm
                    ;
            }
            set
            {
                if (this.m_BeamTrussOutreachPerpSeccondary_mm == value) return;
                this.m_BeamTrussOutreachPerpSeccondary_mm = value;
                RaisePropertyChanged(() => BeamTrussOutreachPerpSeccondary_mm);
            }
        }
        internal double getModeAdjusted_BeamTrussOutreachPerpSeccondary_mm()
        {
            if (!this.IsKnagaMode_technicalExternallySettedFlag)
            {
                return this.BeamTrussOutreachPerpSeccondary_mm;
            }
            else
            {
                return -this.BeamTrussOffsetLengwiseTop_mm;
            }
        }

        private double m_BeamTrussOffsetPerpendicular_mm;
        public double BeamTrussOffsetPerpendicular_mm
        {
            get
            {
                return m_BeamTrussOffsetPerpendicular_mm
                    ;
            }
            set
            {
                if (this.m_BeamTrussOffsetPerpendicular_mm == value) return;
                this.m_BeamTrussOffsetPerpendicular_mm = value;
                RaisePropertyChanged(() => BeamTrussOffsetPerpendicular_mm);
            }
        }

        // see https://stackoverflow.com/questions/6145888/how-to-bind-an-enum-to-a-combobox-control-in-wpf
        private PlaneAlignementEnum m_PlaneAlignement;
        public PlaneAlignementEnum PlaneAlignement
        {
            get { return m_PlaneAlignement; }
            set
            {
                if (this.m_PlaneAlignement == value) return;
                this.m_PlaneAlignement = value;
                RaisePropertyChanged(() => PlaneAlignement);
            }
        }

        // 20240328 Knaga
        // de facto disabling whole concept (always on)
        private bool m_AutoaddSupports = true;
        // autoset in initDataFieldsRaw() / Flag_isRistekPlugin_isKnagaMode
        [System.Xml.Serialization.XmlIgnore]
        public bool AutoaddSupports
        {
            get
            {
                // de facto disabled
                //return m_AutoaddSupports;
                return true;
            }
            set
            {
                if (this.m_AutoaddSupports == value) return;
                this.m_AutoaddSupports = value;
                RaisePropertyChanged(() => AutoaddSupports);
            }
        }

        private bool m_AutosetTrussParamsInEditMode = false;
        public bool AutosetTrussParamsInEditMode
        {
            get { return m_AutosetTrussParamsInEditMode; }
            set
            {
                if (this.m_AutosetTrussParamsInEditMode == value) return;
                this.m_AutosetTrussParamsInEditMode = value;
                RaisePropertyChanged(() => AutosetTrussParamsInEditMode);
            }
        }

        private PlaneAlignementWrapper m_PlaneAlignementWrapperObj = new PlaneAlignementWrapper();
        public PlaneAlignementWrapper PlaneAlignementWrapperObj
        {
            get { return m_PlaneAlignementWrapperObj; }
            set
            {
                if (this.m_PlaneAlignementWrapperObj == value) return;
                if (value != null)
                {
                    this.m_PlaneAlignementWrapperObj = PlaneAlignementWrapper.getEqualByEnumFromList(value, PlaneAlignementListFull);
                    if (this.m_PlaneAlignementWrapperObj != null)
                    {
                        this.m_PlaneAlignementWrapperObj.adjustWrapperToReferenceObj(value);
                    }
                    RaisePropertyChanged(() => PlaneAlignementWrapperObj);
                }
            }
        }

        #endregion

        #region flags

        // 20240328 Knaga
        private bool m_IsKnagaMode_technicalExternallySettedFlag = false;
        [System.Xml.Serialization.XmlIgnore]
        public bool IsKnagaMode_technicalExternallySettedFlag
        {
            get { return m_IsKnagaMode_technicalExternallySettedFlag; }
            set
            {
                if (this.m_IsKnagaMode_technicalExternallySettedFlag == value) return;
                this.m_IsKnagaMode_technicalExternallySettedFlag = value;
                RaisePropertyChanged(() => IsKnagaMode_technicalExternallySettedFlag);
            }
        }

        #endregion

        #region technical fields

        // troche restover z RistekPluginPolska, potrzebny dla RibbonComboBoxWithEditor
        // bo tutaj jest zaimplementowane cale OnPropertyChanged
        private bool m_IsRistekPluginInitializedFinal_technicalExternallySettedFlag = false;
        [System.Xml.Serialization.XmlIgnore]
        public bool IsRistekPluginInitializedFinal_technicalExternallySettedFlag
        {
            get { return m_IsRistekPluginInitializedFinal_technicalExternallySettedFlag; }
            set
            {
                if (this.m_IsRistekPluginInitializedFinal_technicalExternallySettedFlag == value) return;
                this.m_IsRistekPluginInitializedFinal_technicalExternallySettedFlag = value;
                RaisePropertyChanged(() => IsRistekPluginInitializedFinal_technicalExternallySettedFlag);
            }
        }

        #endregion

        #region fields helpers

        [System.Xml.Serialization.XmlIgnore]
        public ReadOnlyObservableCollection<PlaneAlignementWrapper> PlaneAlignementListFull { get; set; }

        #endregion

        #region _implementations generic

        #endregion

        #region property changed listener

        protected override void ThisPropertyChangedListener(object sender, PropertyChangedEventArgs e)
        {
            /*
            if (e.PropertyName == nameof(this.xxx))
            {
                // do stuff
            }
            else if (e.PropertyName == nameof(this.yyy))
            {
                // do stuff
            }
            */
        }

        #endregion

    }

    [Serializable()]
    public class PlaneAlignementWrapper : EditableValEnumWrapper<PlaneAlignementEnum>
    {

        #region base

        private static readonly Dictionary<PlaneAlignementEnum, PlaneAlignementWrapper> m_getDefaultDict;
        private static readonly ReadOnlyObservableCollection<PlaneAlignementWrapper> m_getDefaultList;
        static PlaneAlignementWrapper()
        {
            m_getDefaultDict = Enum.GetValues(typeof(PlaneAlignementEnum)).Cast<PlaneAlignementEnum>().Select(x => new KeyValuePair<PlaneAlignementEnum, PlaneAlignementWrapper>(x, new PlaneAlignementWrapper() { PlaneAlignement = x, StoredEnumDouble = MyUtils.GetMyExtendedInfoAttribute(x).MyDoubleValue })).ToDictionary(t => t.Key, t => t.Value);
            m_getDefaultList = new ReadOnlyObservableCollection<PlaneAlignementWrapper>(new ObservableCollection<PlaneAlignementWrapper>(m_getDefaultDict.Values.ToList()));
        }

        public static ReadOnlyObservableCollection<PlaneAlignementWrapper> getDefaultList()
        {
            if (m_getDefaultList == null)
            {
                //m_getDefaultList = Enum.GetValues(typeof(SnowExposureEnum)).Cast<SnowExposureEnum>().Select(x => new RppModuleSnowDataExposureWrapper() { SnowExposure = x }).ToList();
                throw new NotImplementedException();
            }
            return m_getDefaultList;
        }

        //public static PlaneAlignementWrapper WrapperEnabledForPossibleEdit { get { return m_getDefaultDict[PlaneAlignementEnum._Custom]; } }
        public static PlaneAlignementWrapper WrapperEnabledForPossibleEdit { get { return null; } }

        #endregion

        #region fields

        //public override SnowExposureEnum EnumWithEditableVal_staticForReferenceOnly { get { return WrapperEnabledForPossibleEdit.StoredEnum_forComparison; } }
        //public override PlaneAlignementEnum[] EnumWithEditableVal_staticForReferenceOnlyArr { get { return new PlaneAlignementEnum[] { WrapperEnabledForPossibleEdit.StoredEnum_forComparison }; } }
        public override PlaneAlignementEnum[] EnumWithEditableVal_staticForReferenceOnlyArr { get { return new PlaneAlignementEnum[] { }; } }
        public override PlaneAlignementEnum StoredEnum_forComparison { get { return PlaneAlignement; } }

        private PlaneAlignementEnum m_planeAlignement = PlaneAlignementEnum.toBottom;
        [System.Xml.Serialization.XmlIgnore]
        public PlaneAlignementEnum PlaneAlignement
        {
            get { return m_planeAlignement; }
            set
            {
                m_planeAlignement = value;
                // bo resetowanie wartosci przy deserialize
                if (!m_storedEnumDoubleInitliazed)
                {
                    m_storedEnumDouble = MyUtils.GetMyExtendedInfoAttribute(PlaneAlignement).MyDoubleValue;
                }
            }
        }

        public override PlaneAlignementEnum StoredEnum_forXMLDeserialization
        {
            get
            {
                return PlaneAlignement;
            }
            set
            {
                PlaneAlignement = value;
            }
        }

        #endregion

        #region getters

        #endregion

        #region methods helpers

        public static PlaneAlignementWrapper getEqualByEnumFromList(PlaneAlignementWrapper obj, ICollection<PlaneAlignementWrapper> list)
        {
            return EditableValEnumWrapper<PlaneAlignementEnum>.getEqualByEnumFromList(obj, list.Cast<EditableValEnumWrapper<PlaneAlignementEnum>>().ToList()) as PlaneAlignementWrapper;
        }

        #endregion

        #region _implementations

        protected override void ThisPropertyChangedListener(object sender, PropertyChangedEventArgs e)
        {
            // do nothing
        }

        #endregion

    }

    [System.Diagnostics.DebuggerDisplay("{__DebugDisplayName}")]
    [System.Xml.Serialization.XmlInclude(typeof(PlaneAlignementWrapper))]
    [Serializable()]
    public abstract class EditableValEnumWrapper<T> : DataFieldsRawAbst, ICloneable, IEquatable<EditableValEnumWrapper<T>>
        where T : Enum
    {

        #region base

        #endregion

        #region fields

        public abstract T[] EnumWithEditableVal_staticForReferenceOnlyArr { get; }
        public abstract T StoredEnum_forComparison { get; }

        #endregion

        #region getters

        public string StoredEnumName
        {
            get
            {
                return MyUtils.GetMyExtendedInfoAttribute(StoredEnum_forComparison).Description;
            }
        }

        public string StoredEnumDesc
        {
            get
            {
                return MyUtils.GetMyExtendedInfoAttribute(StoredEnum_forComparison).MyExtendedInfo1;
            }
        }

        public virtual string StoredEnumPresentationFull
        {
            get
            {
                //if (StoredEnum_forComparison.Equals(EnumWithEditableVal_staticForReferenceOnly))
                if (isEnumInEditableVarArr(this.StoredEnum_forComparison))
                {
                    return StoredEnumName;
                }
                else
                {
                    return StoredEnumDoublePresentation + " - " + StoredEnumName;
                }
            }
        }

        protected virtual bool m_storedEnumDoubleDisableValueIncoheranceCheck { get { return false; } }
        protected bool m_storedEnumDoubleInitliazed = false;
        protected double m_storedEnumDouble;
        [XmlElement(Order = 2)]
        public double StoredEnumDouble
        {
            get { return m_storedEnumDouble; }
            set
            {
                if (m_storedEnumDouble != value
                    //&& !this.StoredEnum_forComparison.Equals(this.EnumWithEditableVal_staticForReferenceOnly)
                    && (!isEnumInEditableVarArr(this.StoredEnum_forComparison)
                        && !m_storedEnumDoubleDisableValueIncoheranceCheck
                        )
                    )
                { throw new NotImplementedException(); }

                m_storedEnumDouble = value;
                m_storedEnumDoubleInitliazed = true;
                RaisePropertyChanged(() => StoredEnumDouble);
                RaisePropertyChanged(() => StoredEnumDoublePresentation);
                RaisePropertyChanged(() => StoredEnumPresentationFull);
            }
        }

        [XmlElement(Order = 1)]
        public abstract T StoredEnum_forXMLDeserialization { get; set; }

        public virtual string StoredEnumDoublePresentation
        {
            get
            {
                return MyUtils.pv(StoredEnumDouble, 2);
            }
        }

        #endregion

        #region methods helpers

        public static bool isEqualByEnum(EditableValEnumWrapper<T> obj1, EditableValEnumWrapper<T> obj2)
        {
            return obj1.StoredEnum_forComparison.Equals(obj2.StoredEnum_forComparison);
        }

        public static EditableValEnumWrapper<T> getEqualByEnumFromList(EditableValEnumWrapper<T> obj, ICollection<EditableValEnumWrapper<T>> list)
        {
            foreach (EditableValEnumWrapper<T> _curr in list)
            {
                if (isEqualByEnum(_curr, obj))
                {
                    return _curr;
                }
            }
            return null;
        }
        public void adjustWrapperToReferenceObj(EditableValEnumWrapper<T> objRef)
        {
            if (!isEqualByEnum(this, objRef))
            { throw new NotImplementedException(); }

            this.StoredEnumDouble = objRef.StoredEnumDouble;
        }

        public string __DebugDisplayName
        {
            get
            {
                return this.StoredEnum_forComparison + " ; " + this.StoredEnumDoublePresentation;
            }
        }

        protected bool isEnumInEditableVarArr(T _storedEnum_forComparison)
        {
            return this.EnumWithEditableVal_staticForReferenceOnlyArr.Count(x => x.Equals(_storedEnum_forComparison)) > 0;
        }

        #endregion

        #region _implementations

        public override string ToString()
        {
            //return SnowExposurePresentationFull;
            return StoredEnumDoublePresentation;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool Equals(EditableValEnumWrapper<T> other)
        {
            return isEqualByEnum(this, other);
        }

        protected override void ThisPropertyChangedListener(object sender, PropertyChangedEventArgs e)
        {
            // do nothing
        }

        #endregion

    }

    // see https://www.codeproject.com/Articles/4491/Load-and-save-objects-to-XML-using-serialization
    //[XmlRootAttribute("RistekPluginPoland2_Fields", Namespace = "", IsNullable = false)]
    [System.Xml.Serialization.XmlInclude(typeof(DataFieldsRawContainer))]
    [Serializable()]
    public abstract class DataFieldsRawAbst : MyINotifyPropertyChanged
    {
        #region base

        public DataFieldsRawAbst()
        {
            this.PropertyChanged += ThisPropertyChangedListener;
        }

        #endregion

        #region flags

        #endregion

        #region fields

        #endregion

        #region _implementations generic

        #endregion

        #region property changed listener

        protected abstract void ThisPropertyChangedListener(object sender, PropertyChangedEventArgs e);

        #endregion

    }

    // adm
    public abstract class MyINotifyPropertyChanged : INotifyPropertyChanged
    {
        public MyINotifyPropertyChanged()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpresssion)
        {
            var propertyName = MyUtils.GetPropertyName(propertyExpresssion);
            this.RaisePropertyChanged(propertyName);
        }

        public void FirePropiertyChangeOnAllProperties()
        {
            Type t = this.GetType();
            PropertyInfo[] props = t.GetProperties();
            foreach (PropertyInfo prp in props)
            {
                string propertyName = prp.Name;
                RaisePropertyChanged(propertyName);
            }
        }

        public void RemovePropiertyChangeEvents()
        {
            foreach (Delegate d in PropertyChanged.GetInvocationList())
            {
                PropertyChanged -= (PropertyChangedEventHandler)d;
            }
        }
    }
}
