using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.Properties;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TextViewerVM : ViewModelBase, ITextViewerVM {
        readonly Asn1TreeNode rootNode;
        const String master = "123";
        const String delimieter = "      |      |       |";
        const Int32 minLength = 60;
        const Int32 defaultLength = 80;
        const Int32 maxLength = 400;
        Boolean? dialogResult;
        String n = Environment.NewLine;
        String text;
        Int32 currentLength = 80;
        String currentLengthStr = "80";
        Double width;

        public TextViewerVM(IDataSource data) {
            rootNode = data.SelectedNode;
            CurrentLength = defaultLength.ToString(CultureInfo.InvariantCulture);
            SaveCommand = new RelayCommand(saveFile);
            PrintCommand = new RelayCommand(print);
            ApplyCommand = new RelayCommand(applyNewLength);
            CloseCommand = new RelayCommand(close);
            TextBoxWidth = Tools.MeasureString(master, Settings.Default.FontSize, false);
            generateTable();
        }

        public ICommand SaveCommand { get; set; }
        public ICommand PrintCommand { get; set; }
        public ICommand ApplyCommand { get; set; }
        public ICommand CloseCommand { get; }

        public String Text {
            get => text;
            set {
                text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        public String CurrentLength {
            get => currentLengthStr;
            set {
                currentLengthStr = value;
                OnPropertyChanged(nameof(CurrentLength));
            }
        }
        public Double TextBoxWidth {
            get => width;
            set {
                width = value;
                OnPropertyChanged(nameof(TextBoxWidth));
            }
        }
        public Boolean? DialogResult {
            get => dialogResult;
            set {
                dialogResult = value;
                OnPropertyChanged(nameof(DialogResult));
            }
        }

        void close(Object o) {
            DialogResult = true;
        }
        void print(Object obj) {
            StaticCommands.Print(Text);
        }
        void applyNewLength(Object obj) {
            if (!Int32.TryParse(CurrentLength, NumberStyles.Integer, null, out Int32 value)) {
                CurrentLength = currentLength.ToString(CultureInfo.InvariantCulture);
                return;
            }
            if (value == currentLength) { return; }
            currentLength = value < minLength || value > maxLength
                ? minLength
                : value;
            CurrentLength = currentLength.ToString(CultureInfo.InvariantCulture);
            generateTable();
        }
        void generateTable() {
            if (rootNode == null) { return; }
            StringBuilder SB = new StringBuilder("Offset|Length|LenByte|" + n);
            SB.Append("======+======+=======+" + new String('=', currentLength + 10) + n);
            foreach (Asn1Lite node in rootNode.Flatten()) {
                String padding = new String(' ', (node.Deepness - rootNode.Value.Deepness + 1) * 3);
                String str = String.Format("{0,6}|{1,6}|{2,7}|{3}{4} : ",
                    node.Offset,
                    node.PayloadLength,
                    node.HeaderLength - 1,
                    padding,
                    node.TagName);
                SB.Append(str + calculateValue(node, padding.Length));
            }
            Text = SB.ToString();
        }
        String calculateValue(Asn1Lite node, Int32 padding) {
            if (String.IsNullOrEmpty(node.ExplicitValue)) { return n; }
            if (24 + padding + node.ExplicitValue.Length <= currentLength) {
                return $"'{node.ExplicitValue.Trim()}'{n}";
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
                    current + $"{delimieter}{new String(' ', padding + 3)}{line.Trim()}{n}");
        }
        void saveFile(Object obj) {
            String path = Tools.GetSaveFileName();
            if (String.IsNullOrEmpty(path)) { return; }
            try {
                File.WriteAllText(path, Text);
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }
    }
}
