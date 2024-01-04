using System;
using System.Timers;
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
using System.Diagnostics;
using System.Windows.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace MonsterClicker
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// Monster Clicker Game
    /// --------------------------------------
    /// Player must click the monster to kill them and gain gold.
    /// With the gold they can buy upgrades that automate their income.
    /// Requirements:
    /// - Game starts with gold per second at 0 and a visible monster image with transparent background (PNG)
    /// - Imagebrush for background of the app
    /// - Player can click the monster to gain gold.
    /// - Clicking the monster shrinks the image
    /// - Clicking an upgrade checks the amount of gold in order to unlock.
    /// - Upgrades increase gold per second.




    public partial class MainWindow : Window
    {
        public class Upgrade
        {
            //Constructor for shop items, all properties tied to the shop mechanic
            public string Item { get; set; }

            public int Price { get; set; }

            public double Goldps { get; set; }

            public double Gpms { get; set; }
            public int Amount { get; set; }

        }

        // Variables
        public int Gold;
        public int newGold;
        public double goldClick;
        public int clickCount;

        public int currentWidth = 240;
        public int currentHeight = 360;
        public int newWidth = 250;
        public int newHeight = 370;

        public double goldPerSecond = 0;

        public Timer gameTime;
        public int tickCounter = 0;
        public int currentTick = 0;
        public int CounterMax = 10;

        public int shopIndex;

        List<Upgrade> LstUpgrades = new List<Upgrade>(); //Initialize List
        System.Windows.Controls.Image monsterImg = new System.Windows.Controls.Image(); //Initialize image

        public MainWindow()
        {
            // Application starts.
            InitializeComponent();

        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {

            //Load shoplist, monster image and start the timer.
            ShopList();
            MonsterImage(sender, e);
            GameTimeStart();
            InitClock();
        }

        DispatcherTimer gameClock = new DispatcherTimer();
        int time = 0;
        private void InitClock()
        {
            gameClock.Tick += new EventHandler(GameClock_Tick);
            gameClock.Interval = TimeSpan.FromMilliseconds(10);
            gameClock.Start();
        }

        private void GameClock_Tick(object sender, EventArgs e)
        {
            time += 1;
            Gold += (int)goldPerSecond;
        }


        private void GameTimeStart()
        {
            //Check if gameTime is enabled, if not, initialize gameTime.
            if (gameTime == null)
            {
                gameTime = new Timer();
            }

            // Make a new interval for time that 'ticks'every 10 milliseconds, executing the time_Tick event.
            gameTime.Interval = 10;
            gameTime.Elapsed += time_Tick;
            gameTime.Start();

        }

        private void time_Tick(object sender, ElapsedEventArgs e)
        {

            //Every tick, update labels.
            tickCounter++;

            Dispatcher.Invoke(() =>
            {
                DrawLabels();
            });
            goldClick++;
            //Reset click counter.
            clickCount = 0;
        }

        private void DrawLabels()
        {
            LblTicks.Content = tickCounter.ToString();
            LblGold.Content = Gold.ToString();
            LblGoldps.Content = goldPerSecond.ToString();
        }

        private void ShopList()
        {
            LstUpgrades.Add(new Upgrade { Item = "Sword", Price = 15, Goldps = 0.1, Gpms = 0.001, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Adventurers", Price = 100, Goldps = 1, Gpms = 0.01, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Knights", Price = 1100, Goldps = 8, Gpms = 0.08, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Wizards", Price = 12000, Goldps = 47, Gpms = 0.47, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Adventurer's Guild", Price = 130000, Goldps = 260, Gpms = 2.60, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Knight's Barracks", Price = 1400000, Goldps = 1400, Gpms = 14, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Wizard Colleges", Price = 20000000, Goldps = 7800, Gpms = 78, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Summoners", Price = 330000000, Goldps = 44000, Gpms = 440, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Summoner circles", Price = 510000000, Goldps = 260000, Gpms = 2600, Amount = 0 });
            LstUpgrades.Add(new Upgrade { Item = "Dragon", Price = 750000000, Goldps = 16000000, Gpms = 16000, Amount = 0 });
            LstBUpgrades.ItemsSource = LstUpgrades;



        }

        private void MonsterImage(object sender, RoutedEventArgs e)
        {
            monsterImg = sender as System.Windows.Controls.Image;

            if (sender as System.Windows.Controls.Image != null)
            {
                monsterImg.Source = new BitmapImage(new Uri($"/MonsterClicker;component/Resources/Slime01.png", UriKind.Relative));

                monsterImg.Width = 240;
                monsterImg.Height = 360;


            }

        }

        private void ImageSizeClick(object sender, MouseButtonEventArgs e)
        {
            monsterImg.Width = newWidth;
            monsterImg.Height = newHeight;


        }

        private void ImageSizeClick_released(object sender, MouseButtonEventArgs e)
        {
            monsterImg.Width = currentWidth;
            monsterImg.Height = currentHeight;
            if (monsterImg.IsMouseOver)
            {
                Gold++;
                clickCount++;
            }
        }


        private void LstBUpgrades_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Upgrade selectedUpgrade = (Upgrade)LstBUpgrades.SelectedItem;

            if (Gold >= selectedUpgrade.Price)
            {
                Gold = Gold - selectedUpgrade.Price;
                selectedUpgrade.Amount++;
                LstBUpgrades.Items.Refresh();
                goldPerSecond += Convert.ToDouble(selectedUpgrade.Amount * selectedUpgrade.Goldps);
            }
            else
            {
                MessageBox.Show("Not enough gold.");
            }

        }


    }
}
