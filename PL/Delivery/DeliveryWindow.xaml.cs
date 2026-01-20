﻿﻿using System;
using System.Windows;
using BlApi;

namespace PL.Delivery;

/// <summary>
/// Interaction logic for DeliveryWindow.xaml
/// </summary>
public partial class DeliveryWindow : Window
{
    // Access to Business Layer
    private static IBl s_bl = Factory.Get();

    public BO.Delivery CurrentDelivery
    {
        get { return (BO.Delivery)GetValue(CurrentDeliveryProperty); }
        set { SetValue(CurrentDeliveryProperty, value); }
    }

    // Dependency Property for the Delivery entity
    public static readonly DependencyProperty CurrentDeliveryProperty =
        DependencyProperty.Register("CurrentDelivery", typeof(BO.Delivery), typeof(DeliveryWindow), new PropertyMetadata(null));

    /// <summary>
    /// Constructor for DeliveryWindow.
    /// </summary>
    /// <param name="id">ID of the delivery to view.</param>
    public DeliveryWindow(int id)
    {
        InitializeComponent();
        LoadDelivery(id);
    }

    /// <summary>
    /// Initialize window state.
    /// </summary>
    private void LoadDelivery(int id)
    {
        try
        {
            CurrentDelivery = s_bl.Delivery.Read(id);
            // Register observer to keep view updated if delivery changes while open
            s_bl.Delivery.AddObserver(id, Observer);
            Closing += (s, e) => s_bl.Delivery.RemoveObserver(id, Observer);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    /// <summary>
    /// Observer to refresh delivery data.
    /// </summary>
    private void Observer()
    {
        Dispatcher.Invoke(() => 
        {
            try { CurrentDelivery = s_bl.Delivery.Read(CurrentDelivery.Id); } catch { }
        });
    }

    /// <summary>
    /// Closes the window.
    /// </summary>
    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}