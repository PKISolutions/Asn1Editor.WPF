using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using SysadminsLV.Asn1Editor.API;
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.WPF;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Views.Windows;
using Unity;
using Path = System.IO.Path;

namespace SysadminsLV.Asn1Editor;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
    readonly String _basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Sysadmins LV\Asn1Editor");
    static readonly Logger _logger = new();
    readonly NodeViewOptions _options;

    public App() {
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        _options = readSettings();
        _options.PropertyChanged += onOptionsChanged;
    }

    public static IUnityContainer Container { get; private set; }

    void OnDispatcherUnhandledException(Object s, DispatcherUnhandledExceptionEventArgs e) {
        _logger.Write(e.Exception);
    }
    public static void Write(Exception e) {
        _logger.Write(e);
    }
    public static void Write(String s) {
        _logger.Write(s);
    }
    protected override void OnStartup(StartupEventArgs e) {
        _logger.Write("******************************** Started ********************************");
        _logger.Write($"Process: {Process.GetCurrentProcess().ProcessName}");
        _logger.Write($"PID    : {Process.GetCurrentProcess().Id}");
        _logger.Write($"Version: {Assembly.GetExecutingAssembly().GetName().Version}");
        configureUnity();
        readOids();
        parseArguments(e.Args);
        base.OnStartup(e);
        Container.Resolve<MainWindow>().Show();
    }
    protected override void OnExit(ExitEventArgs e) {
        _logger.Dispose();
        base.OnExit(e);
    }
    void readOids() {
        const String oidFileName = "OID.txt";
        String appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        foreach (String oidFolderLocation in new[] { appPath, _basePath }) {
            String filePath = Path.Combine(oidFolderLocation, oidFileName);
            readOidMapFromFile(filePath);
        }
    }
    static void readOidMapFromFile(String filePath) {
        if (!File.Exists(filePath)) {
            return;
        }
        String[] strings = File.ReadAllLines(filePath);
        foreach (String[] tokens in strings.Select(str => str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))) {
            if (tokens.Length != 2) {
                continue;
            }
            try {
                OidResolver.Add(tokens[0], tokens[1].Trim());
            } catch { }
        }
    }
    async void parseArguments(IReadOnlyList<String> args) {
        for (Int32 i = 0; i < args.Count;) {
            switch (args[i].ToLower()) {
                case "-path":  // open from a file
                    i++;
                    if (args.Count <= i) {
                        throw new ArgumentException(args[i]);
                    }
                    await Container.Resolve<IMainWindowVM>().OpenExistingAsync(args[i]);
                    return;
                case "-raw":  // base64 raw string
                    i++;
                    if (args.Count <= i) {
                        throw new ArgumentException(args[i]);
                    }
                    await Container.Resolve<IMainWindowVM>().OpenRawAsync(args[i]);
                    return;
                default:
                    Console.WriteLine($"Unknown parameter '{args[i]}'");
                    return;
            }
        }
    }
    void configureUnity() {
        Container = new UnityContainer();
        Container.RegisterType<MainWindow>();

        Container.RegisterType<IWindowFactory, WindowFactory>();
        Container.RegisterType<IAppCommands, AppCommands>();
        Container.RegisterType<ITagDataEditor, TagDataEditor>();
        // view models
        Container.RegisterSingleton<MainWindowVM>();
        Container.RegisterType<IMainWindowVM, MainWindowVM>();
        Container.RegisterType<IHasSelectedTab, MainWindowVM>();
        Container.RegisterType<ITextViewerVM, TextViewerVM>();
        Container.RegisterType<ITagDataEditorVM, TagDataEditorVM>();
        Container.RegisterInstance(_options);
    }
    void onOptionsChanged(Object s, PropertyChangedEventArgs e) {
        using var sw = new StreamWriter(Path.Combine(_basePath, "user.config"), false);
        using var xw = XmlWriter.Create(sw);
        new XmlSerializer(typeof(NodeViewOptions)).Serialize(xw, s);
    }
    NodeViewOptions readSettings() {
        if (File.Exists(Path.Combine(_basePath, "user.config"))) {
            try {
                using var sr = new StreamReader(Path.Combine(_basePath, "user.config"));
                return (NodeViewOptions) new XmlSerializer(typeof(NodeViewOptions)).Deserialize(sr);
            } catch {
                return new NodeViewOptions();
            }
        }

        return new NodeViewOptions();
    }
}