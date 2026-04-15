using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BestOilApp
{
    public partial class Form1 : Form
    {
        // Цены на топливо
        private Dictionary<string, decimal> fuelPrices = new Dictionary<string, decimal>()
        {
            { "А-92", 58.83m },
            { "А-95", 64.22m },
            { "ДТ", 71.52m }
        };

        // Цены на товары кафе
        private Dictionary<string, decimal> cafePrices = new Dictionary<string, decimal>()
        {
            { "Хот-дог", 170.00m },
            { "Гамбургер", 99.99m },
            { "Картофель-фри", 75.48m },
            { "Кока-кола", 78.99m }
        };

        // Элементы управления для кафе
        private List<CheckBox> cafeCheckBoxes = new List<CheckBox>();
        private List<TextBox> cafeQtyTextBoxes = new List<TextBox>();

        // Общая выручка за день
        private decimal totalRevenue = 0;

        // Таймер для отсрочки очистки
        private Timer clearTimer = new Timer();
        private int clearCountdown = 10;

        // Элементы формы
        private GroupBox gbFuelPayment;
        private ComboBox cbFuel;
        private TextBox tbFuelPrice;
        private RadioButton rbQuantity;
        private RadioButton rbSum;
        private TextBox tbQuantity;
        private TextBox tbSum;
        private Label lblFuelPaymentAmount;

        private GroupBox gbCafe;
        private Label lblCafePaymentAmount;

        private Label lblTotalLabel;
        private Button btnCalculate;
        private PictureBox pbSmile;
        private PictureBox pbCafe;
        private PictureBox pbFuel;
        public Form1()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = "BestOil";
            this.Size = new Size(547, 400); // Размер формы
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            // Основные параметры геометрии: отступы и ширины групп
            int margin = 10; // Универсальный отступ между элементами и от краёв формы (для "воздуха")
            int groupWidth = 250; // Ширина каждой группы ("Автозаправка" и "Мини-кафе")
            int groupHeight = 230; // Высота каждой группы ("Автозаправка" и "Мини-кафе")

            // Расчет высот групп с запасом для всех элементов
            int groupHeightFuel = groupHeight; // Фиксированная высота группы "Автозаправка" (достаточно для всех элементов от Y=25 до Y=180)
            int cafeItemsCount = cafePrices.Count; // Количество товаров кафе (4)
            int cafeItemHeight = 30; // Высота строки на один товар кафе (CheckBox + TextBox)
            int cafeStartY = 25; // Стартовый Y для товаров кафе (отступ от верха группы)
            int cafeGroupHeight = groupHeight; // cafeStartY + cafeItemsCount * cafeItemHeight + 5 + cafePaymentHeight + 10; // Динамический расчет высоты группы кафе: 25 + 4*30 + 5 + 40 + 10 ≈ 200 px (адаптируется под количество товаров)

            int PaymentHeight = 70; // Высота блока "К оплате"

            // Позиции внутри блока "К оплате"
            int PaymentLocation_X = 10;
            int PaymentLocation_Y = 150;
            int PaymentAmountLocation_X = 65;
            int PaymentAmountLocation_Y = 30;
            int pbLocation_X = 20;
            int pbLocation_Y = 25;


// --- Группа Автозаправка (слева) ---
            // Расположение: слева с отступом margi
            GroupBox gbFuel = new GroupBox()
            {
                Text = "Автозаправка",
                Location = new Point(margin, margin), // X=10, Y=10 (левый верхний угол формы с отступом)
                Size = new Size(groupWidth, groupHeightFuel) // Ширина 210 px, высота 190 px
            };
            this.Controls.Add(gbFuel);

            // Метка "Бензин:" расположена на Y=30 (отступ от верха группы), X=15 (левый край)
            Label lblFuel = new Label()
            {
                Text = "Бензин:",
                Location = new Point(15, 30), // X=15 (левый край группы), Y=30
                AutoSize = true
            };
            gbFuel.Controls.Add(lblFuel);

            // ComboBox для выбора топлива: расположен справа от метки, Y=25 (выше метки для выравнивания), ширина 100 px
            cbFuel = new ComboBox()
            {
                Location = new Point(80, 25), // X=80 (справа от метки), Y=25
                Width = 100,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cbFuel.Items.AddRange(new string[] { "А-92", "А-95", "ДТ" });
            cbFuel.SelectedIndex = 0;
            cbFuel.SelectedIndexChanged += CbFuel_SelectedIndexChanged;
            gbFuel.Controls.Add(cbFuel);

            // Метка "Цена:" на Y=60, X=15 (ниже предыдущих элементов с отступом 30 px)
            Label lblPrice = new Label()
            {
                Text = "Цена:",
                Location = new Point(15, 60), // X=15, Y=60
                AutoSize = true
            };
            gbFuel.Controls.Add(lblPrice);

            // TextBox цены: справа от метки
            tbFuelPrice = new TextBox()
            {
                Location = new Point(80, 55),
                Width = 80,
                ReadOnly = true,
                TextAlign = HorizontalAlignment.Right
            };
            gbFuel.Controls.Add(tbFuelPrice);

            // Метка "руб.": справа от TextBox цены, Y=60, X=165 (отступ 5 px от TextBox)
            Label lblCurrency = new Label()
            {
                Text = "руб.",
                Location = new Point(165, 60), // X=165, Y=60
                AutoSize = true
            };
            gbFuel.Controls.Add(lblCurrency);

            // Радиокнопка "Количество": на Y=90, X=15 (отступ 30 px от предыдущих)
            rbQuantity = new RadioButton()
            {
                Text = "Количество (л):",
                Location = new Point(15, 90), // X=15, Y=90
                AutoSize = true,
                Checked = true
            };
            rbQuantity.CheckedChanged += RbQuantity_CheckedChanged;
            gbFuel.Controls.Add(rbQuantity);

            // TextBox количества: справа от радиокнопки, Y=87 (выше для выравнивания), ширина 70 px
            tbQuantity = new TextBox()
            {
                Location = new Point(130, 87), // X=130, Y=87
                Width = 70,
                Text = "0",
                TextAlign = HorizontalAlignment.Right
            };
            tbQuantity.KeyPress += NumericTextBox_KeyPress;
            gbFuel.Controls.Add(tbQuantity);

            // Радиокнопка "Сумма": на Y=115, X=15 (отступ 25 px от предыдущей)
            rbSum = new RadioButton()
            {
                Text = "Сумма (руб.):",
                Location = new Point(15, 115), // X=15, Y=115
                AutoSize = true
            };
            rbSum.CheckedChanged += RbSum_CheckedChanged;
            gbFuel.Controls.Add(rbSum);

            // TextBox суммы: справа от радиокнопки, Y=112 (выше для выравнивания), ширина 70 px, отключен по умолчанию
            tbSum = new TextBox()
            {
                Location = new Point(130, 112), // X=130, Y=112
                Width = 70,
                Text = "0",
                TextAlign = HorizontalAlignment.Right,
                Enabled = false
            };
            tbSum.KeyPress += NumericTextBox_KeyPress;
            gbFuel.Controls.Add(tbSum);

// Блок "К оплате" для топлива

            gbFuelPayment = new GroupBox()
            {
                Text = "К оплате",
                Location = new Point(PaymentLocation_X, PaymentLocation_Y), // PaymentLocation_X=10 (отступ внутри группы), PaymentLocation_Y=150
                Size = new Size(groupWidth - 20, PaymentHeight) // Ширина 190 px, высота 40 px
            };
            gbFuel.Controls.Add(gbFuelPayment);

            // PictureBox иконки: слева
            pbFuel = new PictureBox()
            {
                Location = new Point(pbLocation_X, pbLocation_Y),
                Size = new Size(35, 35),
                SizeMode = PictureBoxSizeMode.StretchImage,
                //                Image = SystemIcons.Information.ToBitmap()
                Image = Properties.Resources.fuel_payment // имя ресурса с картинкой
            };

            gbFuelPayment.Controls.Add(pbFuel);

            // Сумма внутри блока: X=120, Y=15 (справа от метки), жирный шрифт
            lblFuelPaymentAmount = new Label()
            {
                Text = "0,00 руб.",
                Location = new Point(PaymentAmountLocation_X, PaymentAmountLocation_Y), // X=120, Y=15 (внутри gbCafePayment)
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold)
            };
            gbFuelPayment.Controls.Add(lblFuelPaymentAmount);

// --- Группа Мини-кафе (справа) ---
            // Расположение: справа от первой группы с отступом margin; размер: 210x~200 px (ширина группы + динамическая высота)
            gbCafe = new GroupBox()
            {
                Text = "Мини-кафе",
                Location = new Point(margin + groupWidth + margin, margin), // справа от gbFuel с отступом
                Size = new Size(groupWidth, cafeGroupHeight) // Ширина
            };
            this.Controls.Add(gbCafe);

            // Цикл для товаров кафе: каждый товар на новой строке, шаг Y=30 px
            int yPos = cafeStartY; // Стартовый Y=25
            for (int i = 0; i < cafePrices.Count; i++)
            {
                var item = new List<KeyValuePair<string, decimal>>(cafePrices)[i];

                // CheckBox товара: слева, Y=yPos, X=15
                CheckBox cb = new CheckBox()
                {
                    Text = item.Key,
                    Location = new Point(15, yPos), // X=15, Y=yPos (25, 55, 85, 115)
                    AutoSize = true
                };
                cb.CheckedChanged += CafeItem_CheckedChanged;
                gbCafe.Controls.Add(cb);
                cafeCheckBoxes.Add(cb);

                // TextBox цены: посередине, Y=yPos-3 (выше для выравнивания), X=120, ширина 50 px
                TextBox tbPrice = new TextBox()
                {
                    Location = new Point(120, yPos - 3), // X=120, Y=yPos-3
                    Width = 50,
                    ReadOnly = true,
                    TextAlign = HorizontalAlignment.Right,
                    Text = item.Value.ToString("0.00")
                };
                gbCafe.Controls.Add(tbPrice);

                // TextBox количества: справа, Y=yPos-3, X=180, ширина 50 px, отключен по умолчанию
                TextBox tbQty = new TextBox()
                {
                    Location = new Point(180, yPos - 3), // X=180, Y=yPos-3
                    Width = 50,
                    Text = "0",
                    TextAlign = HorizontalAlignment.Right,
                    Enabled = false
                };
                tbQty.KeyPress += NumericTextBox_KeyPress;
                gbCafe.Controls.Add(tbQty);
                cafeQtyTextBoxes.Add(tbQty);

                yPos += cafeItemHeight; // Следующий товар ниже на 30 px
            }

// Блок "К оплате" для кафе
            GroupBox gbCafePayment = new GroupBox()
            {
                Text = "К оплате",
                Location = new Point(PaymentLocation_X, PaymentLocation_Y), // PaymentLocation_X=10 (отступ внутри группы), PaymentLocation_Y=150
                Size = new Size(groupWidth - 20, PaymentHeight)
            };
            gbCafe.Controls.Add(gbCafePayment);

            // PictureBox иконки: слева
            pbCafe = new PictureBox()
            {
                Location = new Point(pbLocation_X, pbLocation_Y),
                Size = new Size(35, 35),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.coffee_shop // имя ресурса с картинкой
            };

            gbCafePayment.Controls.Add(pbCafe);

            // Сумма внутри блока: X=120, Y=15, жирный шрифт
            lblCafePaymentAmount = new Label()
            {
                Text = "0,00 руб.",
                Location = new Point(PaymentAmountLocation_X, PaymentAmountLocation_Y), // X=120, Y=15 (внутри gbCafePayment)
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 16, FontStyle.Bold)
            };
            gbCafePayment.Controls.Add(lblCafePaymentAmount);

// --- Общий блок "ВСЕГО к оплате" снизу по центру ---
            // Расположение: под группами с отступом 60 px для разделения; размер: 430x80 px (ширина двух групп + отступ)
            GroupBox gbTotal = new GroupBox()
            {
                Text = "ВСЕГО к оплате",
                Location = new Point(margin, Math.Max(groupHeightFuel, cafeGroupHeight) + margin), // X=10, Y=max(190,200)+10+60=270
                Size = new Size(groupWidth * 2 + margin, 110) // Ширина 210*2+10=430 px, высота 80 px
            };
            this.Controls.Add(gbTotal);

            // PictureBox иконки: слева
            pbSmile = new PictureBox()
            {
                Location = new Point(10, 20), // X=10, Y=20 (внутри gbTotal)
                Size = new Size(80, 80),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = Properties.Resources.BestOil // имя ресурса с картинкой
            };

            gbTotal.Controls.Add(pbSmile);

            // Кнопка "Рассчитать"
            btnCalculate = new Button()
            {
                Text = "Рассчитать",
                Location = new Point(120, 40),
                Size = new Size(100, 30)
            };
            btnCalculate.Click += BtnCalculate_Click;
            gbTotal.Controls.Add(btnCalculate);

            // Метка итоговой суммы: справа
            lblTotalLabel = new Label()
            {
                Text = "0,00 руб.",
                Location = new Point(250, 35), // (внутри gbTotal)
                AutoSize = true,
                Font = new Font(FontFamily.GenericSansSerif, 22, FontStyle.Bold)
            };
            gbTotal.Controls.Add(lblTotalLabel);

            // Инициализация значений
            UpdateFuelPrice();
            UpdateFuelPayment();
            UpdateCafePayment();

            // Настройка таймера
            clearTimer.Interval = 1000; // 1 секунда
            clearTimer.Tick += ClearTimer_Tick;

            // Обработка закрытия формы
            this.FormClosing += Form1_FormClosing;
        }

        private void CbFuel_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFuelPrice();
            UpdateFuelPayment();
        }

        private void UpdateFuelPrice()
        {
            string selectedFuel = cbFuel.SelectedItem.ToString();
            if (fuelPrices.TryGetValue(selectedFuel, out decimal price))
            {
                tbFuelPrice.Text = price.ToString("0.00");
            }
            else
            {
                tbFuelPrice.Text = "0.00";
            }
        }

        private void RbQuantity_CheckedChanged(object sender, EventArgs e)
        {
            if (rbQuantity.Checked)
            {
                tbQuantity.Enabled = true;
                tbSum.Enabled = false;
                tbSum.Text = "0";
                lblFuelPaymentAmount.Text = "0,00 руб.";
            }
            UpdateFuelPayment();
        }

        private void RbSum_CheckedChanged(object sender, EventArgs e)
        {
            if (rbSum.Checked)
            {
                tbSum.Enabled = true;
                tbQuantity.Enabled = false;
                tbQuantity.Text = "0";
                gbFuelPayment.Text = "К выдаче:";
            }
            UpdateFuelPayment();
        }

        private void NumericTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем цифры, запятую, Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',' && e.KeyChar != '.')
            {
                e.Handled = true;
            }
            // Запятая или точка только один раз
            TextBox tb = sender as TextBox;
            if ((e.KeyChar == ',' || e.KeyChar == '.') && tb.Text.Contains(","))
            {
                e.Handled = true;
            }
            // Заменяем точку на запятую
            if (e.KeyChar == '.')
            {
                e.KeyChar = ',';
            }
        }

        private void UpdateFuelPayment()
        {
            if (!decimal.TryParse(tbFuelPrice.Text, out decimal price))
            {
                price = 0;
            }

            if (rbQuantity.Checked)
            {
                if (decimal.TryParse(tbQuantity.Text, out decimal qty))
                {
                    decimal sum = qty * price;
                    lblFuelPaymentAmount.Text = sum.ToString("0.00") + " руб.";
                }
                else
                {
                    lblFuelPaymentAmount.Text = "0,00 руб.";
                }
            }
            else // rbSum.Checked
            {
                if (decimal.TryParse(tbSum.Text, out decimal sum))
                {
                    if (price > 0)
                    {
                        decimal qty = sum / price;
                        lblFuelPaymentAmount.Text = qty.ToString("0.00") + " л.";
                    }
                    else
                    {
                        lblFuelPaymentAmount.Text = "0,00 л.";
                    }
                }
                else
                {
                    lblFuelPaymentAmount.Text = "0,00 л.";
                }
            }
        }

        private void CafeItem_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < cafeCheckBoxes.Count; i++)
            {
                cafeQtyTextBoxes[i].Enabled = cafeCheckBoxes[i].Checked;
                if (!cafeCheckBoxes[i].Checked)
                {
                    cafeQtyTextBoxes[i].Text = "0";
                }
            }
            UpdateCafePayment();
        }

        private void UpdateCafePayment()
        {
            decimal total = 0;
            for (int i = 0; i < cafeCheckBoxes.Count; i++)
            {
                if (cafeCheckBoxes[i].Checked)
                {
                    if (decimal.TryParse(cafeQtyTextBoxes[i].Text, out decimal qty) && qty > 0)
                    {
                        string product = cafeCheckBoxes[i].Text;
                        decimal price = cafePrices[product];
                        total += qty * price;
                    }
                }
            }
            lblCafePaymentAmount.Text = total.ToString("0.00") + " руб.";
        }

        private void BtnCalculate_Click(object sender, EventArgs e)
        {
            // Обновляем платежи
            UpdateFuelPayment();
            UpdateCafePayment();

            decimal fuelSum = 0;
            decimal cafeSum = 0;

            // Получаем сумму за топливо
            if (rbQuantity.Checked)
            {
                if (!decimal.TryParse(tbQuantity.Text, out decimal qtyFuel) || qtyFuel < 0)
                {
                    MessageBox.Show("Введите корректное количество топлива.");
                    return;
                }
                if (!decimal.TryParse(tbFuelPrice.Text, out decimal priceFuel))
                {
                    priceFuel = 0;
                }
                fuelSum = qtyFuel * priceFuel;
            }
            else // rbSum.Checked
            {
                if (!decimal.TryParse(tbSum.Text, out decimal sumFuel) || sumFuel < 0)
                {
                    MessageBox.Show("Введите корректную сумму для топлива.");
                    return;
                }
                fuelSum = sumFuel;
            }

            // Получаем сумму за кафе
            if (!decimal.TryParse(lblCafePaymentAmount.Text.Replace(" руб.", ""), out cafeSum))
            {
                cafeSum = 0;
            }

            decimal total = fuelSum + cafeSum;
            lblTotalLabel.Text = total.ToString("0.00") + " руб.";

            // Добавляем к общей выручке
            totalRevenue += total;

            // Запускаем таймер очистки с отсчетом
            clearCountdown = 10;
            clearTimer.Start();
            btnCalculate.Enabled = false; // Чтобы не нажали повторно до очистки
        }

        private void ClearTimer_Tick(object sender, EventArgs e)
        {
            clearCountdown--;
            if (clearCountdown <= 0)
            {
                clearTimer.Stop();
                var result = MessageBox.Show("Очистить форму для следующего клиента?", "Очистка", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    ResetForm();
                    btnCalculate.Enabled = true;
                }
                else
                {
                    clearCountdown = 10; // Продлить еще 10 секунд
                    clearTimer.Start();
                }
            }
        }

        private void ResetForm()
        {
            // Сброс значений топлива
            cbFuel.SelectedIndex = 0;
            tbQuantity.Text = "0";
            tbSum.Text = "0";
            rbQuantity.Checked = true;

            // Сброс кафе
            for (int i = 0; i < cafeCheckBoxes.Count; i++)
            {
                cafeCheckBoxes[i].Checked = false;
                cafeQtyTextBoxes[i].Text = "0";
                cafeQtyTextBoxes[i].Enabled = false;
            }

            gbFuelPayment.Text = "К оплате:";  // Теперь работает, так как gbFuelPayment - поле класса
            lblFuelPaymentAmount.Text = "0,00 руб.";
            lblCafePaymentAmount.Text = "0,00 руб.";
            lblTotalLabel.Text = "0,00 руб.";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            MessageBox.Show($"Общая выручка за день: {totalRevenue:0.00} руб.", "Итог", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // Чтобы обновлять суммы при изменении ввода
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            tbQuantity.TextChanged += (s, ev) => { if (rbQuantity.Checked) UpdateFuelPayment(); };
            tbSum.TextChanged += (s, ev) => { if (rbSum.Checked) UpdateFuelPayment(); };
            foreach (var tb in cafeQtyTextBoxes)
            {
                tb.TextChanged += (s, ev) => UpdateCafePayment();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
