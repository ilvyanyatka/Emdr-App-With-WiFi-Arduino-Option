using Syncfusion.XForms.Buttons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Emdr_App
{
    #region Xamarin.Forms Chips Color Picker

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ColorPicker : ContentView, INotifyPropertyChanged
    {
        #region Members

        private ObservableCollection<Color> colors;
        private object selectedItem;
        private Color selectedColor;
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructor

        public ColorPicker()
        {
            InitializeComponent();
            Colors = new ObservableCollection<Color>();
            Colors.Add(Color.Green);
            Colors.Add(Color.GreenYellow);
            Colors.Add(Color.DarkGreen);
            Colors.Add(Color.White);
            Colors.Add(Color.Blue);
            Colors.Add(Color.DarkBlue);
            Colors.Add(Color.Turquoise); 
            Colors.Add(Color.Red);
            Colors.Add(Color.Orange);
            Colors.Add(Color.Yellow);
            Colors.Add(Color.PaleVioletRed);
           
            /*

            foreach (var color in typeof(Color).GetFields())
            {
                Colors.Add((Color)typeof(Color).GetField(color.Name).GetValue(this));
            }
            */
            this.BindingContext = this;
        }

        #endregion

        #region Properties

        public ObservableCollection<Color> Colors
        {
            get
            {
                return colors;
            }
            set
            {
                colors = value;
                OnPropertyChanged("Employees");
            }
        }

        public object SelectedItem
        {
            get
            {
                return selectedItem;
            }
            set
            {
                selectedItem = value;
                OnPropertyChanged("SelectedItem");
            }
        }

        public Color SelectedColor
        {
            get
            {
                return selectedColor;
            }
            set
            {
                selectedColor = value;
                OnPropertyChanged("SelectedColor");
            }
        }

        #endregion

        #region On Property Changed

        public void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        #endregion

    }

    #endregion

    #region  Color to chip converter
    public class ColorToChipConverter : IValueConverter
    {
        #region Member

        SfChip selectedChip = null;

        #endregion

        #region Convert
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ObservableCollection<SfChip> colorChips = new ObservableCollection<SfChip>();
            foreach (var item in value as ObservableCollection<Color>)
            {

                var colorChip = new SfChip() { BackgroundColor = (Color)item, ShowSelectionIndicator = true, SelectionIndicatorColor = Color.Transparent, CornerRadius = 20, WidthRequest = 40, HeightRequest = 40, Margin = 10, BorderWidth = 1 };
                colorChip.BorderColor = Color.FromRgb(-(colorChip.BackgroundColor.R - 1), -(colorChip.BackgroundColor.G - 1), -(colorChip.BackgroundColor.B - 1));
                var mean = (colorChip.BackgroundColor.R + colorChip.BackgroundColor.G + colorChip.BackgroundColor.B) / 3;
                colorChip.BorderColor = mean < 0.5 ? Color.White : Color.Black;

                colorChip.Clicked += ColorChip_Clicked;
                colorChips.Add(colorChip);

            }
            return colorChips;
        }

        #endregion

        #region Event

        private void ColorChip_Clicked(object sender, EventArgs e)
        {
            if (selectedChip != null)
            {
                selectedChip.ShowSelectionIndicator = false;
                selectedChip.BorderWidth = 1;
            }

            selectedChip = (sender as SfChip);
            (selectedChip.Parent as FlexLayout).BackgroundColor = selectedChip.BackgroundColor;

            selectedChip.ShowSelectionIndicator = true;
            selectedChip.SelectionIndicatorColor = selectedChip.BorderColor;
            selectedChip.BorderWidth = 3;
        }

        #endregion

        #region Convert Back

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

    #endregion

}

