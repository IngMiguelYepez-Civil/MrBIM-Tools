using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MrBIM.Tools
{
    internal class RevitWindow : IWin32Window
    {
        public IntPtr Handle { get; private set; }
        public RevitWindow(IntPtr handle) { Handle = handle; }
    }

    internal class ToolForm : Form
    {
        public ToolForm(string title, int width, int height)
        {
            Text = "MrBIM Tools | " + title;
            StartPosition = FormStartPosition.CenterParent;
            ClientSize = new Size(width, height);
            MinimumSize = new Size(width, height);
            Font = new Font("Segoe UI", 10F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ShowInTaskbar = false;
            MaximizeBox = false;
            MinimizeBox = false;
        }
        protected static Button MakeButton(string text, EventHandler onClick)
        {
            var button = new Button { Text = text, AutoSize = true, Padding = new Padding(8, 3, 8, 3) };
            button.Click += onClick;
            return button;
        }
        protected static TableLayoutPanel Layout(Control parent)
        {
            var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(14), ColumnCount = 1 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            parent.Controls.Add(layout);
            return layout;
        }
        protected static void AddRow(TableLayoutPanel panel, Control control, float height, bool percent = false)
        {
            int row = panel.RowCount++;
            panel.RowStyles.Add(new RowStyle(percent ? SizeType.Percent : SizeType.Absolute, height));
            control.Dock = DockStyle.Fill;
            panel.Controls.Add(control, 0, row);
        }
    }

    internal class ScopeForm : ToolForm
    {
        public static readonly string[] ScopeNames = { "Selección actual", "Vista activa", "Proyecto completo" };
        private readonly ComboBox scope = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        public int Scope { get { return scope.SelectedIndex; } }
        public ScopeForm() : base("Alcance de revisión", 500, 220)
        {
            var layout = Layout(this);
            AddRow(layout, new Label { Text = "Elija qué elementos desea revisar.", AutoSize = true }, 34);
            scope.Items.AddRange(ScopeNames);
            scope.SelectedIndex = 0;
            AddRow(layout, scope, 38);
            AddRow(layout, new Label { Text = "Se revisa el modelo anfitrión. El contenido de los vínculos no se incluye." }, 58);
            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft };
            var next = MakeButton("Continuar", (s, e) => { DialogResult = DialogResult.OK; Close(); });
            var cancel = MakeButton("Cancelar", (s, e) => { DialogResult = DialogResult.Cancel; Close(); });
            buttons.Controls.Add(next); buttons.Controls.Add(cancel);
            AddRow(layout, buttons, 48);
            AcceptButton = next; CancelButton = cancel;
        }
    }

    internal class CategoryChoice
    {
        public long Id { get; private set; }
        public string Name { get; private set; }
        public CategoryChoice(long id, string name) { Id = id; Name = name; }
        public override string ToString() { return Name + " [" + Id + "]"; }
    }

    internal class ConfigurationForm : ToolForm
    {
        private readonly CheckedListBox categories = new CheckedListBox { CheckOnClick = true };
        private readonly TextBox parameters = new TextBox { Multiline = true, ScrollBars = ScrollBars.Vertical };
        private readonly CheckBox typeFallback = new CheckBox { Text = "Buscar en tipo solo si el parámetro no existe en instancia", AutoSize = true };
        public HashSet<long> SelectedCategories { get; private set; }
        public List<string> ParameterNames { get; private set; }
        public bool AllowType { get { return typeFallback.Checked; } }
        public ConfigurationForm(IEnumerable<CategoryChoice> choices) : base("Configurar revisión", 670, 610)
        {
            var layout = Layout(this);
            AddRow(layout, new Label { Text = "Categorías que desea revisar:" }, 28);
            foreach (CategoryChoice choice in choices) categories.Items.Add(choice, false);
            AddRow(layout, categories, 100, true);
            var toggles = new FlowLayoutPanel();
            toggles.Controls.Add(MakeButton("Marcar todas", (s, e) => SetAll(true)));
            toggles.Controls.Add(MakeButton("Desmarcar todas", (s, e) => SetAll(false)));
            AddRow(layout, toggles, 48);
            AddRow(layout, new Label { Text = "Parámetros obligatorios: nombres exactos separados por punto y coma o por línea." }, 48);
            AddRow(layout, parameters, 88);
            AddRow(layout, new Label { Text = "Ejemplo: Código; Clasificación; Zona. Use los nombres reales de su proyecto." }, 46);
            AddRow(layout, typeFallback, 44);
            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft };
            var review = MakeButton("Revisar", (s, e) => Confirm());
            var cancel = MakeButton("Cancelar", (s, e) => { DialogResult = DialogResult.Cancel; Close(); });
            buttons.Controls.Add(review); buttons.Controls.Add(cancel);
            AddRow(layout, buttons, 48);
            CancelButton = cancel;
        }
        private void SetAll(bool state)
        {
            for (int i = 0; i < categories.Items.Count; i++) categories.SetItemChecked(i, state);
        }
        private void Confirm()
        {
            ParameterNames = Rules.ParseNames(parameters.Text);
            SelectedCategories = new HashSet<long>(categories.CheckedItems.Cast<CategoryChoice>().Select(c => c.Id));
            if (ParameterNames.Count == 0 || SelectedCategories.Count == 0)
            {
                MessageBox.Show(this, "Seleccione al menos una categoría y escriba un parámetro.", "MrBIM Tools");
                return;
            }
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    internal class AuditProgressForm : ToolForm
    {
        private readonly ProgressBar bar;
        private readonly Label status = new Label();
        private bool running;
        public bool CancelRequested { get; private set; }
        public AuditProgressForm(int count) : base("Revisando parámetros", 480, 170)
        {
            ControlBox = false;
            var layout = Layout(this);
            AddRow(layout, status, 36);
            bar = new ProgressBar { Maximum = Math.Max(1, count) };
            AddRow(layout, bar, 30);
            AddRow(layout, MakeButton("Cancelar revisión", (s, e) => { CancelRequested = true; }), 48);
            FormClosing += (s, e) => { if (running) { CancelRequested = true; e.Cancel = true; } };
        }
        public void Run(IWin32Window owner, Action audit)
        {
            Exception failure = null;
            Shown += (s, e) => {
                running = true;
                try { audit(); }
                catch (Exception ex) { failure = ex; }
                finally { running = false; Close(); }
            };
            // ShowDialog deshabilita la ventana dueña mientras se procesa la revisión.
            ShowDialog(owner);
            if (failure != null) throw new InvalidOperationException("Falló la revisión.", failure);
        }
        public void UpdateCount(int value)
        {
            bar.Value = value;
            status.Text = "Elementos revisados: " + value + " / " + bar.Maximum;
            Application.DoEvents();
        }
    }

    internal class ResultsForm : ToolForm
    {
        private readonly AuditReport report;
        private readonly DataGridView grid = new DataGridView();
        public List<long> SelectedIds { get; private set; } = new List<long>();
        public bool Locate { get; private set; }
        public ResultsForm(AuditReport report) : base("Resultados de revisión", 1100, 670)
        {
            this.report = report;
            var layout = Layout(this);
            string counts = string.Join(" | ", report.Findings.GroupBy(x => x.State).OrderBy(g => g.Key)
                .Select(g => g.Key + ": " + g.Count()));
            AddRow(layout, new Label {
                Text = report.Project + " | " + report.Scope + "\nElementos: " + report.ElementCount
                    + " | Comprobaciones: " + report.CheckCount + " | OK: " + report.OkCount
                    + " | Hallazgos: " + report.Findings.Count + "\n" + counts
            }, 78);
            grid.AutoGenerateColumns = false;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.MultiSelect = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            foreach (var col in new[] {
                new[] { "ElementId", "ID" }, new[] { "Category", "Categoría" }, new[] { "Element", "Elemento" },
                new[] { "Parameter", "Parámetro" }, new[] { "State", "Estado" }, new[] { "Source", "Origen" }, new[] { "Detail", "Detalle" }
            }) grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = col[0], HeaderText = col[1] });
            grid.DataSource = report.Findings;
            AddRow(layout, grid, 100, true);
            AddRow(layout, new Label {
                Text = report.Findings.Count == 0 ? "Sin hallazgos para las reglas y alcance elegidos. Puede exportar el resumen."
                    : "Seleccione filas para localizar sus elementos. El CSV incluye todos los hallazgos."
            }, 40);
            var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.LeftToRight, WrapContents = true };
            buttons.Controls.Add(MakeButton("Exportar CSV", (s, e) => Export()));
            buttons.Controls.Add(MakeButton("Localizar filas", (s, e) => Choose(true, false)));
            buttons.Controls.Add(MakeButton("Seleccionar todos", (s, e) => Choose(false, true)));
            var close = MakeButton("Cerrar", (s, e) => Close());
            buttons.Controls.Add(close);
            AddRow(layout, buttons, 60);
            CancelButton = close;
        }
        private void Choose(bool locate, bool all)
        {
            var rows = all ? report.Findings : grid.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as Finding).Where(r => r != null).ToList();
            SelectedIds = rows.Select(r => r.ElementId).Distinct().ToList();
            if (SelectedIds.Count == 0) { MessageBox.Show(this, "Seleccione al menos una fila con hallazgos.", "MrBIM Tools"); return; }
            Locate = locate;
            Close();
        }
        private void Export()
        {
            using (var dialog = new SaveFileDialog {
                Filter = "Informe CSV (*.csv)|*.csv", DefaultExt = "csv", AddExtension = true,
                FileName = "MrBIM_Parametros_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".csv",
                OverwritePrompt = true
            })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try { CsvReport.Write(dialog.FileName, report); MessageBox.Show(this, "Informe guardado.", "MrBIM Tools"); }
                catch (Exception ex) { MessageBox.Show(this, "No se pudo guardar el informe:\n" + ex.Message, "MrBIM Tools"); }
            }
        }
    }
}
