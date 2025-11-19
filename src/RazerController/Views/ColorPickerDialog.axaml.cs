using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;

namespace RazerController.Views;

public partial class ColorPickerDialog : Window
{
    private Slider? _redSlider;
    private Slider? _greenSlider;
    private Slider? _blueSlider;
    private Border? _colorPreview;
    
    public byte RedValue { get; set; }
    public byte GreenValue { get; set; }
    public byte BlueValue { get; set; }
    
    public ColorPickerDialog()
    {
        InitializeComponent();
        
        // Get references to controls
        _redSlider = this.FindControl<Slider>("RedSlider");
        _greenSlider = this.FindControl<Slider>("GreenSlider");
        _blueSlider = this.FindControl<Slider>("BlueSlider");
        _colorPreview = this.FindControl<Border>("ColorPreview");
        
        // Initialize slider values after controls are loaded
        this.Opened += (s, e) =>
        {
            if (_redSlider != null) _redSlider.Value = RedValue;
            if (_greenSlider != null) _greenSlider.Value = GreenValue;
            if (_blueSlider != null) _blueSlider.Value = BlueValue;
            
            // Subscribe to value changes
            if (_redSlider != null) _redSlider.PropertyChanged += Slider_PropertyChanged;
            if (_greenSlider != null) _greenSlider.PropertyChanged += Slider_PropertyChanged;
            if (_blueSlider != null) _blueSlider.PropertyChanged += Slider_PropertyChanged;
            
            UpdateColorPreview();
        };
    }
    
    private void Slider_PropertyChanged(object? sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Slider.ValueProperty)
        {
            UpdateColorPreview();
        }
    }
    
    private void UpdateColorPreview()
    {
        if (_colorPreview == null || _redSlider == null || _greenSlider == null || _blueSlider == null)
            return;
            
        byte r = (byte)Math.Clamp(_redSlider.Value, 0, 255);
        byte g = (byte)Math.Clamp(_greenSlider.Value, 0, 255);
        byte b = (byte)Math.Clamp(_blueSlider.Value, 0, 255);
        
        _colorPreview.Background = new SolidColorBrush(Color.FromRgb(r, g, b));
    }
    
    private void OK_Click(object? sender, RoutedEventArgs e)
    {
        if (_redSlider != null) RedValue = (byte)Math.Clamp(_redSlider.Value, 0, 255);
        if (_greenSlider != null) GreenValue = (byte)Math.Clamp(_greenSlider.Value, 0, 255);
        if (_blueSlider != null) BlueValue = (byte)Math.Clamp(_blueSlider.Value, 0, 255);
        
        Close(true);
    }
    
    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
}
