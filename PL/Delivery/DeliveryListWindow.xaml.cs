﻿﻿﻿﻿﻿using System;
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

namespace PL.Delivery;

    /// <summary>
    /// Interaction logic for DeliveryInListWindow.xaml
    /// </summary>
    public partial class DeliveryListWindow : Window
    {
        // Access to the Business Layer
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public object StatusFilter { get; set; } = "None";
        public object EndTypeFilter { get; set; } = "None";

        // Dependency Property for the list of deliveries
        public IEnumerable<BO.DeliveryInList> DeliveryList
        {
            get { return (IEnumerable<BO.DeliveryInList>)GetValue(DeliveryListProperty); }
            set { SetValue(DeliveryListProperty, value); }
        }

        public static readonly DependencyProperty DeliveryListProperty =
            DependencyProperty.Register("DeliveryList", typeof(IEnumerable<BO.DeliveryInList>), typeof(DeliveryListWindow), new PropertyMetadata(null));

        // Currently selected delivery
        public BO.DeliveryInList? SelectedDelivery { get; set; }

        public DeliveryListWindow()
        {
            InitializeComponent();
        }

        // Refreshes the list applying both Status and EndType filters
        private void RefreshList()
        {
            var deliveries = s_bl.Delivery.ReadAll();

            if (StatusFilter is BO.ScheduleStatus status)
                deliveries = deliveries.Where(d => d.ScheduleStatus == status);

            if (EndTypeFilter is BO.DeliveryEndTypes endType)
                deliveries = deliveries.Where(d => d.DeliveryEndType == endType);

            DeliveryList = deliveries;
        }

        // Event handler for filter changes
        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

        // Observer setup
        private void deliveryListObserver() => Dispatcher.Invoke(RefreshList);
        private void Window_Loaded(object sender, RoutedEventArgs e) => s_bl.Delivery.AddObserver(deliveryListObserver);
        private void Window_Closed(object sender, EventArgs e) => s_bl.Delivery.RemoveObserver(deliveryListObserver);

        // Opens DeliveryWindow in Update mode
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (SelectedDelivery != null)
                new DeliveryWindow(SelectedDelivery.Id).Show();
        }

        // Opens DeliveryWindow in Add mode
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            new DeliveryAssignmentWindow().Show();
        }

        // Cancels the delivery (via Order cancellation logic)
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is BO.DeliveryInList delivery)
            {
                if (MessageBox.Show($"Are you sure you want to cancel delivery {delivery.Id}?", "Cancel Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        s_bl.Order.Cancel(delivery.OrderId);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Cancellation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
