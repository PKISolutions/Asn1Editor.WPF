using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Asn1Editor.API.ViewModel;
using Asn1Editor.Views.Windows;

namespace Asn1Editor {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App {
        void AppStartup(Object sender, StartupEventArgs e) {
            //AsnFormatter = true;
            readOids();
            MainWindow mw = new MainWindow();
            parseArguments((App)sender, e.Args);
            mw.Show();
        }
        void readOids() {
            MainWindowVM.OIDs = new Dictionary<String, String>();
            var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (!File.Exists(path + @"\OID.txt")) { return; }
            String[] strings = File.ReadAllLines(path + @"\OID.txt");
            foreach (String[] tokens in strings.Select(str => str.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))) {
                try {
                    MainWindowVM.OIDs.Add(tokens[0], tokens[1].Trim());
                } catch { }
            }
        }
        void parseArguments(App sender, String[] args) {
            for (Int32 i = 0; i < args.Length;) {
                switch (args[i].ToLower()) {
                    case "-path":  // SRP log file path
                        i++;
                        if (args.Length <= i) throw new ArgumentException(args[i]);
                        ((MainWindowVM)sender.MainWindow.DataContext).OpenExisting(args[i]);
                        return;
                    case "-raw":  // excluded path patterns (path begins)
                        i++;
                        if (args.Length <= i) throw new ArgumentException(args[i]);
                        ((MainWindowVM)sender.MainWindow.DataContext).OpenRaw(args[i]);
                        return;
                    default:
                        Console.WriteLine($"Unknown parameter '{args[i]}'");
                        return;
                }
            }
        }
    }
}
