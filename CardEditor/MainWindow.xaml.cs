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
using UbiBeamPlusPlus.Model.Card;
using UbiBeamPlusPlus.Model.Component.Ability;
using UbiBeamPlusPlus.Model.Component.Ability.CreatureAbility;
using UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility;
using UbiBeamPlusPlus.Model.Component.Ability.SpellAbility;
using UbiBeamPlusPlus.Model.Component.Ability.StructureAbility;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;
using UbiBeamPlusPlus.Model.Component.Attack;



namespace CardEditor {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged {

        public event PropertyChangedEventHandler PropertyChanged;

        private String _CardName = "CardName";
        public String CardName {
            get { return _CardName; }
            set {
                _CardName = value;
            }
        }

        private int _CardID = 0;
        public int CardID {
            get { return _CardID; }
            set { _CardID = value; }
        }

        private byte _Cost = 1;
        public byte Cost {
            get { return _Cost; }
            set {
                _Cost = value;
                OnPropertyChanged("Cost");
            }
        }

        private byte _Damage = 0;
        public byte Damage {
            get { return _Damage; }
            set {
                _Damage = value;
                OnPropertyChanged("Damage");
            }
        }

        private byte _Countdown = 0;
        public byte Countdown {
            get { return _Countdown; }
            set {
                _Countdown = value;
                OnPropertyChanged("Countdown");
            }
        }

        private byte _Health = 0;
        public byte Health {
            get { return _Health; }
            set {
                _Health = value;
                OnPropertyChanged("Health");
            }
        }

        private byte _FirstAbilityValue = 1;
        public byte FirstAbilityValue {
            get { return _FirstAbilityValue; }
            set {
                _FirstAbilityValue = value;
                OnPropertyChanged("FirstAbilityValue");
            }
        }

        private byte _SecondAbilityValue = 1;
        public byte SecondAbilityValue {
            get { return _SecondAbilityValue; }
            set {
                _SecondAbilityValue = value;
                OnPropertyChanged("SecondAbilityValue");
            }
        }

        private List<CreatureAbilityComponent> CreatureAbilities = new List<CreatureAbilityComponent>();
        private List<EnchantmentAbilityComponent> EnchantmentAbilities = new List<EnchantmentAbilityComponent>();
        private List<SpellAbilityComponent> SpellAbilities = new List<SpellAbilityComponent>();
        private List<StructureAbilityComponent> StructureAbilities = new List<StructureAbilityComponent>();

        /// <summary>
        /// Image Template for Creature and Structure Card Types
        /// </summary>
        private System.Drawing.Image UnitCardTemplate = new Bitmap(401, 622);

        /// <summary>
        /// Image Template for Spell and Enchantment Card Type
        /// </summary>
        private System.Drawing.Image CardTemplate = new Bitmap(401, 622);

        private System.Drawing.Image CardPicture = new Bitmap(1, 1);

        /// <summary>
        /// Final Card Image with Text and picture
        /// </summary>
        private Bitmap CardImage = new Bitmap(401, 622);

        private Boolean isUnit = true;

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow() {
            InitializeComponent();
            DataContext = this;

            //Get Card Templates
            this.UnitCardTemplate = System.Drawing.Image.FromFile("../../Res/Card_unit.png");
            this.CardTemplate = System.Drawing.Image.FromFile("../../Res/Card.png");

            // Get standart background Picture
            this.CardPicture = System.Drawing.Image.FromFile("../../Res/TestPic.png");

            //Add availabel Abilitys
            CreatureAbilities.Add(new Haste());
            CreatureAbilities.Add(new Regeneration(0));
            EnchantmentAbilities.Add(new UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility.AdditionalDamage(0));
            EnchantmentAbilities.Add(new AdditionalHealth(0));
            EnchantmentAbilities.Add(new UbiBeamPlusPlus.Model.Component.Ability.EnchantmentAbility.Healing(0));
            SpellAbilities.Add(new DamageToOne(0));
            SpellAbilities.Add(new UbiBeamPlusPlus.Model.Component.Ability.SpellAbility.Healing(0));
            SpellAbilities.Add(new IncreaseCountdown(0));

            cmbAbility1.ItemsSource = CreatureAbilities;
            cmbAbility2.ItemsSource = CreatureAbilities;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void Value_Handler(object sender, RoutedEventArgs e) {
            Button button = sender as Button;

            switch (button.Name) {
                case "CostMinus":
                    Cost--;
                    break;
                case "CostPlus":
                    Cost++;
                    break;

                case "DamageMinus":
                    Damage--;
                    break;
                case "DamagePlus":
                    Damage++;
                    break;

                case "HealthMinus":
                    Health--;
                    break;
                case "HealthPlus":
                    Health++;
                    break;

                case "CountdownMinus":
                    Countdown--;
                    break;
                case "CountdownPlus":
                    Countdown++;
                    break;

                case "FirstAbilityMinus":
                    FirstAbilityValue--;
                    break;
                case "FirstAbilityPlus":
                    FirstAbilityValue++;
                    break;

                case "SecondAbilityMinus":
                    SecondAbilityValue--;
                    break;
                case "SecondAbilityPlus":
                    SecondAbilityValue++;
                    break;
            }
            updateImage();
        }

        /// <summary>
        /// To Support one Way (Two Way) Binding, such that UI element updates when the source has been changed.
        /// </summary>
        /// <param name="name">Name of Source</param>
        protected void OnPropertyChanged(string name) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        /// <summary>
        /// Is called when selection of the Card Type Combobox changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBox cmbBox = sender as ComboBox;

            // make sure that the UI components are already initialized
            if (DamagePlus == null)
                return;
            FirstAbility.IsChecked = false;
            SecondAbility.IsChecked = false;

            Cost = 1;
            Damage = 0;
            Health = 0;
            Countdown = 0;
            FirstAbilityValue = 1;
            SecondAbilityValue = 1;

            DamagePlus.IsEnabled = false;
            DamageMinus.IsEnabled = false;
            HealthPlus.IsEnabled = false;
            HealthMinus.IsEnabled = false;
            CountdownPlus.IsEnabled = false;
            CountdownMinus.IsEnabled = false;

            switch (cmbBox.SelectedIndex) {
                case 0:
                    // creature
                    DamagePlus.IsEnabled = true;
                    DamageMinus.IsEnabled = true;
                    HealthPlus.IsEnabled = true;
                    HealthMinus.IsEnabled = true;
                    CountdownPlus.IsEnabled = true;
                    CountdownMinus.IsEnabled = true;
                    isUnit = true;
                    cmbAbility1.ItemsSource = CreatureAbilities;
                    cmbAbility2.ItemsSource = CreatureAbilities;
                    break;
                case 1:
                    // Structure
                    HealthPlus.IsEnabled = true;
                    HealthMinus.IsEnabled = true;
                    isUnit = true;
                    cmbAbility1.ItemsSource = StructureAbilities;
                    cmbAbility2.ItemsSource = StructureAbilities;
                    break;
                case 2:
                    // Spell
                    isUnit = false;
                    cmbAbility1.ItemsSource = SpellAbilities;
                    cmbAbility2.ItemsSource = SpellAbilities;
                    break;
                case 3:
                    // enchantment
                    isUnit = false;
                    cmbAbility1.ItemsSource = EnchantmentAbilities;
                    cmbAbility2.ItemsSource = EnchantmentAbilities;
                    break;
            }

            //Fix nunwanted behavior that Combox have still the Text of the previous Item 
            cmbBox.Text = (e.AddedItems[0] as ComboBoxItem).Content as string;

            updateImage();
        }

        /// <summary>
        /// Is Called when Name is changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
            updateImage();
        }

        /// <summary>
        /// Update Preview Image
        /// </summary>
        private void updateImage() {

            Graphics graphics = Graphics.FromImage(this.CardImage);

            graphics.Clear(System.Drawing.Color.Transparent);

            // draw background Picture
            graphics.DrawImage(this.CardPicture, new System.Drawing.Rectangle(38, 120, 325, 179));

            //draw Template
            System.Drawing.Image template = isUnit ? this.UnitCardTemplate : this.CardTemplate;
            graphics.DrawImage(template, new System.Drawing.Rectangle(0, 0, 401, 622));

            //draw Name 
            System.Drawing.FontFamily myFontFamily = new System.Drawing.FontFamily("Arial");
            Font NameFont = new Font(myFontFamily, 40, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            graphics.DrawString(this.CardName, NameFont, System.Drawing.Brushes.Black, new System.Drawing.Point(42, 40));

            //Draw Cost Value
            Font CostFont = new Font(myFontFamily, 45, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            graphics.DrawString("[" + Cost + "]", CostFont, System.Drawing.Brushes.Black, new System.Drawing.Point(290, 38));

            if (isUnit) {
                Font ValueFont = new Font(myFontFamily, 55, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
                //draw Damage value
                graphics.DrawString("" + Damage, ValueFont, System.Drawing.Brushes.White, new System.Drawing.Point(88, 295));
                //draw countdown value
                graphics.DrawString("" + Countdown, ValueFont, System.Drawing.Brushes.White, new System.Drawing.Point(203, 295));
                //draw Health value
                graphics.DrawString("" + Health, ValueFont, System.Drawing.Brushes.White, new System.Drawing.Point(320, 295));

            }

            //Draw Abilities
            Font AbilityFont = new Font(myFontFamily, 30, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
            List<AbstractAbilityComponent> Abilities = getAbilities();
            for (int i = 0; i < Abilities.Count; i++) {
                graphics.DrawString(Abilities[i].Description, AbilityFont, System.Drawing.Brushes.Black, new System.Drawing.Rectangle(30, 380 + (i * 120), 341, 110));
            }

            //Draw Card Type
            Font TypeFont = new Font(myFontFamily, 45, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
            if (cmbCardType.Text.Equals("Creature") || cmbCardType.Text.Equals("Structure")) {
                graphics.DrawString(cmbCardType.Text, TypeFont, System.Drawing.Brushes.Black, new System.Drawing.Point(100, 550));
            } else if (cmbCardType.Text.Equals("Enchantment")) {
                graphics.DrawString(cmbCardType.Text, TypeFont, System.Drawing.Brushes.Black, new System.Drawing.Point(55, 550));
            } else if (cmbCardType.Text.Equals("Spell")) {
                graphics.DrawString(cmbCardType.Text, TypeFont, System.Drawing.Brushes.Black, new System.Drawing.Point(130, 550));
            }
            
            // convert Image and update Canvas
            var hBitmap = new System.Drawing.Bitmap(CardImage).GetHbitmap();
            try {
                var source = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(hBitmap, IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                var brush = new ImageBrush(source);
                _Canvas.Background = brush;
            } finally {
                DeleteObject(hBitmap);
            }
        }

        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);


        /// <summary>
        /// Try to save the Card Image at the given Path
        /// </summary>
        /// <param name="path">The Path the Image to save.</param>
        private void SaveImage(String path) {

            //try to save image
            try {
                this.CardImage.Save(path, System.Drawing.Imaging.ImageFormat.Png);

            } catch (Exception e) {
                Console.WriteLine(e.StackTrace);
                MessageBox.Show("A Error occured while saving Card", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// Return a List with the choosen Abilities
        /// </summary>
        /// <returns></returns>
        public List<AbstractAbilityComponent> getAbilities() {
            List<AbstractAbilityComponent> Abilities = new List<AbstractAbilityComponent>();

            if (FirstAbility.IsChecked.HasValue && (bool)FirstAbility.IsChecked && cmbAbility1.SelectedItem != null) {
                ((AbstractAbilityComponent)cmbAbility1.SelectedItem).Value = _FirstAbilityValue;
                Abilities.Add((AbstractAbilityComponent)cmbAbility1.SelectedItem);
            }
            if (SecondAbility.IsChecked.HasValue && (bool)SecondAbility.IsChecked && cmbAbility2.SelectedItem != null) {
                ((AbstractAbilityComponent)cmbAbility2.SelectedItem).Value = _SecondAbilityValue;
                Abilities.Add((AbstractAbilityComponent)cmbAbility2.SelectedItem);
            }

            return Abilities;
        }

        /// <summary>
        /// Save the Card at the given Path
        /// </summary>
        /// <param name="Path">destination Path </param>
        private void SaveCard(String Path) {
            AbstractCard newCard = new Creature(0, "noName", 0, 0, 0, 0);
            String XmlPath = Path;

            switch (cmbCardType.SelectedIndex) {
                case 0: //Creature
                    Creature creature = new Creature(CardID, CardName, Cost, Health, Damage, Countdown);
                    creature.Attack = new MeleeAttackComponent();
                    newCard = creature;
                    XmlPath += "_Creature.xml";
                    break;
                case 1:
                    newCard = new Structure(CardID, CardName, Cost, Health);
                    XmlPath += "_Structure.xml";
                    break;
                case 2:
                    newCard = new Spell(CardID, CardName, Cost);
                    XmlPath += "_Spell.xml";
                    break;
                case 3:
                    newCard = new Enchantment(CardID, CardName, Cost);
                    XmlPath += "_Enchantment.xml";
                    break;
            }

            foreach (var ability in getAbilities()) {
                Console.WriteLine(ability.Description);
                newCard.Abilities.Add(ability);
            }
            SaveToXMLFile(XmlPath, newCard);

            SaveImage(Path + "_ID_" + CardID + ".png");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {
            // Intit Save File Dialog
            var saveDialog = new System.Windows.Forms.SaveFileDialog();
            saveDialog.RestoreDirectory = true;
            saveDialog.FileName = CardName;


            if (saveDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                SaveCard(saveDialog.FileName);
                MessageBox.Show("Saved");
            }

        }


        private void ChoosePicButton_Click(object sender, RoutedEventArgs e) {
            //Init File Choos Dialog
            var fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "PNG|*.png";
            var result = fileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK) {
                String path = fileDialog.FileName;

                try {
                    this.CardPicture = System.Drawing.Image.FromFile(path);

                    float aspectRatio = 1.825F; // Optimal Aspect Ratio
                    float ratio = CardPicture.Width / CardPicture.Height; // Aspect Ratio of the choosen picture

                    if (Math.Abs(aspectRatio - ratio) > 0.1F) {
                        MessageBox.Show("Selected Picture will be distorted", "Deformation", MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }

                } catch (System.IO.FileNotFoundException exception) {
                    MessageBox.Show("Can't find selected File.\n" + exception.Message, "File not Found", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            updateImage();
        }

        /// <summary>
        /// Saves to an xml file
        /// </summary>
        /// <param name="FileName">File path of the new xml file</param>
        public void SaveToXMLFile(string FileName, AbstractCard card) {
            using (var writer = new System.IO.StreamWriter(FileName)) {
                var serializer = new XmlSerializer(card.GetType());
                serializer.Serialize(writer, card);
                writer.Flush();
            }
        }

        private void UpdateImage(object sender, RoutedEventArgs e) {
            this.updateImage();
        }

        private void UpdateImage(object sender, SelectionChangedEventArgs e) {
            this.updateImage();
        }

    }
}
