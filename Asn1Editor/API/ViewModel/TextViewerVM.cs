using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Microsoft.Win32;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TextViewerVM : INotifyPropertyChanged {
        Asn1TreeNode rootNode;
        const String master = "123";
        const String delimieter = "      |      |       |";
        const Int32 minLength = 60;
        const Int32 defaultLength = 80;
        const Int32 maxLength = 400;
        String n = Environment.NewLine;
        String text;
        Int32 currentLength = 80;
        String currentLengthStr = "80";
        Double width;

        public TextViewerVM() {
            CurrentLength = defaultLength.ToString(CultureInfo.InvariantCulture);
            SaveCommand = new RelayCommand(SaveFile);
            PrintCommand = new RelayCommand(Print);
            ApplyCommand = new RelayCommand(applyNewLength);
            TextBoxWidth = Tools.MeasureString(master, Settings.Default.FontSize, false);
        }

        public ICommand SaveCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand ApplyCommand { get; set; }

        public String Text {
            get => text;
            set {
                text = value;
                OnPropertyChanged("Text");
            }
        }
        public String CurrentLength {
            get => currentLengthStr;
            set {
                currentLengthStr = value;
                OnPropertyChanged("CurrentLength");
            }
        }
        public Double TextBoxWidth {
            get => width;
            set {
                width = value;
                OnPropertyChanged("TextBoxWidth");
            }
        }

        void Print(Object obj) {
            StaticCommands.Print(Text);
        }
        void applyNewLength(Object obj) {
            Int32 value;
            if (!Int32.TryParse(CurrentLength, NumberStyles.Integer, null, out value)) {
                CurrentLength = currentLength.ToString(CultureInfo.InvariantCulture);
                return;
            }
            if (value == currentLength) { return; }
            currentLength = value < minLength || value > maxLength
                ? minLength
                : value;
            CurrentLength = currentLength.ToString(CultureInfo.InvariantCulture);
            GenerateTable();
        }
        void GenerateTable() {
            if (rootNode == null) { return; }
            StringBuilder SB = new StringBuilder("Offset|Length|LenByte|" + n);
            SB.Append("======+======+=======+" + new String('=', currentLength + 10) + n);
            foreach (Asn1Lite node in rootNode.Flatten()) {
                String padding = new String(' ', (node.Deepness - rootNode.Value.Deepness + 1) * 3);
                var str = String.Format("{0,6}|{1,6}|{2,7}|{3}{4} : ",
                    node.Offset,
                    node.PayloadLength,
                    node.HeaderLength - 1,
                    padding,
                    node.TagName
                    );
                SB.Append(str + CalculateValue(node, str.Length, padding.Length));
            }
            Text = SB.ToString();
        }
        String CalculateValue(Asn1Lite node, Int32 lineLength, Int32 padding) {
            if (String.IsNullOrEmpty(node.ExplicitValue)) { return n; }
            if (24 + padding + node.ExplicitValue.Length <= currentLength) {
                return "'" + node.ExplicitValue.Trim() + "'" + n;
            }
            Int32 remaining = currentLength - 22 - padding;
            if (node.ExplicitValue.Length <= remaining - 2) {
                return String.Format(
                    "{0}{1}{4}{2}{3}{2}{0}",
                    n,
                    delimieter,
                    "'",
                    node.ExplicitValue.Trim(),
                    new String(' ', padding + 3)
                    );
            }
            return Utils.Extensions.StringExtensions
                .SplitByLength(node.ExplicitValue, currentLength - padding)
                .Aggregate(n, (current, line) => 
                    current + String.Format("{0}{1}{2}{3}", delimieter, new String(' ', padding + 3), line.Trim(), n));
        }
        void SaveFile(Object obj) {
            String path = GetFilePath();
            if (String.IsNullOrEmpty(path)) { return; }
            try {
                File.WriteAllText(path, Text);
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }

        static String GetFilePath() {
            SaveFileDialog dlg = new SaveFileDialog {
                FileName = "",
                DefaultExt = ".*",
                Filter = "All files (*.*)|*.*"
            };
            Boolean? result = dlg.ShowDialog();
            return result != true
                ? String.Empty
                : dlg.FileName;
        }
        public void SetBinding(Asn1TreeNode node) {
            rootNode = node;
            GenerateTable();
        }
        void OnPropertyChanged(String PropertyName) {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) {
                handler(this, new PropertyChangedEventArgs(PropertyName));
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
