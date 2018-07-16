using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.Views.Windows;
using SysadminsLV.Asn1Parser;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;
using Unity;

namespace SysadminsLV.Asn1Editor.API.ViewModel {
    class TreeViewCommands {
        readonly MainWindowVM _parentVM;
        readonly List<Byte> _excludedTags = new List<Byte>(
            new Byte[] { 0, 1, 2, 5, 6, 9, 10, 13 }
        );

        public TreeViewCommands(MainWindowVM Parent) {
            _parentVM = Parent;
            SaveNodeCommand = new RelayCommand(saveNode, canContextMenu);
            EditNodeCommand = new RelayCommand(editNode, canContextMenu);
            ShowTextCommand = new RelayCommand(StaticCommands.ShowText, canContextMenu);
            NewNodeCommand = new RelayCommand(newNode, canNew);
            CutNodeCommand = new RelayCommand(cutNode, canRemove);
            CopyNodeCommand = new RelayCommand(copyNode, canContextMenu);
            PasteBeforeCommand = new RelayCommand(pasteBefore, canPasteAfterBefore);
            PasteAfterCommand = new RelayCommand(pasteAfter, canPasteAfterBefore);
            PasteLastCommand = new RelayCommand(pasteLast, canPasteLast);
            DeleteNodeCommand = new RelayCommand(removeNode, canRemove);
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

        void editNode(Object obj) {
            TagDataEditor dlg = (String)obj == "hex"
                ? new TagDataEditor(_parentVM.SelectedTreeNode.Value, true)
                : new TagDataEditor(_parentVM.SelectedTreeNode.Value);
            dlg.ShowDialog();
        }
        void newNode(Object obj) {
            var data = App.Container.Resolve<IDataSource>();
            Asn1Lite asn = new Asn1Lite(new Asn1Reader(new Byte[] { 48, 0 })) { IsContainer = true };
            TagDataEditor dlg = new TagDataEditor(asn);
            ((TagDataEditorVM)dlg.DataContext).TagIsReadOnly = false;
            dlg.ShowDialog();
            if (!((TagDataEditorVM)dlg.DataContext).Accepted) { return; }
            if (obj == null) {
                Asn1TreeNode node = new Asn1TreeNode(asn);
                data.Tree.Add(node);
                data.RawData.AddRange(new Byte[] { 48, 0 });
                data.FinishBinaryUpdate();
            } else {
                asn = new Asn1Lite(new Asn1Reader(new Byte[] { ((TagDataEditorVM)dlg.DataContext).Tag, 0 })) {
                    Offset =
                        _parentVM.SelectedTreeNode
                        .Value.Offset +
                        _parentVM.SelectedTreeNode
                        .Value.TagLength
                };
                asn.PayloadStartOffset = asn.Offset + 2;
                data.RawData.Insert(_parentVM.SelectedTreeNode.Value.Offset + _parentVM.SelectedTreeNode.Value.TagLength, 0);
                data.RawData.Insert(_parentVM.SelectedTreeNode.Value.Offset + _parentVM.SelectedTreeNode.Value.TagLength, asn.Tag);
                asn.IsRoot = false;
                asn.Deepness += _parentVM.SelectedTreeNode.Value.Deepness;
                _parentVM.SelectedTreeNode.AddChild(asn, true);
                data.FinishBinaryUpdate();
            }
        }
        void cutNode(Object obj) {
            ClipboardManager.SetClipboardData(
                App.Container.Resolve<IDataSource>().RawData
                .Skip(((Asn1TreeNode)obj).Value.Offset)
                .Take(((Asn1TreeNode)obj).Value.TagLength)
            );
            _parentVM.HasClipboardData = true;
            ((Asn1TreeNode)obj).Parent.RemoveChild((Asn1TreeNode)obj);
        }
        void copyNode(Object obj) {
            ClipboardManager.SetClipboardData(
                App.Container.Resolve<IDataSource>().RawData
                .Skip(((Asn1TreeNode)obj).Value.Offset)
                .Take(((Asn1TreeNode)obj).Value.TagLength)
            );
            _parentVM.HasClipboardData = true;
        }
        static async void pasteBefore(Object obj) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardData();
            ((Asn1TreeNode)obj).Parent.InsertChildNode(
                childNode,
                (Asn1TreeNode)obj,
                NodeAddOption.Before
            );
        }
        static async void pasteAfter(Object obj) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardData();
            ((Asn1TreeNode)obj).Parent.InsertChildNode(
                childNode,
                (Asn1TreeNode)obj,
                NodeAddOption.After
            );
        }
        static async void pasteLast(Object obj) {
            Asn1TreeNode childNode = await ClipboardManager.GetClipboardData();
            ((Asn1TreeNode)obj).InsertChildNode(
                childNode,
                (Asn1TreeNode)obj,
                NodeAddOption.Last
            );
        }
        static void saveNode(Object obj) {
            SaveFileDialog dlg = new SaveFileDialog {
                FileName = "",
                DefaultExt = ".*",
                Filter = "All files (*.*)|*.*"
            };
            Boolean? result = dlg.ShowDialog();
            if (result == true) {
                Asn1Lite node = (Asn1Lite)obj;
                try {
                    File.WriteAllBytes(dlg.FileName, App.Container.Resolve<IDataSource>().RawData.Skip(node.Offset).Take(node.TagLength).ToArray());
                } catch (Exception e) {
                    Tools.MsgBox("Save Error", e.Message);
                }
            }
        }
        static void removeNode(Object obj) {
            MessageBoxResult response = Tools.MsgBox("Delete", "Do you want to delete the node?\nThis action cannot be undone.", MessageBoxImage.Question, MessageBoxButton.YesNo);
            if (response == MessageBoxResult.Yes) {
                ((Asn1TreeNode)obj).Parent.RemoveChild((Asn1TreeNode)obj);
            }
        }

        Boolean canNew(Object obj) {
            return _parentVM.Tree.Count == 0 ||
                   _parentVM.SelectedTreeNode != null &&
                   !_excludedTags.Contains(_parentVM.SelectedTreeNode.Value.Tag);
        }
        Boolean canRemove(Object obj) {
            if (obj == null) { return false; }
            return ((Asn1TreeNode)obj).Parent != null && canContextMenu(null);
        }
        Boolean canPasteAfterBefore(Object obj) {
            return _parentVM.HasClipboardData &&
                _parentVM.SelectedTreeNode != null &&
                _parentVM.SelectedTreeNode.Parent != null;
        }
        Boolean canPasteLast(Object obj) {
            return obj != null &&
                _parentVM.HasClipboardData &&
                !_excludedTags.Contains(((Asn1TreeNode)obj).Value.Tag) &&
                String.IsNullOrEmpty(((Asn1TreeNode)obj).Value.ExplicitValue);
        }
        Boolean canContextMenu(Object obj) {
            return _parentVM.SelectedTreeNode != null;
        }
    }
}
