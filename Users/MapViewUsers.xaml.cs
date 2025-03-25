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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ComputerClub.Users
{
    /// <summary>
    /// Логика взаимодействия для MapViewUsers.xaml
    /// </summary>
    public partial class MapViewUsers : UserControl
    {
        public MapViewUsers()
        {
            InitializeComponent();
            LoadPCs();
        }
        private readonly DatabaseManager _databaseManager = new DatabaseManager();
         
        private void LoadPCs()
        {

            PcWrapPanel.Children.Clear();

      
            var mainStackPanel = new StackPanel { Orientation = Orientation.Vertical };

            // Создаем заголовки и WrapPanel для каждой зоны
            var zoneContainers = new Dictionary<string, WrapPanel>
    {
        { "Игровая", CreateZoneContainer("Игровая", mainStackPanel) },
        { "VIP", CreateZoneContainer("VIP", mainStackPanel) },
        { "PlayStation", CreateZoneContainer("PlayStation", mainStackPanel) },
        { "Кокпит", CreateZoneContainer("Кокпит", mainStackPanel) }
    };

            // Получаем список ПК из базы данных
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
            // Создаем заголовок зоны
            var zoneLabel = new Label
            {
                Content = zoneName,
                FontWeight = FontWeights.Bold,
                FontSize = 16,
                Margin = new Thickness(0, 0, 0, 5),
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            // Создаем контейнер для кнопок ПК
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
            int currentUserId = GetCurrentUserId();
            var pcInfoWindow = new PCInfoWindow(pc, currentUserId);
            pcInfoWindow.Owner = Window.GetWindow(this);
            pcInfoWindow.ShowDialog();
            LoadPCs(); 
        }

       private int GetCurrentUserId()
        {
            if (CurrentUser.Instance != null)
            {
                return CurrentUser.Instance.Id;
            }
            else
            {
                throw new Exception("Пользователь не авторизован.");
            }
        }
    }
}
