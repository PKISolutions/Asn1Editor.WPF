using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Windows.Input;
using SysadminsLV.Asn1Editor.API.Abstractions;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.WPF.OfficeTheme.Toolkit.Commands;

namespace SysadminsLV.Asn1Editor.API.ViewModel;
class OidEditorVM : ViewModelBase, IOidEditorVM {
    readonly Regex _regex = new(@"^[0-2]\.([0-9]|[12][0-9]|3[0-9])(\.\d+)*$", RegexOptions.Compiled);
    readonly ObservableCollection<OidDto> _oidList = new();

    String oidValue, friendlyName, searchText;
    Boolean? dialogResult;
    OidDto selectedItem;
    OidSearchScope searchScope;

    public OidEditorVM() {
        ReloadCommand = new RelayCommand(reload);
        SaveCommand = new RelayCommand(save, canSave);
        ResetCommand = new RelayCommand(reset);
        CloseCommand = new RelayCommand(_ => DialogResult = true);
        RemoveOidCommand = new RelayCommand(removeOid, canRemoveOid);

        OidView = CollectionViewSource.GetDefaultView(_oidList);
        OidView.SortDescriptions.Add(new SortDescription(nameof(OidDto.Value), ListSortDirection.Ascending));
        OidView.Filter = filterOidList;

        SearchScope = OidSearchScope.UserDefined;
    }
    Boolean filterOidList(Object obj) {
        if (obj is not OidDto entry) {
            return false;
        }

        switch (SearchScope) {
            case OidSearchScope.BuiltIn:
                if (entry.UserDefined) {
                    return false;
                }
                break;
            case OidSearchScope.UserDefined:
                if (!entry.UserDefined) {
                    return false;
                }
                break;
        }
        if (String.IsNullOrEmpty(SearchText)) {
            return true;
        }
        return entry.Value.StartsWith(SearchText)
               || entry.FriendlyName.IndexOf(SearchText, StringComparison.CurrentCultureIgnoreCase) >= 0;
    }

    public ICommand ReloadCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand ResetCommand { get; }
    public ICommand RemoveOidCommand { get; }
    public ICommand CloseCommand { get; }

    public ICollectionView OidView { get; }

    public Boolean? DialogResult {
        get => dialogResult;
        set {
            dialogResult = value;
            OnPropertyChanged(nameof(DialogResult));
        }
    }
    public String OidValue {
        get => oidValue;
        set {
            oidValue = value;
            OnPropertyChanged(nameof(OidValue));
        }
    }
    public String FriendlyName {
        get => friendlyName;
        set {
            friendlyName = value;
            OnPropertyChanged(nameof(FriendlyName));
        }
    }
    public String SearchText {
        get => searchText;
        set {
            searchText = value;
            OnPropertyChanged(nameof(SearchText));
            OidView.Refresh();
        }
    }
    public OidDto SelectedItem {
        get => selectedItem;
        set {
            selectedItem = value;
            OnPropertyChanged(nameof(SelectedItem));
        }
    }
    public OidSearchScope SearchScope {
        get => searchScope;
        set {
            if (searchScope != value) {
                searchScope = value;
                OnPropertyChanged(nameof(SearchScope));
                OidView.Refresh();
            }
        }
    }

    void reload(Object o) {
        _oidList.Clear();
        foreach (OidDto oid in OidResolver.GetOidLookup()) {
            _oidList.Add(oid);
        }
    }
    void save(Object o) {
        OidResolver.Add(OidValue, FriendlyName, true);
        _oidList.Add(new OidDto(OidValue, FriendlyName, true));
        if (OidDbManager.SaveUserLookup()) {
            reset(null);
        }
    }
    Boolean canSave(Object o) {
        return !String.IsNullOrWhiteSpace(OidValue) && !String.IsNullOrWhiteSpace(FriendlyName) && _regex.IsMatch(OidValue);
    }
    void reset(Object o) {
        OidValue = null;
        FriendlyName = null;
    }
    void removeOid(Object o) {
        OidDto backupOid = SelectedItem;
        OidValue = SelectedItem.Value;
        FriendlyName = SelectedItem.FriendlyName;
        _oidList.Remove(SelectedItem);
        OidResolver.Remove(OidValue);
        // fall-back
        if (!OidDbManager.SaveUserLookup()) {
            OidValue = null;
            FriendlyName = null;
            OidResolver.Add(backupOid.Value, backupOid.FriendlyName, backupOid.UserDefined);
        }
    }
    Boolean canRemoveOid(Object o) {
        return SelectedItem != null;
    }
}