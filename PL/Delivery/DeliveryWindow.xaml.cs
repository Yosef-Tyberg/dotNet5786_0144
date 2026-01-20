﻿using System;
using System.Windows;
using BlApi;

namespace PL.Delivery;

/// <summary>
/// Interaction logic for DeliveryWindow.xaml
/// </summary>
public partial class DeliveryWindow : Window
{
    private static IBl s_bl = Factory.Get();

    public BO.Delivery CurrentDelivery
    {
        get { return (BO.Delivery)GetValue(CurrentDeliveryProperty); }
        set { SetValue(CurrentDeliveryProperty, value); }
    }

    public static readonly DependencyProperty CurrentDeliveryProperty =
        DependencyProperty.Register("CurrentDelivery", typeof(BO.Delivery), typeof(DeliveryWindow), new PropertyMetadata(null));

    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(DeliveryWindow), new PropertyMetadata("Add"));

    public DeliveryWindow(int id = 0)
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
                // Note: Creating a delivery manually is complex due to logic constraints, 
                // but we provide a default object for the view.
                CurrentDelivery = new BO.Delivery
                {
                    DeliveryStartTime = s_bl.Admin.GetClock(),
                    DeliveryType = BO.DeliveryTypes.Motorcycle
                };
            }
            else
            {
                CurrentDelivery = s_bl.Delivery.Read(id);
                s_bl.Delivery.AddObserver(id, Observer);
                Closing += (s, e) => s_bl.Delivery.RemoveObserver(id, Observer);
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
        Dispatcher.Invoke(() => CurrentDelivery = s_bl.Delivery.Read(CurrentDelivery.Id));
    }

    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        // Delivery updates are generally restricted by BL logic (e.g. PickUp, Deliver).
        // This button is a placeholder for generic update if supported by BL, 
        // or specific actions should be implemented instead.
        // For strict adherence to the generic instructions, we leave this empty or show a message
        // as the BL interface for generic 'Update(Delivery)' might not exist or be intended for this use.
        MessageBox.Show("Generic update for Delivery is not supported via this window in the current BL implementation.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}