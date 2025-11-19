using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using System;
using System.Collections.Generic;

namespace RazerController.Views;

public partial class ColorPickerDialog : Window
{
    private Border? _colorPreview;
    private ItemsControl? _colorPalette;
    private byte _selectedRed;
    private byte _selectedGreen;
    private byte _selectedBlue;
    
    public byte RedValue { get; set; }
    public byte GreenValue { get; set; }
    public byte BlueValue { get; set; }
    
    public ColorPickerDialog()
    {
        InitializeComponent();
        
        // Initialize after controls are loaded
        this.Opened += (s, e) =>
        {
            // Get references to controls after visual tree is built
            _colorPreview = this.FindControl<Border>("ColorPreview");
            _colorPalette = this.FindControl<ItemsControl>("ColorPalette");
            
            _selectedRed = RedValue;
            _selectedGreen = GreenValue;
            _selectedBlue = BlueValue;
            
            InitializeColorPalette();
            UpdateColorPreview();
        };
    }
    
    private void InitializeColorPalette()
    {
        if (_colorPalette == null) return;
        
        var colors = new List<ColorItem>
        {
            // Basic colors - Row 1
            new ColorItem("Red", Color.FromRgb(255, 0, 0)),
            new ColorItem("Orange", Color.FromRgb(255, 127, 0)),
            new ColorItem("Yellow", Color.FromRgb(255, 255, 0)),
            new ColorItem("Lime", Color.FromRgb(127, 255, 0)),
            new ColorItem("Green", Color.FromRgb(0, 255, 0)),
            new ColorItem("Teal", Color.FromRgb(0, 255, 127)),
            new ColorItem("Cyan", Color.FromRgb(0, 255, 255)),
            new ColorItem("Sky Blue", Color.FromRgb(0, 127, 255)),
            
            // Basic colors - Row 2
            new ColorItem("Blue", Color.FromRgb(0, 0, 255)),
            new ColorItem("Purple", Color.FromRgb(127, 0, 255)),
            new ColorItem("Magenta", Color.FromRgb(255, 0, 255)),
            new ColorItem("Pink", Color.FromRgb(255, 0, 127)),
            new ColorItem("White", Color.FromRgb(255, 255, 255)),
            new ColorItem("Light Gray", Color.FromRgb(192, 192, 192)),
            new ColorItem("Gray", Color.FromRgb(128, 128, 128)),
            new ColorItem("Dark Gray", Color.FromRgb(64, 64, 64)),
            
            // Darker variations - Row 3
            new ColorItem("Dark Red", Color.FromRgb(192, 0, 0)),
            new ColorItem("Dark Orange", Color.FromRgb(192, 96, 0)),
            new ColorItem("Dark Yellow", Color.FromRgb(192, 192, 0)),
            new ColorItem("Dark Lime", Color.FromRgb(96, 192, 0)),
            new ColorItem("Dark Green", Color.FromRgb(0, 192, 0)),
            new ColorItem("Dark Teal", Color.FromRgb(0, 192, 96)),
            new ColorItem("Dark Cyan", Color.FromRgb(0, 192, 192)),
            new ColorItem("Dark Sky", Color.FromRgb(0, 96, 192)),
            
            // Darker variations - Row 4
            new ColorItem("Dark Blue", Color.FromRgb(0, 0, 192)),
            new ColorItem("Dark Purple", Color.FromRgb(96, 0, 192)),
            new ColorItem("Dark Magenta", Color.FromRgb(192, 0, 192)),
            new ColorItem("Dark Pink", Color.FromRgb(192, 0, 96)),
            new ColorItem("Light Pink", Color.FromRgb(255, 192, 203)),
            new ColorItem("Peach", Color.FromRgb(255, 218, 185)),
            new ColorItem("Light Yellow", Color.FromRgb(255, 255, 224)),
            new ColorItem("Light Green", Color.FromRgb(144, 238, 144)),
            
            // Deep variations - Row 5
            new ColorItem("Maroon", Color.FromRgb(128, 0, 0)),
            new ColorItem("Brown", Color.FromRgb(165, 42, 42)),
            new ColorItem("Olive", Color.FromRgb(128, 128, 0)),
            new ColorItem("Forest Green", Color.FromRgb(34, 139, 34)),
            new ColorItem("Navy", Color.FromRgb(0, 0, 128)),
            new ColorItem("Indigo", Color.FromRgb(75, 0, 130)),
            new ColorItem("Violet", Color.FromRgb(138, 43, 226)),
            new ColorItem("Black", Color.FromRgb(0, 0, 0)),
        };
        
        _colorPalette.ItemsSource = colors;
    }
    
    private void ColorButton_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Border border && border.Background is SolidColorBrush brush)
        {
            var color = brush.Color;
            _selectedRed = color.R;
            _selectedGreen = color.G;
            _selectedBlue = color.B;
            UpdateColorPreview();
        }
    }
    
    private void UpdateColorPreview()
    {
        if (_colorPreview == null) return;
        _colorPreview.Background = new SolidColorBrush(Color.FromRgb(_selectedRed, _selectedGreen, _selectedBlue));
    }
    
    private void OK_Click(object? sender, RoutedEventArgs e)
    {
        RedValue = _selectedRed;
        GreenValue = _selectedGreen;
        BlueValue = _selectedBlue;
        Close(true);
    }
    
    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close(false);
    }
    
    public class ColorItem
    {
        public string Name { get; }
        public Color Color { get; }
        
        public ColorItem(string name, Color color)
        {
            Name = name;
            Color = color;
        }
    }
}
