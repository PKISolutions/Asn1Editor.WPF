using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Asn1Editor.API.ModelObjects;
using Asn1Editor.API.Utils;
using Asn1Editor.Views.Windows;
using Microsoft.Win32;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace Asn1Editor.API.ViewModel {
    class TreeViewCommands {
        readonly MainWindowVM _parentVM;
        readonly List<Byte> _excludedTags = new List<Byte>(
            new Byte[] { 0, 1, 2, 5, 6, 9, 10, 13 }
        );

        public TreeViewCommands(MainWindowVM Parent) {
            _parentVM = Parent;
            SaveNodeCommand = new RelayCommand(SaveNode, CanContextMenu);
            EditNodeCommand = new RelayCommand(EditNode, CanContextMenu);
            ShowTextCommand = new RelayCommand(StaticCommands.ShowText, CanContextMenu);
            NewNodeCommand = new RelayCommand(NewNode, CanNew);
            CutNodeCommand = new RelayCommand(CutNode, CanRemove);
            CopyNodeCommand = new RelayCommand(CopyNode, CanContextMenu);
            PasteBeforeCommand = new RelayCommand(PasteBefore, CanPasteAfterBefore);
            PasteAfterCommand = new RelayCommand(PasteAfter, CanPasteAfterBefore);
            PasteLastCommand = new RelayCommand(PasteLast, CanPasteLast);
            DeleteNodeCommand = new RelayCommand(RemoveNode, CanRemove);
        }

        public ICommand EditNodeCommand { get; set; }
        public ICommand SaveNodeCommand { get; set; }
        public ICommand ShowTextCommand { get; set; }
        public ICommand NewNodeCommand { get; set; }
        public ICommand DeleteNodeCommand { get; set; }
        public ICommand CutNodeCommand { get; set; }
        public ICommand CopyNodeCommand { get; set; }
        public ICommand PasteBeforeCommand { get; set; }
        public ICommand PasteAfterCommand { get; set; }
        public ICommand PasteLastCommand { get; set; }

        void EditNode(Object obj) {
            TagDataEditor dlg = (String)obj == "hex"
                ? new TagDataEditor(_parentVM.SelectedTreeNode.Value, true)
                : new TagDataEditor(_parentVM.SelectedTreeNode.Value);
            dlg.ShowDialog();
        }
        void NewNode(Object obj) {
            Asn1Lite asn = new Asn1Lite(new Asn1Reader(new Byte[] { 48, 0 })) { IsContainer = true };
            TagDataEditor dlg = new TagDataEditor(asn);
            ((TagDataEditorVM)dlg.DataContext).TagIsReadOnly = false;
            dlg.ShowDialog();
            if (!((TagDataEditorVM)dlg.DataContext).Accepted) { return; }
            if (obj == null) {
                Asn1TreeNode node = new Asn1TreeNode(asn);
                _parentVM.Tree.Add(node);
                MainWindowVM.RawData.AddRange(new Byte[] { 48, 0 });
                _parentVM.HexViewerContext.BuildHexView(null);
            } else {
                asn = new Asn1Lite(new Asn1Reader(new Byte[] { ((TagDataEditorVM)dlg.DataContext).Tag, 0 })) {
                    Offset =
                        _parentVM.SelectedTreeNode
                        .Value.Offset +
                        _parentVM.SelectedTreeNode
                        .Value.TagLength
                };
                asn.PayloadStartOffset = asn.Offset + 2;
                MainWindowVM.RawData.Insert(_parentVM.SelectedTreeNode.Value.Offset + _parentVM.SelectedTreeNode.Value.TagLength, 0);
                MainWindowVM.RawData.Insert(_parentVM.SelectedTreeNode.Value.Offset + _parentVM.SelectedTreeNode.Value.TagLength, asn.Tag);
                asn.IsRoot = false;
                asn.Deepness += _parentVM.SelectedTreeNode.Value.Deepness;
                _parentVM.SelectedTreeNode.AddChild(asn, true);
                _parentVM.HexViewerContext.BuildHexView(null);
            }
        }
        void CutNode(Object obj) {
            ClipboardManager.SetClipboardData(
                MainWindowVM.RawData
                .Skip(((Asn1TreeNode)obj).Value.Offset)
                .Take(((Asn1TreeNode)obj).Value.TagLength)
            );
            _parentVM.HasClipboardData = true;
            ((Asn1TreeNode)obj).Parent.RemoveChild((Asn1TreeNode)obj);
        }
        void CopyNode(Object obj) {
            ClipboardManager.SetClipboardData(
                MainWindowVM.RawData
                .Skip(((Asn1TreeNode)obj).Value.Offset)
                .Take(((Asn1TreeNode)obj).Value.TagLength)
            );
            _parentVM.HasClipboardData = true;
        }
        static async void PasteBefore(Object obj) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardData();
            ((Asn1TreeNode)obj).Parent.InsertChildNode(
                childNode,
                (Asn1TreeNode)obj,
                NodeAddOption.Before
            );
        }
        static async void PasteAfter(Object obj) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardData();
            ((Asn1TreeNode)obj).Parent.InsertChildNode(
                childNode,
                (Asn1TreeNode)obj,
                NodeAddOption.After
            );
        }
        static async void PasteLast(Object obj) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardData();
            ((Asn1TreeNode)obj).InsertChildNode(
                childNode,
                (Asn1TreeNode)obj,
                NodeAddOption.Last
            );
        }
        static void SaveNode(Object obj) {
            SaveFileDialog dlg = new SaveFileDialog {
                FileName = "",
                DefaultExt = ".*",
                Filter = "All files (*.*)|*.*"
            };
            Boolean? result = dlg.ShowDialog();
            if (result == true) {
                Asn1Lite node = (Asn1Lite)obj;
                try {
                    File.WriteAllBytes(dlg.FileName, MainWindowVM.RawData.Skip(node.Offset).Take(node.TagLength).ToArray());
                } catch (Exception e) {
                    Tools.MsgBox("Save Error", e.Message);
                }
            }
        }
        static void RemoveNode(Object obj) {
            MessageBoxResult response = Tools.MsgBox("Delete", "Do you want to delete the node?\nThis action cannot be undone.", MessageBoxImage.Question, MessageBoxButton.YesNo);
            if (response == MessageBoxResult.Yes) {
                ((Asn1TreeNode)obj).Parent.RemoveChild((Asn1TreeNode)obj);
            }
        }

        Boolean CanNew(Object obj) {
            return _parentVM.Tree.Count == 0 ||
                   _parentVM.SelectedTreeNode != null &&
                   !_excludedTags.Contains(_parentVM.SelectedTreeNode.Value.Tag);
        }
        Boolean CanRemove(Object obj) {
            if (obj == null) { return false; }
            return ((Asn1TreeNode)obj).Parent != null && CanContextMenu(null);
        }
        Boolean CanPasteAfterBefore(Object obj) {
            return _parentVM.HasClipboardData &&
                _parentVM.SelectedTreeNode != null &&
                _parentVM.SelectedTreeNode.Parent != null;
        }
        Boolean CanPasteLast(Object obj) {
            return obj != null &&
                _parentVM.HasClipboardData &&
                !_excludedTags.Contains(((Asn1TreeNode)obj).Value.Tag) &&
                String.IsNullOrEmpty(((Asn1TreeNode)obj).Value.ExplicitValue);
        }
        Boolean CanContextMenu(Object obj) {
            return _parentVM.SelectedTreeNode != null;
        }
    }
}
