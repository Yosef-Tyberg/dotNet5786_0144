﻿﻿﻿using System;
﻿﻿﻿﻿using System;
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

    // Button text ("Add" or "Update")
    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(DeliveryWindow), new PropertyMetadata("Close"));

    /// <summary>
    /// Constructor for DeliveryWindow.
    /// </summary>
    /// <param name="id">ID of the delivery (0 for new).</param>
    public DeliveryWindow(int id = 0)
    {
        ButtonText = "Close";
        InitializeComponent();
        Init(id);
    }

    // Initialize window state
    private void Init(int id)
    {
        try
        {
            if (id == 0)
            {
                // Add Mode: Initialize default delivery
                CurrentDelivery = new BO.Delivery
                {
                    DeliveryStartTime = s_bl.Admin.GetClock(),
                    DeliveryType = BO.DeliveryTypes.Motorcycle
                };
            }
            else
            {
                // Update Mode: Load delivery and register observer
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

    // Observer to refresh delivery data
    private void Observer()
    {
        Dispatcher.Invoke(() => CurrentDelivery = s_bl.Delivery.Read(CurrentDelivery.Id));
    }

    // Handle Add/Update action
    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        // Delivery updates are generally restricted by BL logic (e.g. PickUp, Deliver).
        // This button is a placeholder for generic update if supported by BL, 
        // or specific actions should be implemented instead.
        // For strict adherence to the generic instructions, we leave this empty or show a message
        // as the BL interface for generic 'Update(Delivery)' might not exist or be intended for this use.
        MessageBox.Show("Generic update for Delivery is not supported via this window in the current BL implementation.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        Close();
    }
}