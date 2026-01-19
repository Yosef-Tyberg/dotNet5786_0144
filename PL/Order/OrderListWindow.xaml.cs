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
using System.Windows.Shapes;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderInListWindow.xaml
/// </summary>
public partial class OrderListWindow : Window
{
    static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

    public object StatusFilter { get; set; } = "None";

    public IEnumerable<BO.OrderInList> OrderList
    {
        get { return (IEnumerable<BO.OrderInList>)GetValue(OrderListProperty); }
        set { SetValue(OrderListProperty, value); }
    }

    public static readonly DependencyProperty OrderListProperty =
        DependencyProperty.Register("OrderList", typeof(IEnumerable<BO.OrderInList>), typeof(OrderListWindow), new PropertyMetadata(null));

    public OrderListWindow()
    {
        InitializeComponent();
    }

    private void RefreshList()
    {
        OrderList = (StatusFilter is BO.OrderStatus status)
            ? s_bl.Order.ReadAll(o => o.OrderStatus == status)
            : s_bl.Order.ReadAll();
    }

    private void cbStatusSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

    private void orderListObserver() => Dispatcher.Invoke(RefreshList);
    private void Window_Loaded(object sender, RoutedEventArgs e) => s_bl.Order.AddObserver(orderListObserver);
    private void Window_Closed(object sender, EventArgs e) => s_bl.Order.RemoveObserver(orderListObserver);
}
