using ComputerClub.BD;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace ComputerClub.Admin
{
    public partial class DashboardControl : UserControl, INotifyPropertyChanged
    {
        private readonly DatabaseManager _db = new DatabaseManager();

        private double originalWidth = 300;
        private double originalHeight = 200;
        private BlurEffect blurEffect = new BlurEffect { Radius = 5 };

        private decimal _productsTotal;
        private decimal _servicesTotal;
        private decimal _bookingsTotal;
        private decimal _totalRevenue;
        private DateTime? _selectedDate;
        private PlotModel _plotModel;

        public DashboardControl()
        {
            InitializeComponent();
            DataContext = this;

        
            PlotModel = new PlotModel { 
                Title = "", 
                PlotAreaBorderColor = OxyColors.Transparent
            };

         
            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(SelectedDate) && SelectedDate.HasValue)
                {
                    LoadCustomDate(SelectedDate.Value);
                }
            };

            LoadToday();
        }

        private void ZoomButton_Click(object sender, RoutedEventArgs e)
        {
            var newPlotModel = CreateNewPlotModel();
            
            var detailWindow = new ChartDetailWindow(newPlotModel);
            detailWindow.Show();
        }

        private PlotModel CreateNewPlotModel()
        {
            var newPlotModel = new PlotModel { 
                Title = PlotModel.Title, 
                PlotAreaBorderColor = PlotModel.PlotAreaBorderColor
            };
            
            foreach (var series in PlotModel.Series)
            {
                if (series is LineSeries lineSeries)
                {
                    var newSeries = new LineSeries
                    {
                        Title = lineSeries.Title,
                        Color = lineSeries.Color,
                        MarkerType = lineSeries.MarkerType,
                        MarkerSize = lineSeries.MarkerSize,
                        MarkerStroke = lineSeries.MarkerStroke,
                        MarkerFill = lineSeries.MarkerFill
                    };
                    
                    // Copy all points
                    foreach (var point in lineSeries.Points)
                    {
                        newSeries.Points.Add(new DataPoint(point.X, point.Y));
                    }
                    
                    newPlotModel.Series.Add(newSeries);
                }
            }
            
            // Copy all axes
            foreach (var axis in PlotModel.Axes)
            {
                if (axis is LinearAxis linearAxis)
                {
                    var newAxis = new LinearAxis
                    {
                        Position = linearAxis.Position,
                        Title = linearAxis.Title,
                        MajorGridlineStyle = linearAxis.MajorGridlineStyle,
                        MinorGridlineStyle = linearAxis.MinorGridlineStyle,
                        Minimum = linearAxis.Minimum,
                        Maximum = linearAxis.Maximum,
                        MajorStep = linearAxis.MajorStep
                    };
                    
                    // Copy label formatter if exists
                    if (linearAxis.LabelFormatter != null)
                    {
                        newAxis.LabelFormatter = linearAxis.LabelFormatter;
                    }
                    
                    newPlotModel.Axes.Add(newAxis);
                }
            }
            
            // Copy legends
            foreach (var legend in PlotModel.Legends)
            {
                var newLegend = new OxyPlot.Legends.Legend
                {
                    LegendPosition = legend.LegendPosition,
                    LegendPlacement = legend.LegendPlacement
                };
                
                newPlotModel.Legends.Add(newLegend);
            }
            
            return newPlotModel;
        }

        public PlotModel PlotModel
        {
            get => _plotModel;
            set
            {
                _plotModel = value;
                OnPropertyChanged();
            }
        }

        public decimal ProductsTotal
        {
            get => _productsTotal;
            set
            {
                _productsTotal = value;
                OnPropertyChanged();
                UpdateTotalRevenue();
            }
        }

        public decimal ServicesTotal
        {
            get => _servicesTotal;
            set
            {
                _servicesTotal = value;
                OnPropertyChanged();
                UpdateTotalRevenue();
            }
        }

        public decimal BookingsTotal
        {
            get => _bookingsTotal;
            set
            {
                _bookingsTotal = value;
                OnPropertyChanged();
                UpdateTotalRevenue();
            }
        }

        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
            }
        }

        private void UpdateTotalRevenue()
        {
            TotalRevenue = ProductsTotal + ServicesTotal + BookingsTotal;
        }

        private void Button_Click_Today(object sender, RoutedEventArgs e) => LoadToday();
        private void Button_Click_Week(object sender, RoutedEventArgs e) => LoadWeek();
        private void Button_Click_Month(object sender, RoutedEventArgs e) => LoadMonth();

        private void LoadToday()
        {
            var date = DateTime.Now;
            LoadData(date.Date, date.Date.AddDays(1));
            
         
            UpdateChartForToday();
        }

        private void LoadWeek()
        {
            var startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
            LoadData(startDate, startDate.AddDays(7));
            
          
            UpdateChartForWeek(startDate);
        }

        private void LoadMonth()
        {
            var startDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            LoadData(startDate, startDate.AddMonths(1));
            
         
            UpdateChartForMonth(startDate);
        }

        private void LoadCustomDate(DateTime date)
        {
            LoadData(date.Date, date.Date.AddDays(1));
            
       
            UpdateChartForCustomDate(date);
        }

        private void LoadData(DateTime startDate, DateTime endDate)
        {
            ProductsTotal = _db.GetPaymentsTotal("Товар", startDate, endDate);
            ServicesTotal = _db.GetPaymentsTotal("Другое", startDate, endDate);
            BookingsTotal = _db.GetPaymentsTotal("Бронирование ПК", startDate, endDate);
        }

        private void UpdateChartForToday()
        {
            var today = DateTime.Today;
            var productValues = new List<DataPoint>();
            var serviceValues = new List<DataPoint>();
            var bookingValues = new List<DataPoint>();

         
            for (int i = 0; i < 24; i++)
            {
                var startHour = today.AddHours(i);
                var endHour = today.AddHours(i + 1);
                
                productValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Товар", startHour, endHour)));
                serviceValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Другое", startHour, endHour)));
                bookingValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Бронирование ПК", startHour, endHour)));
            }

            string[] hourLabels = new string[24];
            for (int i = 0; i < 24; i++)
            {
                hourLabels[i] = i.ToString() + ":00";
            }

            UpdateChart("Часы", productValues, serviceValues, bookingValues, 0, 23, 2, hourLabels);
        }

        private void UpdateChartForWeek(DateTime startDate)
        {
            var productValues = new List<DataPoint>();
            var serviceValues = new List<DataPoint>();
            var bookingValues = new List<DataPoint>();

        
            for (int i = 0; i < 7; i++)
            {
                var day = startDate.AddDays(i);
                var nextDay = day.AddDays(1);
                
                productValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Товар", day, nextDay)));
                serviceValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Другое", day, nextDay)));
                bookingValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Бронирование ПК", day, nextDay)));
            }

            string[] dayLabels = { "Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб" };
            UpdateChart("Дни недели", productValues, serviceValues, bookingValues, 0, 6, 1, dayLabels);
        }

        private void UpdateChartForMonth(DateTime startDate)
        {
            var productValues = new List<DataPoint>();
            var serviceValues = new List<DataPoint>();
            var bookingValues = new List<DataPoint>();
            var daysInMonth = DateTime.DaysInMonth(startDate.Year, startDate.Month);
            
          
            int weekCount = (daysInMonth + 6) / 7;
            
            for (int week = 0; week < weekCount; week++)
            {
                var weekStart = startDate.AddDays(week * 7);
                var weekEnd = weekStart.AddDays(7);
                if (weekEnd > startDate.AddMonths(1))
                    weekEnd = startDate.AddMonths(1);
                
                productValues.Add(new DataPoint(week, (double)_db.GetPaymentsTotal("Товар", weekStart, weekEnd)));
                serviceValues.Add(new DataPoint(week, (double)_db.GetPaymentsTotal("Другое", weekStart, weekEnd)));
                bookingValues.Add(new DataPoint(week, (double)_db.GetPaymentsTotal("Бронирование ПК", weekStart, weekEnd)));
            }

            var weekLabels = new string[weekCount];
            for (int i = 0; i < weekCount; i++)
            {
                var weekStart = startDate.AddDays(i * 7);
                var weekEnd = weekStart.AddDays(6);
                if (weekEnd > startDate.AddMonths(1).AddDays(-1))
                    weekEnd = startDate.AddMonths(1).AddDays(-1);
                
                weekLabels[i] = $"{weekStart.Day}-{weekEnd.Day}";
            }

            UpdateChart("Недели", productValues, serviceValues, bookingValues, 0, weekCount - 1, 1, weekLabels);
        }

        private void UpdateChartForCustomDate(DateTime date)
        {
            var selectedDate = date.Date;
            var productValues = new List<DataPoint>();
            var serviceValues = new List<DataPoint>();
            var bookingValues = new List<DataPoint>();

            for (int i = 0; i < 24; i++)
            {
                var startHour = selectedDate.AddHours(i);
                var endHour = selectedDate.AddHours(i + 1);
                
                productValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Товар", startHour, endHour)));
                serviceValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Другое", startHour, endHour)));
                bookingValues.Add(new DataPoint(i, (double)_db.GetPaymentsTotal("Бронирование ПК", startHour, endHour)));
            }

            string[] hourLabels = new string[24];
            for (int i = 0; i < 24; i++)
            {
                hourLabels[i] = i.ToString() + ":00";
            }

            UpdateChart("Часы", productValues, serviceValues, bookingValues, 0, 23, 2, hourLabels);
        }

        private void UpdateChart(string xAxisTitle, List<DataPoint> productValues, List<DataPoint> serviceValues, 
                                List<DataPoint> bookingValues, double min, double max, double step, 
                                string[] customLabels = null)
        {
          
            var plotModel = new PlotModel { 
                Title = "", 
                PlotAreaBorderColor = OxyColors.Transparent
            };

            
            var productSeries = new LineSeries { 
                Title = "Товары", 
                Color = OxyColor.FromRgb(206, 0, 0),
                MarkerType = MarkerType.Circle,
                MarkerSize = 4,
                MarkerStroke = OxyColor.FromRgb(206, 0, 0),
                MarkerFill = OxyColor.FromRgb(206, 0, 0)
            };
            productSeries.Points.AddRange(productValues);
            
         
            
            var bookingSeries = new LineSeries { 
                Title = "Бронирования", 
                Color = OxyColor.FromRgb(0, 150, 0),
                MarkerType = MarkerType.Diamond,
                MarkerSize = 4,
                MarkerStroke = OxyColor.FromRgb(0, 150, 0),
                MarkerFill = OxyColor.FromRgb(0, 150, 0)
            };
            bookingSeries.Points.AddRange(bookingValues);

          
            plotModel.Series.Add(productSeries);
      
            plotModel.Series.Add(bookingSeries);

           
            var xAxis = new LinearAxis {
                Position = AxisPosition.Bottom,
                Title = xAxisTitle,
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.None,
                Minimum = min,
                Maximum = max,
                MajorStep = step
            };

            if (customLabels != null)
            {
                xAxis.LabelFormatter = (double value) => {
                    int index = (int)value;
                    if (index >= 0 && index < customLabels.Length)
                        return customLabels[index];
                    return "";
                };
            }

            var yAxis = new LinearAxis {
                Position = AxisPosition.Left,
                Title = "Доход (₽)",
                MajorGridlineStyle = LineStyle.Dot,
                MinorGridlineStyle = LineStyle.None,
                Minimum = 0
            };

            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

         
            var legend = new OxyPlot.Legends.Legend
            {
                LegendPosition = OxyPlot.Legends.LegendPosition.RightTop,
                LegendPlacement = OxyPlot.Legends.LegendPlacement.Inside
            };
            
            plotModel.Legends.Add(legend);

        
            PlotModel = plotModel;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}