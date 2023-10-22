using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils.ASN;

namespace SysadminsLV.Asn1Editor.API.ViewModel;

public class Asn1DocumentVM : AsyncViewModel {
    String path, fileName;
    Asn1TreeNode selectedNode;
    Boolean hasClipboard, isModified;

    public Asn1DocumentVM(NodeViewOptions nodeViewOptions) {
        DataSource = new DataSource(nodeViewOptions);
        DataSource.CollectionChanged += (_, _) => IsModified = true;
    }

    public IDataSource DataSource { get; }
    public NodeViewOptions NodeViewOptions => DataSource.NodeViewOptions;
    public ObservableCollection<Asn1TreeNode> Tree => DataSource.Tree;

    public String Header {
        get {
            String template = fileName ?? "untitled";
            if (IsModified) {
                template += "*";
            }

            return template;
        }
    }
    public String Path {
        get => path;
        set {
            path = value;
            if (path != null) {
                fileName = new FileInfo(path).Name;
            }
            OnPropertyChanged(nameof(Path));
            OnPropertyChanged(nameof(Header));
        }
    }
    public Boolean IsModified {
        get => isModified;
        set {
            isModified = value;
            OnPropertyChanged(nameof(IsModified));
        }
    }

    public Boolean HasClipboardData {
        get => hasClipboard;
        set {
            hasClipboard = value;
            OnPropertyChanged(nameof(HasClipboardData));
        }
    }
    public Asn1TreeNode SelectedTreeNode {
        get => selectedNode;
        set {
            selectedNode = value;
            OnPropertyChanged(nameof(SelectedTreeNode));
        }
    }

    void decodeFile(IEnumerable<Byte> bytes) {
        if (DataSource.RawData.Count > 0) {
            return;
        }
        DataSource.RawData.AddRange(bytes);
    }

    public async Task Decode(IEnumerable<Byte> bytes) {
        IsBusy = true;

        decodeFile(bytes);
        Asn1TreeNode rootNode = await AsnTreeBuilder.BuildTree(DataSource.RawData.ToArray());
        Tree.Add(rootNode);
        DataSource.FinishBinaryUpdate();

        IsBusy = false;
    }
    public void Reset() {
        DataSource.Tree.Clear();
        DataSource.SelectedNode = null;
        DataSource.RawData.Clear();
        Path = String.Empty;
        IsModified = false;
    }
}
