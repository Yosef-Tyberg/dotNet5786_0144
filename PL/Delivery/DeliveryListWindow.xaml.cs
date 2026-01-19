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

namespace PL.Delivery;

    /// <summary>
    /// Interaction logic for DeliveryInListWindow.xaml
    /// </summary>
    public partial class DeliveryListWindow : Window
    {
        static readonly BlApi.IBl s_bl = BlApi.Factory.Get();

        public object StatusFilter { get; set; } = "None";

        public IEnumerable<BO.DeliveryInList> DeliveryList
        {
            get { return (IEnumerable<BO.DeliveryInList>)GetValue(DeliveryListProperty); }
            set { SetValue(DeliveryListProperty, value); }
        }

        public static readonly DependencyProperty DeliveryListProperty =
            DependencyProperty.Register("DeliveryList", typeof(IEnumerable<BO.DeliveryInList>), typeof(DeliveryListWindow), new PropertyMetadata(null));

        public DeliveryListWindow()
        {
            InitializeComponent();
        }

        private void RefreshList()
        {
            DeliveryList = (StatusFilter is BO.ScheduleStatus status)
                ? s_bl.Delivery.ReadAll(d => d.ScheduleStatus == status)
                : s_bl.Delivery.ReadAll();
        }

        private void cbStatusSelector_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshList();

        private void deliveryListObserver() => Dispatcher.Invoke(RefreshList);
        private void Window_Loaded(object sender, RoutedEventArgs e) => s_bl.Delivery.AddObserver(deliveryListObserver);
        private void Window_Closed(object sender, EventArgs e) => s_bl.Delivery.RemoveObserver(deliveryListObserver);
    }
