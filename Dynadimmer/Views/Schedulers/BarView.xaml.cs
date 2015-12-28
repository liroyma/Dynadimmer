﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Dynadimmer.Views.Schedulers.Inner
{
    /// <summary>
    /// Interaction logic for BarView.xaml
    /// </summary>
    public partial class BarView : UserControl,INotifyPropertyChanged
    {
        private int _precentage;
        public int Precentage
        {
            get{ return _precentage; }
            set
            {
                _precentage = value;
                NotifyPropertyChanged("Precentage");
            }
        }

        private double _barheight;
        public double BarHeight
        {
            get { return _barheight; }
            set
            {
                _barheight = value;
                NotifyPropertyChanged("BarHeight");
            }
        }

        public BarView()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region Event Handler
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        private void DockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            BarHeight = this.Height * Precentage / 100;
        }
    }
}
