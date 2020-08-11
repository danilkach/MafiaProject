using System;
using System.Collections.Generic;
using System.Drawing;
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
using MafiaClassLibrary;
using System.Text.RegularExpressions;
using Microsoft.Win32;
using System.IO;

namespace MafiaGame
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RoleEditor editor = new RoleEditor();
            editor.ShowDialog();
        }

        private void Ellipse_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Ellipse_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (IPTextbox.Text != "" && PortTextbox.Text != "")
            {
                GameWindow window = new GameWindow(IPTextbox.Text, int.Parse(PortTextbox.Text), (ImageSource)(new ImageSourceConverter()).ConvertFrom(playerImage.Source), usernameTextbox.Text);
                this.Hide();
                window.ShowDialog();
            }
        }

        private void MakeServerButtonClick_Click(object sender, RoutedEventArgs e)
        {
            createServerBlock.Visibility = Visibility.Visible;
            if (createServerBlock.Visibility == Visibility.Visible && roleBorder.Visibility == Visibility.Visible)
            {
                if (IPTextbox.Text != "" && PortTextbox.Text != "")
                    if (userAmount.Text != "0")
                    {
                        Dictionary<Role, int> roles = new Dictionary<Role, int>();
                        for (int i = 0; i < rolePicker.RowDefinitions.Count; i++)
                        {
                            Label label = (Label)GetElementInGridPosition(rolePicker, i, 0);
                            var element = GetElementInGridPosition(rolePicker, i, 1);
                            if(element is CheckBox)
                            { 
                                if((bool)((element as CheckBox).IsChecked))
                                roles.Add(chosenPack.Roles.Find( (r) => r.Name == (string)label.Content), 1);
                            }
                            else
                            {
                                if((element as TextBox).Text != "0" && (element as TextBox).Text != "")
                                    roles.Add(chosenPack.Roles.Find((r) => r.Name == (string)label.Content), int.Parse((element as TextBox).Text));
                            }
                        }
                        ImageSourceConverter converter = new ImageSourceConverter();
                        Host host = new Host(IPTextbox.Text, int.Parse(PortTextbox.Text));
                        GameWindow window = new GameWindow(IPTextbox.Text, int.Parse(PortTextbox.Text), playerImage.Source, usernameTextbox.Text);
                        this.Hide();
                        window.ShowDialog();
                    }
            }
        }
        private UIElement GetElementInGridPosition(Grid grid, int row, int column)
        {
            foreach (UIElement element in grid.Children)
            {
                if (Grid.GetColumn(element) == column && Grid.GetRow(element) == row)
                    return element;
            }

            return null;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Filter = "Кастомные пакеты для игры в мафию (*.ppak)|*.ppak";
            dialog.ShowDialog();
            if (File.Exists(dialog.FileName))
            {
                try
                {
                    Pack loadedPack = new Pack();
                    loadedPack.LoadPack(dialog.FileName);
                    int i = 0;
                    Style roleLabelStyle = new Style();
                    chosenPackLabel.Content = dialog.FileName;
                    foreach (Role r in loadedPack.Roles)
                    {
                        int k = 0;
                        RowDefinition newRow = new RowDefinition();
                        newRow.Height = new GridLength(50);
                        rolePicker.RowDefinitions.Add(newRow);
                        Label roleLabel = new Label();
                        roleLabel.Name = "rolelabel" + i;
                        roleLabel.Content = r.Name;
                        roleLabel.VerticalAlignment = VerticalAlignment.Center;
                        Grid.SetRow(roleLabel, i);
                        Grid.SetColumn(roleLabel, k++);
                        rolePicker.Children.Add(roleLabel);
                        if (r.Singular)
                        {
                            CheckBox checkbox = new CheckBox();
                            checkbox.VerticalAlignment = VerticalAlignment.Center;
                            checkbox.HorizontalAlignment = HorizontalAlignment.Center;
                            checkbox.Name = "checkbox" + i;
                            Grid.SetRow(checkbox, i);
                            Grid.SetColumn(checkbox, k);
                            rolePicker.Children.Add(checkbox);
                        }
                        else
                        {
                            TextBox textbox = new TextBox();
                            textbox.Text = "0";
                            textbox.PreviewTextInput += TextBox_PreviewTextInput;
                            textbox.VerticalAlignment = VerticalAlignment.Center;
                            textbox.MaxHeight = 30;
                            textbox.MaxWidth = 50;
                            textbox.Name = "textbox" + i;
                            Grid.SetRow(textbox, i);
                            Grid.SetColumn(textbox, k);
                            rolePicker.Children.Add(textbox);
                        }
                        chosenPack = loadedPack;
                        i++;
                    }
                    roleBorder.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Неверный файл, загрузка отменена", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            
        }
        private Pack chosenPack;
    }
}
