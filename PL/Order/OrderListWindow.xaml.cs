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

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderInListWindow.xaml
/// </summary>
public partial class OrderListWindow : Window
{
    // Access to Business Layer
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public object StatusFilter { get; set; } = "None";
    public object TypeFilter { get; set; } = "None";
    public object ScheduleStatusFilter { get; set; } = "None";

    // Dependency Property for Order List
    public IEnumerable<BO.OrderInList> OrderList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

    // Selected Order
    public BO.OrderInList? SelectedOrder { get; set; }

    public OrderListWindow()
    {
        InitializeComponent();
    }

    // Refresh list with all 3 filters applied
    private void RefreshList()
    {
        var orders = s_bl.Order.ReadAll();

        if (StatusFilter is BO.OrderStatus status)
            orders = orders.Where(o => o.OrderStatus == status);

        if (TypeFilter is BO.OrderTypes type)
            orders = orders.Where(o => o.OrderType == type);

        if (ScheduleStatusFilter is BO.ScheduleStatus scheduleStatus)
            orders = orders.Where(o => o.ScheduleStatus == scheduleStatus);

        OrderList = orders;
    }

    // Event handlers for filters
    private void cbStatusSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();
    private void cbFilter_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

    // Observer setup
    private void orderListObserver() => Dispatcher.Invoke(RefreshList);
    private void Window_Loaded(object sender, RoutedEventArgs e) => s_bl.Order.AddObserver(orderListObserver);
    private void Window_Closed(object sender, EventArgs e) => s_bl.Order.RemoveObserver(orderListObserver);

    // Open OrderWindow in Update mode
    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedOrder != null)
            new OrderWindow(SelectedOrder.Id).Show();
    }

    // Open OrderWindow in Add mode
    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        new OrderWindow().Show();
    }

    // Cancel Order logic
    private void BtnCancel_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.DataContext is BO.OrderInList order)
        {
            if (MessageBox.Show($"Are you sure you want to cancel order {order.Id}?", "Cancel Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    s_bl.Order.Cancel(order.Id);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Cancellation Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
