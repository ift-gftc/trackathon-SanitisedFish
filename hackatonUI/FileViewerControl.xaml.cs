using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace hackatonUI
{
    /// <summary>
    /// Interaction logic for NormalFileViewerControl.xaml
    /// </summary>
    public partial class FileViewerControl : UserControl
    {
        public FileViewerControl(string file)
        {
            InitializeComponent();

            if(File.Exists(file))
                using (StreamReader reader = new StreamReader(file))
                {
                    while (!reader.EndOfStream)
                        listview.Items.Add(reader.ReadLine());
                }
        }
    }
}
