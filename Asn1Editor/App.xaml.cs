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
using SysadminsLV.Asn1Editor.API.Interfaces;
using SysadminsLV.Asn1Editor.API.ModelObjects;
using SysadminsLV.Asn1Editor.API.Utils;
using SysadminsLV.Asn1Editor.API.Utils.WPF;
using SysadminsLV.Asn1Editor.API.ViewModel;
using SysadminsLV.Asn1Editor.Views.Windows;
using Unity;

namespace SysadminsLV.Asn1Editor; 

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App {
    readonly String basePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Sysadmins LV\Asn1Editor");
    static readonly Logger _logger = new Logger();
    NodeViewOptions options;

    public App() {
        Dispatcher.UnhandledException += OnDispatcherUnhandledException;
        options = readSettings();
        options.PropertyChanged += onOptionsChanged;
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
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!File.Exists(path + @"\OID.txt")) { return; }
        String[] strings = File.ReadAllLines(path + @"\OID.txt");
        foreach (String[] tokens in strings.Select(str => str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))) {
            try {
                MainWindowVM.OIDs.Add(tokens[0], tokens[1].Trim());
            } catch { }
        }
    }
    void parseArguments(IReadOnlyList<String> args) {
        for (Int32 i = 0; i < args.Count;) {
            switch (args[i].ToLower()) {
                case "-path":  // open from a file
                    i++;
                    if (args.Count <= i) {
                        throw new ArgumentException(args[i]);
                    }
                    Container.Resolve<IMainWindowVM>().OpenExisting(args[i]);
                    return;
                case "-raw":  // base64 raw string
                    i++;
                    if (args.Count <= i) {
                        throw new ArgumentException(args[i]);
                    }
                    Container.Resolve<IMainWindowVM>().OpenRaw(args[i]);
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
        Container.RegisterType<ITreeCommands, TreeViewCommands>();
        Container.RegisterSingleton<IDataSource, DataSource>();
        Container.RegisterType<ITagDataEditor, TagDataEditor>();
        // view models
        Container.RegisterSingleton<IMainWindowVM, MainWindowVM>();
        Container.RegisterType<ITextViewerVM, TextViewerVM>();
        Container.RegisterType<ITreeViewVM, TreeViewVM>();
        Container.RegisterType<ITagDataEditorVM, TagDataEditorVM>();
        Container.RegisterInstance(options);
    }
    void onOptionsChanged(Object s, PropertyChangedEventArgs e) {
        using (var sw = new StreamWriter(Path.Combine(basePath, "user.config"), false)) {
            using (var xw = XmlWriter.Create(sw)) {
                new XmlSerializer(typeof(NodeViewOptions)).Serialize(xw, s);
            }
        }
    }
    NodeViewOptions readSettings() {
        if (File.Exists(Path.Combine(basePath, "user.config"))) {
            try {
                using (var sr = new StreamReader(Path.Combine(basePath, "user.config"))) {
                    return (NodeViewOptions) new XmlSerializer(typeof(NodeViewOptions)).Deserialize(sr);
                }
            } catch {
                return new NodeViewOptions();
            }
        } else {
            return new NodeViewOptions();
        }
    }
}