//generated with Gemini by the prompt: make all timespan values easier to read - 
// display them with explicit numbers for days, hours, minutes
using System;
using System.Windows;
using System.Windows.Controls;

namespace PL.Controls;

/// <summary>
/// Interaction logic for TimeSpanEditor.xaml.
/// A custom control to edit TimeSpan values using separate fields for Days, Hours, and Minutes.
/// </summary>
public partial class TimeSpanEditor : UserControl
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TimeSpanEditor"/> class.
    /// </summary>
    public TimeSpanEditor()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Identifies the <see cref="Value"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register("Value", typeof(TimeSpan?), typeof(TimeSpanEditor),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnValueChanged));

    /// <summary>
    /// Gets or sets the TimeSpan value being edited.
    /// </summary>
    public TimeSpan? Value
    {
        get { return (TimeSpan?)GetValue(ValueProperty); }
        set { SetValue(ValueProperty, value); }
    }

    /// <summary>
    /// Identifies the <see cref="IsReadOnly"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty IsReadOnlyProperty =
        DependencyProperty.Register("IsReadOnly", typeof(bool), typeof(TimeSpanEditor),
            new FrameworkPropertyMetadata(false, OnIsReadOnlyChanged));

    /// <summary>
    /// Gets or sets a value indicating whether the control is read-only.
    /// </summary>
    public bool IsReadOnly
    {
        get { return (bool)GetValue(IsReadOnlyProperty); }
        set { SetValue(IsReadOnlyProperty, value); }
    }

    /// <summary>
    /// Callback invoked when IsReadOnly changes to update field visibilities.
    /// </summary>
    /// <param name="d">The dependency object.</param>
    /// <param name="e">The event arguments.</param>
    private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((TimeSpanEditor)d).UpdateVisibilities();
    }

    // Flag to prevent infinite loops between Value changed and Part changed
    private bool _isUpdating = false;

    /// <summary>
    /// Callback invoked when the Value property changes.
    /// Updates the individual component properties (Days, Hours, Minutes).
    /// </summary>
    /// <param name="d">The dependency object (TimeSpanEditor).</param>
    /// <param name="e">The event arguments.</param>
    private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TimeSpanEditor)d;
        if (control._isUpdating) return;
        control.NormalizeParts();
    }

    private void NormalizeParts()
    {
        // Set flag to prevent OnPartChanged from triggering while we update the parts
        _isUpdating = true;
        if (Value is TimeSpan ts)
        {
            int totalDays = ts.Days;
            Years = totalDays / 365;
            int remainderDays = totalDays % 365;
            Months = remainderDays / 30;
            Days = remainderDays % 30;
            Hours = ts.Hours;
            Minutes = ts.Minutes;
        }
        else
        {
            Years = 0;
            Months = 0;
            Days = 0;
            Hours = 0;
            Minutes = 0;
        }
        _isUpdating = false;
        // Update the visibility of fields based on the new values (e.g., hide 0 Years if ReadOnly)
        UpdateVisibilities();
    }

    protected override void OnIsKeyboardFocusWithinChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnIsKeyboardFocusWithinChanged(e);
        if (!(bool)e.NewValue) NormalizeParts();
    }

    /// <summary>
    /// Identifies the <see cref="Years"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty YearsProperty =
        DependencyProperty.Register("Years", typeof(int), typeof(TimeSpanEditor),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPartChanged));

    /// <summary>
    /// Gets or sets the Years component of the TimeSpan (approximated as 365 days).
    /// </summary>
    public int Years { get => (int)GetValue(YearsProperty); set => SetValue(YearsProperty, value); }

    /// <summary>
    /// Identifies the <see cref="Months"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MonthsProperty =
        DependencyProperty.Register("Months", typeof(int), typeof(TimeSpanEditor),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPartChanged));

    /// <summary>
    /// Gets or sets the Months component of the TimeSpan (approximated as 30 days).
    /// </summary>
    public int Months { get => (int)GetValue(MonthsProperty); set => SetValue(MonthsProperty, value); }

    /// <summary>
    /// Identifies the <see cref="Days"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty DaysProperty =
        DependencyProperty.Register("Days", typeof(int), typeof(TimeSpanEditor),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPartChanged));

    /// <summary>
    /// Gets or sets the Days component of the TimeSpan.
    /// </summary>
    public int Days { get => (int)GetValue(DaysProperty); set => SetValue(DaysProperty, value); }

    /// <summary>
    /// Identifies the <see cref="Hours"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty HoursProperty =
        DependencyProperty.Register("Hours", typeof(int), typeof(TimeSpanEditor),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPartChanged));

    /// <summary>
    /// Gets or sets the Hours component of the TimeSpan.
    /// </summary>
    public int Hours { get => (int)GetValue(HoursProperty); set => SetValue(HoursProperty, value); }

    /// <summary>
    /// Identifies the <see cref="Minutes"/> dependency property.
    /// </summary>
    public static readonly DependencyProperty MinutesProperty =
        DependencyProperty.Register("Minutes", typeof(int), typeof(TimeSpanEditor),
            new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPartChanged));

    /// <summary>
    /// Gets or sets the Minutes component of the TimeSpan.
    /// </summary>
    public int Minutes { get => (int)GetValue(MinutesProperty); set => SetValue(MinutesProperty, value); }

    /// <summary>
    /// Gets or sets the visibility of the Years field.
    /// </summary>
    public Visibility YearsVisibility
    {
        get { return (Visibility)GetValue(YearsVisibilityProperty); }
        set { SetValue(YearsVisibilityProperty, value); }
    }
    public static readonly DependencyProperty YearsVisibilityProperty =
        DependencyProperty.Register("YearsVisibility", typeof(Visibility), typeof(TimeSpanEditor), new PropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Gets or sets the visibility of the Months field.
    /// </summary>
    public Visibility MonthsVisibility
    {
        get { return (Visibility)GetValue(MonthsVisibilityProperty); }
        set { SetValue(MonthsVisibilityProperty, value); }
    }
    public static readonly DependencyProperty MonthsVisibilityProperty =
        DependencyProperty.Register("MonthsVisibility", typeof(Visibility), typeof(TimeSpanEditor), new PropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Gets or sets the visibility of the Days field.
    /// </summary>
    public Visibility DaysVisibility
    {
        get { return (Visibility)GetValue(DaysVisibilityProperty); }
        set { SetValue(DaysVisibilityProperty, value); }
    }
    public static readonly DependencyProperty DaysVisibilityProperty =
        DependencyProperty.Register("DaysVisibility", typeof(Visibility), typeof(TimeSpanEditor), new PropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Gets or sets the visibility of the Hours field.
    /// </summary>
    public Visibility HoursVisibility
    {
        get { return (Visibility)GetValue(HoursVisibilityProperty); }
        set { SetValue(HoursVisibilityProperty, value); }
    }
    public static readonly DependencyProperty HoursVisibilityProperty =
        DependencyProperty.Register("HoursVisibility", typeof(Visibility), typeof(TimeSpanEditor), new PropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Gets or sets the visibility of the Minutes field.
    /// </summary>
    public Visibility MinutesVisibility
    {
        get { return (Visibility)GetValue(MinutesVisibilityProperty); }
        set { SetValue(MinutesVisibilityProperty, value); }
    }
    public static readonly DependencyProperty MinutesVisibilityProperty =
        DependencyProperty.Register("MinutesVisibility", typeof(Visibility), typeof(TimeSpanEditor), new PropertyMetadata(Visibility.Visible));

    /// <summary>
    /// Updates the visibility of each time unit based on IsReadOnly state and values.
    /// </summary>
    private void UpdateVisibilities()
    {
        // If the control is editable (not ReadOnly), all fields must be visible to allow input.
        if (!IsReadOnly)
        {
            YearsVisibility = MonthsVisibility = DaysVisibility = HoursVisibility = MinutesVisibility = Visibility.Visible;
            return;
        }

        // In ReadOnly mode, we hide leading zero units for a cleaner display.
        // The logic is: once a unit is non-zero, all subsequent units (smaller timeframes) are shown.
        bool show = false;

        // Check each unit. The '|=' operator sets 'show' to true if the current unit is > 0,
        // and it stays true for all subsequent units.
        YearsVisibility = (show |= Years > 0) ? Visibility.Visible : Visibility.Collapsed;
        MonthsVisibility = (show |= Months > 0) ? Visibility.Visible : Visibility.Collapsed;
        DaysVisibility = (show |= Days > 0) ? Visibility.Visible : Visibility.Collapsed;
        HoursVisibility = (show |= Hours > 0) ? Visibility.Visible : Visibility.Collapsed;
        MinutesVisibility = Visibility.Visible; // Always show at least minutes (e.g. "0m")
    }

    /// <summary>
    /// Callback invoked when any of the component properties (Days, Hours, Minutes) change.
    /// Updates the main Value property.
    /// </summary>
    /// <param name="d">The dependency object (TimeSpanEditor).</param>
    /// <param name="e">The event arguments.</param>
    private static void OnPartChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var control = (TimeSpanEditor)d;
        
        // Update visibility immediately when a part changes (e.g. user types a number)
        control.UpdateVisibilities();
        if (control._isUpdating) return;
        
        try
        {
            // Construct new TimeSpan from parts
            // Approximation: 1 Year = 365 days, 1 Month = 30 days
            long totalDays = (long)control.Years * 365 + (long)control.Months * 30 + control.Days;
            
            // Validate bounds for TimeSpan (which takes int days)
            // TimeSpan limits are tighter than int.MaxValue (approx 10.6 million days vs 2 billion)
            if (totalDays > TimeSpan.MaxValue.TotalDays || totalDays < TimeSpan.MinValue.TotalDays)
                throw new ArgumentOutOfRangeException();

            control._isUpdating = true;
            control.Value = new TimeSpan((int)totalDays, control.Hours, control.Minutes, 0);
            control._isUpdating = false;
        }
        catch (ArgumentOutOfRangeException)
        {
            // If the calculated TimeSpan is invalid (e.g., overflow), revert the UI fields to match the last valid Value.
            control._isUpdating = true;
            if (control.Value is TimeSpan ts)
            {
                int totalDays = ts.Days;
                control.Years = totalDays / 365;
                int remainderDays = totalDays % 365;
                control.Months = remainderDays / 30;
                control.Days = remainderDays % 30;
                control.Hours = ts.Hours;
                control.Minutes = ts.Minutes;
            }
            else
            {
                control.Years = 0;
                control.Months = 0;
                control.Days = 0;
                control.Hours = 0;
                control.Minutes = 0;
            }
            control._isUpdating = false;
        }
    }
}