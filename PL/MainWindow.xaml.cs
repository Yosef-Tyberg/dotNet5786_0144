﻿using System.Text;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.ComponentModel;
using BlApi;

namespace PL;

public partial class MainWindow : Window
{
    // BL is created lazily to prevent WPF designer crashes
    private static IBl? s_bl;

    #region Dependency Properties

    public DateTime CurrentTime
    {
        get => (DateTime)GetValue(CurrentTimeProperty);
        set => SetValue(CurrentTimeProperty, value);
    }

    public static readonly DependencyProperty CurrentTimeProperty =
        DependencyProperty.Register(
            nameof(CurrentTime),
            typeof(DateTime),
            typeof(MainWindow));

    public BO.Config Configuration
    {
        get => (BO.Config)GetValue(ConfigurationProperty);
        set => SetValue(ConfigurationProperty, value);
    }

    public static readonly DependencyProperty ConfigurationProperty =
        DependencyProperty.Register(
            nameof(Configuration),
            typeof(BO.Config),
            typeof(MainWindow));

    #endregion

    public MainWindow()
    {
        InitializeComponent();

        // Register lifecycle event handlers explicitly
        Loaded += MainWindow_Loaded;
        Closing += MainWindow_Closing;
    }

    #region Window Lifecycle

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DesignerProperties.GetIsInDesignMode(this))
            return;

        try
        {
            s_bl ??= Factory.Get();

            // Register observers
            s_bl.Admin.AddClockObserver(ClockObserver);
            s_bl.Admin.AddConfigObserver(ConfigObserver);

            // Initialize bound properties
            CurrentTime = s_bl.Admin.GetClock();
            Configuration = s_bl.Admin.GetConfig();
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Failed to initialize the management console.",
                "Initialization Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void MainWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (s_bl == null)
            return;

        try
        {
            s_bl.Admin.RemoveClockObserver(ClockObserver);
            s_bl.Admin.RemoveConfigObserver(ConfigObserver);
        }
        catch
        {
            // Suppress shutdown errors
        }
    }

    #endregion

    #region Observer Methods (6g)

    private void ClockObserver()
    {
        Dispatcher.Invoke(() =>
            CurrentTime = s_bl!.Admin.GetClock());
    }

    private void ConfigObserver()
    {
        Dispatcher.Invoke(() =>
            Configuration = s_bl!.Admin.GetConfig());
    }

    #endregion

    #region Clock Controls

    private void AdvanceMinute_Click(object sender, RoutedEventArgs e) =>
        AdvanceClock(TimeSpan.FromMinutes(1));

    private void AdvanceHour_Click(object sender, RoutedEventArgs e) =>
        AdvanceClock(TimeSpan.FromHours(1));

    private void AdvanceDay_Click(object sender, RoutedEventArgs e) =>
        AdvanceClock(TimeSpan.FromDays(1));

    private void AdvanceMonth_Click(object sender, RoutedEventArgs e) =>
        AdvanceClock(TimeSpan.FromDays(30));

    private void AdvanceYear_Click(object sender, RoutedEventArgs e) =>
        AdvanceClock(TimeSpan.FromDays(365));

    private void AdvanceClock(TimeSpan span)
    {
        try
        {
            s_bl!.Admin.ForwardClock(span);
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Failed to advance the system clock.",
                "Clock Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    #endregion

    #region Configuration

    private void SaveConfiguration_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            s_bl!.Admin.SetConfig(Configuration);
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Failed to save configuration changes.",
                "Configuration Error",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    #endregion

    #region Database Management (6k)

    private void InitializeDb_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(
                "Are you sure you want to initialize the database?",
                "Confirm Initialization",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question) != MessageBoxResult.Yes)
            return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CloseAllSecondaryWindows();
            s_bl!.Admin.InitializeDB();
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Database initialization failed.",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private void ResetDb_Click(object sender, RoutedEventArgs e)
    {
        if (MessageBox.Show(
                "Are you sure you want to reset the database?",
                "Confirm Reset",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        try
        {
            Mouse.OverrideCursor = Cursors.Wait;
            CloseAllSecondaryWindows();
            s_bl!.Admin.ResetDB();
        }
        catch (Exception)
        {
            MessageBox.Show(
                "Database reset failed.",
                "Database Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            Mouse.OverrideCursor = null;
        }
    }

    private static void CloseAllSecondaryWindows()
    {
        foreach (Window window in Application.Current.Windows)
        {
            if (window is not MainWindow)
                window.Close();
        }
    }

    #endregion

    #region Navigation Buttons (6j)

    private void OpenCourierList_Click(object sender, RoutedEventArgs e) =>
        new Courier.CourierListWindow().Show();

    private void OpenOrderList_Click(object sender, RoutedEventArgs e) =>
        new Order.OrderListWindow().Show();

    private void OpenDeliveryList_Click(object sender, RoutedEventArgs e) =>
        new Delivery.DeliveryListWindow().Show();

    #endregion
}