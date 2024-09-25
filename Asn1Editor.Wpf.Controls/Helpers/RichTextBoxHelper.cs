using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Asn1Editor.Wpf.Controls.Helpers
{
    public class RichTextBoxHelper : RichTextBox
    {
        public int CurrentLine
        {
            get { return (int)GetValue(CurrentLineProperty); }
            set { SetValue(CurrentLineProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentLine.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentLineProperty =
            DependencyProperty.Register("CurrentLine", typeof(int), typeof(RichTextBoxHelper), new PropertyMetadata(0));



        public int CurrentColumn
        {
            get { return (int)GetValue(CurrentColumnProperty); }
            set { SetValue(CurrentColumnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentColumn.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentColumnProperty =
            DependencyProperty.Register("CurrentColumn", typeof(int), typeof(RichTextBoxHelper), new PropertyMetadata(0));


    }
}
