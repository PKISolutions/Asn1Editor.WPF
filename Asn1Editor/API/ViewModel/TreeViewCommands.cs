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

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TreeViewCommands : ITreeCommands {
        readonly IWindowFactory _windowFactory;
        readonly IDataSource _data;
        readonly List<Byte> _excludedTags = new List<Byte>(
            new Byte[] { 0, 1, 2, 5, 6, 9, 10, 13 }
        );

        public TreeViewCommands(IWindowFactory windowFactory, IDataSource data) {
            _windowFactory = windowFactory;
            _data = data;
            SaveNodeCommand = new RelayCommand(saveBinaryNode, canExecuteTreeCommands);
            ShowNodeTextViewer = new RelayCommand(showNodeTextViewer, canExecuteTreeCommands);
            EditNodeCommand = new RelayCommand(editNodeContent, canExecuteTreeCommands);
            AddNewNodeCommand = new RelayCommand(addNewNode, canAddNewNode);
            DeleteNodeCommand = new RelayCommand(removeNode, canExecuteTreeCommands);
            CutNodeCommand = new RelayCommand(cutNode, canCut);
            CopyNodeCommand = new RelayCommand(copyNode, canExecuteTreeCommands);
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
            var file = Tools.GetSaveFileName();
            if (String.IsNullOrWhiteSpace(file)) {
                return;
            }
            try {
                File.WriteAllBytes(file, _data.RawData.Skip(_data.SelectedNode.Value.Offset).Take(_data.SelectedNode.Value.TagLength).ToArray());
            } catch (Exception e) {
                Tools.MsgBox("Save Error", e.Message);
            }
        }
        void showNodeTextViewer(Object o) {
            _windowFactory.ShowNodeTextViewer();
        }
        void editNodeContent(Object o) {
            if (_data.SelectedNode == null) {
                return;
            }
            _windowFactory.ShowNodeContentEditor((NodeEditMode)o);
        }
        void addNewNode(Object o) {
            Asn1Lite nodeValue = _windowFactory.ShowNodeContentEditor(NodeEditMode.NewNode);
            if (nodeValue == null) { return; }
            if (_data.Tree.Count == 0) {
                // add new root node
                Asn1TreeNode node = new Asn1TreeNode(nodeValue);
                _data.Tree.Add(node);
                _data.FinishBinaryUpdate();
            } else {
                _data.SelectedNode.AddChild(nodeValue, true);
                _data.FinishBinaryUpdate();
            }
        }
        void removeNode(Object obj) {
            MessageBoxResult response = Tools.MsgBox("Delete", "Do you want to delete the node?\nThis action cannot be undone.", MessageBoxImage.Question, MessageBoxButton.YesNo);
            if (response == MessageBoxResult.Yes) {
                if (_data.SelectedNode.Parent == null) {
                    _data.Tree.Clear();
                    _data.RawData.Clear();
                    _data.SelectedNode = null;
                } else {
                    _data.RawData.RemoveRange(_data.SelectedNode.Value.Offset, _data.SelectedNode.Value.TagLength);
                    _data.SelectedNode.Parent.RemoveChild(_data.SelectedNode);
                }
                _data.FinishBinaryUpdate();
            }
        }
        void cutNode(Object o) {
            copyNode(null);
            _data.RawData.RemoveRange(_data.SelectedNode.Value.Offset, _data.SelectedNode.Value.TagLength);
            _data.SelectedNode.Parent.RemoveChild(_data.SelectedNode);
            _data.FinishBinaryUpdate();
        }
        void copyNode(Object o) {
            ClipboardManager.SetClipboardData(
                _data.RawData
                    .Skip(_data.SelectedNode.Value.Offset)
                    .Take(_data.SelectedNode.Value.TagLength)
            );
            _data.HasClipboardData = true;
        }
        async void pasteBefore(Object o) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardDataAsync();
            _data.RawData.InsertRange(_data.SelectedNode.Value.Offset, ClipboardManager.GetClipboardBytes());
            _data.SelectedNode.Parent.InsertChildNode(
                childNode,
                _data.SelectedNode,
                NodeAddOption.Before
            );
            _data.FinishBinaryUpdate();
        }
        async void pasteAfter(Object o) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardDataAsync();
            var newOffset = _data.SelectedNode.Value.Offset + _data.SelectedNode.Value.TagLength;
            _data.RawData.InsertRange(newOffset, ClipboardManager.GetClipboardBytes());
            _data.SelectedNode.Parent.InsertChildNode(
                childNode,
                _data.SelectedNode,
                NodeAddOption.After
            );
            _data.FinishBinaryUpdate();
        }
        async void pasteLast(Object o) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardDataAsync();
            var newOffset = _data.SelectedNode.Value.Offset + _data.SelectedNode.Value.TagLength;
            _data.RawData.InsertRange(newOffset, ClipboardManager.GetClipboardBytes());
            _data.SelectedNode.InsertChildNode(
                childNode,
                _data.SelectedNode,
                NodeAddOption.Last
            );
            _data.FinishBinaryUpdate();
        }

        Boolean canExecuteTreeCommands(Object o) {
            return _data.SelectedNode != null;
        }
        Boolean canAddNewNode(Object o) {
            return _data.Tree.Count == 0 ||
                   _data.SelectedNode != null &&
                   !_excludedTags.Contains(_data.SelectedNode.Value.Tag);
        }
        Boolean canCut(Object o) {
            return canExecuteTreeCommands(null) && _data.SelectedNode.Parent != null;
        }
        Boolean canPasteBeforeAfter(Object obj) {
            return _data.HasClipboardData && canCut(null);
        }
        Boolean canPasteLast(Object obj) {
            return _data.HasClipboardData &&
                   !_excludedTags.Contains(_data.SelectedNode.Value.Tag) &&
                   String.IsNullOrEmpty(_data.SelectedNode.Value.ExplicitValue);
        }
    }
}
