using ComputerClub.BD;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComputerClub.Admin
{
    /// <summary>
    /// Логика взаимодействия для MapView.xaml
    /// </summary>
    public partial class MapView : UserControl
    {

        private readonly DatabaseManager _databaseManager = new DatabaseManager();

        public MapView()
        {
            InitializeComponent();
            LoadPCs();
            _databaseManager.UpdatePCStatusForExpiredBookings();
        }
        private void LoadPCs()
        {
            // Очищаем PcWrapPanel перед добавлением новых элементов
            PcWrapPanel.Children.Clear();

            // Создаем внешнюю StackPanel для вертикального размещения
            var mainStackPanel = new StackPanel { Orientation = Orientation.Vertical };

            // Создаем заголовки и WrapPanel для каждой зоны
            var zoneContainers = new Dictionary<string, WrapPanel>
    {
        { "Игровая", CreateZoneContainer("Игровая", mainStackPanel) },
        { "VIP", CreateZoneContainer("VIP", mainStackPanel) },
        { "PlayStation", CreateZoneContainer("PlayStation", mainStackPanel) },
        { "Кокпит", CreateZoneContainer("Кокпит", mainStackPanel) }
    };

  
            var pcs = _databaseManager.GetPCs();

            foreach (var pc in pcs)
            {
           
                if (zoneContainers.ContainsKey(pc.Zone))
                {
          
                    var button = new Button
                    {
                        Content = $"  {pc.Id}",
                        Width = 100,
                        Height = 50,
                        FontFamily = new FontFamily("Segoe UI"),
                        Margin = new Thickness(10),
                        Background = GetColorByZone(pc.Zone),
                        FontSize = 20,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        Style = CreateRoundedButtonStyle()
                    };

                    
                    button.Click += (sender, args) => ShowPCInfo(pc);

                    zoneContainers[pc.Zone].Children.Add(button);
                }
            }

 
            PcWrapPanel.Children.Add(mainStackPanel);
        }

        private WrapPanel CreateZoneContainer(string zoneName, StackPanel parentPanel)
        {

            var zoneLabel = new Label
            {
                Content = zoneName,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            var wrapPanel = new WrapPanel { Margin = new Thickness(10) };

            parentPanel.Children.Add(zoneLabel);
            parentPanel.Children.Add(wrapPanel);

            return wrapPanel;
        }

        private Brush GetColorByZone(string zone)
        {
            switch (zone)
            {
                case "Игровая":
                    return Brushes.LightBlue;
                case "VIP":
                    return Brushes.Gold;
                case "Playstation":
                    return Brushes.DarkGreen;
                case "Кокпит":
                    return Brushes.Red;
                default:
                    return Brushes.Gray;
            }
        }
        private Style CreateRoundedButtonStyle()
        {
            var style = new Style(typeof(Button));
            var template = new ControlTemplate(typeof(Button));
            var border = new FrameworkElementFactory(typeof(Border));
            border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));
            border.SetBinding(Border.BackgroundProperty, new Binding("Background") { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
            border.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));

            template.VisualTree = border;
            style.Setters.Add(new Setter(Button.TemplateProperty, template));

            return style;
        }
        private void ShowPCInfo(PC pc)
        {
            string Activ = string.Empty;


            if (pc.Activity == true)
            {
                Activ = "Выключен";
            }
            else 
            {
                Activ = "Включен";
            }
            MessageBox.Show($"{Activ}\n" +
                            $"Номер: {pc.Id}\n" +
                            $"Зона: {pc.Zone}\n" +
                            $"Цена за час: {pc.PricePerHour} ₽\n" +
                            $"Видеокарта: {pc.VideoCard}\n" +
                            $"Процессор: {pc.CPU}\n" +
                            $"Монитор: {pc.Monitor}\n" +
                            $"Hertz: {pc.MonitorHertz} Hz\n" +
                            $"Клавиатура: {pc.Keyboard}\n");
        }
    }
}

