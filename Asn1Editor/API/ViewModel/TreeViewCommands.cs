using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel;

class TreeViewCommands : ITreeCommands {
    readonly IWindowFactory _windowFactory;
    readonly IHasSelectedTab _tabs;
    readonly List<Byte> _excludedTags = new(
        new Byte[] { 0, 1, 2, 5, 6, 9, 10, 13 }
    );

    public TreeViewCommands(IWindowFactory windowFactory, IHasSelectedTab appTabs) {
        _windowFactory = windowFactory;
        _tabs = appTabs;
        SaveNodeCommand = new RelayCommand(saveBinaryNode, ensureNodeSelected);
        ShowNodeTextViewer = new RelayCommand(showNodeTextViewer, ensureNodeSelected);
        EditNodeCommand = new RelayCommand(editNodeContent, ensureNodeSelected);
        AddNewNodeCommand = new RelayCommand(addNewNode, canAddNewNode);
        DeleteNodeCommand = new RelayCommand(removeNode, ensureNodeSelected);
        CutNodeCommand = new RelayCommand(cutNode, canCutNode);
        CopyNodeCommand = new RelayCommand(copyNode, ensureNodeSelected);
        PasteBeforeCommand = new RelayCommand(pasteBefore, canPasteBeforeAfter);
        PasteAfterCommand = new RelayCommand(pasteAfter, canPasteBeforeAfter);
        PasteLastCommand = new RelayCommand(pasteLast, canPasteLast);
    }

    public ICommand ShowNodeTextViewer { get; }
    public ICommand EditNodeCommand { get; }
    public ICommand SaveNodeCommand { get; }
    public ICommand AddNewNodeCommand { get; }
    public ICommand DeleteNodeCommand { get; }
    public ICommand CutNodeCommand { get; }
    public ICommand CopyNodeCommand { get; }
    public ICommand PasteBeforeCommand { get; }
    public ICommand PasteAfterCommand { get; }
    public ICommand PasteLastCommand { get; }

    void saveBinaryNode(Object o) {
        if (!Tools.TryGetSaveFileName(out String filePath)) {
            return;
        }
        isTabSelected(out IDataSource data); // granted to be non-null
        try {
            File.WriteAllBytes(filePath, data.RawData.Skip(data.SelectedNode.Value.Offset).Take(data.SelectedNode.Value.TagLength).ToArray());
        } catch (Exception e) {
            Tools.MsgBox("Save Error", e.Message);
        }
    }
    void showNodeTextViewer(Object o) {
        _windowFactory.ShowNodeTextViewer();
    }
    void editNodeContent(Object o) {
        isTabSelected(out IDataSource data); // granted to be non-null
        if (data.SelectedNode != null) {
            _windowFactory.ShowNodeContentEditor((NodeEditMode)o);
        }
    }
    void addNewNode(Object o) {
        Asn1Lite nodeValue = _windowFactory.ShowNodeContentEditor(NodeEditMode.NewNode);
        if (nodeValue == null) { return; }
        isTabSelected(out IDataSource data); // granted to be non-null
        if (data.Tree.Count == 0) {
            // add new root node
            var node = new Asn1TreeNode(nodeValue, data);
            data.Tree.Add(node);
            data.FinishBinaryUpdate();
        } else {
            data.SelectedNode.AddChild(nodeValue, true);
            data.FinishBinaryUpdate();
        }
    }
    void removeNode(Object o) {
        isTabSelected(out IDataSource data); // granted to be non-null
        MessageBoxResult response = Tools.MsgBox("Delete", "Do you want to delete the node?\nThis action cannot be undone.", MessageBoxImage.Question, MessageBoxButton.YesNo);
        if (response == MessageBoxResult.Yes) {
            if (data.SelectedNode.Parent == null) {
                data.Tree.Clear();
                data.RawData.Clear();
                data.SelectedNode = null;
            } else {
                data.RawData.RemoveRange(data.SelectedNode.Value.Offset, data.SelectedNode.Value.TagLength);
                data.SelectedNode.Parent.RemoveChild(data.SelectedNode);
            }
            data.FinishBinaryUpdate();
        }
    }
    void cutNode(Object o) {
        isTabSelected(out IDataSource data); // granted to be non-null
        copyNodePrivate(data);
        data.RawData.RemoveRange(data.SelectedNode.Value.Offset, data.SelectedNode.Value.TagLength);
        data.SelectedNode.Parent.RemoveChild(data.SelectedNode);
        data.FinishBinaryUpdate();
    }
    void copyNode(Object o) {
        isTabSelected(out IDataSource data); // granted to be non-null
        copyNodePrivate(data);
    }
    void copyNodePrivate(IDataSource data) {
        ClipboardManager.SetClipboardData(
        data.RawData
                .Skip(data.SelectedNode.Value.Offset)
                .Take(data.SelectedNode.Value.TagLength)
        );
        data.HasClipboardData = true;
    }
    async void pasteBefore(Object o) {
        isTabSelected(out IDataSource data); // granted to be non-null
        Asn1TreeNode childNode = await ClipboardManager.GetClipboardDataAsync(data);
        data.RawData.InsertRange(data.SelectedNode.Value.Offset, ClipboardManager.GetClipboardBytes());
        data.SelectedNode.Parent.InsertChildNode(
            childNode,
            data.SelectedNode,
            NodeAddOption.Before
        );
        data.FinishBinaryUpdate();
    }
    async void pasteAfter(Object o) {
        isTabSelected(out IDataSource data); // granted to be non-null
        Asn1TreeNode childNode = await ClipboardManager.GetClipboardDataAsync(data);
        Int32 newOffset = data.SelectedNode.Value.Offset + data.SelectedNode.Value.TagLength;
        data.RawData.InsertRange(newOffset, ClipboardManager.GetClipboardBytes());
        data.SelectedNode.Parent.InsertChildNode(
            childNode,
            data.SelectedNode,
            NodeAddOption.After
        );
        data.FinishBinaryUpdate();
    }
    async void pasteLast(Object o) {
        isTabSelected(out IDataSource data); // granted to be non-null
        Asn1TreeNode childNode = await ClipboardManager.GetClipboardDataAsync(data);
        Int32 newOffset = data.SelectedNode.Value.Offset + data.SelectedNode.Value.TagLength;
        data.RawData.InsertRange(newOffset, ClipboardManager.GetClipboardBytes());
        data.SelectedNode.InsertChildNode(
            childNode,
            data.SelectedNode,
            NodeAddOption.Last
        );
        data.FinishBinaryUpdate();
    }

    Boolean ensureNodeSelected(Object o) {
        return isTabSelected(out IDataSource data) && data.SelectedNode != null;
    }
    Boolean canAddNewNode(Object o) {
        return isTabSelected(out IDataSource data)
               && (data.Tree.Count == 0 || (data.SelectedNode != null && !_excludedTags.Contains(data.SelectedNode.Value.Tag)));
    }
    Boolean canCutNode(Object o) {
        return isTabSelected(out IDataSource data)
               && data.SelectedNode is { Parent: not null };
    }
    Boolean canPasteBeforeAfter(Object o) {
        return isTabSelected(out IDataSource data) && data.HasClipboardData && canCutNode(null);
    }
    Boolean canPasteLast(Object o) {
        return isTabSelected(out IDataSource data) && data.HasClipboardData &&
               !_excludedTags.Contains(data.SelectedNode.Value.Tag) &&
               String.IsNullOrEmpty(data.SelectedNode.Value.ExplicitValue);
    }
    Boolean isTabSelected(out IDataSource dataSource) {
        dataSource = null;
        if (_tabs.SelectedTab != null) {
            dataSource = _tabs.SelectedTab.DataSource;

            return true;
        }

        return false;
    }
}