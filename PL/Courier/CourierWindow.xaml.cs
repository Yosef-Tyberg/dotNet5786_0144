﻿﻿﻿using System;
using System.Windows;
using BlApi;

namespace PL.Courier;

/// <summary>
/// Interaction logic for CourierWindow.xaml
/// </summary>
public partial class CourierWindow : Window
{
    // Access to the Business Layer via Factory
    private static IBl s_bl = Factory.Get();

    public BO.Courier CurrentCourier
    {
        get { return (BO.Courier)GetValue(CurrentCourierProperty); }
        set { SetValue(CurrentCourierProperty, value); }
    }

    // Dependency Property for the Courier entity being added or updated
    public static readonly DependencyProperty CurrentCourierProperty =
        DependencyProperty.Register("CurrentCourier", typeof(BO.Courier), typeof(CourierWindow), new PropertyMetadata(null));

    // Text for the action button ("Add" or "Update")
    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(CourierWindow), new PropertyMetadata("Add"));

    // Controls whether the ID field is editable (Editable only in Add mode)
    public bool IsIdReadOnly
    {
        get { return (bool)GetValue(IsIdReadOnlyProperty); }
        set { SetValue(IsIdReadOnlyProperty, value); }
    }

    public static readonly DependencyProperty IsIdReadOnlyProperty =
        DependencyProperty.Register("IsIdReadOnly", typeof(bool), typeof(CourierWindow), new PropertyMetadata(false));

    /// <summary>
    /// Constructor for CourierWindow.
    /// </summary>
    /// <param name="id">The ID of the courier to update, or 0 to add a new courier.</param>
    public CourierWindow(int id = 0)
    {
        ButtonText = id == 0 ? "Add" : "Update";
        InitializeComponent();
        Init(id);
    }

    // Initializes the window state based on the mode (Add vs Update)
    private void Init(int id)
    {
        IsIdReadOnly = id != 0;
        try
        {
            if (id == 0)
            {
                // Add Mode: Initialize with default values
                CurrentCourier = new BO.Courier
                {
                    Active = true,
                    EmploymentStartTime = s_bl.Admin.GetClock(),
                    DeliveryType = BO.DeliveryTypes.Motorcycle // Default
                };
            }
            else
            {
                // Update Mode: Load existing courier and register observer
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

    // Observer method to refresh the displayed courier data if it changes externally
    private void Observer()
    {
        Dispatcher.Invoke(() =>
        {
            if (CurrentCourier != null)
                CurrentCourier = s_bl.Courier.Read(CurrentCourier.Id);
        });
    }

    // Handles the Add/Update button click
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
