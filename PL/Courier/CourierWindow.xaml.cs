﻿using System;
using System.Windows;
using BlApi;

namespace PL.Courier
{
    /// <summary>
    /// Interaction logic for CourierWindow.xaml
    /// </summary>
    public partial class CourierWindow : Window
    {
        private static IBl s_bl = Factory.Get();

        public BO.Courier CurrentCourier
        {
            get { return (BO.Courier)GetValue(CurrentCourierProperty); }
            set { SetValue(CurrentCourierProperty, value); }
        }

        public static readonly DependencyProperty CurrentCourierProperty =
            DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

        public string ButtonText
        {
            get { return (string)GetValue(ButtonTextProperty); }
            set { SetValue(ButtonTextProperty, value); }
        }

        public static readonly DependencyProperty ButtonTextProperty =
            DependencyProperty.Register("ButtonText", typeof(string), typeof(CourierWindow), new PropertyMetadata("Add"));

        public CourierWindow(int id = 0)
        {
            ButtonText = id == 0 ? "Add" : "Update";
            InitializeComponent();
            Init(id);
        }

        private void Init(int id)
        {
            try
            {
                if (id == 0)
                {
                    CurrentCourier = new BO.Courier
                    {
                        Active = true,
                        EmploymentStartTime = s_bl.Admin.GetClock(),
                        DeliveryType = BO.DeliveryTypes.Motorcycle // Default
                    };
                }
                else
                {
                    CurrentCourier = s_bl.Courier.Read(id);
                    s_bl.Courier.AddObserver(id, Observer);
                    Closing += (s, e) => s_bl.Courier.RemoveObserver(id, Observer);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void Observer()
        {
            Dispatcher.Invoke(() =>
            {
                if (CurrentCourier != null)
                    CurrentCourier = s_bl.Courier.Read(CurrentCourier.Id);
            });
        }

        private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
        {
            Tools.ExecuteSafeAction(this, () =>
            {
                if (ButtonText == "Add")
                    s_bl.Courier.Create(CurrentCourier);
                else
                    s_bl.Courier.Update(CurrentCourier);
            }, $"Courier {ButtonText}ed successfully!");
        }
    }
}
