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
        public BO.DeliveryInList? SelectedDelivery
        {
            get { return (BO.DeliveryInList?)GetValue(SelectedDeliveryProperty); }
            set { SetValue(SelectedDeliveryProperty, value); }
        }

        public static readonly DependencyProperty SelectedDeliveryProperty =
            DependencyProperty.Register("SelectedDelivery", typeof(BO.DeliveryInList), typeof(DeliveryListWindow), new PropertyMetadata(null));

        public DeliveryListWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Refreshes the list applying both Status and EndType filters.
        /// </summary>
        private void RefreshList()
        {
            var deliveries = s_bl.Delivery.ReadAll();

            if (StatusFilter is BO.ScheduleStatus status)
                deliveries = deliveries.Where(d => d.ScheduleStatus == status);

            if (EndTypeFilter is BO.DeliveryEndTypes endType)
                deliveries = deliveries.Where(d => d.DeliveryEndType == endType);

            DeliveryList = deliveries;
        }

        /// <summary>
        /// Event handler for filter changes.
        /// </summary>
        private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

        /// <summary>
        /// Observer setup to keep the list updated.
        /// </summary>
        private void deliveryListObserver() => Dispatcher.Invoke(RefreshList);

        /// <summary>
        /// Registers the observer when the window is loaded.
        /// </summary>
        private void Window_Loaded(object sender, RoutedEventArgs e) => s_bl.Delivery.AddObserver(deliveryListObserver);

        /// <summary>
        /// Unregisters the observer when the window is closed.
        /// </summary>
        private void Window_Closed(object sender, EventArgs e) => s_bl.Delivery.RemoveObserver(deliveryListObserver);

        /// <summary>
        /// Opens DeliveryWindow in Update mode.
        /// </summary>
        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid && grid.SelectedItem is BO.DeliveryInList delivery)
                new DeliveryWindow(delivery.Id).Show();
        }

        /// <summary>
        /// Opens DeliveryWindow in Add mode.
        /// </summary>
        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            new DeliveryAssignmentWindow().Show();
        }

        /// <summary>
        /// Cancels the delivery (via Order cancellation logic).
        /// </summary>
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
