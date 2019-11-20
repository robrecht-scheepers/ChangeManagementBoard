using System;
using System.Collections.Generic;
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

namespace CM
{
    /// <summary>
    /// Interaction logic for ProjectMarker.xaml
    /// </summary>
    public partial class ProjectMarker : UserControl
    {
        public ProjectMarker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty OccupantsProperty = DependencyProperty.Register(
            "Occupants", typeof(int), typeof(ProjectMarker), new PropertyMetadata(default(int)));

        public int Occupants
        {
            get { return (int) GetValue(OccupantsProperty); }
            set { SetValue(OccupantsProperty, value); }
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(ProjectMarker), new PropertyMetadata(default(Brush)));

        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty RotationProperty = DependencyProperty.Register(
            "Rotation", typeof(double), typeof(ProjectMarker), new PropertyMetadata(default(double)));

        public double Rotation
        {
            get { return (double) GetValue(RotationProperty); }
            set { SetValue(RotationProperty, value); }
        }
    }
}
