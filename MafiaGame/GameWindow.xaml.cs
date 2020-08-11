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
using MafiaClassLibrary;
using System.Drawing;
using System.IO;
using System.Threading;

namespace MafiaGame
{
    /// <summary>
    /// Interaction logic for GameWindow.xaml
    /// </summary>
    public partial class GameWindow : Window
    {
        public void ShowMessage(string message)
        {   
            Dispatcher.Invoke(() => chatLabel.Content += "\n" + message);
        }
        public void RecieveMessage(string message)
        {
            messageBuffer = message;
        }
        public void DrawPlayer(User user)
        {
            Dispatcher.Invoke(() =>
            {
                if (playerRow + 1 < PlayersGrid.RowDefinitions.Count)
                    PlayersGrid.RowDefinitions.Add(new RowDefinition());
                ImageSourceConverter converter = new ImageSourceConverter();
                System.Windows.Controls.Image img = new System.Windows.Controls.Image();
                img.Source = (ImageSource)converter.ConvertFrom(user.PlayerImage);
                img.Width = img.Height = 75;
                Label playerName = new Label();
                StackPanel panel = new StackPanel();
                playerName.FontSize = 16;
                playerName.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                playerName.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                panel.Children.Add(img);
                panel.Children.Add(playerName);
                Grid.SetRow(panel, playerRow);
                Grid.SetColumn(panel, playerColumn);
                PlayersGrid.Children.Add(panel);
                if (playerColumn == 4)
                {
                    playerColumn = 0;
                    playerRow++;
                }
                else
                    playerColumn++;
                playerName.Content = user.Name;
            });
        }
        public GameWindow(string ip, int port, ImageSource imageData, string username)
        {
            InitializeComponent();
            playerRow = playerColumn = 0;
            player = new User();
            player.RegisterMessageHandler(ShowMessage);
            player.RegisterDrawPlayerHandler(DrawPlayer);
            player.Connect(ip, port);
            //player.PlayerImage = imageData;
            player.Name = username;
            DrawPlayer(player);
            player.SendPacket(PacketInfo.ConnectionInfo, null);
        }
        private void sendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (chatTextBox.Text != "")
            {
                player.SendPacket(PacketInfo.ServerMessage, chatTextBox.Text);
                player.Message(player.Name + ": " + chatTextBox.Text);
                chatTextBox.Text = "";
            }
        }
        async private void GameCycle()
        {

        }
        int playerRow, playerColumn;
        string messageBuffer;
        private User player;
    }
}
