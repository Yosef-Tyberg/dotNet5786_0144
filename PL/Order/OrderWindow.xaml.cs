﻿﻿﻿using System;
using System.Windows;
using BlApi;
using PL;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderWindow.xaml
/// </summary>
public partial class OrderWindow : Window
{
    // Access to Business Layer
    private static IBl s_bl = Factory.Get();

    public BO.Order CurrentOrder
    {
        get { return (BO.Order)GetValue(CurrentOrderProperty); }
        set { SetValue(CurrentOrderProperty, value); }
    }

    // Dependency Property for Order entity
    public static readonly DependencyProperty CurrentOrderProperty =
        DependencyProperty.Register("CurrentOrder", typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

    // Button Text ("Add" or "Update")
    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(OrderWindow), new PropertyMetadata("Add"));

    /// <summary>
    /// Constructor for OrderWindow.
    /// </summary>
    /// <param name="id">Order ID (null for new).</param>
    public OrderWindow(int? id = null)
    {
        ButtonText = id == null ? "Add" : "Update";
        InitializeComponent();
        Init(id);
    }

    /// <summary>
    /// Initialize window state.
    /// </summary>
    private void Init(int? id)
    {
        try
        {
            if (id == null)
            {
                // Add Mode: Default values
                CurrentOrder = new BO.Order
                {
                    OrderOpenTime = s_bl.Admin.GetClock(),
                    OrderStatus = BO.OrderStatus.Open,
                    OrderType = BO.OrderTypes.Pizza // Default
                };
            }
            else
            {
                // Update Mode: Load order and register observer
                CurrentOrder = s_bl.Order.Read(id.Value);
                s_bl.Order.AddObserver(id.Value, Observer);
                Closing += (s, e) => s_bl.Order.RemoveObserver(id.Value, Observer);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Close();
        }
    }

    /// <summary>
    /// Observer to refresh order data.
    /// </summary>
    private void Observer()
    {
        Dispatcher.Invoke(() => CurrentOrder = s_bl.Order.Read(CurrentOrder.Id));
    }

    /// <summary>
    /// Handle Add/Update action.
    /// </summary>
    private void BtnAddUpdate_Click(object sender, RoutedEventArgs e)
    {
        Tools.ExecuteSafeAction(this, () =>
        {
            if (ButtonText == "Add")
                s_bl.Order.Create(CurrentOrder);
            else
                s_bl.Order.Update(CurrentOrder);
        }, $"Order {ButtonText}ed successfully!");
    }
}