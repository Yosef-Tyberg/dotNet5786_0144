﻿using System;
using System.Windows;
using BlApi;
using PL;

namespace PL.Order;

/// <summary>
/// Interaction logic for OrderWindow.xaml
/// </summary>
public partial class OrderWindow : Window
{
    private static IBl s_bl = Factory.Get();

    public BO.Order CurrentOrder
    {
        get { return (BO.Order)GetValue(CurrentOrderProperty); }
        set { SetValue(CurrentOrderProperty, value); }
    }

    public static readonly DependencyProperty CurrentOrderProperty =
        DependencyProperty.Register("CurrentOrder", typeof(BO.Order), typeof(OrderWindow), new PropertyMetadata(null));

    public string ButtonText
    {
        get { return (string)GetValue(ButtonTextProperty); }
        set { SetValue(ButtonTextProperty, value); }
    }

    public static readonly DependencyProperty ButtonTextProperty =
        DependencyProperty.Register("ButtonText", typeof(string), typeof(OrderWindow), new PropertyMetadata("Add"));

    public OrderWindow(int id = 0)
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
                CurrentOrder = new BO.Order
                {
                    OrderOpenTime = s_bl.Admin.GetClock(),
                    OrderStatus = BO.OrderStatus.Open,
                    OrderType = BO.OrderTypes.Pizza // Default
                };
            }
            else
            {
                CurrentOrder = s_bl.Order.Read(id);
                s_bl.Order.AddObserver(id, Observer);
                Closing += (s, e) => s_bl.Order.RemoveObserver(id, Observer);
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
        Dispatcher.Invoke(() => CurrentOrder = s_bl.Order.Read(CurrentOrder.Id));
    }

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