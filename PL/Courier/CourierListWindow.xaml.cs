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
using System.Windows.Shapes;

namespace PL.Courier;

    /// <summary>
    /// Interaction logic for CourierListWindow.xaml
    /// </summary>
    public partial class CourierListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        // Using object to allow "None" (string) or Enum value
        public object StatusFilter { get; set; } = "None";

        public IEnumerable<BO.CourierInList> CourierList
        {
            get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
            set { SetValue(CourierListProperty, value); }
        }

        public static readonly DependencyProperty CourierListProperty =
            DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(CourierListWindow), new PropertyMetadata(null));

        public BO.CourierInList? SelectedCourier { get; set; }

        public CourierListWindow()
        {
            InitializeComponent();
        }

        private void RefreshList()
        {
            CourierList = (StatusFilter is BO.DeliveryTypes type)
                ? s_bl.Courier.ReadAll(c => c.DeliveryType == type)
                : s_bl.Courier.ReadAll();
        }

        private void cbStatusSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

        private void courierListObserver() => Dispatcher.Invoke(RefreshList);
        private void Window_Loaded(object sender, RoutedEventArgs e) => s_bl.Courier.AddObserver(courierListObserver);
        private void Window_Closed(object sender, EventArgs e) => s_bl.Courier.RemoveObserver(courierListObserver);

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedCourier != null)
                new CourierWindow(SelectedCourier.Id).Show();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            new CourierWindow().Show();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BO.CourierInList courier)
            {
                if (MessageBox.Show($"Are you sure you want to delete courier {courier.Id}?", "Delete Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Courier.Delete(courier.Id);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Deletion Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
