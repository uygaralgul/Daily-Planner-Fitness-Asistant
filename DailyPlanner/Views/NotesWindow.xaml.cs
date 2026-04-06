using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using DailyPlanner.Models;
using DailyPlanner.Services;
using System.Windows.Markup;

namespace DailyPlanner.Views
{
    public partial class NotesWindow : Window
    {
        private readonly DataService _dataService;
        private ObservableCollection<NoteItem> _notes;
        private NoteItem _currentNote;
        private bool _suppressEvents = false;
        private readonly Guid? _highlightNoteId;
        private AppSettings _settings;

        private static readonly (string Key, string Hex)[] SidebarPalette = new[]
        {
            ("Text_ColorDefault",     null),
            ("Text_ColorDarkNavy",    "#1E3A5F"),
            ("Text_ColorDarkGreen",   "#1A3A2A"),
            ("Text_ColorDarkPurple",  "#2D1B69"),
            ("Text_ColorDarkRed",     "#4A1A1A"),
            ("Text_ColorDarkBrown",   "#3A2210"),
            ("Text_ColorSteelBlue",   "#2B3A4A"),
            ("Text_ColorAnthracite",  "#2A2A2A"),
            ("Text_ColorForestGreen", "#234530"),
            ("Text_ColorNightBlue",   "#0D2137"),
            ("Text_ColorDarkEmerald", "#064E3B"),
            ("Text_ColorDarkMaroon",  "#7F1D1D")
        };

        public NotesWindow(DataService dataService, Guid? highlightNoteId = null)
        {
            InitializeComponent();

            // Set language for WPF controls
            this.Language = XmlLanguage.GetLanguage(LanguageService.CurrentCulture.IetfLanguageTag);

            _dataService = dataService;
            _highlightNoteId = highlightNoteId;
            _notes = _dataService.LoadNotes();
            NoteList.ItemsSource = _notes;
            UpdateEmptyState();

            // Settings yükle
            _settings = _dataService.LoadSettings();
            ApplySidebarColor(_settings.NotesSidebarColor);

            // Hatırlatıcıdan açıldıysa ilgili notu seçili göster
            if (_highlightNoteId.HasValue)
            {
                var note = _notes.FirstOrDefault(n => n.Id == _highlightNoteId.Value);
                if (note != null)
                    Loaded += (_, __) => SelectNote(note);
            }
        }

        // ──────────────────────────────────────────────
        // SIDEBAR RENK
        // ──────────────────────────────────────────────
        private void ApplySidebarColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                SidebarBorder.SetResourceReference(Border.BackgroundProperty, "SidebarBackgroundBrush");
            else
                SidebarBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }

        private void SidebarColorBtn_Click(object sender, RoutedEventArgs e)
        {
            var popup = new Popup
            {
                StaysOpen       = false,
                Placement       = PlacementMode.Bottom,
                PlacementTarget = sender as Button,
                AllowsTransparency = true,
                PopupAnimation  = PopupAnimation.Fade
            };

            var outer = new Border
            {
                Background      = (Brush)TryFindResource("ContentBackgroundBrush") ?? Brushes.White,
                BorderBrush     = (Brush)TryFindResource("BorderBrush") ?? Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(10),
                Padding         = new Thickness(10),
                Effect          = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black, Opacity = 0.18, BlurRadius = 12, ShadowDepth = 2
                }
            };

            var stack = new StackPanel { Width = 180 };
            stack.Children.Add(new TextBlock
            {
                Text       = (string)Application.Current.TryFindResource("Text_SidebarColorHeader") ?? "Sidebar Color",
                FontSize   = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)TryFindResource("SecondaryTextBrush") ?? Brushes.Gray,
                Margin     = new Thickness(0, 0, 0, 8)
            });

            var wrapPanel = new WrapPanel
            {
                ItemWidth = 36,
                ItemHeight = 36,
                Orientation = Orientation.Horizontal,
                MaxWidth = 160
            };

            foreach (var (key, hex) in SidebarPalette)
            {
                var isSelected = _settings.NotesSidebarColor == hex;
                var localizedLabel = (string)Application.Current.TryFindResource(key) ?? key;
                var btn = new Button
                {
                    Width = 30,
                    Height = 30,
                    Margin = new Thickness(3),
                    Cursor = System.Windows.Input.Cursors.Hand,
                    ToolTip = localizedLabel,
                    BorderBrush = isSelected ? Brushes.White : Brushes.Transparent,
                    BorderThickness = new Thickness(isSelected ? 2 : 0)
                };

                var interior = new Border
                {
                    CornerRadius = new CornerRadius(4),
                    Background = hex == null
                        ? ((Brush)TryFindResource("SidebarBackgroundBrush") ?? Brushes.DimGray)
                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)),
                    BorderBrush = (Brush)TryFindResource("BorderBrush") ?? Brushes.LightGray,
                    BorderThickness = new Thickness(1)
                };

                if (hex == null)
                {
                    interior.Child = new TextBlock
                    {
                        Text = "X",
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontSize = 10,
                        Opacity = 0.5
                    };
                }

                btn.Content = interior;
                
                // Remove default button styles to just show the square
                btn.Template = new ControlTemplate(typeof(Button))
                {
                    VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
                };

                var capturedHex = hex;
                btn.Click += (s, _) =>
                {
                    _settings.NotesSidebarColor = capturedHex;
                    _dataService.SaveSettings(_settings);
                    ApplySidebarColor(capturedHex);
                    popup.IsOpen = false;
                };
                wrapPanel.Children.Add(btn);
            }

            stack.Children.Add(wrapPanel);

            outer.Child  = stack;
            popup.Child  = outer;
            popup.IsOpen = true;
        }

        // ──────────────────────────────────────────────
        // NOT EKLEME / SEÇİM
        // ──────────────────────────────────────────────
        private void AddNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var note = new NoteItem
                {
                    Title     = (string)Application.Current.TryFindResource("Text_NewNote") ?? "New Note",
                    Content   = string.Empty,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _notes.Add(note);
                SelectNote(note);
                UpdateEmptyState();
                NoteTitleBox.SelectAll();
                NoteTitleBox.Focus();
            }
            catch (Exception ex)
            {
                var errorMsg = (string)Application.Current.TryFindResource("Text_ErrorAddingNote") ?? "Error adding new note";
                var errorTitle = (string)Application.Current.TryFindResource("Text_Error") ?? "Error";
                MessageBox.Show($"{errorMsg}: {ex.Message}\n{ex.StackTrace}", 
                    errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void NoteItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((sender as Button)?.Tag is NoteItem note)
                    SelectNote(note);
            }
            catch (Exception ex)
            {
                var errorMsg = (string)Application.Current.TryFindResource("Text_ErrorLoadingNote") ?? "Error loading note";
                var errorTitle = (string)Application.Current.TryFindResource("Text_Error") ?? "Error";
                MessageBox.Show($"{errorMsg}: {ex.Message}", errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectNote(NoteItem note)
        {
            try
            {
                _suppressEvents = true;
                _currentNote = note;
                NoteTitleBox.Text = note.Title;

                // RichTextBox içeriği yükle
                LoadRtfContent(note);

                NoteTitleBox.IsEnabled   = true;
                NoteContentBox.IsEnabled = true;
                DeleteBtn.IsEnabled      = true;
                ReminderBtn.IsEnabled    = true;
                FormatToolbar.Visibility = Visibility.Visible;
                EmptyState.Visibility    = Visibility.Collapsed;

                var lastEditedText = (string)Application.Current.TryFindResource("Text_LastEdited") ?? "Last edited: {0}";
                StatusText.Text = string.Format(lastEditedText, note.UpdatedAt.ToString("dd MMM yyyy HH:mm", LanguageService.CurrentCulture));
                UpdateReminderButton(note);
            }
            finally
            {
                _suppressEvents = false;
            }
        }

        private void LoadRtfContent(NoteItem note)
        {
            try
            {
                NoteContentBox.Document.Blocks.Clear();
                if (!string.IsNullOrEmpty(note.RtfContent))
                {
                    // RTF içeriği yükle
                    var bytes = System.Text.Encoding.UTF8.GetBytes(note.RtfContent);
                    using (var ms = new MemoryStream(bytes))
                    {
                        var range = new TextRange(NoteContentBox.Document.ContentStart,
                                                  NoteContentBox.Document.ContentEnd);
                        range.Load(ms, DataFormats.Rtf);
                    }
                }
                else if (!string.IsNullOrEmpty(note.Content))
                {
                    // Eski düz metin notları geriye dönük uyumlu yükle
                    var para = new Paragraph(new Run(note.Content));
                    NoteContentBox.Document.Blocks.Clear();
                    NoteContentBox.Document.Blocks.Add(para);
                }
            }
            catch
            {
                // Hata durumunda düz metin dene
                NoteContentBox.Document.Blocks.Clear();
                if (!string.IsNullOrEmpty(note.Content))
                    NoteContentBox.Document.Blocks.Add(new Paragraph(new Run(note.Content)));
            }
        }

        private string GetRtfContent()
        {
            try
            {
                var range = new TextRange(NoteContentBox.Document.ContentStart,
                                          NoteContentBox.Document.ContentEnd);
                using (var ms = new MemoryStream())
                {
                    range.Save(ms, DataFormats.Rtf);
                    return System.Text.Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch { return string.Empty; }
        }

        private string GetPlainText()
        {
            try
            {
                return new TextRange(NoteContentBox.Document.ContentStart,
                                     NoteContentBox.Document.ContentEnd).Text?.Trim() ?? string.Empty;
            }
            catch { return string.Empty; }
        }

        // ──────────────────────────────────────────────
        // FORMAT TOOLBAR
        // ──────────────────────────────────────────────
        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            NoteContentBox.Focus();
            NoteContentBox.Selection.ApplyPropertyValue(
                TextElement.FontWeightProperty,
                NoteContentBox.Selection.GetPropertyValue(TextElement.FontWeightProperty) is FontWeight fw && fw == FontWeights.Bold
                    ? FontWeights.Normal : FontWeights.Bold);
            UpdateToolbarState();
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            NoteContentBox.Focus();
            NoteContentBox.Selection.ApplyPropertyValue(
                TextElement.FontStyleProperty,
                NoteContentBox.Selection.GetPropertyValue(TextElement.FontStyleProperty) is FontStyle fs && fs == FontStyles.Italic
                    ? FontStyles.Normal : FontStyles.Italic);
            UpdateToolbarState();
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            NoteContentBox.Focus();
            var current = NoteContentBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            NoteContentBox.Selection.ApplyPropertyValue(
                Inline.TextDecorationsProperty,
                current == TextDecorations.Underline ? null : TextDecorations.Underline);
            UpdateToolbarState();
        }

        private void NoteContentBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (_suppressEvents || NoteContentBox == null) return;
            UpdateToolbarState();
        }

        private void UpdateToolbarState()
        {
            // Senkronize ToggleButton states
            var fw = NoteContentBox.Selection.GetPropertyValue(TextElement.FontWeightProperty);
            BoldBtn.IsChecked = (fw != DependencyProperty.UnsetValue) && (fw is FontWeight f && f == FontWeights.Bold);

            var fs = NoteContentBox.Selection.GetPropertyValue(TextElement.FontStyleProperty);
            ItalicBtn.IsChecked = (fs != DependencyProperty.UnsetValue) && (fs is FontStyle s && s == FontStyles.Italic);

            var td = NoteContentBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty);
            UnderlineBtn.IsChecked = (td != DependencyProperty.UnsetValue) && (td == TextDecorations.Underline);
        }

        private void FontSize_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (_suppressEvents || NoteContentBox == null) return;
            if (FontSizeBox.SelectedItem is ComboBoxItem item &&
                double.TryParse(item.Content?.ToString(), out double size))
            {
                NoteContentBox.Focus();
                NoteContentBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, size);
            }
        }

        private void TextColorBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string tag)
            {
                NoteContentBox.Focus();
                if (tag == "default")
                {
                    var defaultBrush = (Brush)TryFindResource("PrimaryTextBrush") ?? Brushes.Black;
                    NoteContentBox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, defaultBrush);
                }
                else
                {
                    var color = (Color)ColorConverter.ConvertFromString(tag);
                    NoteContentBox.Selection.ApplyPropertyValue(
                        TextElement.ForegroundProperty, new SolidColorBrush(color));
                }
            }
        }

        // ──────────────────────────────────────────────
        // HATIRLATICI & SİL
        // ──────────────────────────────────────────────
        private void UpdateReminderButton(NoteItem note)
        {
            if (note?.ReminderDate.HasValue == true)
                ReminderBtn.Content = $"🔔 {note.ReminderDate.Value.ToString("dd MMM yyyy", LanguageService.CurrentCulture)}";
            else
                ReminderBtn.Content = (string)Application.Current.TryFindResource("Text_Reminder") ?? "🔔 Reminder";
        }

        private void AddReminder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNote == null) return;
        
            var dialog = new ReminderPickerDialog(_currentNote.ReminderDate) { Owner = this };
            if (dialog.ShowDialog() == true && dialog.Confirmed)
            {
                _currentNote.ReminderDate = dialog.SelectedDate;
                _currentNote.UpdatedAt    = DateTime.Now;
                _dataService.SaveNotes(_notes);
                UpdateReminderButton(_currentNote);

                if (dialog.SelectedDate.HasValue)
                {
                    var msg = (string)Application.Current.TryFindResource("Text_ReminderSet") ?? "Reminder set: {0}";
                    StatusText.Text = string.Format(msg, dialog.SelectedDate.Value.ToString("dd MMM yyyy", LanguageService.CurrentCulture));
                }
                else
                {
                    StatusText.Text = (string)Application.Current.TryFindResource("Text_ReminderRemoved") ?? "Reminder removed.";
                }
            }
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (_currentNote == null) return;
            
            var confirmMsg = (string)Application.Current.TryFindResource("Text_DeleteNoteConfirm") ?? "Delete note '{0}'?";
            var confirmTitle = (string)Application.Current.TryFindResource("Text_DeleteNoteTitle") ?? "Delete Note";
            
            var result = MessageBox.Show(string.Format(confirmMsg, _currentNote.Title),
                confirmTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            _notes.Remove(_currentNote);
            _currentNote = null;
            NoteTitleBox.Text    = (string)Application.Current.TryFindResource("Text_SelectOrCreateNote") ?? "Select a note or create a new one";
            NoteContentBox.Document.Blocks.Clear();
            NoteTitleBox.IsEnabled    = false;
            NoteContentBox.IsEnabled  = false;
            DeleteBtn.IsEnabled       = false;
            ReminderBtn.IsEnabled     = false;
            ReminderBtn.Content       = (string)Application.Current.TryFindResource("Text_Reminder") ?? "🔔 Reminder";
            FormatToolbar.Visibility  = Visibility.Collapsed;
            UpdateEmptyState();
        }

        // ──────────────────────────────────────────────
        // TEXT CHANGED
        // ──────────────────────────────────────────────
        private void NoteTitleBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;
                NoteContentBox.Focus();
            }
        }

        private void NoteTitleBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressEvents || _currentNote == null) return;
            _currentNote.Title     = NoteTitleBox.Text;
            _currentNote.UpdatedAt = DateTime.Now;
            NoteList.Items.Refresh();
            
            var lastEditedText = (string)Application.Current.TryFindResource("Text_LastEdited") ?? "Last edited: {0}";
            StatusText.Text = string.Format(lastEditedText, _currentNote.UpdatedAt.ToString("dd MMM yyyy HH:mm", LanguageService.CurrentCulture));
        }

        private void NoteContentBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_suppressEvents || _currentNote == null) return;
            _currentNote.RtfContent = GetRtfContent();
            _currentNote.Content    = GetPlainText(); // geriye dönük uyum
            _currentNote.UpdatedAt  = DateTime.Now;
            
            var lastEditedText = (string)Application.Current.TryFindResource("Text_LastEdited") ?? "Last edited: {0}";
            StatusText.Text = string.Format(lastEditedText, _currentNote.UpdatedAt.ToString("dd MMM yyyy HH:mm", LanguageService.CurrentCulture));
        }

        // ──────────────────────────────────────────────
        // HELPERS
        // ──────────────────────────────────────────────
        private void UpdateEmptyState()
        {
            EmptyState.Visibility = _notes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _dataService.SaveNotes(_notes);
        }
    }
}
