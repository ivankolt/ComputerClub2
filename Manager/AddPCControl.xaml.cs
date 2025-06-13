using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Npgsql;
using System.Data;
using ComputerClub.BD;

namespace ComputerClub.Manager
{
    public partial class AddPCControl : UserControl
    {
        private readonly DatabaseManager _db = new DatabaseManager();
        private List<Equipment> _equipmentList = new List<Equipment>();
        private ObservableCollection<PCInfo> _pcList = new ObservableCollection<PCInfo>();

        public AddPCControl()
        {
            InitializeComponent();
            LoadEquipment();
            LoadZones();
            LoadPCs();
            
            dgPCs.ItemsSource = _pcList;
        }

        private void LoadEquipment()
        {
            try
            {
                _equipmentList = _db.GetAllEquipment();
                cmbEquipment.ItemsSource = _equipmentList;
                cmbEquipment.DisplayMemberPath = "DisplayName";
                cmbEquipment.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке оборудования: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadZones()
        {
            try
            {
                var zones = _db.GetPCZones();
                cmbZone.ItemsSource = zones;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке зон: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void LoadPCs()
        {
            try
            {
                _pcList.Clear();
                var pcs = _db.GetAllPCs();
                foreach (var pc in pcs)
                {
                    _pcList.Add(pc);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке списка ПК: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnAddPC_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (cmbEquipment.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите оборудование", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (cmbZone.SelectedItem == null)
                {
                    MessageBox.Show("Пожалуйста, выберите зону", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!decimal.TryParse(txtPricePerHour.Text, out decimal pricePerHour) || pricePerHour < 0)
                {
                    MessageBox.Show("Пожалуйста, введите корректную цену за час", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                int equipmentId = (int)cmbEquipment.SelectedValue;
                string zone = cmbZone.SelectedItem.ToString();

                bool success = _db.AddNewPC(pricePerHour, zone, equipmentId);
                
                if (success)
                {
                    MessageBox.Show("ПК успешно добавлен", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtPricePerHour.Text = "";
                    cmbEquipment.SelectedIndex = -1;
                    cmbZone.SelectedIndex = -1;
                    
                    // Refresh PC list
                    LoadPCs();
                }
                else
                {
                    MessageBox.Show("Не удалось добавить ПК", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении ПК: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void btnDeletePC_Click(object sender, RoutedEventArgs e)
        {
            if (dgPCs.SelectedItem is PCInfo selectedPC)
            {
                var result = MessageBox.Show(
                    $"Вы действительно хотите удалить ПК #{selectedPC.Id}?\nЭто действие нельзя отменить.",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        bool success = _db.DeletePC(selectedPC.Id);
                        
                        if (success)
                        {
                            MessageBox.Show("ПК успешно удален", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                            LoadPCs();
                        }
                        else
                        {
                            MessageBox.Show("Не удалось удалить ПК. Возможно, он используется в бронированиях.", 
                                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении ПК: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите ПК для удаления", "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

    public class Equipment
    {
        public int Id { get; set; }
        public string VideoCard { get; set; }
        public string CPU { get; set; }
        public string Monitor { get; set; }
        public string Keyboard { get; set; }
        public int MonitorHertz { get; set; }
        
        public string DisplayName => $"{VideoCard} | {CPU} | {Monitor} ({MonitorHertz}Hz) | {Keyboard}";
    }
    
    public class PCInfo
    {
        public int Id { get; set; }
        public string Zone { get; set; }
        public decimal PricePerHour { get; set; }
        public int EquipmentId { get; set; }
        public string EquipmentInfo { get; set; }
    }
}
