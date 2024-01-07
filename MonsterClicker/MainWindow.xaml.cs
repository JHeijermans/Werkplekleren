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
using System.Globalization;
using System.Windows.Media.Animation;

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
    /// - Bought upgrades are visualized.
    /// - Bonus upgrades add a multiplier to previously bought upgrades
    /// ---------------------------------------
    /// To fix:
    /// FIX 01 - Gradually visualize Shop Items




    public partial class MainWindow : Window
    {
        public class Upgrade
        {
            //Constructor for shop items, all properties tied to the shop mechanic
            public string Item { get; set; }
            public int ItemGroup { get; set; }
            public double Price { get; set; }
            public string FormattedPrice
            {
                get { return MainWindow.FormatNumber(this.Price); }
            }
            public double Goldps { get; set; }
            public double Goldpms { get; set; }
            public int Amount { get; set; }
            public string ImagePath { get; set; }
            public int Multiplier { get; set; }
            public bool IsEnabled { get; set; } = false;

        }

        public class Quest
        {
            public string Name { get; set; }
            public string Description { get; set; }
            public int QuestID { get; set; }
            public int Condition { get; set; }

        }

        // Variables
        public double currentGold { get; set; }
        public double totalGold { get; set; }
        public int clickCount { get; set; }
        public double goldPerSecond { get; set; }

        private int clicksThisSecond { get; set; }
        private int totalClicks { get; set; }
        private int totalClick { get; set; }
        private int secondsPassed { get; set; }
        private int tickCounter { get; set; }

        public int goldPerClick = 1;

        private bool imageClick = false;
        private int currentWidth = 180;
        private int currentHeight = 160;
        private int newWidth = 200;
        private int newHeight = 180;

        private List<Upgrade> lstUpgrades = new List<Upgrade>();
        private List<Upgrade> lstBonus = new List<Upgrade>();
        private List<Upgrade> visibleBonuses = new List<Upgrade>();

        private List<Quest> LstQuest = new List<Quest>();


        private System.Windows.Controls.Image monsterImg = new System.Windows.Controls.Image();
        private System.Windows.Controls.Image bonusImg = new System.Windows.Controls.Image();

        private int activeAnimations = 0;
        private const int MaxAnimations = 50;

        DispatcherTimer gameClock = new DispatcherTimer();
        DispatcherTimer gameTime = new DispatcherTimer();

        private MediaPlayer ClickSFX = new MediaPlayer();
        private MediaPlayer BuySFX = new MediaPlayer();

        public MainWindow()
        {
            // Application starts.
            InitializeComponent();

        }

        public void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitClock();
            MonsterImage(sender, e);
            ShopList();
            BonusList();
            QuestList();
            ClickingSystem();

        }

        //Image Clicking functionaliteit
        private void MonsterImage(object sender, RoutedEventArgs e)
        {
            monsterImg = sender as System.Windows.Controls.Image;

            if (sender as System.Windows.Controls.Image != null)
            {
                monsterImg.Source = new BitmapImage(new Uri($"/MonsterClicker;component/Resources/Slime01.png", UriKind.Relative));

                monsterImg.Width = 180;
                monsterImg.Height = 160;
            }

        }

        private void ImageSizeClick(object sender, MouseButtonEventArgs e)
        {
            if (IsMouseOver == true)
            {
                monsterImg.Width = newWidth;
                monsterImg.Height = newHeight;
                imageClick = true;
            }
        }
        private void ImageSizeClick_released(object sender, MouseButtonEventArgs e)
        {
            if (imageClick == true)
            {
                monsterImg.Width = currentWidth;
                monsterImg.Height = currentHeight;
                clickCount++;
                totalClick++;
                clicksThisSecond++;
                UpdateCurrentGold();

                ClickSFX.Open(new Uri("/Resources/Click.wav", UriKind.Relative));
                ClickSFX.Play();

                foreach (var upgrade in lstUpgrades) //FIX 01
                {

                    if (totalClick >= upgrade.Price)
                    {
                        ClickingSystem();
                        LstbShop.Visibility = Visibility.Visible;
                        break;
                    }
                }


                imageClick = false;
            }
        }


        //Game-loop functionaliteit
        private void InitClock()
        {
            gameClock.Tick += new EventHandler(GameClock_Tick);
            gameClock.Interval = TimeSpan.FromSeconds(1);
            gameClock.Start();

            gameTime.Interval = TimeSpan.FromMilliseconds(1);
            gameTime.Tick += new EventHandler(GameTime_Tick);
            gameTime.Start();
        }
        private void GameClock_Tick(object sender, EventArgs e)
        {
            tickCounter++;
            secondsPassed++;
            Calc_Gold();
            UpdateCurrentGold();
            TimeSpan time = TimeSpan.FromSeconds(tickCounter);
            LblTicks.Content = time.ToString(@"hh\:mm\:ss");

        }
        private void GameTime_Tick(object sender, EventArgs e)
        {
            DrawLabels();

        }
        private double UpdateCurrentGold()
        {
            double clickIncome = clickCount * goldPerClick;
            currentGold += clickIncome;
            totalGold += clickIncome;
            clickCount = 0;
            return currentGold;
        }


        private double Calc_Gold()
        {
            GoldAnimation();

            goldPerSecond = 0;
            double averageClickPerSecond = 0;

            totalClicks = clicksThisSecond;
            if (secondsPassed > 0)
            {
                averageClickPerSecond = ((double)totalClicks / secondsPassed) / 10;
                goldPerSecond += (averageClickPerSecond);
                secondsPassed = 0;
                foreach (var item in lstUpgrades)
                {
                    if (item.Amount > 0 && item.Multiplier > 0)
                    {
                        double itemIncome = (item.Goldpms * item.Multiplier) * item.Amount * 100;
                        goldPerSecond += itemIncome;
                    }
                    else
                    {
                        double itemIncome = item.Goldpms * item.Amount * 100;
                        goldPerSecond += itemIncome;
                    }

                }
                totalGold += goldPerSecond;

            }
            clicksThisSecond = 0;

            goldPerSecond = (Math.Round(goldPerSecond * 10) / 10);
            currentGold += goldPerSecond;

            return goldPerSecond;

        }
        private void DrawLabels()
        {
            this.Title = "Monster Clicker - Current gold: " + FormatNumber(currentGold);

            LblGold.Content = FormatNumber(currentGold);
            LblGoldps.Content = FormatNumber(goldPerSecond);
            LblTotalGold.Content = FormatNumber(totalGold);
            LblTotalClicks.Content = totalClick.ToString();

            GuildName();

        }
        private void GoldAnimation()
        {
            Random posX = new Random();
            TimeSpan delay = TimeSpan.Zero;
            TimeSpan delayIncrement = TimeSpan.FromMilliseconds(50);

            for (int i = 0; i < goldPerSecond && activeAnimations < MaxAnimations; i++)
            {
                activeAnimations++;

                if (activeAnimations >= MaxAnimations)
                {
                    break;
                }

                System.Windows.Controls.Image goldImg = new System.Windows.Controls.Image();
                goldImg.Source = new BitmapImage(new Uri("/MonsterClicker;component/Resources/GoldCoin_01.png", UriKind.Relative));
                goldImg.Width = 20;
                goldImg.Height = 20;

                GameArea.Children.Add(goldImg);


                DoubleAnimation moveAnimation = new DoubleAnimation();
                moveAnimation.From = 0;
                moveAnimation.To = GameArea.ActualHeight;
                moveAnimation.Duration = TimeSpan.FromSeconds(1);
                moveAnimation.BeginTime = delay;

                TranslateTransform moveTransform = new TranslateTransform();
                goldImg.RenderTransform = moveTransform;

                moveTransform.X = posX.Next(0, (int)GameArea.ActualWidth - (int)goldImg.Width);

                moveAnimation.Completed += (s, e) =>
                {
                    GameArea.Children.Remove(goldImg);
                    activeAnimations--;
                };

                moveTransform.BeginAnimation(TranslateTransform.YProperty, moveAnimation);

                delay += delayIncrement;
            }
        }


        //Formatting
        public static string FormatNumber(double number)
        {
            if (number >= 1000000000000000000)
                return (number / 1000000000000000000).ToString("0.00") + " Quintillion";
            if (number >= 1000000000000000)
                return (number / 1000000000000000).ToString("0.00") + " Quadrillion";
            if (number >= 1000000000000)
                return (number / 1000000000000).ToString("0.00") + " Trillion";
            if (number >= 1000000000)
                return (number / 1000000000).ToString("0.00") + " Billion";
            if (number >= 1000000)
                return (number / 1000000).ToString("0.000") + " Million";
            if (number >= 1000)
                return number.ToString("# ##0.0");

            return number.ToString("F1");
        }
        private void GuildName()
        {
            if (TxtGuildName.Text == " ")
            {
                MessageBox.Show("Please pick a suitable name for our guild. We need a name.", "Guild Accountant:");
                TxtGuildName.Text = "Order of PXL Digital knights";
            }
            else if (TxtGuildName.Text == "" && TxtGuildName.IsMouseOver == false)
            {
                MessageBox.Show("I know you have more urgent business than picking a name but until you do, we'll keep the old one.", "Guild Accountant:");
                TxtGuildName.Text = "Order of PXL Digital knights";
            }
            else
            {

            }
        }

        //Shop Functionaliteit
        private void ShopList()
        {
            lstUpgrades.Add(new Upgrade { ItemGroup = 1, Item = "Sword", Price = 15, Goldps = 0.1, Goldpms = 0.001, Amount = 0, ImagePath = "/Resources/Sword.png", IsEnabled = true });
            lstUpgrades.Add(new Upgrade { ItemGroup = 2, Item = "Adventurer", Price = 100, Goldps = 1, Goldpms = 0.01, Amount = 0, ImagePath = "/Resources/Adventurer.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 3, Item = "Knight", Price = 1100, Goldps = 8, Goldpms = 0.08, Amount = 0, ImagePath = "/Resources/knight.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 4, Item = "Wizard", Price = 12000, Goldps = 47, Goldpms = 0.47, Amount = 0, ImagePath = "/Resources/Wizard.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 5, Item = "Summoners", Price = 130000, Goldps = 260, Goldpms = 2.60, Amount = 0, ImagePath = "/Resources/guild.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 6, Item = "Adventurer's Guild", Price = 1400000, Goldps = 1400, Goldpms = 14, Amount = 0, ImagePath = "/Resources/barracks.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 7, Item = "Knight's Barracks", Price = 20000000, Goldps = 7800, Goldpms = 78, Amount = 0, ImagePath = "/Resources/college.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 8, Item = "Wizard Colleges", Price = 330000000, Goldps = 44000, Goldpms = 440, Amount = 0, ImagePath = "/Resources/summoner.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 9, Item = "Summoner circles", Price = 510000000, Goldps = 260000, Goldpms = 2600, Amount = 0, ImagePath = "/Resources/circle.png", IsEnabled = false });
            lstUpgrades.Add(new Upgrade { ItemGroup = 10, Item = "Dragon", Price = 750000000, Goldps = 16000000, Goldpms = 16000, Amount = 0, ImagePath = "/Resources/dragon.png", IsEnabled = false });

            LstbShop.ItemsSource = lstUpgrades;
            LstUpgradeCategories.ItemsSource = lstUpgrades;
        }
        private void BonusList()
        {
            //Item: Sword
            lstBonus.Add(new Upgrade { ItemGroup = 1, Item = "Sharpen Sword", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 1, Item = "Balance Sword", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 1, Item = "Two-hander", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 1, Item = "Steelforged", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 1, Item = "Damascus Steelforged", Price = (15 * 500000), Multiplier = 32 });

            //Item: Adventurer
            lstBonus.Add(new Upgrade { ItemGroup = 2, Item = "Cheaper Mercenary", Price = (100 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 2, Item = "Trained soldier", Price = (100 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 2, Item = "Expert hunter", Price = (100 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 2, Item = "Assassin", Price = (100 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 2, Item = "Chosen One", Price = (100 * 500000), Multiplier = 32 });

            //Item: Knight
            lstBonus.Add(new Upgrade { ItemGroup = 3, Item = "Squire", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 3, Item = "Rookie", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 3, Item = "Captain", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 3, Item = "Lieutenant", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 3, Item = "Corperal", Price = (15 * 500000), Multiplier = 32 });

            //Item: Wizard
            lstBonus.Add(new Upgrade { ItemGroup = 4, Item = "Pledge", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 4, Item = "Apprentice", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 4, Item = "Acolyte", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 4, Item = "Expert", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 4, Item = "Master", Price = (15 * 500000), Multiplier = 32 });

            //Item: Summoner
            lstBonus.Add(new Upgrade { ItemGroup = 5, Item = "Dark arts eh?", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 5, Item = "Another sacrifice, another job done", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 5, Item = "We're going to need a bigger altar", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 5, Item = "Those poor pigs.", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 5, Item = "Who do you want now? Cthulu?", Price = (15 * 500000), Multiplier = 32 });

            //Item: Guild
            lstBonus.Add(new Upgrade { ItemGroup = 6, Item = "Expand, expand, expand", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 6, Item = "Time for home improvement", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 6, Item = "Maybe we need a secretary?", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 6, Item = "Should we get on the stock exchange?", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 6, Item = "Perhaps wecan add .inc to the name.", Price = (15 * 500000), Multiplier = 32 });

            //Item: Barracks
            lstBonus.Add(new Upgrade { ItemGroup = 7, Item = "Where are those training dummies?", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 7, Item = "Have the horses been fed?", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 7, Item = "We should get more cots", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 7, Item = "Figures! Models! Strategy sticks!", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 7, Item = "Prepare for war", Price = (15 * 500000), Multiplier = 32 });

            //Item: College
            lstBonus.Add(new Upgrade { ItemGroup = 8, Item = "First Library pass", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 8, Item = "Another bookcase?", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 8, Item = "Careful with that crystal ball!", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 8, Item = "The worst thing there, all that dripping wax.", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 8, Item = "A jungle of paper...", Price = (15 * 500000), Multiplier = 32 });


            //Item: Circle
            lstBonus.Add(new Upgrade { ItemGroup = 9, Item = "Are those lavender candles?", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 9, Item = "Alas, Poor Yorrick", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 9, Item = "Nobody knows my sorrow", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 9, Item = "None of us really changes", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 9, Item = "Unholy blashpemy", Price = (15 * 500000), Multiplier = 32 });

            //Item: Dragon
            lstBonus.Add(new Upgrade { ItemGroup = 10, Item = "Burn Baby Burn", Price = (15 * 100), Multiplier = 2 });
            lstBonus.Add(new Upgrade { ItemGroup = 10, Item = "Disco Inferno", Price = (15 * 500), Multiplier = 4 });
            lstBonus.Add(new Upgrade { ItemGroup = 10, Item = "Burn that mother down", Price = (15 * 5000), Multiplier = 8 });
            lstBonus.Add(new Upgrade { ItemGroup = 10, Item = "The heat is on!", Price = (15 * 50000), Multiplier = 16 });
            lstBonus.Add(new Upgrade { ItemGroup = 10, Item = "Rising to the top!", Price = (15 * 500000), Multiplier = 32 });

            LstbBonus.ItemsSource = lstBonus;

        }

        private void QuestList()
        {
            //ID: 0 - Clicking
            LstQuest.Add(new Quest { QuestID = 0, Name = "My first gold coin!", Description = "Klik the monster once.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 0, Name = "Pocket money!", Description = "Klik the monster a hundred times.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 0, Name = "I would like a raise!", Description = "Klik the monster ten thousand times.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 0, Name = "Honest work!", Description = "Klik the monster a million times.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 0, Name = "My first gold coin!", Description = "Klik the monster a hundred million times.", Condition = 100000000 });

            //ID 1 - Sword
            LstQuest.Add(new Quest { QuestID = 1, Name = "Knife to meet you!", Description = "Get a sword.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 1, Name = "Armed and ready!", Description = "Get a hundred swords.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 1, Name = "Swords for days!", Description = "Get ten thousand swords.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 1, Name = "Swordfight!", Description = "Get a million swords.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 1, Name = "Metal Monopoly!", Description = "Get a hundred million swords.", Condition = 100000000 });

            //ID 2 - Adventurer
            LstQuest.Add(new Quest { QuestID = 2, Name = "Money for money!", Description = "Hire an adventurer.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 2, Name = "A new job?!", Description = "Hire a hundred adventurers.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 2, Name = "The epic hunt!", Description = "Hire ten thousand adventurers.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 2, Name = "More money for money!!", Description = "Hire a million adventurers.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 2, Name = "The ultimate adventurer!", Description = "Hire a hundred million adventurers.", Condition = 100000000 });

            //ID 3 - Knight
            LstQuest.Add(new Quest { QuestID = 3, Name = "Rookies Assemble!", Description = "Hire a Knight.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 3, Name = "Armed and ready!", Description = "Hire a hundred Knights.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 3, Name = "Armed and ready!", Description = "Hire ten thousand Knights.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 3, Name = "Armed and ready!", Description = "Hire a million Knights.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 3, Name = "Armed and ready!", Description = "Hire a hundred million Knights.", Condition = 100000000 });

            //ID 4 - Wizard
            LstQuest.Add(new Quest { QuestID = 4, Name = "Armed and ready!", Description = "Hire a Wizard.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 4, Name = "Armed and ready!", Description = "Hire a hundred Wizards.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 4, Name = "Armed and ready!", Description = "Hire ten thousand Wizards.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 4, Name = "Armed and ready!", Description = "Hire a million Wizards.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 4, Name = "Armed and ready!", Description = "Hire a hundred million Wizards.", Condition = 100000000 });

            //ID 5 - Summoner
            LstQuest.Add(new Quest { QuestID = 5, Name = "Armed and ready!", Description = "Hire a Summoner.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 5, Name = "Armed and ready!", Description = "Hire a hundred Summoners.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 5, Name = "Armed and ready!", Description = "Hire ten thousand Summoners.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 5, Name = "Armed and ready!", Description = "Hire a million Summoners.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 5, Name = "Armed and ready!", Description = "Hire a hundred million Summoners.", Condition = 100000000 });

            //ID 6 - Guild
            LstQuest.Add(new Quest { QuestID = 6, Name = "Armed and ready!", Description = "Build a Guild.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 6, Name = "Armed and ready!", Description = "Build a hundred Guilds.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 6, Name = "Armed and ready!", Description = "Build ten thousand Guilds.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 6, Name = "Armed and ready!", Description = "Build a million Guilds.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 6, Name = "Armed and ready!", Description = "Build a hundred million Guilds.", Condition = 100000000 });

            //ID 7 - Barracks
            LstQuest.Add(new Quest { QuestID = 7, Name = "Armed and ready!", Description = "Build a sword.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 7, Name = "Armed and ready!", Description = "Build a hundred swords.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 7, Name = "Armed and ready!", Description = "Build ten thousand swords.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 7, Name = "Armed and ready!", Description = "Build a million swords.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 7, Name = "Armed and ready!", Description = "Build a hundred million swords.", Condition = 100000000 });

            //ID 8 - Tower
            LstQuest.Add(new Quest { QuestID = 8, Name = "Armed and ready!", Description = "Build a sword.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 8, Name = "Armed and ready!", Description = "Build a hundred swords.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 8, Name = "Armed and ready!", Description = "Build ten thousand swords.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 8, Name = "Armed and ready!", Description = "Build a million swords.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 8, Name = "Armed and ready!", Description = "Build a hundred million swords.", Condition = 100000000 });

            //ID 9 - Circle
            LstQuest.Add(new Quest { QuestID = 9, Name = "Armed and ready!", Description = "Build a sword.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 9, Name = "Armed and ready!", Description = "Build a hundred swords.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 9, Name = "Armed and ready!", Description = "Build ten thousand swords.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 9, Name = "Armed and ready!", Description = "Build a million swords.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 9, Name = "Armed and ready!", Description = "Build a hundred million swords.", Condition = 100000000 });

            //ID 10 - Dragon
            LstQuest.Add(new Quest { QuestID = 10, Name = "Armed and ready!", Description = "Tame a sword.", Condition = 1 });
            LstQuest.Add(new Quest { QuestID = 10, Name = "Armed and ready!", Description = "Tame a hundred swords.", Condition = 100 });
            LstQuest.Add(new Quest { QuestID = 10, Name = "Armed and ready!", Description = "Tame ten thousand swords.", Condition = 10000 });
            LstQuest.Add(new Quest { QuestID = 10, Name = "Armed and ready!", Description = "Tame a million swords.", Condition = 1000000 });
            LstQuest.Add(new Quest { QuestID = 10, Name = "Armed and ready!", Description = "Tame a hundred million swords.", Condition = 100000000 });

            LstbQuest.ItemsSource = LstQuest;

        }

        private void BonusImage(Upgrade upgrade)
        {
            if (CnvsCategory != null)
            {
                for (int i = 0; i < upgrade.Amount; i++)
                {
                    System.Windows.Controls.Image bonusImg = new System.Windows.Controls.Image();
                    bonusImg.Source = new BitmapImage(new Uri(upgrade.ImagePath, UriKind.Relative));
                    bonusImg.Width = 100;
                    bonusImg.Height = 100;

                    CnvsCategory.Children.Add(bonusImg);
                }
            }
            else
            {

            }
        }

        private Canvas CnvsCategory;

        private void ClickingSystem()
        {
            int selectedIndex = LstbShop.SelectedIndex;

            Upgrade selectedUpgrade = (Upgrade)LstbShop.SelectedItem;

            int selectedIndexQuest = LstbQuest.SelectedIndex;


            if (selectedIndex < 0 || selectedIndex >= lstUpgrades.Count)
            {
                return;
            }

            if (selectedUpgrade == null)
            {
                return;
            }

            if (totalClick == selectedUpgrade.Price)
            {
                UpdateItemVisibility(selectedUpgrade.ItemGroup, true);
            }
        }

        private void Click_Quest(object sender, EventArgs e)
        {

        }
        private void UpdateItemVisibility(int itemGroup, bool isEnabled)
        {
            var upgrade = lstUpgrades.FirstOrDefault(u => u.ItemGroup == itemGroup);
            if (upgrade != null)
            {
                upgrade.IsEnabled = isEnabled;
                LstbShop.Items.Refresh();
            }
        }
        private void ItemSelect(object sender, SelectionChangedEventArgs e)
        {
            int selectedIndex = LstbShop.SelectedIndex;

            Upgrade selectedUpgrade = (Upgrade)LstbShop.SelectedItem;

            if (selectedIndex < 0 || selectedIndex >= lstUpgrades.Count)
            {
                return;
            }


            if (currentGold >= selectedUpgrade.Price)
            {
                currentGold -= selectedUpgrade.Price;
                selectedUpgrade.Amount++;
                selectedUpgrade.Price = Math.Round(selectedUpgrade.Price * (Math.Pow(1.15, selectedUpgrade.Amount)));
                LstbShop.SelectedIndex = -1;

                BuySFX.Open(new Uri("/Resources/Gold.wav", UriKind.Relative));
                BuySFX.Play();
                CheckForBonusItems();
                BonusImage(selectedUpgrade);

                LstbShop.Items.Refresh();
            }
            else
            {
                MessageBox.Show("Sorry but we don't have enough gold for that.", "Guild Accountant:");
                LstbShop.SelectedIndex = -1;
            }

        }
        private void CheckForBonusItems()
        {

            foreach (var upgrade in lstUpgrades)
            {
                int selectedBonusIndex = LstbBonus.SelectedIndex;
                int numberOfBonusesToShow = upgrade.Amount;

                var bonusesForThisItem = selectedBonusIndex;
                var bonus = lstUpgrades;
                var selectedBonus = bonus.Where(b => b.ItemGroup == selectedBonusIndex).FirstOrDefault();
                visibleBonuses.Add(selectedBonus);

            }

        }

        private void Buy_Bonus(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;

            int selectedIndex = LstbBonus.SelectedIndex;
            Upgrade selectedBonus = (Upgrade)LstbBonus.SelectedItem;

            if (selectedIndex < 0 || selectedIndex >= visibleBonuses.Count)
            {
                return;
            }
            else
            {
                if (currentGold >= selectedBonus.Price)
                {
                    currentGold -= selectedBonus.Price;

                    var correspondingUpgrade = lstUpgrades.FirstOrDefault(u => u.ItemGroup == selectedBonus.ItemGroup);
                    if (correspondingUpgrade != null)
                    {
                        correspondingUpgrade.Multiplier *= selectedBonus.Multiplier;
                    }

                    visibleBonuses.Remove(selectedBonus);
                    LstbBonus.ItemsSource = visibleBonuses;
                }
                else
                {
                    MessageBox.Show("We need a little more gold for that, Guild accountant:");
                    LstbBonus.SelectedIndex = -1;
                }
                LstbBonus.Items.Refresh();
            }
        }
    }
}
