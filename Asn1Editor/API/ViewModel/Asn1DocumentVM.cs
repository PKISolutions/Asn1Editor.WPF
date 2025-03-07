using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils.ASN;

namespace SysadminsLV.Asn1Editor.API.ViewModel;

public class Asn1DocumentVM : AsyncViewModel {
    String path, fileName;
    Boolean isModified, suppressModified, isSelected;

    public Asn1DocumentVM(NodeViewOptions nodeViewOptions, ITreeCommands treeCommands) {
        DataSource = new DataSource(nodeViewOptions);
        DataSource.CollectionChanged += onDataSourceCollectionChanged;
        TreeCommands = treeCommands;
    }
    void onDataSourceCollectionChanged(Object sender, NotifyCollectionChangedEventArgs args) {
        if (!suppressModified) {
            IsModified = true;
        }
    }

    public IDataSource DataSource { get; }
    public ITreeCommands TreeCommands { get; }
    public NodeViewOptions NodeViewOptions => DataSource.NodeViewOptions;
    public ReadOnlyObservableCollection<Asn1TreeNode> Tree => DataSource.Tree;

    public String Header {
        get {
            String template = fileName ?? "untitled";
            if (IsModified) {
                template += "*";
            }

            return template;
        }
    }
    public String ToolTipText {
        get {
            if (!String.IsNullOrWhiteSpace(Path)) {
                return Path;
            }

            return "untitled";
        }
    }
    public String Path {
        get => path;
        set {
            path = value;
            if (!String.IsNullOrWhiteSpace(path)) {
                fileName = new FileInfo(path).Name;
            }
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(Header));
            OnPropertyChanged(nameof(ToolTipText));
        }
    }
    public Boolean IsModified {
        get => isModified;
        set {
            isModified = value;
            OnPropertyChanged(nameof(IsModified));
            OnPropertyChanged(nameof(Header));
        }
    }
    public Boolean IsSelected {
        get => isSelected;
        set {
            isSelected = value;
            OnPropertyChanged(nameof(IsSelected));
        }
    }

    void decodeFile(IEnumerable<Byte> bytes) {
        if (DataSource.RawData.Count > 0) {
            return;
        }
        DataSource.RawData.AddRange(bytes);
    }

    public async Task Decode(IEnumerable<Byte> bytes, Boolean doNotSetModifiedFlag) {
        IsBusy = true;
        if (doNotSetModifiedFlag) {
            suppressModified = true;
        }

        try {
            decodeFile(bytes);
            Asn1TreeNode rootNode = await AsnTreeBuilder.BuildTreeAsync(DataSource);
            DataSource.SetRootNode(rootNode);
        } finally {
            suppressModified = false;
            IsBusy = false;
        }
    }
    public void Reset() {
        DataSource.Reset();
        Path = String.Empty;
        IsModified = false;
    }
}
