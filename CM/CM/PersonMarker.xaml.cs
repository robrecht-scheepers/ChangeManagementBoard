﻿using System;
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
    /// Interaction logic for PersonMarker.xaml
    /// </summary>
    public partial class PersonMarker : UserControl
    {
        public PersonMarker()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            "Fill", typeof(Brush), typeof(PersonMarker), new PropertyMetadata(default(Brush)));

        public Brush Fill
        {
            get { return (Brush) GetValue(FillProperty); }
            set { SetValue(FillProperty, value); }
        }

       
    }
}
