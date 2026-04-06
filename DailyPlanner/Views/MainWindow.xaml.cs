using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using DailyPlanner.Models;
using DailyPlanner.ViewModels;

namespace DailyPlanner.Views
{
    public partial class MainWindow : Window
    {
        private MainViewModel _vm;

        private static readonly string[] Palette = new[]
        {
            "#EF4444", "#F97316", "#EAB308", "#22C55E",
            "#14B8A6", "#3B82F6", "#6366F1", "#A855F7",
            "#EC4899"
        };

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

        public MainWindow()
        {
            InitializeComponent();
            
            // Set initial language for WPF controls
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(DailyPlanner.Services.LanguageService.CurrentCulture.IetfLanguageTag);

            _vm = new MainViewModel();
            DataContext = _vm;

            // SelectPageCommand artık PreviewMouseLeftButtonDown ile handle ediliyor
            // (TextBox tıklama olayını yutmaması için Command yerine Preview event kullanıyoruz)
            _vm.SelectPageCommand = new RelayCommand(obj =>
            {
                if (obj is PlanPage page)
                    _vm.SelectedPage = page;
            });

            var dataService = new DailyPlanner.Services.DataService();
            var settings = dataService.LoadSettings();
            ApplySidebarColor(settings.MainSidebarColor);
        }

        // Tunnel aşamasında (TextBox'tan önce) çalışır → sayfa seçimi yapılır
        // Event Handled=false bırakıldığı için TextBox odaklanıp isim düzenlenir
        private void PageTab_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ((sender as Button)?.DataContext is PlanPage page)
                _vm.SelectedPage = page;
            // e.Handled = false (default) → TextBox da olayı alır, odaklanır
        }

        private void ColorDot_Click(object sender, RoutedEventArgs e)
        {
            var page = (sender as Button)?.Tag as PlanPage;
            if (page == null) return;

            // Popup oluştur
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
                Background    = (Brush)TryFindResource("ContentBackgroundBrush") ?? Brushes.White,
                BorderBrush   = (Brush)TryFindResource("BorderBrush") ?? Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius  = new CornerRadius(10, 10, 10, 10),
                Padding       = new Thickness(10, 10, 10, 10),
                Effect        = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color         = Colors.Black,
                    Opacity       = 0.18,
                    BlurRadius    = 12,
                    ShadowDepth   = 2
                }
            };

            var stack = new StackPanel { Width = 170 };

            // Başlık
            stack.Children.Add(new TextBlock
            {
                Text       = (string)TryFindResource("Text_PickColor") ?? "Renk Seç",
                FontSize   = 11,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)TryFindResource("SecondaryTextBrush") ?? Brushes.Gray,
                Margin     = new Thickness(0, 0, 0, 8)
            });

            // 4x3 renk ızgarası
            var grid = new UniformGrid { Columns = 4, Rows = 3 };
            foreach (var hex in Palette)
            {
                var swatch = CreateSwatch(hex, page, popup);
                grid.Children.Add(swatch);
            }
            stack.Children.Add(grid);

            // Temizle butonu
            var clearBtn = new Button
            {
                Content         = (string)TryFindResource("Text_ClearColor") ?? "↩ Rengi Temizle",
                Margin          = new Thickness(0, 8, 0, 0),
                Padding         = new Thickness(6, 4, 6, 4),
                Cursor          = Cursors.Hand,
                Background      = (Brush)TryFindResource("AccentLightBrush") ?? Brushes.AliceBlue,
                BorderThickness = new Thickness(0),
                FontSize        = 11,
                Foreground      = (Brush)TryFindResource("AccentBrush") ?? Brushes.DodgerBlue,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };
            clearBtn.Click += (s, _) =>
            {
                page.Color = null;
                _vm.SetPageColorCommand?.Execute(new object[] { page, null });
                popup.IsOpen = false;
            };
            stack.Children.Add(clearBtn);

            outer.Child  = stack;
            popup.Child  = outer;
            popup.IsOpen = true;
        }

        private Button CreateSwatch(string hex, PlanPage page, Popup popup)
        {
            var color  = (Color)ColorConverter.ConvertFromString(hex);
            var brush  = new SolidColorBrush(color);

            var swatch = new Button
            {
                Width           = 28,
                Height          = 28,
                Margin          = new Thickness(3),
                Background      = brush,
                BorderThickness = new Thickness(page.Color == hex ? 2.5 : 0),
                BorderBrush     = Brushes.White,
                Cursor          = Cursors.Hand,
                Tag             = hex,
                ToolTip         = hex
            };
            swatch.Template = BuildSwatchTemplate();
            swatch.Click += (s, _) =>
            {
                page.Color = hex;
                _vm.SetPageColorCommand?.Execute(new object[] { page, hex });
                popup.IsOpen = false;
            };
            return swatch;
        }

        private ControlTemplate BuildSwatchTemplate()
        {
            var factory = new FrameworkElementFactory(typeof(Border));
            factory.SetBinding(Border.BackgroundProperty, new System.Windows.Data.Binding("Background")
            {
                RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });
            factory.SetBinding(Border.BorderBrushProperty, new System.Windows.Data.Binding("BorderBrush")
            {
                RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });
            factory.SetBinding(Border.BorderThicknessProperty, new System.Windows.Data.Binding("BorderThickness")
            {
                RelativeSource = new System.Windows.Data.RelativeSource(System.Windows.Data.RelativeSourceMode.TemplatedParent)
            });
            factory.SetValue(Border.CornerRadiusProperty, new CornerRadius(6));

            var tpl = new ControlTemplate(typeof(Button)) { VisualTree = factory };
            return tpl;
        }
        private void ThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            var page = _vm.SelectedPage;
            if (page == null) return;

            var popup = new Popup
            {
                StaysOpen = false,
                Placement = PlacementMode.Bottom,
                PlacementTarget = sender as Button,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            var outer = new Border
            {
                Background = (Brush)TryFindResource("ContentBackgroundBrush") ?? Brushes.White,
                BorderBrush = (Brush)TryFindResource("BorderBrush") ?? Brushes.LightGray,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black, Opacity = 0.18, BlurRadius = 12, ShadowDepth = 2
                }
            };

            var stack = new StackPanel { Width = 150 };
            
            stack.Children.Add(new TextBlock
            {
                Text = (string)TryFindResource("Text_BackgroundTheme") ?? "Arka Plan Teması", FontSize = 11, FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)TryFindResource("SecondaryTextBrush") ?? Brushes.Gray,
                Margin = new Thickness(0, 0, 0, 8)
            });

            var themes = new[]
            {
                ((string)TryFindResource("Text_ThemeNone") ?? "❌ Sade (Yok)", (string)null),
                ((string)TryFindResource("Text_ThemeStudy") ?? "📚 Ders / Eğitim", "pack://application:,,,/Resources/Backgrounds/bg_study.png"),
                ((string)TryFindResource("Text_ThemeSport") ?? "🏋️ Spor / Fitness", "pack://application:,,,/Resources/Backgrounds/bg_sport.png"),
                ((string)TryFindResource("Text_ThemeFood") ?? "🍳 Yemek / Mutfak", "pack://application:,,,/Resources/Backgrounds/bg_food.png"),
                ((string)TryFindResource("Text_ThemeWork") ?? "💼 İş / Planlama", "pack://application:,,,/Resources/Backgrounds/bg_work.png"),
                ((string)TryFindResource("Text_ThemeMusic") ?? "🎵 Müzik / Sanat", "pack://application:,,,/Resources/Backgrounds/bg_music.png")
            };

            foreach (var (title, path) in themes)
            {
                var isSelected = page.BackgroundImage == path;
                var btn = new Button
                {
                    Content = title,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(8, 6, 8, 6),
                    Margin = new Thickness(0, 2, 0, 2),
                    Background = isSelected ? ((Brush)TryFindResource("AccentLightBrush") ?? Brushes.AliceBlue) : Brushes.Transparent,
                    Foreground = isSelected ? ((Brush)TryFindResource("AccentBrush") ?? Brushes.DodgerBlue) : ((Brush)TryFindResource("PrimaryTextBrush") ?? Brushes.Black),
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand,
                    FontWeight = isSelected ? FontWeights.SemiBold : FontWeights.Normal
                };
                
                btn.Click += (ss, ee) =>
                {
                    page.BackgroundImage = path;
                    // Trigger save
                    _vm.SetPageColorCommand?.Execute(new object[] { page, page.Color }); 
                    popup.IsOpen = false;
                };
                stack.Children.Add(btn);
            }

            outer.Child = stack;
            popup.Child = outer;
            popup.IsOpen = true;
        }

        private void ThemeToggle_Click(object sender, RoutedEventArgs e)
        {
            _vm.IsDarkMode = !_vm.IsDarkMode;
        }

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
                Placement       = PlacementMode.Right,
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

            var dataService = new DailyPlanner.Services.DataService();
            var settings = dataService.LoadSettings();

            foreach (var (key, hex) in SidebarPalette)
            {
                var isSelected = settings.MainSidebarColor == hex;
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
                
                btn.Template = new ControlTemplate(typeof(Button))
                {
                    VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
                };

                var capturedHex = hex;
                btn.Click += (s, ev) =>
                {
                    settings.MainSidebarColor = capturedHex;
                    dataService.SaveSettings(settings);
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


        private void LanguageToggle_Click(object sender, RoutedEventArgs e)
        {
            var dataService = new DailyPlanner.Services.DataService();
            var settings = dataService.LoadSettings();

            string newLang = settings.Language == "tr" ? "en" : "tr";
            settings.Language = newLang;
            dataService.SaveSettings(settings);

            // SetLanguage handles: thread culture, resource dictionary swap,
            // Window.Language on all windows, and fires LanguageChanged event
            DailyPlanner.Services.LanguageService.SetLanguage(newLang);

            // Refresh VM date strings and calendar
            _vm.RefreshLocalization();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
        }
    }
}
