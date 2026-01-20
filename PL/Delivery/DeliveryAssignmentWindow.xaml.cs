using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PL.Delivery;

public partial class DeliveryAssignmentWindow : Window
{
    private static readonly BlApi.IBl s_bl = BlApi.Factory.Get();
    private int _selectedCourierId;

    public string Legend
    {
        get { return (string)GetValue(LegendProperty); }
        set { SetValue(LegendProperty, value); }
    }
    public static readonly DependencyProperty LegendProperty =
        DependencyProperty.Register("Legend", typeof(string), typeof(DeliveryAssignmentWindow), new PropertyMetadata(string.Empty));

    public IEnumerable<BO.CourierInList> CourierList
    {
        get { return (IEnumerable<BO.CourierInList>)GetValue(CourierListProperty); }
        set { SetValue(CourierListProperty, value); }
    }
    public static readonly DependencyProperty CourierListProperty =
        DependencyProperty.Register("CourierList", typeof(IEnumerable<BO.CourierInList>), typeof(DeliveryAssignmentWindow), new PropertyMetadata(null));

    public IEnumerable<BO.OrderInList> OrderList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }
    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(DeliveryAssignmentWindow), new PropertyMetadata(null));

    public Visibility CourierListVisibility
    {
        get { return (Visibility)GetValue(CourierListVisibilityProperty); }
        set { SetValue(CourierListVisibilityProperty, value); }
    }
    public static readonly DependencyProperty CourierListVisibilityProperty =
        DependencyProperty.Register("CourierListVisibility", typeof(Visibility), typeof(DeliveryAssignmentWindow), new PropertyMetadata(Visibility.Visible));

    public Visibility OrderListVisibility
    {
        get { return (Visibility)GetValue(OrderListVisibilityProperty); }
        set { SetValue(OrderListVisibilityProperty, value); }
    }
    public static readonly DependencyProperty OrderListVisibilityProperty =
        DependencyProperty.Register("OrderListVisibility", typeof(Visibility), typeof(DeliveryAssignmentWindow), new PropertyMetadata(Visibility.Collapsed));

    public BO.CourierInList? SelectedCourier
    {
        get { return (BO.CourierInList?)GetValue(SelectedCourierProperty); }
        set { SetValue(SelectedCourierProperty, value); }
    }
    public static readonly DependencyProperty SelectedCourierProperty =
        DependencyProperty.Register("SelectedCourier", typeof(BO.CourierInList), typeof(DeliveryAssignmentWindow), new PropertyMetadata(null));

    public BO.OrderInList? SelectedOrder
    {
        get { return (BO.OrderInList?)GetValue(SelectedOrderProperty); }
        set { SetValue(SelectedOrderProperty, value); }
    }
    public static readonly DependencyProperty SelectedOrderProperty =
        DependencyProperty.Register("SelectedOrder", typeof(BO.OrderInList), typeof(DeliveryAssignmentWindow), new PropertyMetadata(null));

    public DeliveryAssignmentWindow()
    {
        InitializeComponent();
        LoadCouriers();
    }

    private void LoadCouriers()
    {
        Legend = "Choose courier to assign";
        CourierList = s_bl.Courier.ReadAll(c => c.Active && !s_bl.Courier.IsCourierInDelivery(c.Id));
        CourierListVisibility = Visibility.Visible;
        OrderListVisibility = Visibility.Collapsed;
    }

    private void Courier_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedCourier == null) return;

        _selectedCourierId = SelectedCourier.Id;
        Legend = "Choose order to assign";
        
        try 
        {
            OrderList = s_bl.Courier.GetOpenOrders(_selectedCourierId);
            CourierListVisibility = Visibility.Collapsed;
            OrderListVisibility = Visibility.Visible;
        }
        catch (System.Exception ex)
        {
             MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Order_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (SelectedOrder == null) return;

        try
        {
            s_bl.Delivery.PickUp(_selectedCourierId, SelectedOrder.Id);
            MessageBox.Show("Delivery Assigned", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}