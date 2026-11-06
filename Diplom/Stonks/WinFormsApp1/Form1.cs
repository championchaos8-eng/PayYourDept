using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public class Debt
    {
        public string Person { get; set; } = "";
        public decimal Amount { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.Today;
    }

    public partial class Form1 : Form
    {
        private readonly List<Debt> debts = new List<Debt>();
        private readonly string dataFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PayYourDebt", "debts.json");

        private Label totalAmountLabel = new Label();
        private Label activeCountLabel = new Label();
        private Label paidTotalLabel = new Label();
        private ListView debtList = new ListView();

        private decimal totalPaid = 0m;

        private readonly Color backgroundColor = Color.FromArgb(15, 15, 22);
        private readonly Color panelColor = Color.FromArgb(28, 28, 40);
        private readonly Color purpleColor = Color.FromArgb(124, 58, 237);
        private readonly Color textColor = Color.White;
        private readonly Color mutedColor = Color.Silver;

        public Form1()
        {
            InitializeComponent();
            BuildInterface();
            LoadData();
            RefreshDashboard();
        }

        private void BuildInterface()
        {
            Text = "PayYourDebt";
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(1100, 720);
            MinimumSize = new Size(850, 600);
            BackColor = backgroundColor;
            ForeColor = textColor;
            Font = new Font("Segoe UI", 10);
            FormBorderStyle = FormBorderStyle.Sizable;

            // Верхний заголовок
            Label titleLabel = new Label
            {
                Text = "PayYourDebt",
                Font = new Font("Segoe UI", 25, FontStyle.Bold),
                ForeColor = textColor,
                Location = new Point(28, 20),
                AutoSize = true
            };
            Controls.Add(titleLabel);

            Label subtitleLabel = new Label
            {
                Text = "Менеджер задолженностей",
                Font = new Font("Segoe UI", 10),
                ForeColor = mutedColor,
                Location = new Point(32, 66),
                AutoSize = true
            };
            Controls.Add(subtitleLabel);

            // Общая задолженность
            Label totalCaption = new Label
            {
                Text = "ОБЩАЯ СУММА ДОЛГА",
                ForeColor = mutedColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(32, 115),
                AutoSize = true
            };
            Controls.Add(totalCaption);

            totalAmountLabel = new Label
            {
                Text = "0 сум",
                ForeColor = Color.FromArgb(196, 181, 253),
                Font = new Font("Segoe UI", 28, FontStyle.Bold),
                Location = new Point(28, 140),
                AutoSize = true
            };
            Controls.Add(totalAmountLabel);

            // Карточки показателей
            Panel activePanel = CreateCard(new Point(32, 215), new Size(300, 95));
            Controls.Add(activePanel);
            Label activeCaption = new Label
            {
                Text = "Активные долги",
                ForeColor = mutedColor,
                Location = new Point(18, 14),
                AutoSize = true
            };
            activePanel.Controls.Add(activeCaption);
            activeCountLabel = new Label
            {
                Text = "0",
                ForeColor = textColor,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                Location = new Point(18, 39),
                AutoSize = true
            };
            activePanel.Controls.Add(activeCountLabel);

            Panel paidPanel = CreateCard(new Point(350, 215), new Size(300, 95));
            Controls.Add(paidPanel);
            Label paidCaption = new Label
            {
                Text = "Погашено всего",
                ForeColor = mutedColor,
                Location = new Point(18, 14),
                AutoSize = true
            };
            paidPanel.Controls.Add(paidCaption);
            paidTotalLabel = new Label
            {
                Text = "0 сум",
                ForeColor = Color.LightGreen,
                Font = new Font("Segoe UI", 18, FontStyle.Bold),
                Location = new Point(18, 42),
                AutoSize = true
            };
            paidPanel.Controls.Add(paidTotalLabel);

            // Кнопки
            Button addButton = CreateButton("+ Добавить долг", new Point(32, 330), purpleColor);
            addButton.Click += AddDebtButton_Click;
            Controls.Add(addButton);

            Button payButton = CreateButton("− Погасить долг", new Point(245, 330), panelColor);
            payButton.FlatAppearance.BorderColor = purpleColor;
            payButton.Click += PayDebtButton_Click;
            Controls.Add(payButton);

            Button deleteButton = CreateButton("Удалить запись", new Point(458, 330), panelColor);
            deleteButton.FlatAppearance.BorderColor = Color.FromArgb(90, 90, 105);
            deleteButton.Click += DeleteDebtButton_Click;
            Controls.Add(deleteButton);

            // Список долгов
            Label listCaption = new Label
            {
                Text = "СПИСОК ДОЛГОВ",
                ForeColor = mutedColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Location = new Point(32, 395),
                AutoSize = true
            };
            Controls.Add(listCaption);

            debtList = new ListView
            {
                Location = new Point(32, 425),
                Size = new Size(1015, 220),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false,
                HideSelection = false,
                BackColor = panelColor,
                ForeColor = textColor,
                BorderStyle = BorderStyle.FixedSingle
            };
            debtList.Columns.Add("Кому должны", 330);
            debtList.Columns.Add("Сумма (сум)", 220);
            debtList.Columns.Add("Дата добавления", 200);
            debtList.Columns.Add("Статус", 180);
            Controls.Add(debtList);

            Label footer = new Label
            {
                Text = "Данные сохраняются на этом компьютере.",
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(32, 655),
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left
            };
            Controls.Add(footer);
        }

        private Panel CreateCard(Point location, Size size)
        {
            return new Panel
            {
                Location = location,
                Size = size,
                BackColor = panelColor
            };
        }

        private Button CreateButton(string caption, Point location, Color color)
        {
            Button button = new Button
            {
                Text = caption,
                Location = location,
                Size = new Size(195, 44),
                BackColor = color,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 1;
            return button;
        }

        private void AddDebtButton_Click(object? sender, EventArgs e)
        {
            using Form dialog = new Form
            {
                Text = "Добавить долг",
                Size = new Size(390, 260),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = backgroundColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10)
            };

            Label nameLabel = new Label
            {
                Text = "Кому вы должны?",
                Location = new Point(20, 20),
                AutoSize = true
            };
            TextBox nameBox = new TextBox
            {
                Location = new Point(20, 47),
                Size = new Size(330, 27),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Label amountLabel = new Label
            {
                Text = "Сумма долга (сум):",
                Location = new Point(20, 85),
                AutoSize = true
            };
            TextBox amountBox = new TextBox
            {
                Location = new Point(20, 112),
                Size = new Size(330, 27),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Button saveButton = CreateButton("Сохранить", new Point(20, 160), purpleColor);
            saveButton.DialogResult = DialogResult.None;
            saveButton.Click += (s, args) =>
            {
                string person = nameBox.Text.Trim();
                string amountText = amountBox.Text.Trim().Replace(" ", "");

                bool parsed = decimal.TryParse(amountText, NumberStyles.Number,
                    CultureInfo.CurrentCulture, out decimal amount);

                if (!parsed)
                    parsed = decimal.TryParse(amountText, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out amount);

                if (string.IsNullOrWhiteSpace(person))
                {
                    MessageBox.Show(dialog, "Введите имя или название организации.",
                        "Не указано имя", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!parsed || amount <= 0)
                {
                    MessageBox.Show(dialog, "Введите сумму больше нуля, например 500000.",
                        "Некорректная сумма", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                debts.Add(new Debt
                {
                    Person = person,
                    Amount = amount,
                    DateAdded = DateTime.Today
                });

                SaveData();
                RefreshDashboard();
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            };

            dialog.Controls.Add(nameLabel);
            dialog.Controls.Add(nameBox);
            dialog.Controls.Add(amountLabel);
            dialog.Controls.Add(amountBox);
            dialog.Controls.Add(saveButton);
            dialog.AcceptButton = saveButton;
            dialog.ShowDialog(this);
        }

        private void PayDebtButton_Click(object? sender, EventArgs e)
        {
            if (debtList.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "Сначала выберите долг в списке.",
                    "Долг не выбран", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = debtList.SelectedIndices[0];
            if (index < 0 || index >= debts.Count)
                return;

            Debt debt = debts[index];

            using Form dialog = new Form
            {
                Text = "Погасить долг",
                Size = new Size(390, 220),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = backgroundColor,
                ForeColor = textColor,
                Font = new Font("Segoe UI", 10)
            };

            Label info = new Label
            {
                Text = $"Долг: {debt.Person}\nОсталось: {FormatMoney(debt.Amount)}",
                Location = new Point(20, 18),
                AutoSize = true
            };
            Label amountLabel = new Label
            {
                Text = "Сколько погасить (сум):",
                Location = new Point(20, 70),
                AutoSize = true
            };
            TextBox amountBox = new TextBox
            {
                Location = new Point(20, 96),
                Size = new Size(330, 27),
                Text = debt.Amount.ToString(CultureInfo.InvariantCulture)
            };
            Button payButton = CreateButton("Подтвердить", new Point(20, 135), purpleColor);
            payButton.Click += (s, args) =>
            {
                string amountText = amountBox.Text.Trim().Replace(" ", "");
                bool parsed = decimal.TryParse(amountText, NumberStyles.Number,
                    CultureInfo.CurrentCulture, out decimal payment);

                if (!parsed)
                    parsed = decimal.TryParse(amountText, NumberStyles.Number,
                        CultureInfo.InvariantCulture, out payment);

                if (!parsed || payment <= 0)
                {
                    MessageBox.Show(dialog, "Введите сумму больше нуля.",
                        "Некорректная сумма", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (payment > debt.Amount)
                {
                    MessageBox.Show(dialog, "Платёж не может быть больше оставшегося долга.",
                        "Слишком большая сумма", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                debt.Amount -= payment;
                totalPaid += payment;

                if (debt.Amount == 0)
                    debts.Remove(debt);

                SaveData();
                RefreshDashboard();
                dialog.DialogResult = DialogResult.OK;
                dialog.Close();
            };

            dialog.Controls.Add(info);
            dialog.Controls.Add(amountLabel);
            dialog.Controls.Add(amountBox);
            dialog.Controls.Add(payButton);
            dialog.AcceptButton = payButton;
            dialog.ShowDialog(this);
        }

        private void DeleteDebtButton_Click(object? sender, EventArgs e)
        {
            if (debtList.SelectedIndices.Count == 0)
            {
                MessageBox.Show(this, "Сначала выберите запись в списке.",
                    "Запись не выбрана", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int index = debtList.SelectedIndices[0];
            if (index < 0 || index >= debts.Count)
                return;

            Debt debt = debts[index];
            DialogResult result = MessageBox.Show(this,
                $"Удалить долг «{debt.Person}» на сумму {FormatMoney(debt.Amount)}?\n" +
                "Это удалит запись без отметки о погашении.",
                "Подтверждение удаления",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                debts.RemoveAt(index);
                SaveData();
                RefreshDashboard();
            }
        }

        private void RefreshDashboard()
        {
            decimal total = debts.Sum(debt => debt.Amount);
            totalAmountLabel.Text = FormatMoney(total);
            activeCountLabel.Text = debts.Count.ToString();
            paidTotalLabel.Text = FormatMoney(totalPaid);

            debtList.BeginUpdate();
            debtList.Items.Clear();

            foreach (Debt debt in debts)
            {
                ListViewItem item = new ListViewItem(debt.Person);
                item.SubItems.Add(debt.Amount.ToString("N0", CultureInfo.CurrentCulture));
                item.SubItems.Add(debt.DateAdded.ToString("dd.MM.yyyy"));
                item.SubItems.Add("Не погашен");
                debtList.Items.Add(item);
            }

            debtList.EndUpdate();
        }

        private string FormatMoney(decimal amount)
        {
            return amount.ToString("N0", CultureInfo.CurrentCulture) + " сум";
        }

        private void LoadData()
        {
            try
            {
                if (!File.Exists(dataFile))
                    return;

                string json = File.ReadAllText(dataFile);
                SavedData? saved = JsonSerializer.Deserialize<SavedData>(json);

                if (saved == null)
                    return;

                debts.Clear();
                if (saved.Debts != null)
                    debts.AddRange(saved.Debts);

                totalPaid = saved.TotalPaid;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось загрузить сохранённые данные.\n" + ex.Message,
                    "Ошибка загрузки", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SaveData()
        {
            try
            {
                string? directory = Path.GetDirectoryName(dataFile);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                SavedData saved = new SavedData
                {
                    Debts = debts,
                    TotalPaid = totalPaid
                };

                string json = JsonSerializer.Serialize(saved,
                    new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(dataFile, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Не удалось сохранить данные.\n" + ex.Message,
                    "Ошибка сохранения", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private class SavedData
        {
            public List<Debt>? Debts { get; set; } = new List<Debt>();
            public decimal TotalPaid { get; set; }
        }
    }
}
