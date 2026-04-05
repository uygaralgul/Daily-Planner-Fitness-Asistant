using System;
using System.Windows;

namespace DailyPlanner.Views
{
    public partial class CalculatorsWindow : Window
    {
        public CalculatorsWindow() => InitializeComponent();

        // ─────────────────────────────────────────────────────────────
        // TAB 1 — RIR (Reps In Reserve)
        // Gerçek 1RM = Weight / (1 - (Reps + RIR) * 0.025)
        // ─────────────────────────────────────────────────────────────
        private void CalcRIR_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            try
            {
                double w    = Parse(RirWeight.Text);
                double reps = Parse(RirReps.Text);
                int    rir  = RirValue.SelectedIndex; // 0..4

                double totalReps = reps + rir;
                // Brzycki variant adapted for RIR
                double oneRM = w / (1.0278 - 0.0278 * totalReps);

                RirResultText.Text = isEn ? $"Estimated 1RM:  {oneRM:F1} kg" : $"Tahmini 1RM:  {oneRM:F1} kg";
                RirDetailText.Text = isEn
                    ? $"Current load: {w:F1} kg × {reps:F0} reps + {rir} RIR\n\n" +
                      $"Recommended training loads:\n" +
                      $"  • 95%  → {0.95 * oneRM,6:F1} kg   (1-2 reps)\n" +
                      $"  • 90%  → {0.90 * oneRM,6:F1} kg   (3 reps)\n" +
                      $"  • 85%  → {0.85 * oneRM,6:F1} kg   (4-5 reps)\n" +
                      $"  • 80%  → {0.80 * oneRM,6:F1} kg   (6-8 reps)\n" +
                      $"  • 75%  → {0.75 * oneRM,6:F1} kg   (8-10 reps)\n" +
                      $"  • 70%  → {0.70 * oneRM,6:F1} kg   (10-12 reps)\n" +
                      $"  • 65%  → {0.65 * oneRM,6:F1} kg   (12-15 reps)"
                    : $"Mevcut yükleme: {w:F1} kg × {reps:F0} tekrar + {rir} RIR\n\n" +
                      $"Önerilen antrenman yükleri:\n" +
                      $"  • %95  → {0.95 * oneRM,6:F1} kg   (1-2 tekrar)\n" +
                      $"  • %90  → {0.90 * oneRM,6:F1} kg   (3 tekrar)\n" +
                      $"  • %85  → {0.85 * oneRM,6:F1} kg   (4-5 tekrar)\n" +
                      $"  • %80  → {0.80 * oneRM,6:F1} kg   (6-8 tekrar)\n" +
                      $"  • %75  → {0.75 * oneRM,6:F1} kg   (8-10 tekrar)\n" +
                      $"  • %70  → {0.70 * oneRM,6:F1} kg   (10-12 tekrar)\n" +
                      $"  • %65  → {0.65 * oneRM,6:F1} kg   (12-15 tekrar)";

                RirResult.Visibility = Visibility.Visible;
            }
            catch { ShowError(isEn ? "Please enter valid values." : "Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 2 — Body Fat % (US Navy Method)
        // ─────────────────────────────────────────────────────────────
        private bool _bfIsMale = true;

        private void BfMale_Checked(object sender, RoutedEventArgs e)
        {
            _bfIsMale = true;
            if (BfFemale != null) BfFemale.IsChecked = false;
            if (HipLabel != null) HipLabel.Visibility = Visibility.Collapsed;
            if (BfHip   != null) BfHip.Visibility    = Visibility.Collapsed;
        }

        private void BfFemale_Checked(object sender, RoutedEventArgs e)
        {
            _bfIsMale = false;
            if (BfMale  != null) BfMale.IsChecked  = false;
            if (HipLabel != null) HipLabel.Visibility = Visibility.Visible;
            if (BfHip   != null) BfHip.Visibility    = Visibility.Visible;
        }

        private void CalcBodyFat_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            try
            {
                double height = Parse(BfHeight.Text);
                double neck   = Parse(BfNeck.Text);
                double waist  = Parse(BfWaist.Text);
                double bf;

                if (_bfIsMale)
                {
                    bf = 495.0 / (1.0324 - 0.19077 * Math.Log10(waist - neck)
                                         + 0.15456 * Math.Log10(height)) - 450;
                }
                else
                {
                    double hip = Parse(BfHip.Text);
                    bf = 495.0 / (1.29579 - 0.35004 * Math.Log10(waist + hip - neck)
                                          + 0.22100 * Math.Log10(height)) - 450;
                }

                string category;
                string ideal;
                if (_bfIsMale)
                {
                    category = isEn 
                        ? (bf < 6  ? "⚡ Essential Fat (elite)" : bf < 14 ? "🏆 Athlete" : bf < 18 ? "✅ Fit" : bf < 25 ? "🟡 Acceptable" : "🔴 Obese")
                        : (bf < 6  ? "⚡ Temel Yağ (elit)" : bf < 14 ? "🏆 Sporcu" : bf < 18 ? "✅ Fit" : bf < 25 ? "🟡 Kabul Edilebilir" : "🔴 Obez");
                    ideal = isEn ? "Ideal for men: 10–17%" : "Erkekler için ideal: %10–17";
                }
                else
                {
                    category = isEn 
                        ? (bf < 14 ? "⚡ Essential Fat (elite)" : bf < 21 ? "🏆 Athlete" : bf < 25 ? "✅ Fit" : bf < 32 ? "🟡 Acceptable" : "🔴 Obese")
                        : (bf < 14 ? "⚡ Temel Yağ (elit)" : bf < 21 ? "🏆 Sporcu" : bf < 25 ? "✅ Fit" : bf < 32 ? "🟡 Kabul Edilebilir" : "🔴 Obez");
                    ideal = isEn ? "Ideal for women: 18–24%" : "Kadınlar için ideal: %18–24";
                }

                BfResultText.Text  = isEn ? $"Body Fat %: {bf:F1}%" : $"Vücut Yağ Oranı: %{bf:F1}";
                BfCategoryText.Text = isEn 
                    ? $"{category}\n{ideal}\n\nLean body mass: approx. {100 - bf:F1}% (lean tissue)"
                    : $"{category}\n{ideal}\n\nYağsız vücut kütlesi: yakl. %{100 - bf:F1} (yağsız doku)";
                BfResult.Visibility = Visibility.Visible;
            }
            catch { ShowError(isEn ? "Please enter valid values." : "Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 3 — Kalori / BMR / TDEE (Mifflin-St Jeor)
        // ─────────────────────────────────────────────────────────────
        private void CalcBMR_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            try
            {
                bool   isMale = BmrMale.IsChecked == true;
                double w = Parse(BmrWeight.Text);
                double h = Parse(BmrHeight.Text);
                double a = Parse(BmrAge.Text);

                double bmr = isMale
                    ? 10 * w + 6.25 * h - 5 * a + 5
                    : 10 * w + 6.25 * h - 5 * a - 161;

                double[] factors = { 1.2, 1.375, 1.55, 1.725, 1.9 };
                double tdee = bmr * factors[BmrActivity.SelectedIndex];

                BmrResultText.Text = isEn ? $"TDEE: {tdee:F0} kcal / day" : $"TDEE: {tdee:F0} kcal / gün";
                BmrDetailText.Text = isEn 
                    ? $"BMR (basal metabolic rate): {bmr:F0} kcal\n\n" +
                      $"Daily calories by goal:\n" +
                      $"  🔻 Fat loss  →  {tdee - 500:F0} kcal  (−500)\n" +
                      $"  ⚖️ Maintain  →  {tdee:F0} kcal\n" +
                      $"  💪 Muscle gain →  {tdee + 300:F0} kcal  (+300)"
                    : $"BMR (bazal metabolizma): {bmr:F0} kcal\n\n" +
                      $"Hedefe göre günlük kalori:\n" +
                      $"  🔻 Yağ yakma  →  {tdee - 500:F0} kcal  (−500)\n" +
                      $"  ⚖️ İdame       →  {tdee:F0} kcal\n" +
                      $"  💪 Kas kazanma →  {tdee + 300:F0} kcal  (+300)";

                BmrResult.Visibility = Visibility.Visible;
            }
            catch { ShowError(isEn ? "Please enter valid values." : "Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 4 — Makro Besin Hesabı
        // Protein: 1.8–2.5 g/kg | Yağ: %25–35 | Karb: kalan
        // ─────────────────────────────────────────────────────────────
        private void CalcMacro_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            try
            {
                double totalCal = Parse(MacroCalories.Text);
                double weight   = Parse(MacroWeight.Text);

                // Protein gram per kg based on goal+activity
                double proteinPerKg;
                double fatRatio;

                int goal     = MacroGoal.SelectedIndex;     // 0=cut 1=maintain 2=bulk
                int activity = MacroActivity.SelectedIndex; // 0=low 1=mid 2=high

                // Protein g/kg
                if (goal == 0)       proteinPerKg = activity == 2 ? 2.4 : activity == 1 ? 2.2 : 2.0;
                else if (goal == 2)  proteinPerKg = activity == 2 ? 2.2 : activity == 1 ? 2.0 : 1.8;
                else                 proteinPerKg = activity == 2 ? 2.0 : activity == 1 ? 1.8 : 1.6;

                // Fat ratio of total calories
                fatRatio = goal == 0 ? 0.25 : goal == 2 ? 0.28 : 0.30;

                double proteinG = weight * proteinPerKg;
                double proteinCal = proteinG * 4.0;

                double fatCal = totalCal * fatRatio;
                double fatG   = fatCal / 9.0;

                double carbCal = totalCal - proteinCal - fatCal;
                double carbG   = Math.Max(0, carbCal / 4.0);

                string goalLabel = isEn
                    ? (goal == 0 ? "Fat Loss" : goal == 2 ? "Muscle Gain" : "Maintain")
                    : (goal == 0 ? "Yağ Yakma" : goal == 2 ? "Kas Kazanma" : "İdame");

                MacroResultText.Text = $"{goalLabel} — {totalCal:F0} kcal";

                MacroDetailText.Text = isEn 
                    ? $"🥩 Protein:       {proteinG,6:F0} g   ({proteinCal:F0} kcal)  — {proteinCal / totalCal * 100:F0}%\n" +
                      $"🥑 Fat:           {fatG,6:F0} g   ({fatCal:F0} kcal)  — {fatRatio * 100:F0}%\n" +
                      $"🍞 Carbs:         {carbG,6:F0} g   ({carbCal:F0} kcal)  — {carbCal / totalCal * 100:F0}%\n\n" +
                      $"Protein pref.: {proteinPerKg:F1} g/kg body weight\n" +
                      $"Note: Values are estimates and vary individually."
                    : $"🥩 Protein:       {proteinG,6:F0} g   ({proteinCal:F0} kcal)  — %{proteinCal / totalCal * 100:F0}\n" +
                      $"🥑 Yağ:           {fatG,6:F0} g   ({fatCal:F0} kcal)  — %{fatRatio * 100:F0}\n" +
                      $"🍞 Karbonhidrat:  {carbG,6:F0} g   ({carbCal:F0} kcal)  — %{carbCal / totalCal * 100:F0}\n\n" +
                      $"Protein tercihi: {proteinPerKg:F1} g/kg vücut ağırlığı\n" +
                      $"Not: Değerler tahmini olup bireysel faktörlere göre değişir.";

                MacroResult.Visibility = Visibility.Visible;
            }
            catch { ShowError(isEn ? "Please enter valid values." : "Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 5 — VKİ / BMI
        // ─────────────────────────────────────────────────────────────
        private void CalcBMI_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            try
            {
                double kg = Parse(BmiWeight.Text);
                double cm = Parse(BmiHeight.Text);
                double m  = cm / 100.0;
                double bmi = kg / (m * m);

                string category;
                string detail;
                string indicator;

                if (bmi < 18.5)
                {
                    category  = isEn ? "🔵 Underweight" : "🔵 Zayıf";
                    detail    = isEn ? "BMI under 18.5 — low weight risk" : "VKİ 18.5'in altında — düşük kilo riski";
                    indicator = isEn ? "► Left zone of the scale" : "► Ölçeğin sol bölgesinde";
                }
                else if (bmi < 25.0)
                {
                    category  = isEn ? "🟢 Normal Weight" : "🟢 Normal Kilolu";
                    detail    = isEn ? "BMI 18.5–24.9 — healthy range" : "VKİ 18.5–24.9 arası — sağlıklı aralık";
                    indicator = isEn ? "► Normal zone of the scale ✅" : "► Ölçeğin normal bölgesinde ✅";
                }
                else if (bmi < 30.0)
                {
                    category  = isEn ? "🟡 Overweight" : "🟡 Fazla Kilolu";
                    detail    = isEn ? "BMI 25–29.9 — mild health risk" : "VKİ 25–29.9 arası — hafif sağlık riski";
                    indicator = isEn ? "► Third zone of the scale" : "► Ölçeğin üçüncü bölgesinde";
                }
                else if (bmi < 35.0)
                {
                    category  = isEn ? "🟠 Obese (Class I)" : "🟠 Obez (Sınıf I)";
                    detail    = isEn ? "BMI 30–34.9 — moderate health risk" : "VKİ 30–34.9 arası — orta sağlık riski";
                    indicator = isEn ? "► Fourth zone of the scale" : "► Ölçeğin dördüncü bölgesinde";
                }
                else
                {
                    category  = isEn ? "🔴 Obese (Class II+)" : "🔴 Obez (Sınıf II+)";
                    detail    = isEn ? "BMI 35 and above — high health risk" : "VKİ 35 ve üzeri — yüksek sağlık riski";
                    indicator = isEn ? "► Right zone of the scale" : "► Ölçeğin sağ bölgesinde";
                }

                // Normal kilo aralığı
                double minW = 18.5 * m * m;
                double maxW = 24.9 * m * m;

                BmiResultText.Text    = isEn ? $"BMI: {bmi:F1}" : $"VKİ: {bmi:F1}";
                BmiCategoryText.Text  = isEn ? $"{category}\n{detail}\n\nNormal weight for your height: {minW:F1}–{maxW:F1} kg" : $"{category}\n{detail}\n\nBoyunuza göre normal kilo: {minW:F1}–{maxW:F1} kg";
                BmiIndicatorText.Text = indicator;

                BmiResult.Visibility = Visibility.Visible;
            }
            catch { ShowError(isEn ? "Please enter valid values." : "Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 6 — Kilo Hedefi Ulaşılabilirlik
        // Sağlıklı yağ kaybı: max ~0.5–1 kg/hafta (500–1000 kcal açık/gün)
        // ─────────────────────────────────────────────────────────────
        private void CalcWeightGoal_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            try
            {
                double current = Parse(WgCurrentWeight.Text);
                double target  = Parse(WgTargetWeight.Text);
                double days    = Parse(WgDays.Text);

                if (days <= 0) { ShowError(isEn ? "Duration must be greater than 0." : "Süre 0'dan büyük olmalı."); return; }

                double diff     = current - target;
                bool   gaining  = diff < 0;
                diff = Math.Abs(diff);

                double weeks       = days / 7.0;
                double kgPerWeek   = diff / weeks;
                // 1 kg yağ ≈ 7700 kcal
                double kcalPerDay  = (diff * 7700) / days;

                // TDEE factors
                double[] factors = { 1.2, 1.375, 1.55, 1.725 };
                // We don't have weight/height here; use kcalPerDay as the açık relative to TDEE estimate
                // (açık yorumu yeterli)

                string emoji, feasibility, advice;

                if (gaining)
                {
                    // Kilo alma hedefi
                    if (kgPerWeek <= 0.25)
                    {
                        emoji = "✅"; 
                        feasibility = isEn ? "Realistic — Clean Bulk" : "Gerçekçi — Temiz Bulk";
                        advice = isEn ? "Gaining less than 0.25 kg per week is the healthiest way to add muscle mass while minimizing fat gain." : "Haftada 0.25 kg'dan az kilo almak, yağlanmayı minimize ederek kas kütlesi eklemenin en sağlıklı yoludur.";
                    }
                    else if (kgPerWeek <= 0.5)
                    {
                        emoji = "⚠️"; 
                        feasibility = isEn ? "Acceptable — Fast Bulk" : "Kabul Edilebilir — Hızlı Bulk";
                        advice = isEn ? $"Gaining ~{kgPerWeek:F2} kg per week is manageable, but some fat gain is inevitable. Daily calorie surplus: ~{kcalPerDay:F0} kcal." : $"Haftada ~{kgPerWeek:F2} kg kilo alma hızı yönetilebilir, ancak bir kısım yağlanma kaçınılmaz olabilir. Günlük kalori fazlası: ~{kcalPerDay:F0} kcal.";
                    }
                    else
                    {
                        emoji = "🔴"; 
                        feasibility = isEn ? "Unhealthy — Excessive Gaining" : "Sağlıksız — Aşırı Kilo Alımı";
                        advice = isEn ? $"Gaining {kgPerWeek:F1} kg per week means mostly fat accumulation. We recommend extending the duration to {(diff / 0.25):F0} days (≤0.25 kg/week)." : $"Haftada {kgPerWeek:F1} kg kilo almak büyük ölçüde yağ birikimi anlamına gelir. Süreyi {(diff / 0.25):F0} güne (≤0.25 kg/hafta) uzatmanızı öneririz.";
                    }
                }
                else
                {
                    // Kilo verme hedefi
                    if (kgPerWeek <= 0.5)
                    {
                        emoji = "✅"; 
                        feasibility = isEn ? "Excellent — Sustainable" : "Mükemmel — Sürdürülebilir";
                        advice = isEn ? "This rate is ideal for healthy fat loss while preserving muscle mass. Be patient, the results will last!" : "Bu hız, kas kütlesini koruyarak sağlıklı yağ yakımı için idealdir. Sabırlı olun, sonuçlar kalıcı olacak!";
                    }
                    else if (kgPerWeek <= 1.0)
                    {
                        emoji = "⚠️"; 
                        feasibility = isEn ? "Acceptable — Aggressive Diet" : "Kabul Edilebilir — Agresif Diyet";
                        advice = isEn ? $"Losing {kgPerWeek:F2} kg per week is possible, but risks muscle loss. High protein (≥2g/kg) is essential. Daily deficit: ~{kcalPerDay:F0} kcal." : $"Haftada {kgPerWeek:F2} kg kaybetmek mümkün, ancak kas kaybı riski var. Yüksek protein alımı (≥2g/kg) şart. Günlük açık: ~{kcalPerDay:F0} kcal.";
                    }
                    else if (kgPerWeek <= 1.5)
                    {
                        emoji = "🟠"; 
                        feasibility = isEn ? "Risky — Very Aggressive" : "Riskli — Çok Agresif";
                        advice = isEn ? $"Losing {kgPerWeek:F1} kg per week is a health risk. Severe muscle loss, fatigue, and metabolic slowdown may occur. Recommended duration: at least {(diff / 0.75 * 7):F0} days." : $"Haftada {kgPerWeek:F1} kg kayıp sağlık açısından risklidir. Ciddi kas kaybı, yorgunluk ve metabolik yavaşlama yaşanabilir. Önerilen süre: en az {(diff / 0.75 * 7):F0} gün.";
                    }
                    else
                    {
                        emoji = "🔴"; 
                        feasibility = isEn ? "Impossible / Dangerous" : "İmkânsız / Tehlikeli";
                        double safeDays = (diff / 0.5) * 7;
                        advice = isEn ? $"Losing {kgPerWeek:F1} kg per week through healthy means is impossible. Daily calorie deficit would be ~{kcalPerDay:F0} kcal, which exceeds most people's BMR. Realistic duration: at least {safeDays:F0} days." : $"Haftada {kgPerWeek:F1} kg kilo vermek sağlıklı yollarla imkânsızdır. Günlük kalori açığı ~{kcalPerDay:F0} kcal olur ki bu çoğu insanın bazal metabolizmasını aşar. Gerçekçi süre: en az {safeDays:F0} gün.";
                    }
                }

                WgResultText.Text = $"{emoji} {feasibility}";
                WgDetailText.Text = isEn 
                    ? $"Goal: {current:F1} kg → {target:F1} kg  ({(gaining ? "+" : "-")}{diff:F1} kg, in {days:F0} days)\n" +
                      $"Avg weekly change: {(gaining?"+":" -")}{kgPerWeek:F2} kg/week\n" +
                      $"Req. daily calorie {(gaining ? "surplus" : "deficit")}: ~{kcalPerDay:F0} kcal\n\n" +
                      $"{advice}"
                    : $"Hedef: {current:F1} kg → {target:F1} kg  ({(gaining ? "+" : "-")}{diff:F1} kg, {days:F0} günde)\n" +
                      $"Haftada ortalama değişim: {(gaining?"+":" -")}{kgPerWeek:F2} kg/hafta\n" +
                      $"Gereken günlük kalori {(gaining ? "fazlası" : "açığı")}: ~{kcalPerDay:F0} kcal\n\n" +
                      $"{advice}";

                WgResult.Visibility = Visibility.Visible;
                DrawWeightFigure(WgCurrentCanvas, current);
                DrawWeightFigure(WgTargetCanvas, target);
            }
            catch { ShowError(isEn ? "Please enter valid values." : "Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 7 — V-Vücut (V-Taper) Oranı
        // SHR (Shoulder-to-Waist Ratio): ideal erkek ≥1.618, kadın ≥1.4
        // İdeal bel  = boy × 0.44 (erkek), × 0.42 (kadın)
        // İdeal omuz = ideal bel × 1.618 (erkek) / 1.4 (kadın)
        // ─────────────────────────────────────────────────────────────
        private void CalcVTaper_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                bool   isMale   = VtMale.IsChecked == true;
                double height   = Parse(VtHeight.Text);
                double weight   = Parse(VtWeight.Text);
                double shoulder = Parse(VtShoulder.Text);
                double waist    = Parse(VtWaist.Text);

                double shr = shoulder / waist;  // Shoulder-to-waist ratio
                double idealSHR  = isMale ? 1.618 : 1.40;

                // İdeal ölçüler (boy bazlı ideal bel, ona göre ideal omuz)
                double idealWaist    = isMale ? height * 0.44 : height * 0.42;
                double idealShoulder = idealWaist * idealSHR;

                double shoulderDiff = idealShoulder - shoulder;
                double waistDiff    = waist - idealWaist;

                string shrRating;
                string emoji;
                if (shr >= idealSHR + 0.1)
                { emoji = "🏆"; shrRating = "Mükemmel V-Vücut"; }
                else if (shr >= idealSHR)
                { emoji = "✅"; shrRating = "İdeal V-Vücut"; }
                else if (shr >= idealSHR - 0.1)
                { emoji = "⚠️"; shrRating = "İdeal Sınırında"; }
                else if (shr >= idealSHR - 0.2)
                { emoji = "🟡"; shrRating = "Gelişim Gerekiyor"; }
                else
                { emoji = "🔴"; shrRating = "V-Vücut Uzak"; }

                string shoulderAdvice = shoulderDiff > 0
                    ? $"Omzunuzu ~{shoulderDiff:F1} cm genişletmelisiniz (ağırlık antrenmanı)."
                    : $"Omuz genişliğiniz mükemmel!";

                string waistAdvice = waistDiff > 0
                    ? $"Belinizi ~{waistDiff:F1} cm inceltmelisiniz (diyet + yağ yakımı)."
                    : $"Bel ölçünüz zaten ideal sınırın içinde — tebrikler!";

                VtResultText.Text = $"{emoji} {shrRating}  (SHR: {shr:F2})";
                VtDetailText.Text =
                    $"Mevcut omuz/bel oranı (SHR): {shr:F2}  (ideal: ≥{idealSHR:F2})\n\n" +
                    $"📐 Boy bazlı ideal ölçüler ({height:F0} cm için):\n" +
                    $"   • İdeal omuz genişliği: {idealShoulder:F1} cm  (mevcut: {shoulder:F1} cm)\n" +
                    $"   • İdeal bel çevresi:    {idealWaist:F1} cm  (mevcut: {waist:F1} cm)\n\n" +
                    $"💡 {shoulderAdvice}\n" +
                    $"💡 {waistAdvice}";

                VtResult.Visibility = Visibility.Visible;
                DrawVTaperFigure(VtCurrentCanvas, shoulder, waist);
                DrawVTaperFigure(VtIdealCanvas, idealShoulder, idealWaist);
            }
            catch { ShowError("Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 8 — Su İhtiyacı
        // Temel: Erkek=35 ml/kg, Kadın=31 ml/kg
        // Yaş >55 → -5%, iklim +%10 veya +%20, aktivite +0..+800 ml
        // ─────────────────────────────────────────────────────────────
        private bool _waIsMale = true;

        private void WaMale_Checked(object sender, RoutedEventArgs e)
        {
            _waIsMale = true;
            if (WaFemale != null) WaFemale.IsChecked = false;
        }

        private void WaFemale_Checked(object sender, RoutedEventArgs e)
        {
            _waIsMale = false;
            if (WaMale != null) WaMale.IsChecked = false;
        }

        private void CalcWater_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            try
            {
                double weight = Parse(WaWeight.Text);
                double age    = Parse(WaAge.Text);

                // Base: erkek 35 ml/kg, kadın 31 ml/kg
                double basePerKg = _waIsMale ? 35.0 : 31.0;
                double baseWater = weight * basePerKg;

                // Yaş faktörü
                if (age > 55) baseWater *= 0.95;   // yaşlılar daha az hisseder
                if (age < 18) baseWater *= 1.05;   // gençler biraz daha fazla

                // Aktivite eklemesi (ml)
                double[] activityAdd = { 0, 300, 500, 700, 900 };
                double actAdd = activityAdd[WaActivity.SelectedIndex];

                // İklim eklemesi (%)
                double[] climateBonus = { 0, 0.10, 0.20 };
                double climateMult = 1 + climateBonus[WaClimate.SelectedIndex];

                double totalMl = (baseWater + actAdd) * climateMult;
                double totalL  = totalMl / 1000.0;
                // Bardak sayısı (1 bardak = 250 ml)
                int glasses = (int)Math.Ceiling(totalMl / 250.0);

                string rating;
                if (totalL < 1.5)       rating = isEn ? "⚠️ Low" : "⚠️ Düşük";
                else if (totalL < 2.5)  rating = isEn ? "✅ Normal" : "✅ Normal";
                else if (totalL < 3.5)  rating = isEn ? "💪 High (active)" : "💪 Yüksek (aktif)";
                else                    rating = isEn ? "🏊 Athlete level" : "🏊 Sporcu düzeyi";

                string climateNote = WaClimate.SelectedIndex == 0 ? (isEn ? "temperate" : "ılıman") :
                                     WaClimate.SelectedIndex == 1 ? (isEn ? "hot" : "sıcak") : (isEn ? "very hot/humid" : "çok sıcak/nemli");

                WaResultText.Text = isEn ? $"💧 {totalL:F1} liters / day  ({rating})" : $"💧 {totalL:F1} litre / gün  ({rating})";
                WaDetailText.Text = isEn
                    ? $"Base water need ({(weight):F0} kg × {basePerKg} ml): {baseWater:F0} ml\n" +
                      $"Activity addition: +{actAdd:F0} ml\n" +
                      $"Climate factor ({climateNote}): ×{climateMult:F2}\n\n" +
                      $"📊 Total: {totalMl:F0} ml  ≈  {totalL:F1} liters  ≈  {glasses} glasses\n\n" +
                      $"⏰ Suggested hourly breakdown (16 hours awake):\n" +
                      $"   Morning 8-12:   {totalMl * 0.35 / 250:F1} glasses\n" +
                      $"   Noon    12-17:  {totalMl * 0.40 / 250:F1} glasses\n" +
                      $"   Evening 17-22:  {totalMl * 0.25 / 250:F1} glasses\n\n" +
                      $"💡 Coffee, tea, juice etc. also count as fluid intake.\n" +
                      $"   If urine is light yellow, you are drinking enough."
                    : $"Temel su ihtiyacı ({(weight):F0} kg × {basePerKg} ml): {baseWater:F0} ml\n" +
                      $"Aktivite eklemesi: +{actAdd:F0} ml\n" +
                      $"İklim faktörü ({climateNote}): ×{climateMult:F2}\n\n" +
                      $"📊 Toplam: {totalMl:F0} ml  ≈  {totalL:F1} litre  ≈  {glasses} bardak\n\n" +
                      $"⏰ Saatlik bölüştürme önerisi (16 saat uyanık):\n" +
                      $"   Sabah 8-12 arası:   {totalMl * 0.35 / 250:F1} bardak\n" +
                      $"   Öğle  12-17 arası:  {totalMl * 0.40 / 250:F1} bardak\n" +
                      $"   Akşam 17-22 arası:  {totalMl * 0.25 / 250:F1} bardak\n\n" +
                      $"💡 Kahve, çay, meyve suyu vb. de günlük sıvı sayılır.\n" +
                      $"   İdrar rengi açık sarı ise yeterince su içiyorsunuz.";

                WaResult.Visibility = Visibility.Visible;
            }
            catch { ShowError(isEn ? "Please enter valid values." : "Lütfen geçerli değerler girin."); }
        }

        // ─────────────────────────────────────────────────────────────
        // TAB 9 — Supplement İhtiyaç Testi
        // ─────────────────────────────────────────────────────────────
        private void CalcSupplement_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            double weight = SupWeight.Value;
            bool isMinimal = SupAnalysisMinimal.IsChecked == true;
            bool wt = SupWTYes.IsChecked == true;
            double protein = SupProtein.Value;
            double meal = SupMeal.Value;
            double digestion = SupDigestion.Value;
            double veggie = SupVeggie.Value;
            double satiety = SupSatiety.Value;
            double sleep = SupSleep.Value;
            double trainE = SupTrainEnergy.Value;
            bool chkWinter = SupChkWinter.IsChecked == true;
            bool chkSweat = SupChkSweat.IsChecked == true;
            bool chkNoVeg = SupChkNoVeggie.IsChecked == true;
            bool chkLowE = SupChkLowEnergy.IsChecked == true;
            bool chkPoorSl = SupChkPoorSleep.IsChecked == true;
            bool chkCantSl = SupChkCantSleep.IsChecked == true;
            bool chkStress = SupChkStress.IsChecked == true;
            bool chkTrainOk = SupChkTrainOk.IsChecked == true;
            bool chkWeakP = SupChkWeakPump.IsChecked == true;
            bool chkFaint = SupChkTrainFaint.IsChecked == true;

            int threshold = isMinimal ? 45 : 25;

            var results = new System.Collections.Generic.List<(string name, int score, string dose, string timing, string note)>();

            // 1. Kreatin
            int s = 0;
            if (wt) { s += 30; if (trainE <= 4) s += 25; if (chkWeakP) s += 25; if (!chkTrainOk) s += 10; }
            if (s >= threshold)
                results.Add((isEn ? "Creatine Monohydrate" : "Kreatin Monohidrat", s,
                    "3–5 g/gün",
                    isEn ? "Every day at a fixed time, with or after a meal" : "Her gün sabit saatte, yemekle veya sonrasında",
                    isEn ? "The most well-studied supplement. Increases strength 5–15%, supports lean mass. No loading needed." : "En iyi araştırılmış takviye. Güç %5–15 artışı, yağsız kütle desteği. Yükleme gerekmez."));

            // 2. Whey Protein
            s = 0;
            if (protein <= 3) s += 40; else if (protein <= 5) s += 25; else if (protein <= 7) s += 10;
            if (wt) s += 20; if (satiety <= 4) s += 10;
            if (s >= threshold)
            {
                double targetP = wt ? weight * 2.0 : weight * 1.6;
                results.Add((isEn ? "Whey Protein" : "Protein Tozu (Whey)", s,
                    isEn ? $"Target: ~{targetP:F0} g/day total protein ({(wt ? "2.0" : "1.6")} g/kg)" : $"Hedef: günlük toplam ~{targetP:F0} g protein ({(wt ? "2.0" : "1.6")} g/kg)",
                    isEn ? "Post-workout within 30–60 min, or to complete daily protein" : "Antrenman sonrası 30–60 dk içinde veya günlük proteini tamamlamak için",
                    isEn ? "Whey isolate is fastest absorbing. Not superior to real food — use to fill gaps." : "Whey isolate en hızlı emilen. Gerçek yemekten üstün değil — açığı kapatmak için kullanın."));
            }

            // 3. D3 + K2
            s = 0;
            if (chkWinter) s += 40; if (chkLowE) s += 15; if (sleep <= 4) s += 15; if (chkPoorSl) s += 10;
            if (s >= threshold)
                results.Add((isEn ? "Vitamin D3 + K2" : "D3 Vitamini + K2", s,
                    "2000–4000 IU D3 + 100–200 mcg K2/gün",
                    isEn ? "With a fatty meal (fat-soluble)" : "Yağlı yemekle birlikte (yağda çözünür)",
                    isEn ? "50% of athletes are deficient. Supports muscle strength, hormonal balance, and immunity. Essential Oct–Apr." : "Sporcuların %50'si yetersiz. Kas gücü, hormon dengesi, bağışıklık. Ekim–Nisan arası şart."));

            // 4. Magnezyum
            s = 0;
            if (chkCantSl) s += 30; if (chkStress) s += 25; if (sleep <= 4) s += 20; if (chkPoorSl) s += 15;
            if (s >= threshold)
                results.Add((isEn ? "Magnesium Bisglycinate" : "Magnezyum Bisglisinat", s,
                    "200–400 mg/gün",
                    isEn ? "Glycinate form at bedtime for relaxation and deep sleep" : "Glinat formu gece yatmadan önce, gevşeme ve derin uyku için",
                    isEn ? "60% of athletes are deficient. Reduces cramps, improves sleep quality, insulin sensitivity." : "Sporcuların %60'ında eksik. Kas krampı azaltır, uyku kalitesi ve insülin duyarlılığını artırır."));

            // 5. Omega-3
            s = 0;
            if (digestion <= 4) s += 20; if (veggie <= 4) s += 15; if (chkLowE) s += 10; if (wt) s += 15;
            if (s >= threshold)
                results.Add((isEn ? "Omega-3 (EPA/DHA)" : "Omega-3 (EPA/DHA)", s,
                    "2–3 g/gün (≥1 g EPA)",
                    isEn ? "With meals, consistently every day" : "Yemeklerle birlikte, her gün düzenli",
                    isEn ? "Increases muscle protein synthesis, reduces post-exercise inflammation. Prefer fish oil." : "Kas protein sentezini artırır, egzersiz sonrası inflamasyonu azaltır. Balık yağı tercih edin."));

            // 6. Multivitamin
            s = 0;
            if (veggie <= 3) s += 35; if (chkNoVeg) s += 25; if (meal <= 3) s += 15; if (protein <= 3) s += 10;
            if (s >= threshold)
                results.Add((isEn ? "Multivitamin / Multimineral" : "Multivitamin / Multimineral", s,
                    isEn ? "1 tablet/day (per label directions)" : "1 tablet/gün (etiket yönergesine göre)",
                    isEn ? "With breakfast or lunch" : "Kahvaltı veya öğle yemeğiyle",
                    isEn ? "Fills micronutrient gaps from poor diet variety. Not a substitute for real food." : "Yetersiz beslenme çeşitliliğinden kaynaklanan mikro besin açıklarını kapatır. Gerçek yemeğin yerini tutmaz."));

            // 7. Kafein
            s = 0;
            if (wt) { if (trainE <= 3) s += 35; if (chkFaint) s += 25; if (chkLowE) s += 15; }
            if (s >= threshold)
                results.Add((isEn ? "Caffeine (Pre-workout)" : "Kafein (Pre-workout)", s,
                    $"3–6 mg/kg → {weight * 3:F0}–{weight * 6:F0} mg",
                    isEn ? "45–60 min before training. Avoid after 2 PM for sleep." : "Antrenmandan 45–60 dk önce. Uyku için öğleden sonra 14:00'den sonra kaçının.",
                    isEn ? "Improves endurance 2–4%, strength and focus. Take 1–2 days off/week for tolerance." : "Dayanıklılık %2–4 artışı, güç ve odak artışı. Tolerans için haftada 1–2 gün ara verin."));

            // 8. Probiyotik
            s = 0;
            if (digestion <= 3) s += 40; else if (digestion <= 5) s += 20; if (meal <= 3) s += 10;
            if (s >= threshold)
                results.Add((isEn ? "Probiotic" : "Probiyotik", s,
                    isEn ? "10–50 billion CFU/day, multi-strain" : "10–50 milyar CFU/gün, çoklu suş",
                    isEn ? "On empty stomach in the morning or before bed" : "Sabah aç karnına veya yatmadan önce",
                    isEn ? "Restores gut flora balance. Look for Lactobacillus + Bifidobacterium strains." : "Bağırsak florası dengesini restore eder. Lactobacillus + Bifidobacterium suşları arayın."));

            // 9. Elektrolit
            s = 0;
            if (chkSweat) s += 40; if (wt && trainE <= 4) s += 15; if (chkFaint) s += 20;
            if (s >= threshold)
                results.Add((isEn ? "Electrolyte" : "Elektrolit", s,
                    isEn ? "Sodium 500–1500 mg, Potassium 200–400 mg per session" : "Sodyum 500–1500 mg, Potasyum 200–400 mg (seans başı)",
                    isEn ? "During and after training, especially in hot weather" : "Antrenman sırasında ve sonrasında, özellikle sıcak havada",
                    isEn ? "Prevents dehydration and performance loss from excessive sweating." : "Aşırı terlemeye bağlı sıvı kaybı ve performans düşüşünü önler."));

            // 10. Çinko + C Vitamini
            s = 0;
            if (chkLowE) s += 20; if (veggie <= 4) s += 20; if (chkSweat) s += 15; if (sleep <= 4) s += 10;
            if (s >= threshold)
                results.Add(("Zinc + Vitamin C" , s,
                    isEn ? "Zinc 15–30 mg + Vitamin C 500–1000 mg/day" : "Çinko 15–30 mg + C Vitamini 500–1000 mg/gün",
                    isEn ? "With a meal (zinc can cause nausea on empty stomach)" : "Yemekle birlikte (çinko aç karna mide bulantısı yapabilir)",
                    isEn ? "Supports immune function, testosterone production, and antioxidant defense." : "Bağışıklık, testosteron üretimi ve antioksidan savunmayı destekler."));

            // 11. Beta-Alanin (kapsamlı)
            if (!isMinimal)
            {
                s = 0;
                if (wt) { if (trainE <= 4) s += 25; if (!chkTrainOk) s += 15; s += 10; }
                if (s >= threshold)
                    results.Add((isEn ? "Beta-Alanine" : "Beta-Alanin", s,
                        "3.2–6.4 g/gün",
                        isEn ? "Split into small doses (0.8–1g × 4–6) to minimize tingling" : "Küçük dozlara bölün (0.8–1g × 4–6) karıncalanmayı minimize edin",
                        isEn ? "Buffers pH in 60–240s exercises. Tingling is normal and harmless. Min 4 weeks for effect." : "60–240 sn egzersizde pH tamponlar. Karıncalanma normaldir, zararsızdır. Etki için min 4 hafta."));
            }

            // 12. Lif Takviyesi
            s = 0;
            if (digestion <= 4) s += 25; if (satiety <= 4) s += 25; if (veggie <= 4) s += 15;
            if (s >= threshold)
                results.Add((isEn ? "Fiber Supplement (Psyllium)" : "Lif Takviyesi (Psyllium Husk)", s,
                    "5–10 g/gün",
                    isEn ? "With plenty of water, between meals" : "Bol su ile, öğünler arasında",
                    isEn ? "Improves digestion, increases satiety, regulates blood sugar." : "Sindirimi düzenler, tokluk hissini artırır, kan şekerini dengeler."));

            // 13. Melatonin (kapsamlı)
            if (!isMinimal)
            {
                s = 0;
                if (chkCantSl) s += 40; if (sleep <= 3) s += 20; if (chkPoorSl) s += 15;
                if (s >= threshold)
                    results.Add((isEn ? "Melatonin" : "Melatonin", s,
                        "0.5–3 mg/gün",
                        isEn ? "30–60 min before bedtime, in a dark room" : "Yatmadan 30–60 dk önce, karanlık odada",
                        isEn ? "Start with the lowest dose (0.5 mg). For short-term use to reset sleep cycle." : "En düşük dozla başlayın (0.5 mg). Uyku döngüsünü sıfırlamak için kısa süreli kullanım."));
            }

            // Sonuç sırala (yüksek skor önce)
            results.Sort((a, b) => b.score.CompareTo(a.score));

            // UI oluştur
            SupResultPanel.Children.Clear();

            var accent = (System.Windows.Media.Brush)TryFindResource("AccentBrush") ?? System.Windows.Media.Brushes.DodgerBlue;
            var accentLt = (System.Windows.Media.Brush)TryFindResource("AccentLightBrush") ?? System.Windows.Media.Brushes.AliceBlue;
            var primary = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush") ?? System.Windows.Media.Brushes.Black;
            var secondary = (System.Windows.Media.Brush)TryFindResource("SecondaryTextBrush") ?? System.Windows.Media.Brushes.Gray;
            var bgWin = (System.Windows.Media.Brush)TryFindResource("WindowBackgroundBrush") ?? System.Windows.Media.Brushes.White;

            // Başlık
            var titleTb = new System.Windows.Controls.TextBlock
            {
                Text = isEn ? "🏆 Recommended Supplements For You" : "🏆 Sizin İçin Önerilen Takviyeler",
                FontSize = 16, FontWeight = FontWeights.Bold, Foreground = accent, Margin = new Thickness(0, 0, 0, 4)
            };
            SupResultPanel.Children.Add(titleTb);

            var analysisTb = new System.Windows.Controls.TextBlock
            {
                Text = isEn
                    ? $"Analysis: {(isMinimal ? "Minimal" : "Comprehensive")} | Weight: {weight:F0} kg | Training: {(wt ? "Yes" : "No")}"
                    : $"Analiz: {(isMinimal ? "Minimal" : "Kapsamlı")} | Kilo: {weight:F0} kg | Ağırlık: {(wt ? "Evet" : "Hayır")}",
                FontSize = 11, Foreground = secondary, Margin = new Thickness(0, 0, 0, 12)
            };
            SupResultPanel.Children.Add(analysisTb);

            if (results.Count == 0)
            {
                var noRes = new System.Windows.Controls.TextBlock
                {
                    Text = isEn ? "Congratulations! No additional supplementation needed based on your inputs." : "Tebrikler! Mevcut durumunuza göre ek takviyeye ihtiyaç duyulmamaktadır.",
                    FontSize = 13, Foreground = primary, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0)
                };
                SupResultPanel.Children.Add(noRes);
            }
            else
            {
                int idx = 1;
                foreach (var r in results)
                {
                    string priorityText; string priorityColor;
                    if (r.score >= 60) { priorityText = isEn ? "HIGH" : "YÜKSEK"; priorityColor = "#E74C3C"; }
                    else if (r.score >= 40) { priorityText = isEn ? "MEDIUM" : "ORTA"; priorityColor = "#F39C12"; }
                    else { priorityText = isEn ? "LOW" : "DÜŞÜK"; priorityColor = "#27AE60"; }

                    var card = new System.Windows.Controls.Border
                    {
                        Background = bgWin, CornerRadius = new CornerRadius(10),
                        Padding = new Thickness(14, 12, 14, 12), Margin = new Thickness(0, 0, 0, 8),
                        BorderBrush = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(priorityColor)),
                        BorderThickness = new Thickness(0, 0, 3, 0)
                    };
                    var cardPanel = new System.Windows.Controls.StackPanel();

                    // Header: #number + name + priority badge
                    var headerDock = new System.Windows.Controls.DockPanel();
                    var badge = new System.Windows.Controls.Border
                    {
                        Background = new System.Windows.Media.SolidColorBrush(
                            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(priorityColor)),
                        CornerRadius = new CornerRadius(4), Padding = new Thickness(8, 2, 8, 2),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    badge.Child = new System.Windows.Controls.TextBlock
                    {
                        Text = priorityText, FontSize = 9, FontWeight = FontWeights.Bold,
                        Foreground = System.Windows.Media.Brushes.White
                    };
                    System.Windows.Controls.DockPanel.SetDock(badge, System.Windows.Controls.Dock.Right);
                    headerDock.Children.Add(badge);
                    headerDock.Children.Add(new System.Windows.Controls.TextBlock
                    {
                        Text = $"{idx}. {r.name}", FontSize = 14, FontWeight = FontWeights.Bold,
                        Foreground = primary, VerticalAlignment = VerticalAlignment.Center
                    });
                    cardPanel.Children.Add(headerDock);

                    // Doz
                    cardPanel.Children.Add(new System.Windows.Controls.TextBlock
                    {
                        Text = $"📏 {(isEn ? "Dose" : "Doz")}: {r.dose}",
                        FontSize = 11, Foreground = primary, Margin = new Thickness(0, 6, 0, 0), TextWrapping = TextWrapping.Wrap
                    });
                    // Zamanlama
                    cardPanel.Children.Add(new System.Windows.Controls.TextBlock
                    {
                        Text = $"⏰ {(isEn ? "Timing" : "Zamanlama")}: {r.timing}",
                        FontSize = 11, Foreground = primary, Margin = new Thickness(0, 3, 0, 0), TextWrapping = TextWrapping.Wrap
                    });
                    // Not
                    cardPanel.Children.Add(new System.Windows.Controls.TextBlock
                    {
                        Text = $"💡 {r.note}", FontSize = 10, FontStyle = FontStyles.Italic,
                        Foreground = secondary, Margin = new Thickness(0, 5, 0, 0), TextWrapping = TextWrapping.Wrap, LineHeight = 16
                    });

                    card.Child = cardPanel;
                    SupResultPanel.Children.Add(card);
                    idx++;
                }
            }

            // Testi Sıfırla butonu
            var resetBtn = new System.Windows.Controls.Button
            {
                Content = isEn ? "Reset Test 🔄" : "Testi Sıfırla 🔄",
                HorizontalAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(0, 8, 0, 0), Padding = new Thickness(16, 8, 16, 8),
                FontSize = 12, Cursor = System.Windows.Input.Cursors.Hand,
                Background = System.Windows.Media.Brushes.Transparent,
                Foreground = accent, BorderThickness = new Thickness(1),
                BorderBrush = accent
            };
            resetBtn.Click += (ss, ee) => ResetSupplementTest();
            SupResultPanel.Children.Add(resetBtn);

            SupResult.Visibility = Visibility.Visible;
        }

        private void ResetSupplementTest()
        {
            SupWeight.Value = 80;
            SupAnalysisMinimal.IsChecked = true;
            SupWTYes.IsChecked = true;
            SupProtein.Value = 5; SupMeal.Value = 5; SupDigestion.Value = 5;
            SupVeggie.Value = 5; SupSatiety.Value = 5; SupSleep.Value = 5;
            SupTrainEnergy.Value = 5;
            SupChkWinter.IsChecked = false; SupChkSweat.IsChecked = false;
            SupChkNoVeggie.IsChecked = false; SupChkLowEnergy.IsChecked = false;
            SupChkPoorSleep.IsChecked = false; SupChkCantSleep.IsChecked = false;
            SupChkStress.IsChecked = false;
            SupChkTrainOk.IsChecked = false; SupChkWeakPump.IsChecked = false;
            SupChkTrainFaint.IsChecked = false;
            SupResult.Visibility = Visibility.Collapsed;
            SupResultPanel.Children.Clear();
        }

        // ─────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────
        private static double Parse(string s) =>
            double.Parse(s.Replace(',', '.').Trim(),
                System.Globalization.CultureInfo.InvariantCulture);

        private void ShowError(string msg) =>
            MessageBox.Show(msg, "Hata", MessageBoxButton.OK, MessageBoxImage.Warning);

        // ─────────────────────────────────────────────────────────────
        // SPORCU REHBERİ — 5 konu tek pencerede
        // ─────────────────────────────────────────────────────────────
        private void SportGuideBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowSportGuideWindow();
        }

        private void ShowSportGuideWindow()
        {
            var accent    = (System.Windows.Media.Brush)TryFindResource("AccentBrush")           ?? System.Windows.Media.Brushes.DodgerBlue;
            var accentLt  = (System.Windows.Media.Brush)TryFindResource("AccentLightBrush")      ?? System.Windows.Media.Brushes.AliceBlue;
            var primary   = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush")      ?? System.Windows.Media.Brushes.Black;
            var secondary = (System.Windows.Media.Brush)TryFindResource("SecondaryTextBrush")    ?? System.Windows.Media.Brushes.Gray;
            var borderBr  = (System.Windows.Media.Brush)TryFindResource("BorderBrush")           ?? System.Windows.Media.Brushes.LightGray;
            var bgContent = (System.Windows.Media.Brush)TryFindResource("ContentBackgroundBrush")?? System.Windows.Media.Brushes.White;
            var bgHeader  = (System.Windows.Media.Brush)TryFindResource("HeaderBackgroundBrush") ?? System.Windows.Media.Brushes.WhiteSmoke;
            var bgSidebar = (System.Windows.Media.Brush)TryFindResource("SidebarBackgroundBrush")?? System.Windows.Media.Brushes.WhiteSmoke;

            var win = new Window
            {
                Title                 = "📚 Sporcu Rehberi",
                Width                 = 580,
                Height                = 560,
                ResizeMode            = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner                 = this,
                Background            = bgContent,
                FontFamily            = new System.Windows.Media.FontFamily("Segoe UI")
            };

            // Ana grid: sol menü + sağ içerik
            var mainGrid = new System.Windows.Controls.Grid();
            mainGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(170) });
            mainGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // ── Sol menü ──
            var menuBorder = new System.Windows.Controls.Border
            {
                Background      = bgSidebar,
                BorderBrush     = borderBr,
                BorderThickness = new Thickness(0, 0, 1, 0)
            };
            var menuPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(0, 8, 0, 8) };

            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            win.Title = isEn ? "📚 Athlete Guide" : "📚 Sporcu Rehberi";
            
            var menuHeader = new System.Windows.Controls.Border { Padding = new Thickness(16, 8, 16, 8) };
            menuHeader.Child = new System.Windows.Controls.TextBlock
            {
                Text       = isEn ? "ATHLETE GUIDE" : "SPORCU REHBERİ",
                FontSize   = 10,
                FontWeight = FontWeights.Bold,
                Foreground = accent
            };

            menuPanel.Children.Add(menuHeader);

            // İçerik paneli (sağ)
            var contentBorder = new System.Windows.Controls.Border { Background = bgContent };
            var contentScroll = new System.Windows.Controls.ScrollViewer
            {
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled
            };
            var contentPanel = new System.Windows.Controls.StackPanel();
            contentScroll.Content = contentPanel;
            contentBorder.Child   = contentScroll;

            // ── İçerik tanımları ──
            var sectionsTr = new[]
            {
                ("⏱ Toparlanma Süreleri", new[]
                {
                    ("KAS GRUPLARI — TOPARLANMA", new[]
                    {
                        ("Küçük kaslar (biseps, triseps, omuz)", "48 saat"),
                        ("Orta kaslar (sırt, göğüs, bacak ön)", "48–72 saat"),
                        ("Büyük kaslar (bacak, kilitçi, sırt)","72–96 saat"),
                        ("Merkez / Core",                       "24–48 saat"),
                        ("Karın kasları",                       "24–48 saat"),
                    }),
                    ("ANTRENMAN TÜRÜ — DİNLENME", new[]
                    {
                        ("Düşük yoğunluklu kardiyo",       "12–24 saat"),
                        ("Orta yoğunluklu kardiyo",        "24–48 saat"),
                        ("HIIT / Yüksek yoğunluk",         "48–72 saat"),
                        ("Maksimal güç (düşük tekrar)",    "48–72 saat"),
                        ("Hypertrophy (orta ağır, yüksek hacim)", "48–72 saat"),
                    }),
                    ("TOPARLANMA İPUÇLARI", new[]
                    {
                        ("Aktif dinlenme (yüzme, yürüyüş)", "Kan akışını artırır"),
                        ("Soğuk duş / buz banyosu",         "İltihap azaltır"),
                        ("Masaj / köpük silindir",           "Kas gerginliği azaltır"),
                        ("Uyku",                            "En etkili toparlanma yöntemi"),
                    }),
                }),
                ("💊 Takviye Rehberi", new[]
                {
                    ("🥇 TİER 1 — GÜÇLÜ BİLİMSEL KANIT (ISSN 2023)", new[]
                    {
                        ("Kreatin Monohidrat",        "3–5 g/gün; güç %5–15↑, yağsız kütle↑\nYükleme gerekmez. Tüm formlar arasında en iyi biyoyararlanım."),
                        ("Kafein",                    "3–6 mg/kg, ant. 45–60 dk önce\nDayanıklılık %2–4↑, güç & odak↑. Tolerans için haftada 1–2 gün ara."),
                        ("Beta-Alanin",               "3.2–6.4 g/gün, ≥4 hafta\n60–240 sn egzersizde pH tamponlar. Karıncalanma normaldir, zararsız."),
                        ("Protein Tozu (Whey)",       "Günlük protein hedefini tamamlamak için\nWhey isolate en hızlı; concentrate yeterli. Yemekten üstün değil."),
                        ("Sodyum Bikarbonat",         "0.2–0.3 g/kg, ant. 60–90 dk önce\nKısa yüksek yoğunluklu egzersizde (≤10 dk) laktik asidozu tamponlar."),
                        ("Nitrat / Pancar Suyu",      "6–8 mmol nitrat, ant. 2–3 saat önce\nVO₂max↑, dayanıklılık perf.↑. Aerobik branşlarda en etkili."),
                    }),
                    ("🥈 TİER 2 — ORTA DÜZEY KANIT", new[]
                    {
                        ("Magnezyum (Glinat/Malat)",  "200–400 mg/gün, yatmadan önce\nSporcuların %60'ında eksik. Kas krampı, uyku kalitesi, insülin duyarlılığı."),
                        ("D Vitamini + K2",           "2000–4000 IU D3 + 100–200 mcg K2/gün\nSporcuların %50'si yetersiz; kas gücü, hormon dengesi, bağışıklık."),
                        ("Omega-3 (EPA+DHA)",         "2–3 g/gün (≥1 g EPA)\nKas protein sentezi↑, egzersiz sonrası inflamasyon↓. Balık yağı tercih."),
                        ("HMB (β-Hidroksi β-Metilbütirat)", "3 g/gün (1 g × 3)\nYeni başlayanlarda kas yıkımını azaltır. Deneyimlilerde kanıt zayıf."),
                        ("Kafein + Kreatin Kombo",    "Birlikte alınabilir; eski 'etkileşim' çalışmaları çürütüldü (Trexler 2020)\nCreatine yükleme döneminde kafein yüklemeyi engellemiyor."),
                    }),
                    ("🥉 TİER 3 — ZAYIF / YETERSİZ KANIT", new[]
                    {
                        ("BCAA",                     "Yeterli protein alımında ek katkı YOK (Wolfe 2017, JISSN)\nGünlük 1.6–2.2 g/kg protein ile leucine zaten karşılanır."),
                        ("L-Glutamin",               "Bağırsak bariyeri dışında sporcu için katkı minimal\nKan glutamin düzeyi antrenmanla düşmez; takviye anlamsız."),
                        ("Testosteron Booster",      "D-aspartik asit & tribulus: insanlarda T artışı YOK\nHayvan çalışmalarından extrapolasyon; klinik kanıt yetersiz."),
                        ("CLA (Konj. Linoleik Asit)","Meta-analizlerde yağ kaybı ≈0.09 kg/hafta; kas artışı yok\nPraktik önemi ihmal edilebilir düzeyde (Whigham 2007)."),
                        ("Yağ Yakıcılar (termojenik)","Sinefrin+kafein: hafif termogenez; uzun vadeli güvenlik tartışmalı\nEfedrin içerikli ürünler birçok ülkede yasaklı, kardiyak risk."),
                        ("Glutamin Peptid / Soya BCAA","Düşük biyoyararlanım; whey ile kıyaslandığında anlamlı fark yok"),
                    }),
                    ("⚗ DOZ & ZAMANLAMA KILAVUZU", new[]
                    {
                        ("Kreatin",         "Her gün sabit saat; yemekle veya sonrasında; creatine HCl, etil ester üstün değil"),
                        ("Kafein",          "Sabah 1. kez uyanıştan 90–120 dk sonra al (kortizol ritmiyle çakışma önlenir)"),
                        ("Beta-Alanin",     "Küçük dozlara böl (0.8–1 g × 4–6) → karıncalanma minimize"),
                        ("Whey Protein",    "Antrenman sonrası 30–60 dk; ama toplam günlük doz > zamanlama"),
                        ("Magnezyum",       "Glinat gece, malat sabah; sitrat gevşetici ama ishale yol açabilir"),
                        ("D Vitamini",      "Yağlı yemekle birlikte al (yağda çözünür); ekim–nisan arası özellikle şart"),
                    }),
                    ("⚠ GÜVENLİK & SATIN ALMA KURALLARI", new[]
                    {
                        ("3. Taraf Sertifikası",     "NSF Certified for Sport / Informed Sport etiketi→ doping maddesi riski azalır"),
                        ("Etiket şeffaflığı",        "'Proprietary blend' ibaresi → her bileşenin dozu gizli; kaçın"),
                        ("Kreatin + Böbrek",         "Sağlıklı bireylerde zararlı değil (Antonio 2021). Böbrek hastalarında doktor onayı şart"),
                        ("Kafein + İlaç",            "Beta bloker, antidepresan (SSRI) kullananlarda kardiyak uyarı; danışın"),
                        ("D Vitamini toksisitesi",   "10,000 IU/gün üstü uzun süreli → hiperkalsemi riski; kan testi ile takip"),
                        ("Gerçek yiyecek önce",      "Hiçbir takviye dengeli beslenmenin yerini alamaz; önce gerçek besin"),
                    }),
                }),
                ("🍽 Beslenme Zamanı", new[]
                {
                    ("ANTRENMAN ÖNCESİ (60–90 dk önce)", new[]
                    {
                        ("Karbonhidrat (orta GI)",    "Yulaf, muz, pirinç — enerji deposu"),
                        ("Orta protein",              "20–30 g — kas yıkımını önler"),
                        ("Düşük yağ & lif",           "Sindirimi yavaşlatmaz"),
                        ("Sıvı alımı",                "400–600 ml su — antreamandan önce"),
                    }),
                    ("ANTRENMAN SONRASI (0–60 dk içinde)", new[]
                    {
                        ("Hızlı sindirim proteini",   "30–40 g whey veya tavuk"),
                        ("Yüksek GI karbonhidrat",    "Glikojen yenilemek için (pirinç, meyve)"),
                        ("Sodyum + sıvı",             "Kaybedilen elektrolit yerine"),
                        ("Kreatin (varsa)",            "Su ile en iyi sonuç antrenman sonrası"),
                    }),
                    ("GECE YATMADAN ÖNCE", new[]
                    {
                        ("Kazein protein",            "Yavaş sindirim — gece boyu amino asit"),
                        ("Keçi sütü / süzme peynir", "Doğal kazein kaynağı"),
                        ("Düşük karbonhidrat",        "İnsülin tepkisini düşük tut"),
                    }),
                }),
                ("🤕 DOMS — Kas Ağrısı", new[]
                {
                    ("DOMS NEDİR?", new[]
                    {
                        ("Tam adı",       "Delayed Onset Muscle Soreness"),
                        ("Başlangıç",     "Antrenman sonrası 12–48 saat içinde"),
                        ("Zirve",         "24–72 saat — sonra yavaşça geçer"),
                        ("Neden olur",    "Eksantrik kasılmalar, mikro yırtıklar, iltihap"),
                        ("Laktik asiple ilgisi", "YOKTUR — bu yaygın bir yanılgı"),
                    }),
                    ("DOMS NASIL YÖNETİLİR?", new[]
                    {
                        ("Aktif iyileşme",          "Yürüyüş, yüzme — kan akışı artırır"),
                        ("Soğuk/sıcak terapi",      "İltihap azaltır, rahatlama sağlar"),
                        ("Yeterli protein & uyku",  "Kas onarımını hızlandırır"),
                        ("Hafif esnetme",           "Gerginliği azaltır, esnekliği korur"),
                        ("NSAİD ilaçlar (ibuprofen)","Gerektiğinde kısa süreli kullanılabilir"),
                    }),
                    ("DOMS NE ZAMAN ALARM VERİR?", new[]
                    {
                        ("5+ gün geçmiyorsa",       "Sakatlanma olabilir — dinlen"),
                        ("Şişlik + ısınma varsa",   "Doktora git — inflamasyon ciddi"),
                        ("Koyu çay renkli idrar",   "ACİL — Rabdomiyoliz riski"),
                        ("Tek taraflı ağrı",        "Sakatlanma şüphesi"),
                    }),
                }),
                ("😴 Uyku & Spor", new[]
                {
                    ("NEDEN ÖNEMLİ?", new[]
                    {
                        ("Büyüme hormonu",          "En yüksek salınım derin uyku döneminde"),
                        ("Kas proteini sentezi",    "Uyku sırasında zirveye ulaşır"),
                        ("Kortizol düzenlenmesi",   "Yetersiz uyku = yüksek kortizol = kas yıkımı"),
                        ("Motivasyon & form",       "Az uyku antrenman kalitesini düşürür"),
                        ("Yaralanma riski",         "Yetersiz uyku sakatlanma riskini 2× artırır"),
                    }),
                    ("ÖNERİLEN UYKU SÜRELERİ", new[]
                    {
                        ("Rekreasyon sporcular",    "7–8 saat"),
                        ("Düzenli antrenman yapan", "8–9 saat"),
                        ("Profesyonel sporcu",      "9–10 saat"),
                        ("Yoğun müsabaka dönemi",  "10+ saat önerilir"),
                    }),
                    ("UYKU KALİTESİNİ ARTIRMAK İÇİN", new[]
                    {
                        ("Düzenli uyku saati",         "Sirkadiyen ritmi korur"),
                        ("Karanlık & serin oda",       "Melatonin salınımını destekler"),
                        ("Yatmadan 2 saat önce ekran", "Mavi ışığı sınırla"),
                        ("Kafein cut-off",             "Öğleden sonra 14:00'den kesilmeli"),
                        ("Magnezyum glisinat",         "Gevşeme & derin uyku için"),
                    }),
                }),
                ("🏃 Kondisyon & Patlama", new[]
                {
                    ("KONDİSYON GELİŞTİRME", new[]
                    {
                        ("Aerobik Taban (Zone 2)", "Haftada 3-5×30-60 dk, %60-70 MHR\nYavaş koşu, yüzme, bisiklet — konuşabilir hızda"),
                        ("Anaerobik Eşik", "Haftada 1-2×20-30 dk, %80-85 MHR\n4-8 dk aralıklarla tempo koşusu, dinlenme 1:1"),
                        ("HIIT (Yüksek Yoğunluk)", "Haftada 1-2×15-25 dk\n30 sn sprint : 60 sn yürüyüş × 8-12 tur"),
                        ("VO₂max Geliştirme", "4×4 dk %90-95 MHR, arası 3 dk aktif dinlenme\nHaftada 1 seans, 8 hafta → VO₂max %5-8 artış"),
                        ("Yüzücü Kondisyonu", "Havuz: 50m sprint + 50m yavaş × 10-20 tur\nKuru: Band pull, core plank, omuz rotasyonu"),
                        ("Koşucu Kondisyonu", "Fartlek: 5 dk kolay + 2 dk sert × 4-6 tur\nTempo run: 20-40 dk %80 MHR yarış hızında"),
                    }),
                    ("PATLAMA GÜCÜ (EXPLOSIVE POWER)", new[]
                    {
                        ("Nedir?", "Kısa sürede maksimum kuvvet üretme yeteneği\nGüç = Kuvvet × Hız formülü"),
                        ("Olimpik Kaldırma", "Power Clean: 3-5 set × 2-3 tekrar\nHang Snatch: 3 set × 3 tekrar\nPush Jerk: 4 set × 3-5 tekrar"),
                        ("Balistik Hareketler", "Kettlebell Swing: 4×15\nMedicine Ball Slam: 3×10\nBattle Rope: 30 sn × 5 set"),
                        ("Weighted Jump", "Trap Bar Jump: 3×5 (%20-30 vücut ağırlığı)\nWeighted Squat Jump: 3×5\nBox Jump + Dumbbell: 3×5"),
                        ("Kontras Yöntemi (PAP)", "1. Heavy Squat (3×3) → Box Jump (3×5)\n2. Bench Press (3×3) → Med Ball Chest Pass (3×8)\nPost-Activation Potentiation etkisi"),
                        ("Haftalık Plan", "Pzt: Olimpik kaldırma + plyometrics\nÇrş: Balistik + core patlayıcılık\nCuma: Hız antrenmanı (sprint + sıçrama)"),
                    }),
                    ("SIÇRAMA GÜCÜ (PLYOMETRICS)", new[]
                    {
                        ("Başlangıç Seviyesi", "Squat Jump: 3×8\nTuck Jump: 3×6\nAnkle Hops: 3×20\nLateral Bound: 3×8 (her tarafa)"),
                        ("Orta Seviye", "Box Jump (50-60 cm): 4×5\nDepth Jump (30 cm): 3×5\nSingle-Leg Hop: 3×6 (her bacak)\nBounding: 3×20m"),
                        ("İleri Seviye", "Depth Jump (60-75 cm): 4×4\nReactive Box Jump: 4×4\nSingle-Leg Depth Jump: 3×3\nIce Skater + Box Jump Combo: 3×5"),
                        ("6 Haftalık Program", "Hafta 1-2: Kuvvet temeli (squat %85 1RM)\nHafta 3-4: Güç dönüşümü (squat+jump combo)\nHafta 5-6: Hız-güç optimizasyonu (düşük yük, max hız)"),
                        ("Güvenlik Kuralları", "Zemin: Yumuşak (parke, çim) tercih edin\nAyakkabı: Yastıklı taban şart\nHacim: Haftada max 120-150 yer teması\nDinlenme: Setler arası 2-3 dk"),
                    }),
                    ("SPORCULAR İÇİN ÖZEL REHBER", new[]
                    {
                        ("Futbolcu / Basketbolcu", "Sprint + yön değiştirme\n5-10-5 Drill, T-Testi, Pro Agility\nReaktif sıçrama + lateral hareketler"),
                        ("Yüzücü", "Kuru antrenman: lat pulldown, bench, squat\nBand ile omuz rotasyonu + stabilizasyon\nMedicine ball throw (havuz kenarı)"),
                        ("Koşucu (Kısa mesafe)", "Blok çıkışı + 30m sprint × 6-8\nSled push/pull: 4×20m\nHill sprint: 6-8 × 40-60m"),
                        ("Koşucu (Uzun mesafe)", "Tempo run + interval kombinasyonu\nHaftalık uzun koşu (%20 seans hacmi)\nKuvvet: Tek bacak squat, hip thrust"),
                        ("Voleybolcu / Atlet", "Depth jump → Smaç sıçraması\nSingle-leg box jump: 4×5\nHang Clean: 3×3 + Broad Jump: 3×5"),
                    }),
                }),
                ("💎 Calisthenics", new[]
                {
                    ("SIFIRDAN BAŞLANGIÇ (Level 0)", new[]
                    {
                        ("Temel Kuvvet", "Knee Push-Up: 3×10-15\nAustralian Pull-Up: 3×8-12\nBench Dip: 3×10\nBW Squat: 3×15\nDead Hang: 3×15 sn"),
                        ("Core Temeli", "Plank: 3×20-30 sn\nDead Bug: 3×8 (her taraf)\nSuperman Hold: 3×10 sn\nKnee Raise (yerde): 3×10"),
                        ("Esneklik", "Kol çevirme: 2×15\nDuvarda omuz esneme: 2×20 sn\nDerin squat tutma: 3×20 sn\nPike stretch: 2×20 sn"),
                        ("Haftalık Plan", "Pzt: Push (göğüs, triseps, omuz)\nSalı: Pull (sırt, biseps)\nPrş: Push — Cuma: Pull\nHer gün: Core+mobilite 10 dk"),
                        ("Hedef Süre", "4-8 hafta → düzenli push-up ve çekiş yapabilecek seviye"),
                    }),
                    ("BAŞLANGIÇ SEVİYESİ (Level 1)", new[]
                    {
                        ("Push Hareketleri", "Push-Up: 3×15-20\nDiamond Push-Up: 3×8-12\nPike Push-Up: 3×8-10\nDip (paralel bar): 3×6-10"),
                        ("Pull Hareketleri", "Pull-Up (negatif): 3×5-8\nChin-Up: 3×5-8\nAustralian (ayaklar yüksekte): 3×12\nScapula Pull-Up: 3×10"),
                        ("Bacak ve Core", "Pistol Squat (kutulu): 3×6\nLunge: 3×12\nL-Sit tutma (yerde): 3×10 sn\nHanging Knee Raise: 3×10"),
                        ("Hedef Beceriler", "10 temiz Pull-Up\n20 temiz Push-Up\n5 Dip\n15 sn L-Sit (yerde)"),
                        ("Süre", "8-16 hafta — kişisel ilerlemeye bağlı"),
                    }),
                    ("ORTA SEVİYE (Level 2)", new[]
                    {
                        ("Push Gelişimi", "Pseudo Planche Push-Up: 3×8\nHSPU (duvarda): 3×5-8\nArcher Push-Up: 3×6 (her taraf)\nRing Dip: 3×8"),
                        ("Pull Gelişimi", "Muscle-Up Negatif: 3×3-5\nTypewriter Pull-Up: 3×4\nL-Sit Pull-Up: 3×5-8\nFront Lever Tuck: 3×10 sn"),
                        ("Statik Tutmalar", "Handstand (duvarda): 3×30 sn\nBack Lever (tuck): 3×10 sn\nHuman Flag (tuck): 3×5 sn\nPlanche Lean: 3×10 sn"),
                        ("Hedef Beceriler", "1 Temiz Muscle-Up\n15 sn Handstand (duvarda)\n10 sn Tuck Front Lever\nRing Dip × 10"),
                        ("Süre", "4-8 ay düzenli çalışma"),
                    }),
                    ("İLERİ SEVİYE (Level 3)", new[]
                    {
                        ("Çekiş Ustalığı", "Muscle-Up (temiz): 3×5\nOne-Arm Pull-Up Prog: 3×3\nFront Lever (full): 3×8 sn\nBack Lever (full): 3×10 sn"),
                        ("İtiş Ustalığı", "Free HSPU: 3×3-5\nPlanche Push-Up (straddle): 3×3\n90 derece Push-Up: 3×3\nRing Muscle-Up: 3×5"),
                        ("Dinamik Hareketler", "360 derece Pull-Up: teknik\nClap Muscle-Up: teknik\nHandstand Walk: 10m+\nL-Sit to Handstand: teknik"),
                        ("Hedef Beceriler", "Free Handstand 30 sn\nFull Front Lever 10 sn\nStraddle Planche 5 sn\n3 Temiz Ring Muscle-Up"),
                        ("Süre", "1-3 yıl düzenli çalışma"),
                    }),
                    ("ELİT HAREKETLERE İLERLEME", new[]
                    {
                        ("Full Planche", "Lean → Tuck → Adv.Tuck → Straddle → Full\nÖn gereksinim: 30 sn Pseudo Planche PU\nSüre: 1-3 yıl; bilek güçlendirme şart"),
                        ("Front Lever", "Tuck → Adv.Tuck → Single Leg → Straddle → Full\nÖn gereksinim: 15 Pull-Up, güçlü lat\nSüre: 6-18 ay"),
                        ("Handstand (serbest)", "Duvar → Chest-to-wall → Kick-up → Free\nGünde 10-15 dk pratik; bilek mobilite önemli\nSüre: 3-12 ay"),
                        ("Muscle-Up", "Explosive Pull-Up → Chest-to-Bar → Transition\nFalse grip çalışması: her gün 5 dk\nSüre: 2-8 ay"),
                        ("Human Flag", "Tuck → Single Leg → Straddle → Full\nÖn gereksinim: güçlü oblikler + omuz\nSüre: 6-18 ay"),
                        ("Iron Cross (halka)", "Support → RTO Support → Tuck Cross → Full\nEn zor hareket; tendon hazırlığı 2+ yıl\nSadece rings üzerinde çalışılır"),
                    }),
                }),
                ("🧘 Esneklik & Mobilite", new[]
                {
                    ("ÜST VÜCUT ESNEKLİK", new[]
                    {
                        ("Doorway Stretch (Göğüs)", "Kapı pervazına kollarınızı 90° açıda koyup öne adımlayın\n20-30 sn tutun, göğüs açıklığını artırır\nDirsek yüksekliğini değiştirerek farklı lifleri hedefleyin"),
                        ("Y-T-W Raises (Omuz)", "Yüzüstü veya öne eğilerek Y, T, W şeklinde kol kaldırma\nHer harf 10 tekrar, scapula stabilizasyonu sağlar\nBant ile yapılırsa etkinlik artar"),
                        ("Shoulder Dislocates", "Geniş tutuşla bant/çubuğu başın üzerinden arkaya geçirin\n2×10 tekrar, omuz mobilitesi için altın standart\nZamanla tutuşu daraltarak zorlaştırın"),
                        ("Overhead Tricep Stretch", "Bir kolu başın arkasına katlayıp diğer elle dirsekten bastırın\n20-30 sn her kol, triseps long head'i gerer"),
                        ("Wrist Flexor/Extensor Stretch", "Kolunuzu düz uzatıp parmakları aşağı/yukarı çekerek bileği gerin\n15-20 sn her yön, özellikle ağırlık öncesi şart"),
                    }),
                    ("ALT VÜCUT ESNEKLİK", new[]
                    {
                        ("Standing Forward Fold", "Ayakta dizleri hafif bükük öne eğilin, parmaklara uzanın\n20-30 sn, hamstring + alt sırt açar\nDerin nefes ile her verişte biraz daha inin"),
                        ("Seated Forward Fold", "Yerde bacaklar düz, parmaklara doğru uzanın\n20-30 sn, hamstring esnekliği için temel hareket\nSırtı düz tutmaya çalışın"),
                        ("Pigeon Pose (Güvercin)", "Ön bacak bükük, arka bacak düz, gövdeyi öne eğin\n30-45 sn her taraf, kalça açıcı #1\nDirençle karşılaştığınızda nefes verin"),
                        ("90/90 Stretch", "Yerde ön bacak 90° dış rotasyon, arka bacak 90° iç rotasyon\n20-30 sn her taraf, kalça mobilitesi\nGövdeyi öne eğerek yoğunluğu artırın"),
                        ("Frog Stretch (Kurbağa)", "Dizler yerde geniş açık, kalçayı geriye itin\n20-30 sn, iç bacak (adductor) esnekliği\nYavaş ve kontrollü — hızlı zorlamayın"),
                        ("Butterfly Stretch (Kelebek)", "Oturarak ayak tabanlarını birleştirin, dizleri yere doğru bastırın\n20-30 sn, kalça iç rotasyon + adductor\nDirseklerle dizlere hafif basınç uygulayın"),
                    }),
                    ("SIRT & OMURGA MOBİLİTESİ", new[]
                    {
                        ("Cat-Cow (Kedi-İnek)", "Dört ayak üzerinde sırtı yuvarla (kedi) → çukurlaştır (inek)\n10-15 tekrar, omurga segmental mobilitesi\nNefes ile senkronize: Nefes ver=kedi, nefes al=inek"),
                        ("Child's Pose (Çocuk Pozu)", "Dizler yerde, kalçayı topuklara çekip kolları öne uzatın\n30-45 sn, lat + omurga + kalça uzar\nKolları bir tarafa uzatarak oblik germe ekleyin"),
                        ("Thread the Needle", "Dört ayak üzerinde bir kolu diğerinin altından geçirin, gövdeyi döndürün\n8-10 tekrar her taraf, torasik rotasyon\nOmzunuzu yere yaklaştırmaya çalışın"),
                        ("Scorpion Stretch", "Yüzüstü yatıp bir bacağı karşı tarafa geçirerek omurgayı döndürün\n15-20 sn her taraf, torasik + lomber mobilite\nOmuzları yerde tutmaya çalışın"),
                        ("Jefferson Curl", "Ayakta ağırlıksız, çeneden başlayarak omurgayı vertebra vertebra aşağı yuvarlayın\n6-8 tekrar, omurga fleksion mobilitesi\n⚠ Ağırlık eklemeden önce yeterli esneklik gerekli"),
                    }),
                    ("KALÇA & HAMSTRİNG FOKUSLANMİŞ", new[]
                    {
                        ("Deep Squat Hold (Derin Çömelme)", "Topuklarınız yerde, derin çömelme pozisyonunda kalın\n30-60 sn, ayak bileği + kalça + sırt mobilitesi\nDirseklerle dizleri dışarı itin"),
                        ("Couch Stretch (Kanepe Esneme)", "Bir dizinizi duvara/kanepeye dayayıp ön kalçayı gerin\n30-45 sn her taraf, hip flexor + quadriceps\nKalçayı sıkarak yoğunluğu artırın"),
                        ("Standing Quad Stretch", "Ayakta bir ayağınızı kalçanıza çekip tutun\n20-30 sn her bacak, quadriceps esnekliği\nDizleri yan yana tutun, dışarı açmayın"),
                        ("Half Kneeling Hip Flexor", "Tek diz yerde, ön bacak 90°, kalçayı öne itin\n20-30 sn her taraf, psoas + hip flexor\nKolları yukarı kaldırarak yoğunlaştırın"),
                        ("Reclined Hamstring Stretch", "Sırt üstü yatıp bir bacağı bant/havlu ile yukarı çekin\n20-30 sn her bacak, güvenli hamstring germe"),
                    }),
                    ("DİNAMİK ISINMA RUTİNİ (5-10 DK)", new[]
                    {
                        ("1. Hafif Kardiyo (2 dk)", "Yerinde yürüyüş veya hafif koşu\nKalp atış hızını kademeli artırır, kan akışını başlatır"),
                        ("2. Hip Circles + Leg Swings (1 dk)", "Kalça çevirme 10+10 → Bacak salınımı ileri-geri 10+10\nKalça eklemini lubrike eder"),
                        ("3. Cat-Cow + Torso Twist (1 dk)", "Cat-Cow 8 tekrar → Standing Torso Twist 10+10\nOmurga mobilitesi ve core aktivasyonu"),
                        ("4. Arm Circles + Shoulder Dislocates (1 dk)", "Kol çevirme 15+15 → Bant ile dislocate 10 tekrar\nÜst vücut eklem ısınması"),
                        ("5. Walking Lunges + High Knees (1-2 dk)", "10 lunges → 20 high knees → 10 lunges\nTam vücut aktivasyon, geniş ROM ile"),
                        ("6. Hedef Bölge Aktivasyonu (1-2 dk)", "O günkü antrenman bölgesine özel 2-3 ısınma hareketi\nHafif set veya bant ile spesifik kasları ısıtın"),
                    }),
                    ("ANTRENMAN SONRASI SOĞUMA (10-15 DK)", new[]
                    {
                        ("1. Yavaş Yürüyüş (2-3 dk)", "Kalp atış hızını kademeli düşürün\n%50-60 MHR'ye inin"),
                        ("2. Statik Esneme — Üst Vücut (3-4 dk)", "Doorway Stretch: 30 sn\nOverhead Tricep: 20 sn × her kol\nArm Cross: 20 sn × her kol\nNeck Lateral: 15 sn × her taraf"),
                        ("3. Statik Esneme — Alt Vücut (3-4 dk)", "Standing Forward Fold: 30 sn\nPigeon Pose: 30 sn × her taraf\nStanding Quad Stretch: 20 sn × her bacak\nWall Calf Stretch: 20 sn × her bacak"),
                        ("4. Sırt & Core (2-3 dk)", "Child's Pose: 30 sn\nCat-Cow: 8 tekrar\nSupine Twist: 20 sn × her taraf"),
                        ("5. Derin Nefes (1-2 dk)", "Burundan 4 sn → tutun 4 sn → ağızdan 6 sn\nParasempatik sistemi aktive eder, toparlanmayı hızlandırır"),
                    }),
                    ("MOBİLİTE KURALLARI & BİLGİLER", new[]
                    {
                        ("Statik vs Dinamik", "Statik: 20-45 sn tutarak germe → antrenman SONRASI\nDinamik: Hareket ederek ısınma → antrenman ÖNCESİ\nAntrenman öncesi statik esneme güç kaybına neden olabilir"),
                        ("Esneklik vs Mobilite", "Esneklik: Kasın pasif olarak uzama kapasitesi\nMobilite: Eklemin aktif olarak kontrollü hareket edebilme yetisi\nMobilite > esneklik, çünkü kontrol de gerektirir"),
                        ("Ne Kadar Süre Tutulmalı?", "Minimum 20 sn, ideal 30-45 sn (ACSM önerisi)\nHaftalık toplam 5+ dakika/kas grubu hedefleyin\n3-4 hafta düzenli pratikle belirgin ilerleme"),
                        ("Ne Zaman Yapılmalı?", "Dinamik ısınma: Antrenman öncesi (5-10 dk)\nStatik esneme: Antrenman sonrası veya ayrı seans\nSabah esneme: Eklemleri lubrike eder, gün boyu fayda"),
                        ("Ağrı vs Gerilme", "Hafif gerilme hissi = DOĞRU, ilerleyin\nAğrı / keskin his = YANLIŞ, durun\nNefes veremiyorsanız çok zorlanıyorsunuz demektir"),
                        ("İlerleme Nasıl Sağlanır?", "Her hafta 1-2 sn daha uzun tutun\nROM'u kademeli artırın — zorlamayın\nTutarlılık > yoğunluk; haftada 5×5 dk > 1×30 dk"),
                    }),
                }),
                ("📅 Antrenman Splitleri", new[]
                {
                    ("HANGİ PROGRAMI SEÇMELİYİM?", new[]
                    {
                        ("Haftada 2-3 Gün", "Full Body (Tam Vücut) en idealidir. Kısıtlı vakitte en yüksek verim."),
                        ("Haftada 4 Gün", "Upper/Lower (Alt/Üst) veya Bro Split. Upper/Lower bilimsel olarak daha dengelidir."),
                        ("Haftada 5-6 Gün", "Push/Pull/Legs (İtiş/Çekiş/Bacak) veya özel kombinasyonlar. İleri seviyeler için."),
                    }),
                    ("FULL BODY (TAM VÜCUT)", new[]
                    {
                        ("Nasıl Çalışır?", "Her antrenmanda tüm ana kas grupları (göğüs, sırt, bacak, omuz) çalıştırılır."),
                        ("Kimler İçin?", "Yeni başlayanlar, vakti kısıtlı olanlar, haftada sadece 2-3 gün gidebilenler."),
                        ("Artıları (+)", "Sık kas proteini sentezi (kaslar daha sık uyarılır).\nSistematik toparlanmayı öğretir.\nZaman açısından çok verimlidir."),
                        ("Eksileri (-)", "Vücut geliştikçe antrenmanlar çok uzun ve yorucu hale gelebilir.\nHer kasa yeterli özel hacmi ayırmak zorlaşır."),
                    }),
                    ("UPPER / LOWER (ALT / ÜST)", new[]
                    {
                        ("Nasıl Çalışır?", "1. Gün: Üst Vücut (Göğüs+Sırt+Omuz+Kol)\n2. Gün: Alt Vücut (Quad+Hamstring+Kalf)\n(Genellikle 4 gün yapılır: Alt-Üst-Dinlenme-Alt-Üst)"),
                        ("Kimler İçin?", "Orta ve ileri seviyeler, haftada 4 gün ayırabilenler."),
                        ("Artıları (+)", "Mükemmel kas çalışma frekansı (her kas haftada 2 kez).\nAlt ve üst vücut için harika bir toparlanma dengesi sunar."),
                        ("Eksileri (-)", "Üst vücut günleri bazen çok uzun sürebilir çünkü çalışılacak çok bölge vardır."),
                    }),
                    ("PUSH / PULL / LEGS (PPL)", new[]
                    {
                        ("Nasıl Çalışır?", "İtiş (Göğüs, Omuz, Arka Kol)\nÇekiş (Sırt, Arka Omuz, Ön Kol)\nBacak (Quad, Hamstring, Kalf)"),
                        ("Kimler İçin?", "İleri seviye sporcular, spora haftada 5-6 gün gidebilenler."),
                        ("Artıları (+)", "Yüksek hacim (volume) potansiyeli.\nBirbiriyle çalışan kaslar aynı gün yorulduğu için mükemmel odaklanma sağlar."),
                        ("Eksileri (-)", "Haftada 6 gün gitmek sinir sistemini (CNS) ciddi yorabilir.\nDisiplin ve harika beslenme gerektirir."),
                    }),
                    ("BRO SPLIT (TEK BÖLGE)", new[]
                    {
                        ("Nasıl Çalışır?", "Her gün sadece 1 veya 2 kas. (Pzt: Göğüs, Sal: Sırt, Çrş: Omuz, Prş: Bacak, Cum: Kol)"),
                        ("Kimler İçin?", "Eski tip çalışanlar, zayıf bir bölgesine aşırı odaklanmak isteyen ileri seviyeler."),
                        ("Artıları (+)", "O gün çalışılan kasa maksimum odaklanma ve yıkım.\nAntrenmanlar daha kısa ve 'pump' hissi daha yüksektir."),
                        ("Eksileri (-)", "Bilimsel olarak optimal değildir. Kas proteini sentezi 48 saatte biter, koca bir hafta aynı kası beklersiniz."),
                    }),
                    ("HAFTALIK SET HACMİ & SIKLIK", new[]
                    {
                        ("Başlangıç Seviyesi", "Her kas grubu için haftalık 8-10 Set (Örn: Haftada toplam 3 hareket x 3 set)"),
                        ("Orta Seviye", "Her kas grubu için haftalık 10-15 Set."),
                        ("İleri Seviye", "Her kas grubu için haftalık 15-20+ Set."),
                        ("Bilimsel Sıklık Kuralı", "Bir kası haftada 2 kez (Örn: Pzt 6 set, Prş 6 set) çalıştırmak, aynı kası haftada 1 kez (Örn: Pzt 12 set) çalıştırmaktan kas gelişimi için kanıtlanmış olarak DAHa üstündür."),
                    }),
                    ("⚠ ALTIN KURALLAR (Disiplin & Tutarlılık)", new[]
                    {
                        ("Program Değiştirme Hastalığı", "Her hafta YouTube'da gördüğünüz yeni programa GECMEYİN. Bir programın işe yaraması için en az 8-12 hafta sadık kalmalısınız."),
                        ("Disiplin > Mükemmel Program", "Dünyanın en iyi yazılmış antrenman programı bile, eğer o programı %100 efor ve disiplinle yapmıyorsanız çöp olur. Önemli olan sürekliliktir."),
                        ("Progressive Overload", "Hangi programı yaparsanız yapın, amacınız geçen haftadan 1 tekrar fazla yapmak veya 1 kg daha fazla kaldırmak olmalıdır (Gelişken Yüklenme)."),
                        ("Beslenme Aslında Anahtardır", "Salonda kaslarınızı inşa etmez, onları yıkarsınız. Kaslar Mutfakta ve Yatakta (Uyku) büyür. Diyetiniz kötü ve proteini eksikse hiçbir split sizi kurtaramaz."),
                    }),
                }),
            };

            var sectionsEn = new[]
            {
                ("⏱ Recovery Times", new[]
                {
                    ("MUSCLE GROUPS — RECOVERY", new[]
                    {
                        ("Small muscles (biceps, triceps, calves)", "48 hrs"),
                        ("Medium muscles (chest, shoulders, back)", "48–72 hrs"),
                        ("Large muscles (legs, back, glutes)", "72–96 hrs"),
                        ("Core / Abs", "24–48 hrs"),
                    }),
                    ("TRAINING TYPE — REST", new[]
                    {
                        ("Low intensity cardio", "12–24 hrs"),
                        ("Moderate intensity cardio", "24–48 hrs"),
                        ("HIIT / High intensity", "48–72 hrs"),
                        ("Max strength (low rep)", "48–72 hrs"),
                        ("Hypertrophy (moderate weight)", "48–72 hrs"),
                    }),
                    ("RECOVERY TIPS", new[]
                    {
                        ("Active rest (swimming, walking)", "Increases blood flow"),
                        ("Cold shower / ice bath", "Reduces inflammation"),
                        ("Massage / foam roller", "Reduces muscle tension"),
                        ("Sleep", "Most effective recovery method"),
                    }),
                }),
                ("💊 Supplement Guide", new[]
                {
                    ("🥇 TIER 1 — STRONG SCIENTIFIC EVIDENCE", new[]
                    {
                        ("Creatine Monohydrate", "3–5 g/day; strength %5–15↑, lean mass↑\nNo loading phase needed. Best bioavailability."),
                        ("Caffeine", "3–6 mg/kg, 45–60 mins pre-workout\nEndurance %2–4↑, strength & focus↑."),
                        ("Beta-Alanine", "3.2–6.4 g/day, ≥4 weeks\nBuffers pH during 60–240 sec exercise. Tingling is harmless."),
                        ("Whey Protein", "To meet daily protein goal\nWhey isolate is fastest. Not superior to real food."),
                    }),
                    ("🥈 TIER 2 — MODERATE EVIDENCE", new[]
                    {
                        ("Magnesium", "200–400 mg/day, before bed\n60% of athletes are deficient. Helps sleep & cramps."),
                        ("Vitamin D3 + K2", "2000–4000 IU/day\nCritical for immune, bone health and hormones."),
                        ("Omega-3 (EPA/DHA)", "2–3 g/day\nIncreases synthesis, reduces inflammation."),
                    }),
                    ("🥉 TIER 3 — WEAK EVIDENCE", new[]
                    {
                        ("BCAAs", "No extra benefit if protein intake is sufficient."),
                        ("Fat Burners", "Minimal effect, high cardiac risk for some."),
                    }),
                }),
                ("🍽 Meal Timing", new[]
                {
                    ("PRE-WORKOUT (60–90 mins prior)", new[]
                    {
                        ("Carbs (Moderate GI)", "Oats, banana, rice — fuel"),
                        ("Moderate Protein", "20–30 g — prevents muscle breakdown"),
                        ("Low Fat & Fiber", "Speeds up digestion"),
                        ("Fluids", "400–600 ml water"),
                    }),
                    ("POST-WORKOUT (0–60 mins after)", new[]
                    {
                        ("Fast Protein", "30–40 g whey or chicken"),
                        ("High GI Carbs", "To replenish glycogen (rice, fruit)"),
                    }),
                }),
                ("🤕 DOMS — Muscle Soreness", new[]
                {
                    ("WHAT IS DOMS?", new[]
                    {
                        ("Full name", "Delayed Onset Muscle Soreness"),
                        ("Onset", "12–48 hrs post-workout"),
                        ("Peak", "24–72 hrs"),
                        ("Cause", "Microtears and inflammation, NOT lactic acid"),
                    }),
                    ("MANAGEMENT", new[]
                    {
                        ("Active recovery", "Walking, swimming"),
                        ("Sleep & Protein", "Fastest way to repair"),
                        ("NSAIDs", "Avoid unless necessary for daily life"),
                    }),
                }),
                ("😴 Sleep & Sports", new[]
                {
                    ("WHY IT MATTERS", new[]
                    {
                        ("Growth Hormone", "Peaks during deep sleep"),
                        ("Cortisol", "Lack of sleep increases cortisol (muscle loss)"),
                        ("Injury Risk", "Increases 2x with lack of sleep"),
                    }),
                    ("RECOMMENDED HOURS", new[]
                    {
                        ("Recreational", "7–8 hours"),
                        ("Regular athletes", "8–9 hours"),
                        ("Professional", "9–10 hours"),
                    }),
                }),
                ("🏃 Conditioning & Power", new[]
                {
                    ("CONDITIONING DEVELOPMENT", new[]
                    {
                        ("Aerobic Base (Zone 2)", "3-5x/week, 30-60 mins, 60-70% MHR\nSlow run, swim, bike — conversational pace"),
                        ("Anaerobic Threshold", "1-2x/week, 20-30 mins, 80-85% MHR\nTempo runs with 1:1 rest"),
                        ("HIIT", "1-2x/week, 15-25 mins\n30s sprint : 60s walk x 8-12 rounds"),
                        ("VO₂max Focus", "4x4 mins at 90-95% MHR, 3 mins active rest"),
                    }),
                    ("EXPLOSIVE POWER", new[]
                    {
                        ("What is it?", "Ability to exert max force in minimal time"),
                        ("Olympic Lifts", "Power Clean, Hang Snatch, Push Jerk"),
                        ("Ballistics", "Kettlebell Swing, Med Ball Slams"),
                        ("Plyometrics", "Box jumps, Depth jumps (advanced)"),
                    }),
                }),
                ("💎 Calisthenics", new[]
                {
                    ("LEVEL 0 — FOUNDATION", new[]
                    {
                        ("Basic Strength", "Knee push-ups, Australian pull-ups, squats"),
                        ("Core Base", "Planks, Dead bugs, Knee raises"),
                        ("Goal", "4-8 weeks to build basic push/pull capacity"),
                    }),
                    ("LEVEL 1 — BEGINNER", new[]
                    {
                        ("Push", "Standard push-ups, Dips"),
                        ("Pull", "Negative pull-ups, Chin-ups"),
                        ("Goal", "10 clean pull-ups, 20 push-ups"),
                    }),
                    ("LEVEL 2 — INTERMEDIATE", new[]
                    {
                        ("Skills", "Wall HSPU, Archer push-ups, Muscle-up negatives"),
                        ("Statics", "Wall handstand, Tuck front lever"),
                    }),
                    ("LEVEL 3 — ADVANCED", new[]
                    {
                        ("Elite Skills", "Clean Muscle-ups, Free Handstand, Front Lever"),
                        ("Timeframe", "1-3 years of consistent dedication"),
                    }),
                }),
                ("🧘 Flexibility & Mobility", new[]
                {
                    ("UPPER BODY FLEXIBILITY", new[]
                    {
                        ("Doorway Stretch (Chest)", "Place arms on door frame at 90° and step forward\n20-30 sec hold, improves chest opening\nChange elbow height to target different fibers"),
                        ("Y-T-W Raises (Shoulders)", "Face down or bent over, lift arms in Y, T, W shapes\n10 reps each letter, builds scapula stability\nMore effective with resistance band"),
                        ("Shoulder Dislocates", "Wide grip, pass band/stick over head to behind back\n2×10 reps, gold standard for shoulder mobility\nNarrow grip over time to increase difficulty"),
                        ("Overhead Tricep Stretch", "Fold one arm behind head, press elbow with other hand\n20-30 sec each arm, stretches tricep long head"),
                        ("Wrist Flexor/Extensor Stretch", "Extend arm straight, pull fingers down/up to stretch wrist\n15-20 sec each direction, essential before lifting"),
                    }),
                    ("LOWER BODY FLEXIBILITY", new[]
                    {
                        ("Standing Forward Fold", "Stand with slightly bent knees, fold forward reaching for toes\n20-30 sec, opens hamstrings + lower back\nDeepen with each exhale"),
                        ("Seated Forward Fold", "Sit with legs straight, reach toward toes\n20-30 sec, foundational hamstring stretch\nTry to keep back flat"),
                        ("Pigeon Pose", "Front leg bent, back leg straight, lean forward\n30-45 sec each side, hip opener #1\nBreathe through resistance"),
                        ("90/90 Stretch", "On floor, front leg 90° external rotation, back leg 90° internal\n20-30 sec each side, hip mobility\nLean forward to increase intensity"),
                        ("Frog Stretch", "Knees wide on floor, push hips back\n20-30 sec, inner thigh (adductor) flexibility\nSlow and controlled — don't force"),
                        ("Butterfly Stretch", "Seated, soles of feet together, push knees toward floor\n20-30 sec, hip internal rotation + adductor\nUse elbows to gently press knees"),
                    }),
                    ("SPINE & BACK MOBILITY", new[]
                    {
                        ("Cat-Cow", "On all fours, round spine up (cat) → arch down (cow)\n10-15 reps, segmental spine mobility\nSync with breath: exhale=cat, inhale=cow"),
                        ("Child's Pose", "Knees on ground, sit back on heels, arms extended forward\n30-45 sec, stretches lats + spine + hips\nReach to one side for oblique stretch"),
                        ("Thread the Needle", "On all fours, thread one arm under the other, rotate torso\n8-10 reps each side, thoracic rotation\nTry to bring shoulder closer to floor"),
                        ("Scorpion Stretch", "Face down, bring one leg across to opposite side, rotating spine\n15-20 sec each side, thoracic + lumbar mobility\nTry to keep shoulders on the ground"),
                        ("Jefferson Curl", "Standing, no weight, roll spine down vertebra by vertebra from chin\n6-8 reps, spine flexion mobility\n⚠ Requires adequate flexibility before adding weight"),
                    }),
                    ("HIP & HAMSTRING FOCUSED", new[]
                    {
                        ("Deep Squat Hold", "Heels on ground, hold deep squat position\n30-60 sec, ankle + hip + back mobility\nPush knees out with elbows"),
                        ("Couch Stretch", "One knee against wall/couch, stretch front hip\n30-45 sec each side, hip flexor + quadriceps\nSqueeze glutes to increase intensity"),
                        ("Standing Quad Stretch", "Stand on one leg, pull other foot to glutes\n20-30 sec each leg, quadriceps flexibility\nKeep knees together, don't splay"),
                        ("Half Kneeling Hip Flexor", "One knee on ground, front leg at 90°, push hips forward\n20-30 sec each side, psoas + hip flexor\nRaise arms overhead to intensify"),
                        ("Reclined Hamstring Stretch", "On back, pull one leg up with band/towel\n20-30 sec each leg, safe hamstring stretch"),
                    }),
                    ("DYNAMIC WARM-UP ROUTINE (5-10 MIN)", new[]
                    {
                        ("1. Light Cardio (2 min)", "Walking in place or light jogging\nGradually raises heart rate and blood flow"),
                        ("2. Hip Circles + Leg Swings (1 min)", "Hip circles 10+10 → Leg swings front-back 10+10\nLubricates hip joints"),
                        ("3. Cat-Cow + Torso Twist (1 min)", "Cat-Cow 8 reps → Standing Torso Twist 10+10\nSpine mobility and core activation"),
                        ("4. Arm Circles + Dislocates (1 min)", "Arm circles 15+15 → Band dislocates 10 reps\nUpper body joint warm-up"),
                        ("5. Walking Lunges + High Knees (1-2 min)", "10 lunges → 20 high knees → 10 lunges\nFull body activation with wide ROM"),
                        ("6. Target Area Activation (1-2 min)", "2-3 warm-up exercises specific to today's workout\nLight sets or band work to activate target muscles"),
                    }),
                    ("POST-WORKOUT COOL-DOWN (10-15 MIN)", new[]
                    {
                        ("1. Slow Walking (2-3 min)", "Gradually lower heart rate\nBring down to 50-60% MHR"),
                        ("2. Static Stretch — Upper Body (3-4 min)", "Doorway Stretch: 30 sec\nOverhead Tricep: 20 sec × each arm\nArm Cross: 20 sec × each arm\nNeck Lateral: 15 sec × each side"),
                        ("3. Static Stretch — Lower Body (3-4 min)", "Standing Forward Fold: 30 sec\nPigeon Pose: 30 sec × each side\nStanding Quad Stretch: 20 sec × each leg\nWall Calf Stretch: 20 sec × each leg"),
                        ("4. Back & Core (2-3 min)", "Child's Pose: 30 sec\nCat-Cow: 8 reps\nSupine Twist: 20 sec × each side"),
                        ("5. Deep Breathing (1-2 min)", "Inhale through nose 4 sec → hold 4 sec → exhale through mouth 6 sec\nActivates parasympathetic system, accelerates recovery"),
                    }),
                    ("MOBILITY RULES & INFO", new[]
                    {
                        ("Static vs Dynamic", "Static: Hold stretch 20-45 sec → AFTER training\nDynamic: Movement-based warm-up → BEFORE training\nPre-workout static stretching can cause strength loss"),
                        ("Flexibility vs Mobility", "Flexibility: Passive lengthening capacity of muscle\nMobility: Active, controlled movement ability of joint\nMobility > flexibility, because control is also required"),
                        ("How Long to Hold?", "Minimum 20 sec, ideal 30-45 sec (ACSM recommendation)\nTarget 5+ min/muscle group per week\n3-4 weeks of consistent practice for noticeable progress"),
                        ("When to Do It?", "Dynamic warm-up: Before training (5-10 min)\nStatic stretching: After training or separate session\nMorning stretching: Lubricates joints, benefits all day"),
                        ("Pain vs Stretch", "Mild stretch sensation = CORRECT, continue\nPain / sharp feeling = WRONG, stop\nIf you can't breathe, you're pushing too hard"),
                        ("How to Progress?", "Add 1-2 sec hold each week\nIncrease ROM gradually — don't force\nConsistency > intensity; 5×5 min/week > 1×30 min"),
                    }),
                }),
                ("📅 Training Splits", new[]
                {
                    ("WHICH PROGRAM TO CHOOSE?", new[]
                    {
                        ("2-3 Days a Week", "Full Body is the most ideal. Maximum efficiency with limited time."),
                        ("4 Days a Week", "Upper/Lower or Bro Split. Upper/Lower is scientifically more balanced."),
                        ("5-6 Days a Week", "Push/Pull/Legs (PPL) or custom combinations. For advanced lifters."),
                    }),
                    ("FULL BODY", new[]
                    {
                        ("How it works", "All major muscle groups (chest, back, legs, shoulders) are trained every session."),
                        ("Who is it for?", "Beginners, those with limited time, lifting only 2-3 days a week."),
                        ("Pros (+)", "Frequent muscle protein synthesis (muscles stimulated often).\nTeaches systemic recovery.\nHighly time-efficient."),
                        ("Cons (-)", "As you get stronger, sessions can become very long and exhausting.\nHard to allocate enough specific volume to every muscle."),
                    }),
                    ("UPPER / LOWER", new[]
                    {
                        ("How it works", "Day 1: Upper Body (Chest+Back+Shoulders+Arms)\nDay 2: Lower Body (Quads+Hamstrings+Calves)\n(Usually done 4 days: Upper-Lower-Rest-Upper-Lower)"),
                        ("Who is it for?", "Intermediate to advanced, those who can lift 4 days a week."),
                        ("Pros (+)", "Excellent training frequency (each muscle 2x a week).\nProvides a great recovery balance for upper and lower body."),
                        ("Cons (-)", "Upper body days can sometimes take a long time due to the number of muscle groups involved."),
                    }),
                    ("PUSH / PULL / LEGS (PPL)", new[]
                    {
                        ("How it works", "Push (Chest, Shoulders, Triceps)\nPull (Back, Rear Delts, Biceps)\nLegs (Quads, Hamstrings, Calves)"),
                        ("Who is it for?", "Advanced athletes, those who can go to the gym 5-6 days a week."),
                        ("Pros (+)", "High volume potential.\nMuscles that work together share the same workout, allowing intense focus."),
                        ("Cons (-)", "Training 6 days a week can severely tax the central nervous system (CNS).\nRequires strict discipline and excellent nutrition."),
                    }),
                    ("BRO SPLIT (SINGLE MUSCLE)", new[]
                    {
                        ("How it works", "Only 1 or 2 muscles per day. (Mon: Chest, Tue: Back, Wed: Shoulders, Thu: Legs, Fri: Arms)"),
                        ("Who is it for?", "Old-school lifters, advanced athletes focusing heavily on weak parts."),
                        ("Pros (+)", "Maximum focus and damage to the specific muscle on that day.\nShorter workouts, very high 'pump' feeling."),
                        ("Cons (-)", "Scientifically sub-optimal. Muscle protein synthesis stops after 48 hours, then you wait a whole week to hit it again."),
                    }),
                    ("WEEKLY VOLUME & FREQUENCY", new[]
                    {
                        ("Beginner Level", "8-10 Sets per muscle group per week (e.g., 3 exercises x 3 sets per week)."),
                        ("Intermediate", "10-15 Sets per muscle group per week."),
                        ("Advanced", "15-20+ Sets per muscle group per week."),
                        ("Scientific Frequency Rule", "Training a muscle 2x a week (e.g., Mon 6 sets, Thu 6 sets) is proven to be SUPERIOR for muscle growth compared to training it 1x a week (e.g., Mon 12 sets)."),
                    }),
                    ("⚠ GOLDEN RULES (Discipline & Consistency)", new[]
                    {
                        ("Program Hopping Disease", "Do NOT switch to a new program you see on YouTube every week. You must stick to a program for at least 8-12 weeks for it to work."),
                        ("Discipline > A Perfect Program", "Even the best-written workout program in the world is garbage if you don't execute it with 100% effort and discipline. Consistency is king."),
                        ("Progressive Overload", "No matter what split you do, your goal should be to do 1 more rep or lift 1 more kg than last week."),
                        ("Nutrition is the Real Key", "You don't build muscle in the gym; you tear it down. Muscles grow in the Kitchen and in Bed (Sleep). If your diet is poor and lacking protein, no split will save you."),
                    }),
                }),
            };

            var sections = isEn ? sectionsEn : sectionsTr;

            // ── Menü butonları ve içerik oluştur ──
            System.Windows.Controls.Button firstBtn = null;

            void ShowSection(int idx)
            {
                contentPanel.Children.Clear();
                var (_, cats) = sections[idx];

                foreach (var (catTitle, rows) in cats)
                {
                    // Kategori başlığı
                    contentPanel.Children.Add(new System.Windows.Controls.Border
                    {
                        Background      = accentLt,
                        Padding         = new Thickness(16, 8, 16, 8),
                        Child           = new System.Windows.Controls.TextBlock
                        {
                            Text       = catTitle,
                            FontSize   = 10,
                            FontWeight = FontWeights.Bold,
                            Foreground = accent
                        }
                    });

                    bool odd2 = false;
                    foreach (var (left, right) in rows)
                    {
                        var rowBorder = new System.Windows.Controls.Border
                        {
                            Background = odd2
                                ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(15, 99, 102, 241))
                                : bgContent,
                            Padding = new Thickness(16, 7, 16, 7)
                        };
                        odd2 = !odd2;

                        var g = new System.Windows.Controls.Grid();
                        g.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        g.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        var tb1 = new System.Windows.Controls.TextBlock { Text = left,  FontSize = 12, Foreground = primary, TextWrapping = TextWrapping.Wrap };
                        var tb2 = new System.Windows.Controls.TextBlock { Text = right, FontSize = 12, Foreground = secondary, TextWrapping = TextWrapping.Wrap,
                                                                           HorizontalAlignment = HorizontalAlignment.Right, TextAlignment = TextAlignment.Right };
                        System.Windows.Controls.Grid.SetColumn(tb1, 0);
                        System.Windows.Controls.Grid.SetColumn(tb2, 1);
                        g.Children.Add(tb1); g.Children.Add(tb2);
                        rowBorder.Child = g;
                        contentPanel.Children.Add(rowBorder);
                    }
                }
            }

            for (int i = 0; i < sections.Length; i++)
            {
                var idx   = i;
                var (sectionName, _) = sections[i];

                var btn = new System.Windows.Controls.Button
                {
                    Content             = sectionName,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Background          = System.Windows.Media.Brushes.Transparent,
                    BorderThickness     = new Thickness(0),
                    Padding             = new Thickness(16, 10, 16, 10),
                    FontSize            = 12,
                    Foreground          = System.Windows.Media.Brushes.White,
                    Cursor              = System.Windows.Input.Cursors.Hand,
                    Tag                 = idx
                };

                // Hover & seçim rengi
                btn.Click += (s, _) =>
                {
                    foreach (System.Windows.UIElement child in menuPanel.Children)
                    {
                        if (child is System.Windows.Controls.Button mb)
                        {
                            mb.Background  = System.Windows.Media.Brushes.Transparent;
                            mb.Foreground  = System.Windows.Media.Brushes.White;
                            mb.FontWeight  = FontWeights.Normal;
                        }
                    }
                    var clicked = (System.Windows.Controls.Button)s;
                    clicked.Background = accentLt;
                    clicked.Foreground = accent;   // açık arka planda accent rengi okunur
                    clicked.FontWeight = FontWeights.SemiBold;
                    ShowSection((int)clicked.Tag);
                    contentScroll.ScrollToTop();
                };

                menuPanel.Children.Add(btn);
                if (firstBtn == null) firstBtn = btn;
            }

            menuBorder.Child = menuPanel;
            System.Windows.Controls.Grid.SetColumn(menuBorder,  0);
            System.Windows.Controls.Grid.SetColumn(contentBorder, 1);
            mainGrid.Children.Add(menuBorder);
            mainGrid.Children.Add(contentBorder);

            win.Content = mainGrid;

            // İlk bölümü göster
            firstBtn?.RaiseEvent(new System.Windows.RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

            win.ShowDialog();
        }

        // ─────────────────────────────────────────────────────────────
        // CALORIE BURN BUTTON — Egzersiz Kalori Tablosu
        // ─────────────────────────────────────────────────────────────
        private void CalorieBurnBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowCalorieBurnWindow();
        }

        private void ShowCalorieBurnWindow()
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            var win = new Window
            {
                Title                 = isEn ? "🏃 Exercise Calorie Breakdown" : "🏃 Egzersiz Kalori Tablosu",
                Width                 = 540,
                Height                = 600,
                ResizeMode            = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner                 = this,
                Background            = (System.Windows.Media.Brush)TryFindResource("ContentBackgroundBrush")
                                        ?? System.Windows.Media.Brushes.White,
                FontFamily            = new System.Windows.Media.FontFamily("Segoe UI")
            };

            var scroll = new System.Windows.Controls.ScrollViewer
            {
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto
            };

            var outer = new System.Windows.Controls.StackPanel();

            // Header
            var header = new System.Windows.Controls.Border
            {
                Background      = (System.Windows.Media.Brush)TryFindResource("HeaderBackgroundBrush")
                                  ?? System.Windows.Media.Brushes.WhiteSmoke,
                BorderBrush     = (System.Windows.Media.Brush)TryFindResource("BorderBrush")
                                  ?? System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding         = new Thickness(20, 16, 20, 16)
            };
            var headerStack = new System.Windows.Controls.StackPanel();
            headerStack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text         = isEn ? "🏃 Exercise Calorie Breakdown" : "🏃 Egzersiz Kalori Tablosu",
                FontSize     = 16,
                FontWeight   = FontWeights.Bold,
                Foreground   = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush")
                               ?? System.Windows.Media.Brushes.Black
            });
            headerStack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text         = isEn ? "Hourly calorie burn estimates for a 70 kg reference individual (MET Based)" : "70 kg referans kişi için saatlik kalori yakım değerleri (MET bazlı)",
                FontSize     = 11,
                Foreground   = (System.Windows.Media.Brush)TryFindResource("SecondaryTextBrush")
                               ?? System.Windows.Media.Brushes.Gray,
                Margin       = new Thickness(0, 4, 0, 0)
            });
            header.Child = headerStack;
            outer.Children.Add(header);

            // Kategoriler
            var categoriesTr = new[]
            {
                ("🏃 Kardiyo", new[]
                {
                    ("Koşmak (8 km/h)",       "562 kcal"),
                    ("Koşmak (10 km/h)",      "700 kcal"),
                    ("Koşmak (12 km/h)",      "840 kcal"),
                    ("Sprint (16+ km/h)",     "1120 kcal"),
                    ("Yürüyüş (4 km/h)",      "210 kcal"),
                    ("Yürüyüş (6 km/h)",      "315 kcal"),
                    ("Bisiklet (orta hız)",   "490 kcal"),
                    ("Bisiklet (hızlı)",      "700 kcal"),
                    ("İp atlama",             "700 kcal"),
                    ("HIIT antrenmanı",       "840 kcal"),
                }),
                ("🏋️ Güç & Ağırlık", new[]
                {
                    ("Ağırlık antrenmanı (hafif)",  "210 kcal"),
                    ("Ağırlık antrenmanı (orta)",   "315 kcal"),
                    ("Ağırlık antrenmanı (ağır)",   "420 kcal"),
                    ("Crossfit / Fonksiyonel",      "560 kcal"),
                    ("Kettlebell",                  "490 kcal"),
                    ("Vücut ağırlığı (calisthenics)","350 kcal"),
                }),
                ("🏊 Su Sporları", new[]
                {
                    ("Yüzmek (serbest, yavaş)",  "420 kcal"),
                    ("Yüzmek (serbest, hızlı)",  "630 kcal"),
                    ("Kurbağalama",              "490 kcal"),
                    ("Su topu",                  "490 kcal"),
                    ("Su aerobiği",              "280 kcal"),
                }),
                ("⚽ Takım / Raket Sporları", new[]
                {
                    ("Futbol",          "490 kcal"),
                    ("Basketbol",       "490 kcal"),
                    ("Voleybol",        "280 kcal"),
                    ("Tenis",           "420 kcal"),
                    ("Badminton",       "350 kcal"),
                    ("Squash",          "700 kcal"),
                }),
                ("🧘 Esneme & Zihin-Beden", new[]
                {
                    ("Yoga",             "200 kcal"),
                    ("Pilates",          "245 kcal"),
                    ("Tai Chi",          "175 kcal"),
                    ("Meditasyon",        "70 kcal"),
                }),
                ("🏠 Günlük Aktiviteler", new[]
                {
                    ("Ev işleri (genel)",   "175 kcal"),
                    ("Bahçe işleri",        "280 kcal"),
                    ("Merdiven çıkma",      "490 kcal"),
                    ("Dans",               "350 kcal"),
                    ("Alışveriş",           "140 kcal"),
                    ("Yemek pişirme",       "105 kcal"),
                    ("Ofis çalışması",       "80 kcal"),
                    ("Uyku",                "55 kcal"),
                }),
            };

            var categoriesEn = new[]
            {
                ("🏃 Cardio", new[]
                {
                    ("Running (8 km/h)",       "562 kcal"),
                    ("Running (10 km/h)",      "700 kcal"),
                    ("Running (12 km/h)",      "840 kcal"),
                    ("Sprinting (16+ km/h)",   "1120 kcal"),
                    ("Walking (4 km/h)",       "210 kcal"),
                    ("Walking (6 km/h)",       "315 kcal"),
                    ("Cycling (moderate)",     "490 kcal"),
                    ("Cycling (fast)",         "700 kcal"),
                    ("Jump rope",              "700 kcal"),
                    ("HIIT workout",           "840 kcal"),
                }),
                ("🏋️ Strength & Weights", new[]
                {
                    ("Weight training (light)",     "210 kcal"),
                    ("Weight training (moderate)",  "315 kcal"),
                    ("Weight training (heavy)",     "420 kcal"),
                    ("Crossfit / Functional",       "560 kcal"),
                    ("Kettlebell",                  "490 kcal"),
                    ("Bodyweight (calisthenics)",   "350 kcal"),
                }),
                ("🏊 Water Sports", new[]
                {
                    ("Swimming (freestyle, slow)",  "420 kcal"),
                    ("Swimming (freestyle, fast)",  "630 kcal"),
                    ("Breaststroke",                "490 kcal"),
                    ("Water polo",                  "490 kcal"),
                    ("Water aerobics",              "280 kcal"),
                }),
                ("⚽ Team / Racket Sports", new[]
                {
                    ("Football (Soccer)",      "490 kcal"),
                    ("Basketball",             "490 kcal"),
                    ("Volleyball",             "280 kcal"),
                    ("Tennis",                 "420 kcal"),
                    ("Badminton",              "350 kcal"),
                    ("Squash",                 "700 kcal"),
                }),
                ("🧘 Flexibility & Mind-Body", new[]
                {
                    ("Yoga",             "200 kcal"),
                    ("Pilates",          "245 kcal"),
                    ("Tai Chi",          "175 kcal"),
                    ("Meditation",       "70 kcal"),
                }),
                ("🏠 Daily Activities", new[]
                {
                    ("Housework (general)",   "175 kcal"),
                    ("Gardening",             "280 kcal"),
                    ("Stair climbing",        "490 kcal"),
                    ("Dancing",               "350 kcal"),
                    ("Shopping",              "140 kcal"),
                    ("Cooking",               "105 kcal"),
                    ("Office work",           "80 kcal"),
                    ("Sleeping",              "55 kcal"),
                }),
            };
            
            var categories = isEn ? categoriesEn : categoriesTr;

            var accent   = (System.Windows.Media.Brush)TryFindResource("AccentBrush")      ?? System.Windows.Media.Brushes.DodgerBlue;
            var accentLt = (System.Windows.Media.Brush)TryFindResource("AccentLightBrush") ?? System.Windows.Media.Brushes.AliceBlue;
            var primary  = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush") ?? System.Windows.Media.Brushes.Black;
            var secondary= (System.Windows.Media.Brush)TryFindResource("SecondaryTextBrush")??System.Windows.Media.Brushes.Gray;
            var border   = (System.Windows.Media.Brush)TryFindResource("BorderBrush")      ?? System.Windows.Media.Brushes.LightGray;
            var content  = (System.Windows.Media.Brush)TryFindResource("ContentBackgroundBrush") ?? System.Windows.Media.Brushes.White;

            bool odd = false;
            foreach (var (catName, items) in categories)
            {
                // Kategori başlığı
                var catHeader = new System.Windows.Controls.Border
                {
                    Background = accentLt,
                    Padding    = new Thickness(20, 8, 20, 8),
                    Margin     = new Thickness(0, 0, 0, 0)
                };
                catHeader.Child = new System.Windows.Controls.TextBlock
                {
                    Text       = catName,
                    FontSize   = 12,
                    FontWeight = FontWeights.Bold,
                    Foreground = accent
                };
                outer.Children.Add(catHeader);

                // Satırlar
                foreach (var (activity, kcal) in items)
                {
                    var row = new System.Windows.Controls.Border
                    {
                        Background = odd
                            ? new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 99, 102, 241))
                            : content,
                        Padding    = new Thickness(20, 7, 20, 7)
                    };
                    odd = !odd;

                    var grid = new System.Windows.Controls.Grid();
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    grid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = GridLength.Auto });

                    var actText = new System.Windows.Controls.TextBlock
                    {
                        Text      = activity,
                        FontSize  = 12,
                        Foreground= primary
                    };
                    var kcalText = new System.Windows.Controls.TextBlock
                    {
                        Text      = kcal,
                        FontSize  = 12,
                        FontWeight= FontWeights.SemiBold,
                        Foreground= accent,
                        HorizontalAlignment = HorizontalAlignment.Right
                    };
                    System.Windows.Controls.Grid.SetColumn(actText,  0);
                    System.Windows.Controls.Grid.SetColumn(kcalText, 1);
                    grid.Children.Add(actText);
                    grid.Children.Add(kcalText);

                    row.Child = grid;
                    outer.Children.Add(row);
                }
            }

            // Footer notu
            outer.Children.Add(new System.Windows.Controls.Border
            {
                Background      = accentLt,
                BorderBrush     = border,
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding         = new Thickness(20, 12, 20, 12),
                Child           = new System.Windows.Controls.TextBlock
                {
                    Text        = isEn ? "💡 Values are estimates for a 70 kg individual. Actual burn depends on weight, muscle mass, and intensity.\n    Formula for you: Burn = (Table Value ÷ 70) × Your Weight" : "💡 Değerler 70 kg kişi için tahminidir. Gerçek yakım; ağırlık, kas oranı, kondisyon ve yoğunluğa göre değişir.\n    Kendi kilonuz için: Yakım = (Tablodaki değer ÷ 70) × Kilonuz",
                    FontSize    = 11,
                    TextWrapping= TextWrapping.Wrap,
                    Foreground  = secondary,
                    LineHeight  = 18
                }
            });

            scroll.Content = outer;
            win.Content    = scroll;
            win.ShowDialog();
        }

        // ─────────────────────────────────────────────────────────────
        // INFO BUTTON — Ölçüm Rehberi
        // ─────────────────────────────────────────────────────────────
        private void InfoBtn_Click(object sender, RoutedEventArgs e)
        {
            int tab = MainTabs.SelectedIndex;

            string title;
            string[] tips;

            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            switch (tab)
            {
                case 0: // RIR
                    title = isEn ? "💪 RIR — How to use?" : "💪 RIR — Nasıl Kullanılır?";
                    tips = isEn ? new[]
                    {
                        "Working weight: Enter the actual load in kg you used for the set.",
                        "Reps performed: Enter how many repetitions you completed in that set.",
                        "RIR (Reps in Reserve): How many more reps could you have done at the end of the set?\n     0 RIR = max effort (no more reps possible)\n     2 RIR = you had 2 more reps in the tank",
                        "The result calculates your estimated 1 Rep Max (1RM) and recommended training loads.",
                        "Tip: Actual 1RM tests are fatiguing and risky; RIR-based estimations are safer."
                    } : new[]
                    {
                        "Çalışma ağırlığı: Sette kullandığınız gerçek yükü (kg) girin.",
                        "Yapılan tekrar: Sette tamamladığınız tekrar sayısını girin.",
                        "RIR (Rezervdeki tekrar): Setinizi bitirdiğinizde kaç tekrar daha yapabilirdiniz?\n     0 RIR = maksimal efor (başka tekrar imkânsız)\n     2 RIR = 2 tekrar daha yapabilirdiniz",
                        "Sonuç olarak tahmini 1 tekrar maksimumunuz (1RM) ve önerilen antrenman yükleri hesaplanır.",
                        "İpucu: 1RM testleri yorucu ve risklidir; RIR tabanlı tahmin daha güvenlidir."
                    };
                    break;

                case 1: // Vücut Yağı
                    title = isEn ? "🔥 Body Fat — Measurement Guide" : "🔥 Vücut Yağı — Ölçüm Rehberi";
                    tips = isEn ? new[]
                    {
                        "Height: Measure during the day (preferably morning) without shoes, standing straight.",
                        "Neck circumference: Measure horizontally just below the Adam's apple (larynx), head looking straight. The tape should rest on the skin but not squeeze.",
                        "Waist (Men): Measure horizontally at the level of the navel (belly button) after exhaling.",
                        "Waist (Women): Measure at the narrowest point between the bottom of the ribs and the hip bone.",
                        "Hip (Women only): Measure around the widest portion of the buttocks, keeping the tape horizontal.",
                        "Measuring in the morning, before eating, under the same conditions provides consistency.",
                        "Note: The Navy method is an estimation; hydrostatic weighing or DEXA scans are more accurate."
                    } : new[]
                    {
                        "Boy: Gün içinde (tercihen sabah) ayakkabısız, dik duruşta ölçün.",
                        "Boyun çevresi: Adem elmasının (larynx) hemen altından, baş dik ve ileriye bakarken yatay olarak ölçün. Şerit cilde tam oturmalı ancak sıkmamalı.",
                        "Bel çevresi (Erkek): Göbek deliği hizasından, nefes verdikten sonra ölçün.",
                        "Bel çevresi (Kadın): Kaburgaların en alt noktası ile kalça kemiği arasındaki en ince yerden ölçün.",
                        "Kalça çevresi (yalnızca Kadın): Kalçanın en geniş ve yuvarlak noktasından, şeridi yatay tutarak ölçün.",
                        "Sabah, yemekten önce, aynı koşullarda ölçmek tutarlılık sağlar.",
                        "Not: Navy metodu tahmindir; hidrosatik tartım veya DEXA daha hassastır."
                    };
                    break;

                case 2: // Kalori / BMR
                    title = isEn ? "🍽 Calories — Data Guide" : "🍽 Kalori — Ölçüm & Veri Rehberi";
                    tips = isEn ? new[]
                    {
                        "Weight: Weigh yourself in the morning, after using the restroom, on an empty stomach. Take weekly averages.",
                        "Height: Measure without shoes, standing straight, at a consistent time of day.",
                        "Age: Enter your exact age in years.",
                        "Be honest about your activity level — most people overestimate it.\n     Sedentary = desk job, no active exercise\n     Lightly active = 1–3 hrs per week light exercise\n     Moderately active = 3–5 hrs per week exercise\n     Very active = 6–7 hrs per week heavy training\n     Extra active = heavy daily training + physical job",
                        "The TDEE result is your total daily energy expenditure; adjust your calories based on your goals."
                    } : new[]
                    {
                        "Ağırlık: Sabah, tuvaletten sonra, aç karnına tartının. Haftalık ortalama alın.",
                        "Boy: Ayakkabısız, dik duruşta, gün içinde sabit bir saatte ölçün.",
                        "Yaş: Tam yaşınızı girin.",
                        "Aktivite seviyesi için dürüst olun — çoğu kişi aktivitesini yüksek tahmin eder.\n     Hareketsiz = masa başı iş, aktif spor yok\n     Az aktif   = haftada 1–3 saat hafif egzersiz\n     Orta aktif = haftada 3–5 saat egzersiz\n     Çok aktif  = haftada 6–7 saat yoğun antrenman\n     Ekstra     = günlük ağır antrenman + fiziksel iş",
                        "TDEE sonucu günlük toplam harcamanızdır; hedefinizte buna göre kalori ekleyin/çıkarın."
                    };
                    break;

                case 3: // Makro
                    title = isEn ? "🥦 Macros — How to use?" : "🥦 Makro — Nasıl Kullanılır?";
                    tips = isEn ? new[]
                    {
                        "Daily Calorie Goal: Enter the TDEE you calculated in the previous (Calories) tab.",
                        "Weight: Enter your weight, measured in the morning on an empty stomach.",
                        "Goal:\n     Fat Loss (Cutting) → calorie deficit + high protein\n     Maintenance → balanced diet\n     Muscle Gain (Bulking) → calorie surplus + high carbs",
                        "Activity/Program:\n     Lightly active = cardio-focused\n     Moderately active = mixed training\n     Very active = heavy lifting focused",
                        "The higher your protein intake, the better muscle retention you'll have, especially during fat loss phases.",
                        "These values are starting points; update them every 2–3 weeks based on your body weight changes."
                    } : new[]
                    {
                        "Günlük kalori hedefi: Önceki sekmede (Kalori) hesapladığınız TDEE'yi girin.",
                        "Kilo: Sabah aç karnına ölçülen ağırlığınızı girin.",
                        "Hedef:\n     Yağ Yakma → kalori açığı + yüksek protein\n     İdame     → denge\n     Kas Kazanma → kalori fazlası + yüksek karbonhidrat",
                        "Aktivite/program:\n     Az aktif   = kardiyo ağırlıklı\n     Orta aktif = karma antrenman\n     Çok aktif  = ağırlık antrenmanı",
                        "Protein dozu ne kadar yüksek kalırsa kas koruması o kadar iyi olur, özellikle yağ yakma döneminde.",
                        "Bu değerler başlangıç noktasıdır; 2–3 haftada bir vücut ağırlığı değişiminize göre güncelleyin."
                    };
                    break;

                case 4: // VKİ
                    title = isEn ? "⚖️ BMI — Measurement Guide" : "⚖️ VKİ — Ölçüm Rehberi";
                    tips = isEn ? new[]
                    {
                        "Weight: Weigh yourself wearing light clothing, in the morning on an empty stomach; weekly averages are most reliable.",
                        "Height: Your morning height may be 1–2 cm taller than in the evening. Pick a consistent time.",
                        "BMI does not account for muscle mass — it can be misleading for muscular or highly active individuals.",
                        "WHO Classification:\n     < 18.5 → Underweight\n     18.5–24.9 → Normal weight\n     25–29.9 → Overweight\n     30–34.9 → Obesity Class I\n     ≥ 35    → Obesity Class II+",
                        "It's highly recommended to evaluate BMI alongside body fat measurements or waist-to-hip ratios."
                    } : new[]
                    {
                        "Ağırlık: Sabah aç karnına, hafif giysilerle tartının; haftalık ortalama güvenilirdir.",
                        "Boy: Sabah ölçülen boy, akşama göre 1–2 cm daha fazla olabilir. Tutarlı bir saat seçin.",
                        "VKİ kas kütlesini dikkate almaz — sportif veya kas kütlesi yüksek kişilerde yanıltıcı olabilir.",
                        "WHO Sınıflandırması:\n     < 18.5 → Zayıf\n     18.5–24.9 → Normal\n     25–29.9 → Fazla Kilolu\n     30–34.9 → Obez Sınıf I\n     ≥ 35    → Obez Sınıf II+",
                        "Bel/kalça oranı veya vücut yağ ölçümü ile birlikte değerlendirilmesi önerilir."
                    };
                    break;

                case 5: // Kilo Hedefi
                    title = isEn ? "🎯 Weight Goal — How to use?" : "🎯 Kilo Hedefi — Nasıl Kullanılır?";
                    tips = isEn ? new[]
                    {
                        "Current weight: Enter your current weight (measured morning, empty stomach).",
                        "Target weight: Enter the weight you want to reach.",
                        "Duration (days): In how many days do you want to reach your goal?",
                        "Healthy fat loss rate: 0.5–0.75 kg per week (max 1 kg).",
                        "Realistic muscle gain: 0.1–0.25 kg per week (lean bulk).",
                        "Extremely rapid weight changes can lead to muscle loss, metabolic slowing, and health issues.",
                        "Activity Level: More active individuals can achieve faster changes, but biological limits apply."
                    } : new[]
                    {
                        "Mevcut kilo: Sabah aç karnına ölçülen güncel kilonuzu girin.",
                        "Hedef kilo: Ulaşmak istediğiniz kiloyu girin.",
                        "Süre: Hedefinize kaç gün içinde ulaşmak istiyorsunuz?",
                        "Sağlıklı yağ kaybı hızı: haftada 0.5–0.75 kg (max 1 kg).",
                        "Kas kütlesi kazanımı: haftada 0.1–0.25 kg (temiz bulk) gerçekçidir.",
                        "Çok hızlı kilo değişimi kas kaybına, metabolizma yavaşlamasına ve sağlık sorunlarına yol açabilir.",
                        "Aktivite seviyesi: daha aktif biri daha hızlı değişim sağlayabilir, ancak sınırlar biyolojiktir."
                    };
                    break;

                case 6: // V-Vücut
                    title = isEn ? "🏛 V-Taper — Measurement Guide" : "🏛 V-Vücut — Ölçüm Rehberi";
                    tips = isEn ? new[]
                    {
                        "Height & Weight: Measure in standard morning conditions.",
                        "Shoulder Circumference (widest point):\n     While your arms hang loosely at your sides, wrap the tape around the widest, outermost part of both shoulders (deltoids).\n     The tape must remain strictly horizontal and fit snugly against the skin.",
                        "Waist Circumference (Men & Women):\n     Men → Measure at navel level, after exhaling naturally.\n     Women → Measure at the narrowest point between the bottom ribs and hips.",
                        "Having a friend measure your shoulders yields far more reliable results.",
                        "SHR (Shoulder-to-Hip Ratio): ≥1.618 for men and ≥1.40 for women are considered ideal approximations.",
                        "Height-based ideal measurements are for reference only; genetics and bone structure are significant factors."
                    } : new[]
                    {
                        "Boy & Kilo: Sabah aç karnına standart koşullarda ölçün.",
                        "Omuz çevresi (en geniş nokta):\n     Kollarınız yanlarda serbestçe sarkarken, şeridi her iki omuz kasının (deltoid) en geniş (dışa taşan) noktasından geçirin.\n     Şerit yere paralel (yatay) olmalı ve cilde tam oturmalıdır.",
                        "Bel çevresi (erkek & kadın):\n     Erkek → Göbek deliği hizasından, nefes verdikten sonra ölçün.\n     Kadın → Kaburgaların altı ile kalça arasındaki en ince noktadan ölçün.",
                        "Omuz ölçümünü arkadaşınıza yaptırmak daha güvenilir bir sonuç verir.",
                        "SHR (Shoulder-to-Hip Ratio): Erkekte ≥1.618, kadında ≥1.40 ideal kabul edilir.",
                        "Boy bazlı ideal ölçüler referans niteliğindedir; genetik ve kemik yapısı belirleyicidir."
                    };
                    break;

                case 7: // Su
                    title = isEn ? "💧 Hydration — How to use?" : "💧 Su İhtiyacı — Nasıl Kullanılır?";
                    tips = isEn ? new[]
                    {
                        "Weight: Keep weighing in the morning; daily fluctuations are largely due to water weight.",
                        "Age: Enter your age. Individuals over 55 have a reduced thirst reflex, so needs are adjusted.",
                        "Activity Level:\n     Sedentary = desk job\n     Lightly active = 1–3 mild exercises/wk\n     Moderately active = 3–5 exercises/wk\n     Very active = 6–7 intense trainings/wk\n     Extra active = heavy physical labor / athlete",
                        "Climate: Sweating increases in hot or humid environments, raising water needs.",
                        "If your urine is light yellow (like pale lemonade), you are adequately hydrated.",
                        "Tea, coffee, milk, and juices also count as fluid intake, but caffeinated drinks have a mild diuretic effect."
                    } : new[]
                    {
                        "Ağırlık: Sabah aç karnına tartının; gün içindeki değişimler sıvı ağırlığından kaynaklanabilir.",
                        "Yaş: Tam yaşınızı girin. 55 yaş üstü bireylerde susama hissi azalır, bu nedenle ihtiyaç düzeltilir.",
                        "Aktivite seviyesi:\n     Hareketsiz = masa başı iş\n     Az aktif   = haftada 1–3 hafif egzersiz\n     Orta aktif = haftada 3–5 egzersiz\n     Çok aktif  = haftada 6–7 yoğun antrenman\n     Ekstra     = ağır fiziksel iş / profesyonel sporcu",
                        "İklim: Sıcak veya nemli ortamlarda terleme artar, su ihtiyacı yükselir.",
                        "İdrar rengi açık sarı (limon suyu rengi) ise yeterli sıvı alıyorsunuz demektir.",
                        "Çay, kahve, ayran ve meyve suyu da sıvı sayılır, ancak kafein içecekleri hafif diüretik etkiye sahiptir."
                    };
                    break;

                default:
                    title = isEn ? "ℹ️ Information" : "ℹ️ Bilgi";
                    tips = isEn ? new[] { "No guide available for this tab." } : new[] { "Bu sekme için rehber bulunmamaktadır." };
                    break;
            }

            ShowInfoPopup(sender as System.Windows.UIElement, title, tips);
        }

        private void ShowInfoPopup(System.Windows.UIElement target, string title, string[] tips)
        {
            var win = new Window
            {
                Title                 = title,
                Width                 = 480,
                SizeToContent         = SizeToContent.Height,
                ResizeMode            = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner                 = this,
                Background            = (System.Windows.Media.Brush)TryFindResource("ContentBackgroundBrush")
                                        ?? System.Windows.Media.Brushes.White,
                FontFamily            = new System.Windows.Media.FontFamily("Segoe UI")
            };

            var outer = new System.Windows.Controls.StackPanel { Margin = new Thickness(0) };

            // Header
            var header = new System.Windows.Controls.Border
            {
                Background      = (System.Windows.Media.Brush)TryFindResource("HeaderBackgroundBrush")
                                  ?? System.Windows.Media.Brushes.WhiteSmoke,
                BorderBrush     = (System.Windows.Media.Brush)TryFindResource("BorderBrush")
                                  ?? System.Windows.Media.Brushes.LightGray,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding         = new Thickness(20, 16, 20, 16)
            };
            header.Child = new System.Windows.Controls.TextBlock
            {
                Text       = title,
                FontSize   = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush")
                             ?? System.Windows.Media.Brushes.Black,
                TextWrapping = TextWrapping.Wrap
            };
            outer.Children.Add(header);

            // Tips list
            var listPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20, 14, 20, 20) };
            for (int i = 0; i < tips.Length; i++)
            {
                var row = new System.Windows.Controls.Grid { Margin = new Thickness(0, 0, 0, 10) };
                row.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(22) });
                row.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Bullet
                var bullet = new System.Windows.Controls.Border
                {
                    Width           = 20,
                    Height          = 20,
                    CornerRadius    = new CornerRadius(10, 10, 10, 10),
                    Background      = (System.Windows.Media.Brush)TryFindResource("AccentBrush")
                                      ?? System.Windows.Media.Brushes.DodgerBlue,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin          = new Thickness(0, 1, 0, 0)
                };
                bullet.Child = new System.Windows.Controls.TextBlock
                {
                    Text                = (i + 1).ToString(),
                    FontSize            = 10,
                    FontWeight          = FontWeights.Bold,
                    Foreground          = System.Windows.Media.Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center
                };
                System.Windows.Controls.Grid.SetColumn(bullet, 0);
                row.Children.Add(bullet);

                var tipText = new System.Windows.Controls.TextBlock
                {
                    Text        = tips[i],
                    FontSize    = 12,
                    TextWrapping = TextWrapping.Wrap,
                    Foreground  = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush")
                                  ?? System.Windows.Media.Brushes.Black,
                    Margin      = new Thickness(10, 0, 0, 0),
                    LineHeight  = 18
                };
                System.Windows.Controls.Grid.SetColumn(tipText, 1);
                row.Children.Add(tipText);

                listPanel.Children.Add(row);
            }
            outer.Children.Add(listPanel);

            win.Content = outer;
            win.ShowDialog();
        }

        // ─────────────────────────────────────────────────────────────
        // ZİHİN REHBERİ
        // ─────────────────────────────────────────────────────────────
        private void MindGuideBtn_Click(object sender, RoutedEventArgs e)
        {
            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            var win = new Window
            {
                Title                 = isEn ? "🧠 Mind Development Guide" : "🧠 Zihin Geliştirme Rehberi",
                Width                 = 680,
                Height                = 600,
                ResizeMode            = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner                 = this,
                Background            = (System.Windows.Media.Brush)TryFindResource("ContentBackgroundBrush") ?? System.Windows.Media.Brushes.White,
                FontFamily            = new System.Windows.Media.FontFamily("Segoe UI")
            };

            var scroll = new System.Windows.Controls.ScrollViewer { Padding = new Thickness(24, 18, 24, 18), VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto };
            var stack = new System.Windows.Controls.StackPanel();

            var header = new System.Windows.Controls.TextBlock { Text = isEn ? "🧠  Mind Development Guide" : "🧠  Zihin Geliştirme Rehberi", FontSize = 16, FontWeight = FontWeights.Bold, Foreground = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush") };
            var desc = new System.Windows.Controls.TextBlock { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 16), FontSize = 12, Foreground = (System.Windows.Media.Brush)TryFindResource("SecondaryTextBrush"), Text = isEn ? "Science-based mental development activities, effective routine building methods, discipline, and motivation strategies." : "Bilimsel araştırmalara dayalı zihinsel gelişim aktiviteleri, etkili program oluşturma yöntemleri, disiplin ve motivasyon stratejileri." };

            stack.Children.Add(header);
            stack.Children.Add(desc);

            string[] sectionsTr = {
                "🎯  ZİHİNSEL GELİŞİM AKTİVİTELERİ|• 📖 Okuma — Günde 20-30 dk okuma, prefrontal korteksi güçlendirir ve empati yeteneğini artırır (Berns 2013, Emory Üniversitesi).\n\n• 🧘 Meditasyon — 8 haftalık düzenli meditasyon, amigdala kalınlığını azaltır (stres↓) ve gri maddeyi artırır (Hölzel 2011, Harvard).\n\n• 🧩 Bulmaca / Sudoku / Satranç — Çalışma belleği ve problem çözme becerisi↑. Düzenli satranç IQ artışıyla ilişkilendirilmiyor ancak planlama becerisini güçlendirir.\n\n• 🌍 Yeni Dil Öğrenme — İki dilli bireyler Alzheimer belirtilerini 4-5 yıl geciktirir (Bialystok 2012). Dorsolateral prefrontal korteks güçlenir.\n\n• 🎵 Müzik Aleti Çalma — Corpus callosum kalınlaşır, iki beyin yarım küresi arasındaki iletişim artar. Ritim çalışmaları çalışma belleğini güçlendirir.\n\n• 🏃 Fiziksel Egzersiz — BDNF (Brain-Derived Neurotrophic Factor) salgısını artırır. Hipokampüs hacmi ↑ → bellek ve öğrenme↑ (Erickson 2011, Pittsburgh Üniversitesi).\n\n• ✍️ Yazı Yazma (Günlük tutma) — Reflektif yazma, duygusal işlemleme ve öz-farkındalığı geliştirir (Pennebaker 1997). Planlama yazıları prefrontal korteksi aktive eder.\n\n• 🎨 Yeni Beceri Öğrenme — Jonglörlük, çizim, kodlama gibi yeni beceriler nöroplastisite tetikler. Beyaz madde artışı 6 haftada ölçülebilir (Scholz 2009, Oxford).",
                "📋  ETKİLİ PROGRAM NASIL OLUŞTURULUR?|1. SMART Hedef Belirle\n   • Specific (Belirli): 'Daha fazla oku' yerine 'Günde 25 sayfa oku'\n   • Measurable (Ölçülebilir): İlerlemeyi takip et\n   • Achievable (Ulaşılabilir): Gerçekçi ol\n   • Relevant (İlgili): Büyük hedefinle bağlantılı\n   • Time-bound (Zamanlı): Bitiş tarihi koy\n\n2. Habit Stacking (Alışkanlık Yığma) — James Clear, Atomic Habits\n   'Mevcut alışkanlıktan SONRA yeni alışkanlığı yap'\n   Örnek: 'Kahvemi aldıktan sonra 10 dk kitap okuyacağım'\n\n3. Haftalık Yapı Oluştur\n   • Hafta başı: Hedefleri gözden geçir\n   • Her gün: Aynı saatte aynı aktivite (sirkadiyen ritim)\n   • Hafta sonu: Değerlendirme ve ayarlama\n\n4. 2 Dakika Kuralı (David Allen, GTD)\n   'Bir görevi yapmak 2 dk'dan az sürüyorsa, hemen yap.'\n   Daha büyük görevler için: 'Sadece ilk 2 dk'sı ile başla'\n\n5. Time-Blocking (Zaman Bloklama — Cal Newport)\n   Güne başlamadan tüm saatleri planla. Boş slot bırakma.\n   Derin çalışma blokları = kesintisiz 90 dk odaklanma.",
                "🔒  DİSİPLİN — BİLİMSEL TEMELLERİ|🧪 Nöroplastisite ve Alışkanlık\n   Bir davranış tekrarlandıkça sinaptik bağlantılar güçlenir (Hebb kuralı: 'neurons that fire together wire together'). 66 gün sürekli tekrar → otomatik alışkanlık (Lally 2010, UCL). 21 gün efsanesi bilimsel değildir.\n\n🎯 Implementation Intentions (Gollwitzer 1999)\n   'NE ZAMAN X olursa, Y yapacağım' formatında plan\n   Hedeflere ulaşma oranını %2-3 kat artırır.\n   Örnek: 'Saat 07:00 olduğunda 15 dk meditasyon yapacağım'\n\n🍬 Temptation Bundling (Milkman 2014, UPenn)\n   Zor/sıkıcı aktiviteyi sevdiğin bir şeyle eşleştir.\n   Örnek: 'Sadece koşu bandında podcast dinleyeceğim'\n\n🧊 İrade (Willpower) — Sınırlı Kaynak mı?\n   Baumeister'ın 'ego depletion' teorisi tartışmalı (replikasyon krizi).\n   Yeni görüş: İrade sınırlı değil AMA motivasyon azalıyor.\n   Çözüm: Karar yorgunluğunu azalt → rutin oluştur, seçenek sayısını düşür.\n\n📉 Failure Planning (Başarısızlık Planı)\n   'Eğer programı 1 gün aksatırsam, ertesi gün kesinlikle devam edeceğim.'\n   Araştırmalar: 1 gün aksatmak zararsız, 2+ gün art arda → %90 bırakma riski.",
                "🔥  MOTİVASYON BİLİMİ|🧠 Dopamin Sistemi (Huberman, Stanford)\n   Dopamin bir 'ödül' hormonu değil, 'arzu ve takip' hormonudur.\n   Süreci sevmek, sonuca odaklanmaktan daha sürdürülebilir bir dopamin salgısı sağlar. 'Şu an çalışmayı/yorulmayı bilerek seçiyorum' zihniyeti dopamin temelini (baseline) yükseltir.\n\n🏆 Öz-Belirleme Kuramı (Deci & Ryan)\n   İçsel motivasyon 3 şeye dayanır:\n   1) Özerklik: Kendi seçtiğini hissetme\n   2) Yetkinlik: Geliştiğini görme\n   3) İlişkisellik: Bir amaca veya gruba bağlı olma\n   Dışsal ödüller (para, not, beğeni) uzun vadede içsel motivasyonu yok eder (Overjustification effect).\n\n📈 Progress Principle (Amabile, Harvard)\n   Motivasyonu artırmanın en etkili yolu, her gün kaydedilen 'küçük ilerlemelerdir'.\n   Gelişimi günlük olarak işaretlemek veya görselleştirmek beyne dopamin salgılatır ve döngüyü besler.\n\n🏆 İçsel vs Dışsal Motivasyon (Deci & Ryan, SDT)\n   3 temel ihtiyaç: Yetkinlik, Özerklik, Aitlik\n   • Yetkinlik: 'Bunu yapabiliyorum' hissi → zorluğu kademeli artır\n   • Özerklik: Kendi seçimin olması → dayatma motivasyonu öldürür\n   • Aitlik: Topluluk/grup desteği → hesap verebilirlik ortağı bul\n\n⏱ 5-Second Rule (Mel Robbins)\n   Bir şeyi yapmak istediğinde 5'ten geriye say ve HEMEN başla.\n   Beyin 5 sn sonra 'yapma' bahanesi üretmeye başlar.",
                "⚡  BİLİŞSEL PERFORMANSI ARTIRMAK|😴 Uyku — Matthew Walker, 'Why We Sleep'\n   • REM uykusu: yaratıcılık ve duygusal işlemleme\n   • Derin uyku (NREM 3): bellek konsolidasyonu\n   • 7 saatten az uyku → bilişsel performans %30↓\n\n🏃 Egzersiz ve BDNF\n   • Aerobik egzersiz (koşu, yüzme): BDNF↑ → yeni sinir hücresi büyümesi\n   • Haftada 3-5×30dk orta-yoğun egzersiz yeterli\n   • Egzersiz sonrası 2 saat = öğrenme için altın pencere\n\n🥗 Beslenme\n   • Omega-3 (DHA): sinir zarı yapısı, sinaptik iletişim\n   • Flavonoidler (yaban mersini, bitter çikolata): kan-beyin bariyeri geçer, nöroinflamasyonu azaltır\n   • Kreatin: sadece kas değil, beyin ATP'si için de etkili (Rae 2003 — IQ +5-15 puan)\n   • Kafein: adenozin reseptörleri bloke → uyanıklık↑; tolerans gelişir, stratejik kullan\n\n🧠 Pomodoro + Derin Çalışma Hibrit\n   • 90 dk odaklanma → 20 dk mola (ultradian rhythm)\n   • Yeni başlayanlar: 25 dk odak + 5 dk mola (Pomodoro)\n   • Mola sırasında: yürüyüş, doğaya bak, ekran yok"
            };

            string[] sectionsEn = {
                "🎯  MENTAL DEVELOPMENT ACTIVITIES|• 📖 Reading — Reading 20-30 mins daily strengthens the prefrontal cortex and increases empathy (Berns 2013).\n\n• 🧘 Meditation — 8-week structured meditation decreases amygdala thickness (less stress) and increases grey matter (Harvard).\n\n• 🧩 Puzzles / Chess — Increases working memory. Chess doesn't directly increase IQ but heavily strengthens planning skills.\n\n• 🌍 Learning a New Language — Bilinguals delay Alzheimer's symptoms by 4-5 years. Strengthens the dorsolateral prefrontal cortex.\n\n• 🎵 Playing an Instrument — Thickens the corpus callosum, increasing communication between brain hemispheres.\n\n• 🏃 Physical Exercise — Increases BDNF (Brain-Derived Neurotrophic Factor) secretion. Hippocampus volume↑ → learning↑.\n\n• ✍️ Journaling — Reflective writing develops emotional processing. Planning journals activate the prefrontal cortex.\n\n• 🎨 Learning New Skills — Juggling or drawing triggers neuroplasticity. White matter growth measurable in 6 weeks.",
                "📋  HOW TO BUILD AN EFFECTIVE ROUTINE?|1. Set SMART Goals\n   • Specific: 'Read 25 pages' instead of 'Read more'\n   • Measurable: Track progress\n   • Achievable: Be realistic\n   • Relevant: Connect to bigger goals\n   • Time-bound: Set a deadline\n\n2. Habit Stacking (James Clear)\n   'AFTER I do [current habit], I will do [new habit]'\n\n3. Create a Weekly Structure\n   • Monday: Review goals\n   • Daily: Same activity at same time (circadian rhythm)\n   • Weekend: Review and adjust\n\n4. 2-Minute Rule (David Allen)\n   'If a task takes less than 2 minutes, do it immediately.'\n\n5. Time-Blocking (Cal Newport)\n   Plan every block of your day before it starts. Focus on deep work blocks.",
                "🔒  DISCIPLINE — SCIENTIFIC FOUNDATIONS|🧪 Neuroplasticity & Habits\n   Neurons that fire together wire together. 66 days to form an automatic habit (Lally 2010), not 21 days.\n\n🎯 Implementation Intentions\n   'WHEN X happens, I will do Y'. Increases success by 2-3x.\n\n🍬 Temptation Bundling\n   Pair a hard activity with something you love.\n\n🧊 Willpower\n   Willpower isn't strictly limited, but motivation fades. Avoid decision fatigue by creating routines.\n\n📉 Failure Planning\n   'If I miss 1 day, I will immediately resume the next day.' Missing 1 day is harmless, but missing 2+ days risks a 90% quitting probability.",
                "🔥  THE SCIENCE OF MOTIVATION|🧠 The Dopamine System (Huberman)\n   Dopamine is a molecule of craving, not reward. Loving the process creates a sustainable dopamine baseline.\n\n🏆 Self-Determination Theory (Deci & Ryan)\n   Intrinsic motivation relies on:\n   1) Autonomy: Feeling in control\n   2) Competence: Seeing improvement\n   3) Relatedness: Connection to a purpose\n\n📈 Progress Principle (Amabile)\n   The most effective way to boost motivation is recording 'small daily wins'.\n\n⏱ The 5-Second Rule (Mel Robbins)\n   Count backwards from 5 and act immediately before your brain creates an excuse.",
                "⚡  BOOSTING COGNITIVE PERFORMANCE|😴 Sleep (Matthew Walker)\n   • REM sleep: Creativity & emotional processing\n   • Deep sleep: Memory consolidation\n   • < 7 hours → 30% cognitive drop\n\n🏃 Exercise & BDNF\n   • Aerobic exercises increase BDNF (new brain cells)\n   • 2 hours post-exercise = golden window for learning\n\n🥗 Nutrition\n   • Omega-3: Neural structure\n   • Flavonoids (blueberries): Reduces neuroinflammation\n   • Creatine: Increases brain ATP\n\n🧠 Pomodoro + Deep Work Hybrid\n   • 90 mins focus → 20 mins rest (ultradian rhythm)\n   • Screenless breaks: Walk, look at nature."
            };

            string[] sections = isEn ? sectionsEn : sectionsTr;

            foreach (var item in sections)
            {
                var parts = item.Split('|');
                var b = new System.Windows.Controls.Border { Background = (System.Windows.Media.Brush)TryFindResource("AccentLightBrush"), CornerRadius = new CornerRadius(8), Padding = new Thickness(12, 8, 12, 8), Margin = new Thickness(0, 0, 0, 8) };
                b.Child = new System.Windows.Controls.TextBlock { Text = parts[0], FontSize = 12, FontWeight = FontWeights.Bold, Foreground = (System.Windows.Media.Brush)TryFindResource("AccentBrush") };
                
                var t = new System.Windows.Controls.TextBlock { TextWrapping = TextWrapping.Wrap, FontSize = 12, LineHeight = 22, Margin = new Thickness(4, 0, 4, 16), Foreground = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush"), Text = parts[1] };
                
                stack.Children.Add(b);
                stack.Children.Add(t);
            }

            scroll.Content = stack;
            win.Content = scroll;
            win.ShowDialog();
        }

        // ─────────────────────────────────────────────────────────────
        // ANATOMİ HARİTASI — İnteraktif kas haritası
        // ─────────────────────────────────────────────────────────────
        private void AnatomyBtn_Click(object sender, RoutedEventArgs e)
        {
            ShowAnatomyWindow();
        }

        private void ShowAnatomyWindow()
        {
            var accent    = (System.Windows.Media.Brush)TryFindResource("AccentBrush")           ?? System.Windows.Media.Brushes.DodgerBlue;
            var accentLt  = (System.Windows.Media.Brush)TryFindResource("AccentLightBrush")      ?? System.Windows.Media.Brushes.AliceBlue;
            var primary   = (System.Windows.Media.Brush)TryFindResource("PrimaryTextBrush")      ?? System.Windows.Media.Brushes.Black;
            var secondary = (System.Windows.Media.Brush)TryFindResource("SecondaryTextBrush")    ?? System.Windows.Media.Brushes.Gray;
            var borderBr  = (System.Windows.Media.Brush)TryFindResource("BorderBrush")           ?? System.Windows.Media.Brushes.LightGray;
            var bgContent = (System.Windows.Media.Brush)TryFindResource("ContentBackgroundBrush")?? System.Windows.Media.Brushes.White;
            var bgHeader  = (System.Windows.Media.Brush)TryFindResource("HeaderBackgroundBrush") ?? System.Windows.Media.Brushes.WhiteSmoke;

            bool isEn = DailyPlanner.Services.LanguageService.CurrentLanguage == "en";
            var win = new Window
            {
                Title                 = isEn ? "🦴 Anatomy Map — Muscle Groups & Exercises" : "🦴 Anatomi Haritası — Kas Grupları & Egzersizler",
                Width                 = 920,
                Height                = 680,
                ResizeMode            = ResizeMode.CanResize,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner                 = this,
                Background            = bgContent,
                FontFamily            = new System.Windows.Media.FontFamily("Segoe UI")
            };

            // ── Ana layout: sol=vücut haritası, sağ=detay paneli ──
            var mainGrid = new System.Windows.Controls.Grid();
            mainGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(480) });
            mainGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // ── Sol panel: ön ve arka vücut ──
            var leftPanel = new System.Windows.Controls.StackPanel();

            // Başlık
            var headerBorder = new System.Windows.Controls.Border
            {
                Background      = bgHeader,
                BorderBrush     = borderBr,
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding         = new Thickness(20, 12, 20, 12)
            };
            var headerStack = new System.Windows.Controls.StackPanel();
            headerStack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text       = isEn ? "🦴  Anatomy Map" : "🦴  Anatomi Haritası",
                FontSize   = 16,
                FontWeight = FontWeights.Bold,
                Foreground = primary
            });
            headerStack.Children.Add(new System.Windows.Controls.TextBlock
            {
                Text       = isEn ? "Click on a muscle group to view exercise details" : "Kas grubuna tıklayarak egzersiz detaylarını görüntüleyin",
                FontSize   = 11,
                Foreground = secondary,
                Margin     = new Thickness(0, 3, 0, 0)
            });
            headerBorder.Child = headerStack;
            leftPanel.Children.Add(headerBorder);

            // Ön ve Arka vücut yan yana
            var bodyGrid = new System.Windows.Controls.Grid { Margin = new Thickness(10, 10, 10, 10) };
            bodyGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bodyGrid.ColumnDefinitions.Add(new System.Windows.Controls.ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Sağ panel — detay
            var rightBorder = new System.Windows.Controls.Border
            {
                Background      = bgContent,
                BorderBrush     = borderBr,
                BorderThickness = new Thickness(1, 0, 0, 0)
            };
            var rightScroll = new System.Windows.Controls.ScrollViewer
            {
                VerticalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = System.Windows.Controls.ScrollBarVisibility.Disabled
            };
            var detailPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(20, 16, 20, 16) };

            // Varsayılan mesaj
            var welcomeText = new System.Windows.Controls.TextBlock
            {
                Text         = isEn ? "👈 Click on a muscle group from\nthe body map on the left" : "👈 Soldaki vücut haritasından\nbir kas grubuna tıklayın",
                FontSize     = 14,
                Foreground   = secondary,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = System.Windows.TextAlignment.Center,
                Margin       = new Thickness(0, 80, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            detailPanel.Children.Add(welcomeText);
            rightScroll.Content = detailPanel;
            rightBorder.Child = rightScroll;

            // ── Kas detay verileri ──
            // Her kas: (başlık, emoji, açıklama, alt_bölümler[])
            // Alt bölüm: (alt_başlık, en_iyi_hareket, alternatifler)
            var muscleDataTr = new System.Collections.Generic.Dictionary<string, (string title, string emoji, string desc, (string subRegion, string bestExercise, string alternatives, string tips)[] regions)>
            {
                ["göğüs"] = ("Göğüs (Pectoralis)", "🫁", "Göğüs kasları üst gövdenin ön kısmında yer alır. Pektoralis major ve minor olarak ikiye ayrılır.", new[]
                {
                    ("Üst Göğüs (Clavicular Head)", "İncline Bench Press (30-45°)", "İncline Dumbbell Press, İncline Cable Fly, Landmine Press", "30-45° açı yeterlidir, daha fazlası omuzu devreye sokar"),
                    ("Orta Göğüs (Sternal Head)", "Flat Bench Press", "Dumbbell Bench Press, Machine Chest Press, Push-Up", "Skapula retraksiyon pozisyonunda yapın, omuz sıkışmasını önler"),
                    ("Alt Göğüs (Costal Head)", "Decline Bench Press / Dips", "Decline Dumbbell Fly, Cable Crossover (yüksekten aşağı), High-to-Low Cable Fly", "Dips vücut ağırlığı ile en etkili alt göğüs hareketidir"),
                    ("İç Göğüs (Adduction)", "Cable Crossover / Pec Deck", "Svend Press, Plate Squeeze Press, Close-Grip Bench", "Tam ROM kullanın ve kasılma noktasında 1-2 sn bekleyin"),
                }),
                ["omuz"] = ("Omuz (Deltoid)", "💪", "Deltoid kası üç bölümden oluşur: ön (anterior), yan (lateral) ve arka (posterior). Geniş omuz görünümü için lateral head kritiktir.", new[]
                {
                    ("Ön Omuz (Anterior Deltoid)", "Overhead Press (Barbell/Dumbbell)", "Arnold Press, Front Raise, Military Press", "Bench press zaten ön omuzu çalıştırır, fazla izolasyon gereksiz olabilir"),
                    ("Yan Omuz (Lateral Deltoid)", "Lateral Raise (Dumbbell)", "Cable Lateral Raise, Machine Lateral Raise, Behind-Back Cable Lateral", "Hafif ağırlık + yüksek tekrar (15-20) en iyi sonucu verir"),
                    ("Arka Omuz (Posterior Deltoid)", "Reverse Pec Deck / Face Pull", "Bent-Over Reverse Fly, Cable Reverse Fly, Band Pull-Apart", "Sırt antrenmanına eklenmeli, haftada 2-3 kez çalışın"),
                }),
                ["biseps"] = ("Biseps (Biceps Brachii)", "💪", "Biseps kası iki başlı bir kastır: uzun baş (long head) ve kısa baş (short head). Brachialis da kol kalınlığı için önemlidir.", new[]
                {
                    ("Uzun Baş (Long Head - dış)", "İncline Dumbbell Curl", "Bayesian Curl, Drag Curl, Narrow-Grip Barbell Curl", "Dirseği gövdenin arkasında tutmak uzun başı izole eder"),
                    ("Kısa Baş (Short Head - iç)", "Preacher Curl / Spider Curl", "Wide-Grip Barbell Curl, Concentration Curl, Cable Curl", "Dirseği gövdenin önünde tutun, tam kasılma sağlayın"),
                    ("Brachialis (Kol kalınlığı)", "Hammer Curl", "Cross-Body Hammer Curl, Reverse Curl, Zottman Curl", "Nötral tutuş (avuç içe bakacak şekilde) brachialis'i hedefler"),
                }),
                ["triseps"] = ("Triseps (Triceps Brachii)", "🦾", "Triseps üç başlı bir kastır: lateral, medial ve long head. Kolun arka tarafının ~%66'sını oluşturur.", new[]
                {
                    ("Lateral Head (Dış)", "Tricep Pushdown (V-bar)", "Rope Pushdown, Kickback, Diamond Push-Up", "Düz bar veya V-bar lateral head'i daha çok hedefler"),
                    ("Medial Head (İç)", "Reverse Grip Pushdown", "Close-Grip Bench Press, Overhead Dumbbell Extension", "Medial head tüm triseps hareketlerinde aktiftir"),
                    ("Long Head (Arkada)", "Overhead Tricep Extension", "Skull Crusher, French Press, Incline Overhead Extension", "Dirseğin baş üstünde olması long head'i gerer ve izole eder"),
                }),
                ["karın"] = ("Karın (Abdominals)", "🎯", "Karın kasları: rektus abdominis (sixpack), oblikler (yan karın) ve transversus abdominis (derin stabilizatör).", new[]
                {
                    ("Üst Karın (Upper Abs)", "Crunch / Cable Crunch", "Decline Sit-Up, Ab Roller, Machine Crunch", "Boynu çekmekten kaçının, göğsü kalçaya doğru kıvırın"),
                    ("Alt Karın (Lower Abs)", "Hanging Leg Raise", "Reverse Crunch, Lying Leg Raise, Captain's Chair", "Kalçayı kaldırmak alt karın için kritik — sadece bacak kaldırmak yetmez"),
                    ("Yan Karın (Obliques)", "Wood Chop / Cable Oblique Crunch", "Russian Twist, Side Plank, Bicycle Crunch", "Ağır yan bükülme karını genişletebilir, dikkatli olun"),
                    ("Derin Core (TVA)", "Plank / Dead Bug", "Vacuum, Pallof Press, Bird-Dog", "Stability hareketleri core gücünü artırır, sakatlanmayı önler"),
                }),
                ["ön bacak"] = ("Ön Bacak (Quadriceps)", "🦵", "Quadriceps dört başlı kastır: rectus femoris, vastus lateralis, vastus medialis (VMO) ve vastus intermedius.", new[]
                {
                    ("Genel Quadriceps", "Barbell Squat (Back Squat)", "Front Squat, Hack Squat, Leg Press", "Squat tüm vücut için en etkili bileşik hareketidir"),
                    ("Vastus Medialis (İç - VMO)", "Sissy Squat / Leg Extension (kısa ROM)", "Bulgarian Split Squat, Close-Stance Leg Press, Petersen Step-Up", "Diz sağlığı için VMO güçlü olmalı — tam diz bükme VMO'yu aktive eder"),
                    ("Vastus Lateralis (Dış)", "Leg Extension (tam ROM)", "Wide-Stance Squat, Leg Press (ayaklar dar)", "Ayakları dar tutmak dış quadriceps'i daha çok hedefler"),
                    ("Rectus Femoris (Ön-Orta)", "Front Squat / Leg Extension", "Step-Up, Walking Lunge, Cyclist Squat", "Kalçadan da başlayan tek quadriceps kasıdır, germe önemli"),
                }),
                ["arka bacak"] = ("Arka Bacak (Hamstrings)", "🦵", "Hamstring üç kastan oluşur: biceps femoris, semitendinosus ve semimembranosus. Diz bükme ve kalça açma yapılır.", new[]
                {
                    ("Genel Hamstring (Hip Hinge)", "Romanian Deadlift (RDL)", "Stiff-Leg Deadlift, Good Morning, Barbell Hip Hinge", "RDL hamstring'in STRETCHING altında çalışmasını sağlar — en etkili"),
                    ("Diz Bükme (Knee Flexion)", "Lying Leg Curl / Seated Leg Curl", "Nordic Curl, Swiss Ball Leg Curl, Slider Curl", "Seated curl distal hamstring'i, lying curl proksimal kısmı hedefler"),
                    ("Biceps Femoris (Dış)", "Lying Leg Curl (ayak iç rotasyonda)", "Single-Leg RDL, Sumo RDL", "Ayak parmakları içe bakacak şekilde curl yapın"),
                }),
                ["sırt"] = ("Sırt (Back)", "🔙", "Sırt kasları geniş ve karmaşık bir gruptur: latissimus dorsi, trapezius, rhomboidler, erector spinae (alt sırt).", new[]
                {
                    ("Latissimus Dorsi (Lat - V şekli)", "Pull-Up / Lat Pulldown", "Close-Grip Lat Pulldown, Single-Arm Cable Row, Straight-Arm Pulldown", "Geniş tutuş lat genişliğini, dar tutuş lat kalınlığını hedefler"),
                    ("Orta Sırt (Rhomboid/Trapezius Mid)", "Barbell Row / Cable Row", "T-Bar Row, Chest-Supported Row, Dumbbell Row", "Kürek kemiklerini birbirine yaklaştırma hissi — 'squeeze' yapın"),
                    ("Üst Sırt / Trapezius", "Barbell Shrug / Face Pull", "Dumbbell Shrug, Upright Row, Farmer's Walk", "Face pull arka omuz + üst sırt için mükemmel; her antrenmanda yapılabilir"),
                    ("Alt Sırt (Erector Spinae)", "Deadlift / Back Extension", "Hyperextension, Reverse Hyper, Good Morning", "Alt sırt güçlü olmalı ama aşırı yüklenmekten kaçının — sakatlanma riski"),
                }),
                ["kalça"] = ("Kalça (Glutes)", "🍑", "Gluteus maximus (en büyük kas), gluteus medius (stabilizatör) ve gluteus minimus. Kalça açma ve bacak abdüksiyonundan sorumlu.", new[]
                {
                    ("Gluteus Maximus (Ana kütle)", "Hip Thrust / Barbell Glute Bridge", "Cable Pull-Through, Sumo Deadlift, Bulgarian Split Squat", "Hip thrust en yüksek gluteus maximus aktivasyonu sağlar (Contreras 2015)"),
                    ("Gluteus Medius (Yan stabilizatör)", "Cable Hip Abduction / Side-Lying Leg Raise", "Banded Walks, Clamshell, Single-Leg Squat", "Diz ve kalça sağlığı için kritik — ısınmada mutlaka çalışın"),
                    ("Alt Gluteus (Alt kıvrım)", "Deep Squat / Deficit RDL", "Single-Leg Hip Thrust, Step-Up, Walking Lunge", "Derin ROM kullanmak alt gluteus'u aktive eder"),
                }),
                ["baldır"] = ("Baldır (Calves)", "🦶", "Gastrocnemius (yüzeysel, 2 başlı) ve soleus (derin). Yürüme, koşu, zıplama gibi hareketlerin temel güç kaynağı.", new[]
                {
                    ("Gastrocnemius (Üst baldır)", "Standing Calf Raise", "Donkey Calf Raise, Smith Machine Calf Raise, Single-Leg Standing Raise", "Diz düz iken gastrocnemius çalışır — yavaş negatif (3 sn) yapın"),
                    ("Soleus (Alt / Derin)", "Seated Calf Raise", "Bent-Knee Calf Raise, Leg Press Calf Raise (diz bükük)", "Diz bükülü iken soleus izole edilir — yüksek tekrar (15-25) etkili"),
                }),
                ["ön kol"] = ("Ön Kol (Forearms)", "✊", "Ön kol kasları kavrama gücü, bilek bükmesi ve parmak hareketlerinden sorumludur. Brachioradialis en büyük ön kol kasıdır.", new[]
                {
                    ("Brachioradialis (Üst dış)", "Reverse Curl", "Zottman Curl, Hammer Curl, Pronated Wrist Curl", "Reverse tutus (avuç aşağı) ile curl brachioradialis'i izole eder"),
                    ("Bilek Flexörleri (İç ön kol)", "Wrist Curl (Barbell / Dumbbell)", "Behind-Back Wrist Curl, Finger Curl", "Hafif ağırlık, yüksek tekrar (15-20) en iyi sonucu verir"),
                    ("Kavrama Gücü", "Farmer's Walk / Dead Hang", "Gripper, Plate Pinch, Towel Pull-Up", "Deadlift'te ağırlığı tutamıyorsanız kavrama gücü eksiktir"),
                }),
                ["boyun"] = ("Boyun (Neck)", "🦴", "Boyun kasları başın hareketinden sorumludur: sternocleidomastoid (SCM), semispinalis, splenius ve scalene kasları. Güçlü boyun kasları duruş bozukluklarını önler ve sporcuları korur.", new[]
                {
                    ("Öne Eğme (Flexion — SCM)", "Neck Curl (Plate/Band)", "Weighted Neck Flexion, Manual Resistance Curl, 4-Way Neck Machine", "Hafif ağırlıkla başlayın; boyun sakatlığa çok açıktır. 15-25 tekrar ideal."),
                    ("Arkaya Eğme (Extension — Semispinalis/Splenius)", "Neck Extension (Plate/Band)", "4-Way Neck Machine, Prone Neck Extension, Manual Resistance", "Yüzüstü pozisyonda başın arkasına plaka koyarak yapılabilir. Yavaş ve kontrollü hareket."),
                    ("Yana Eğme (Lateral Flexion — Scalene)", "Lateral Neck Flexion (Band)", "4-Way Neck Machine, Manual Resistance, Side Lying Neck Raise", "Her iki tarafa eşit çalışın. 12-20 tekrar yeterli."),
                    ("Rotasyon (SCM + Splenius)", "Neck Rotation (Band/Manual)", "Isometric Neck Rotation, 4-Way Neck Machine", "İzometrik tutma (5-10 sn) güvenli ve etkilidir. Amerikan futbolu ve güreşçi antrenmanlarında yaygın."),
                    ("Boyun Stabilizasyonu (Genel)", "Isometric Neck Hold (4 yön)", "Prone/Supine Neck Hold, Band Resisted Holds", "Her yöne 10 sn basınç uygulayıp tutun. Günlük yapılabilir, ısınmaya ekleyin."),
                }),
            };

            var muscleDataEn = new System.Collections.Generic.Dictionary<string, (string title, string emoji, string desc, (string subRegion, string bestExercise, string alternatives, string tips)[] regions)>
            {
                ["göğüs"] = ("Chest (Pectoralis)", "🫁", "Chest muscles are on the front of the upper body. Divided into pectoralis major and minor.", new[]
                {
                    ("Upper Chest (Clavicular)", "Incline Bench Press (30-45°)", "Incline Dumbbell Press, Incline Cable Fly", "30-45° angle is enough; more involves the shoulders heavily"),
                    ("Middle Chest (Sternal)", "Flat Bench Press", "Dumbbell Bench Press, Machine Chest Press, Push-Up", "Scapular retraction prevents shoulder impingement"),
                    ("Lower Chest (Costal)", "Decline Bench Press / Dips", "Decline Dumbbell Fly, High-to-Low Cable Fly", "Dips are highly effective for the lower chest using bodyweight"),
                    ("Inner Chest (Adduction)", "Cable Crossover / Pec Deck", "Svend Press, Plate Squeeze Press", "Use full ROM and hold the contraction for 1-2 seconds"),
                }),
                ["omuz"] = ("Deltoid (Shoulders)", "💪", "Deltoid has three parts: anterior (front), lateral (side), and posterior (rear). Lateral head is critical for broad shoulders.", new[]
                {
                    ("Front Deltoid (Anterior)", "Overhead Press", "Arnold Press, Front Raise", "Bench press already works front shoulders; extra isolation may be unnecessary"),
                    ("Side Deltoid (Lateral)", "Lateral Raise (Dumbbell)", "Cable Lateral Raise, Machine Lateral Raise", "Light weights + high reps (15-20) yield the best results"),
                    ("Rear Deltoid (Posterior)", "Reverse Pec Deck / Face Pull", "Bent-Over Reverse Fly, Band Pull-Apart", "Add to back workout days, train 2-3 times a week"),
                }),
                ["biseps"] = ("Biceps Brachii", "💪", "Two-headed muscle: long head and short head. Brachialis is also crucial for arm thickness.", new[]
                {
                    ("Long Head (Outer)", "Incline Dumbbell Curl", "Bayesian Curl, Drag Curl", "Keeping elbows behind the torso isolates the long head"),
                    ("Short Head (Inner)", "Preacher Curl", "Wide-Grip Barbell Curl, Concentration Curl", "Keep elbows in front of the torso for max contraction"),
                    ("Brachialis (Thickness)", "Hammer Curl", "Reverse Curl, Zottman Curl", "Neutral grip (palms facing inward) targets the brachialis"),
                }),
                ["triseps"] = ("Triceps Brachii", "🦾", "Three-headed muscle: lateral, medial, long head. Makes up ~66% of the upper arm volume.", new[]
                {
                    ("Lateral Head (Outer)", "Tricep Pushdown (V-bar)", "Rope Pushdown, Kickback", "Straight or V-bar targets the lateral head better"),
                    ("Medial Head (Inner)", "Reverse Grip Pushdown", "Close-Grip Bench Press", "Medial head is active in all triceps movements"),
                    ("Long Head (Rear)", "Overhead Tricep Extension", "Skull Crusher, French Press", "Having elbows overhead stretches and isolates the long head"),
                }),
                ["karın"] = ("Abdominals (Core)", "🎯", "Consists of rectus abdominis (sixpack), obliques, and transversus abdominis (TVA).", new[]
                {
                    ("Upper Abs", "Crunch / Cable Crunch", "Decline Sit-Up, Ab Roller", "Avoid pulling your neck; roll your chest towards your pelvis"),
                    ("Lower Abs", "Hanging Leg Raise", "Reverse Crunch, Lying Leg Raise", "Lifting the pelvis is key—just lifting legs is not enough"),
                    ("Obliques (Side)", "Wood Chop / Cable Oblique Crunch", "Russian Twist, Side Plank", "Heavy side bends can widen your waist, train carefully"),
                    ("Deep Core (TVA)", "Plank / Dead Bug", "Vacuum, Bird-Dog", "Stability movements increase core strength and prevent injury"),
                }),
                ["ön bacak"] = ("Quadriceps (Quads)", "🦵", "Four-headed thigh muscle: rectus femoris, vastus lateralis, vastus medialis (VMO), vastus intermedius.", new[]
                {
                    ("Overall Quads", "Barbell Squat (Back/Front)", "Hack Squat, Leg Press", "Squat is the most effective compound movement"),
                    ("Vastus Medialis (VMO/Tear drop)", "Sissy Squat / Petersen Step-Up", "Bulgarian Split Squat", "Crucial for knee health—full knee flexion activates VMO"),
                    ("Vastus Lateralis (Outer sweep)", "Leg Extension (full ROM)", "Leg Press (narrow stance)", "Narrow foot placement targets the outer quads more"),
                    ("Rectus Femoris", "Leg Extension", "Walking Lunge", "The only quad muscle that crosses the hip joint"),
                }),
                ["arka bacak"] = ("Hamstrings", "🦵", "Three muscles: biceps femoris, semitendinosus, semimembranosus. Responsible for knee flexion and hip extension.", new[]
                {
                    ("Overall Hamstring (Hip Hinge)", "Romanian Deadlift (RDL)", "Stiff-Leg Deadlift, Good Morning", "RDL works the hamstring under STRETCH — highly effective"),
                    ("Knee Flexion", "Seated Leg Curl", "Nordic Curl, Lying Leg Curl", "Seated curl targets distal hamstrings better"),
                    ("Outer Hamstring", "Lying Leg Curl (internal rotation)", "Sumo RDL", "Curl with toes pointing inward"),
                }),
                ["sırt"] = ("Back Muscles", "🔙", "Large and complex: latissimus dorsi, trapezius, rhomboids, erector spinae.", new[]
                {
                    ("Latissimus Dorsi (Lats)", "Pull-Up / Lat Pulldown", "Single-Arm Cable Row, Pullover", "Wide grip targets width, narrow grip targets thickness"),
                    ("Middle Back (Rhomboids)", "Barbell Row / Cable Row", "T-Bar Row, Chest-Supported Row", "Focus on squeezing the shoulder blades together"),
                    ("Upper Back / Trapezius", "Face Pull / Shrugs", "Upright Row, Farmer's Walk", "Face pulls are excellent for rear delts + upper back"),
                    ("Lower Back (Erector Spinae)", "Deadlift / Back Extension", "Hyperextension, Good Morning", "Lower back must be strong, but avoid overloading to prevent injury"),
                }),
                ["kalça"] = ("Glutes", "🍑", "Gluteus maximus (main), gluteus medius (stabilizer), and gluteus minimus.", new[]
                {
                    ("Gluteus Maximus (Main bulk)", "Hip Thrust / Barbell Glute Bridge", "Cable Pull-Through, Sumo Deadlift", "Hip thrust provides the highest gluteus maximus activation"),
                    ("Gluteus Medius", "Cable Hip Abduction / Clamshell", "Banded Walks, Side-Lying Raise", "Crucial for knee and hip health—do them in warm-ups"),
                    ("Lower Glutes", "Deep Squat / Deficit RDL", "Single-Leg Hip Thrust", "Using a deep ROM activates the lower glutes"),
                }),
                ["baldır"] = ("Calves", "🦶", "Gastrocnemius (superficial, 2 heads) and soleus (deep).", new[]
                {
                    ("Gastrocnemius (Upper calf)", "Standing Calf Raise", "Donkey Calf Raise, Leg Press Calf Raise", "Works when the knee is straight. Use slow negatives (3 sec)"),
                    ("Soleus (Lower / Deep)", "Seated Calf Raise", "Bent-Knee Calf Raise", "Isolated when the knee is bent. High reps (15-25) are effective"),
                }),
                ["ön kol"] = ("Forearms", "✊", "Responsible for grip strength, wrist flexion, and finger movement.", new[]
                {
                    ("Brachioradialis", "Reverse Curl", "Hammer Curl", "Reverse grip (palms down) isolates the brachioradialis"),
                    ("Wrist Flexors", "Wrist Curl", "Behind-Back Wrist Curl", "Light weights, high reps (15-20) yield best results"),
                    ("Grip Strength", "Farmer's Walk / Dead Hang", "Plate Pinch, Towel Pull-Up", "If you drop deadlifts, your grip strength needs work"),
                }),
                ["boyun"] = ("Neck", "🦴", "Responsible for head movement. Strong neck prevents posture issues and protects athletes.", new[]
                {
                    ("Flexion (Front)", "Neck Curl", "Weighted Neck Flexion", "Start light; neck is prone to injury. 15-25 reps ideal."),
                    ("Extension (Back)", "Neck Extension", "4-Way Neck Machine", "Slow and controlled movement."),
                    ("Lateral Flexion (Side)", "Lateral Neck Flexion", "Side Lying Neck Raise", "Work both sides equally. 12-20 reps."),
                    ("Stabilization (General)", "Isometric Neck Hold (4 ways)", "Band Resisted Holds", "Apply pressure and hold for 10 sec in each direction. Great for warm-ups."),
                }),
            };

            var muscleData = isEn ? muscleDataEn : muscleDataTr;

            // ── Isınma hareketleri verileri ──
            var warmupDataTr = new System.Collections.Generic.Dictionary<string, (string exercise, string desc)[]>
            {
                ["göğüs"] = new[]
                {
                    ("Doorway Stretch", "Kapı pervazına kollarınızı koyup öne adımlayın. 20-30 sn tutun. Göğüs kaslarını açar."),
                    ("Arm Circles (Kol Çevirme)", "Kolları yana açıp küçükten büyüğe daireler çizin. 15 ileri + 15 geri."),
                    ("Chest Opener (Göğüs Açıcı)", "Arkada ellerinizi kenetleyip kolları yukarı kaldırın. 15-20 sn tutun."),
                    ("Band Pull-Apart (Hafif)", "Dirençli bandı omuz genişliğinde tutup geriye çekin. 2×15 tekrar."),
                },
                ["omuz"] = new[]
                {
                    ("Y-T-W Raises", "Yüzüstü veya eğilerek Y, T, W harfleri şeklinde kol kaldırma. 10'ar tekrar."),
                    ("Shoulder Dislocates (Bant ile)", "Geniş tutuşla bandı/çubuğu başın üzerinden arkaya geçirin. 2×10."),
                    ("Arm Cross Stretch", "Bir kolu karşı tarafa uzatıp diğer elle bastırın. Her kol 20 sn."),
                    ("Internal/External Rotation", "Dirseği 90° büküp bant ile iç ve dış rotasyon yapın. 2×12."),
                },
                ["biseps"] = new[]
                {
                    ("Duvarda Biseps Esneme", "Kolunuzu duvara düz yapıştırıp gövdeyi karşı tarafa çevirin. 20 sn her kol."),
                    ("Hafif Bant Curl", "Çok hafif direnç bandıyla 15-20 curl yapın. Kasları ısıtır."),
                    ("Arm Circles (Küçük)", "Kolları yana açıp küçük hızlı çevirme. 20 ileri + 20 geri."),
                },
                ["triseps"] = new[]
                {
                    ("Overhead Tricep Stretch", "Bir kolu başınızın arkasına katlayıp diğer elle dirsekten bastırın. 20 sn her kol."),
                    ("Behind-Back Reach", "Bir el üstten, diğer el alttan sırta uzanarak parmaklara değmeye çalışın. 15 sn her taraf."),
                    ("Arm Swings (Kol Salınımı)", "Kolları ileri-geri sallayarak trisepsi dinamik olarak ısıtın. 20 tekrar."),
                },
                ["karın"] = new[]
                {
                    ("Cat-Cow (Kedi-İnek)", "Dört ayak üzerinde sırtı yukarı yuvarlayın (kedi), sonra çukurlaştırın (inek). 10 tekrar."),
                    ("Torso Twist (Gövde Dönüşü)", "Ayakta veya oturarak gövdeyi sağa-sola döndürün. 15 tekrar her yöne."),
                    ("Side Bend (Yan Eğilme)", "Bir kolunuzu yukarı kaldırıp karşı tarafa eğilin. 10 tekrar her yöne."),
                    ("Dead Bug (Yavaş)", "Sırt üstü yatıp karşı kol-bacak uzatma. Core aktivasyonu için 8 tekrar her taraf."),
                },
                ["ön bacak"] = new[]
                {
                    ("Standing Quad Stretch", "Ayakta bir ayağınızı kalçanıza çekerek tutun. 20-30 sn her bacak."),
                    ("Walking Lunges", "Dinamik adımlama hareketi. 10 adım her bacak, kontrollü ve yavaş."),
                    ("Leg Swings (Ön-Arka)", "Bir bacağı tutunarak ileri-geri sallayın. 15 tekrar her bacak."),
                    ("High Knees (Diz Çekme)", "Yerinde koşarak dizleri göğse çekin. 20 tekrar, ısınma hızında."),
                },
                ["arka bacak"] = new[]
                {
                    ("Standing Forward Fold", "Ayakta öne eğilerek parmaklarınızla yere ulaşmaya çalışın. 20-30 sn tutun."),
                    ("Seated Forward Fold", "Oturarak bacakları düz uzatıp parmaklara doğru uzanın. 20-30 sn."),
                    ("Good Morning (Ağırlıksız)", "Eller başın arkasında, kalçadan eğilerek hamstringleri gerin. 12 tekrar yavaş."),
                    ("Inchworm", "Ayakta öne eğilip ellerle yürüyerek plank pozisyonuna gelin, geri dönün. 6-8 tekrar."),
                },
                ["sırt"] = new[]
                {
                    ("Cat-Cow", "Dört ayak üzerinde omurgayı yuvarla ve çukurlaştır. 10 tekrar."),
                    ("Thread the Needle", "Dört ayak üzerinde bir kolu diğerinin altından geçirin. 8 tekrar her taraf."),
                    ("Child's Pose (Çocuk Pozisyonu)", "Dizler yerde, kalçayı topuklara çekip kolları öne uzatın. 20-30 sn."),
                    ("Lat Stretch (Kapıda)", "Kapı pervazına tutunup kalçayı geriye iterek lat'ı gerin. 15-20 sn her taraf."),
                },
                ["kalça"] = new[]
                {
                    ("Hip Circles (Kalça Çevirme)", "Ayakta kalçayla büyük daireler çizin. 10 saat yönü + 10 ters."),
                    ("Pigeon Stretch (Güvercin)", "Bir bacağı önde bükülü, diğerini arkada düz tutarak kalçayı gerin. 20-30 sn."),
                    ("90/90 Stretch", "Yerde ön bacak 90°, arka bacak 90° bükülü. Gövdeyi öne eğin. 20 sn her taraf."),
                    ("Clamshell (Hafif)", "Yan yatıp dizleri bükerek üst dizinizi açın. 15 tekrar her taraf. Glute medius."),
                },
                ["baldır"] = new[]
                {
                    ("Calf Raises (Yavaş)", "Yerden yavaşça yükselin ve inin. 15 tekrar, kontrollü tempo."),
                    ("Wall Calf Stretch", "Duvara yaslanıp bir bacağı arkada düz tutarak baldırı gerin. 20 sn her bacak."),
                    ("Ankle Circles (Ayak Bileği)", "Ayaklarla her yönde 10'ar daire çizin."),
                },
                ["ön kol"] = new[]
                {
                    ("Wrist Circles (Bilek Çevirme)", "Bilekleri saat yönünde ve tersine 10'ar kez çevirin."),
                    ("Prayer Stretch (Dua Esneme)", "Avuç içlerini göğüs hizasında birleştirip aşağı doğru itin. 15-20 sn."),
                    ("Reverse Prayer Stretch", "Ellerin dışını birleştirip bilek arkasını gerin. 15-20 sn."),
                },
                ["boyun"] = new[]
                {
                    ("Neck Circles (Boyun Çevirme)", "Başınızı yavaşça saat yönünde ve tersine çevirin. 5 tur her yöne."),
                    ("Chin Tucks (Çene Çekme)", "Çenenizi içeri çekerek çift çene yapın. 10 tekrar, 3 sn tutun."),
                    ("Lateral Neck Stretch", "Başınızı yana eğip elinizle hafif bastırın. 15 sn her taraf."),
                    ("Neck Flexion/Extension", "Çeneyi göğse yaklaştırın (flexion), sonra tavana bakın (extension). 8 tekrar."),
                },
            };

            var warmupDataEn = new System.Collections.Generic.Dictionary<string, (string exercise, string desc)[]>
            {
                ["göğüs"] = new[]
                {
                    ("Doorway Stretch", "Place arms on door frame and step forward. Hold 20-30 sec. Opens chest muscles."),
                    ("Arm Circles", "Extend arms to sides and make circles small to large. 15 forward + 15 backward."),
                    ("Chest Opener", "Clasp hands behind your back and lift arms up. Hold 15-20 sec."),
                    ("Band Pull-Apart (Light)", "Hold resistance band at shoulder width and pull apart. 2×15 reps."),
                },
                ["omuz"] = new[]
                {
                    ("Y-T-W Raises", "Face down or bent over, lift arms in Y, T, W shapes. 10 reps each."),
                    ("Shoulder Dislocates (Band)", "Wide grip, pass band/stick over head to behind back. 2×10."),
                    ("Arm Cross Stretch", "Extend one arm across body, press with other hand. 20 sec each arm."),
                    ("Internal/External Rotation", "Elbow at 90°, use band for internal and external rotation. 2×12."),
                },
                ["biseps"] = new[]
                {
                    ("Wall Bicep Stretch", "Place arm flat on wall and rotate body away. 20 sec each arm."),
                    ("Light Band Curl", "Very light resistance band curls, 15-20 reps to warm the muscle."),
                    ("Small Arm Circles", "Arms out to sides, quick small circles. 20 forward + 20 backward."),
                },
                ["triseps"] = new[]
                {
                    ("Overhead Tricep Stretch", "Fold one arm behind head, press elbow with other hand. 20 sec each."),
                    ("Behind-Back Reach", "One hand from top, other from bottom, try to touch fingers. 15 sec each side."),
                    ("Arm Swings", "Swing arms forward and backward to dynamically warm triceps. 20 reps."),
                },
                ["karın"] = new[]
                {
                    ("Cat-Cow", "On all fours, round spine up (cat), then arch down (cow). 10 reps."),
                    ("Torso Twist", "Standing or seated, rotate torso left and right. 15 reps each direction."),
                    ("Side Bend", "Raise one arm overhead and lean to opposite side. 10 reps each side."),
                    ("Slow Dead Bug", "On back, extend opposite arm-leg. Core activation. 8 reps each side."),
                },
                ["ön bacak"] = new[]
                {
                    ("Standing Quad Stretch", "Stand on one leg, pull other foot to glutes. 20-30 sec each leg."),
                    ("Walking Lunges", "Dynamic stepping lunges. 10 steps each leg, controlled and slow."),
                    ("Leg Swings (Front-Back)", "Hold onto something, swing leg forward and back. 15 reps each leg."),
                    ("High Knees", "Jog in place bringing knees to chest. 20 reps at warm-up pace."),
                },
                ["arka bacak"] = new[]
                {
                    ("Standing Forward Fold", "Stand and bend forward, reach for toes. Hold 20-30 sec."),
                    ("Seated Forward Fold", "Sit with legs straight, reach toward toes. Hold 20-30 sec."),
                    ("Good Morning (Bodyweight)", "Hands behind head, hinge at hips to stretch hamstrings. 12 slow reps."),
                    ("Inchworm", "Bend forward, walk hands to plank, walk back. 6-8 reps."),
                },
                ["sırt"] = new[]
                {
                    ("Cat-Cow", "On all fours, round and arch the spine. 10 reps."),
                    ("Thread the Needle", "On all fours, thread one arm under the other. 8 reps each side."),
                    ("Child's Pose", "Knees on ground, sit back on heels, arms extended forward. 20-30 sec."),
                    ("Lat Stretch (Doorway)", "Hold door frame and push hips back to stretch lats. 15-20 sec each side."),
                },
                ["kalça"] = new[]
                {
                    ("Hip Circles", "Standing, make large circles with hips. 10 clockwise + 10 counter."),
                    ("Pigeon Stretch", "Front leg bent, back leg straight behind. Stretch hip. 20-30 sec."),
                    ("90/90 Stretch", "On floor, front leg 90°, back leg 90°. Lean forward. 20 sec each side."),
                    ("Clamshell (Light)", "Side-lying, knees bent, open top knee. 15 reps each side. Glute medius."),
                },
                ["baldır"] = new[]
                {
                    ("Slow Calf Raises", "Rise slowly onto toes and lower. 15 reps, controlled tempo."),
                    ("Wall Calf Stretch", "Lean against wall, one leg back straight, stretch calf. 20 sec each leg."),
                    ("Ankle Circles", "Circle each foot 10 times in each direction."),
                },
                ["ön kol"] = new[]
                {
                    ("Wrist Circles", "Rotate wrists clockwise and counter-clockwise, 10 each direction."),
                    ("Prayer Stretch", "Press palms together at chest level, push downward. 15-20 sec."),
                    ("Reverse Prayer Stretch", "Press backs of hands together to stretch wrist extensors. 15-20 sec."),
                },
                ["boyun"] = new[]
                {
                    ("Neck Circles", "Slowly rotate head clockwise and counter-clockwise. 5 circles each way."),
                    ("Chin Tucks", "Pull chin inward creating a double chin. 10 reps, hold 3 sec each."),
                    ("Lateral Neck Stretch", "Tilt head to side, gently press with hand. 15 sec each side."),
                    ("Neck Flexion/Extension", "Bring chin to chest (flexion), then look up (extension). 8 reps."),
                },
            };

            var warmupData = isEn ? warmupDataEn : warmupDataTr;


            // ── Bölge tıklama → detay gösterme fonksiyonu ──
            void ShowMuscleDetail(string key)
            {
                if (!muscleData.ContainsKey(key)) return;
                var (title, emoji, desc, regions) = muscleData[key];

                detailPanel.Children.Clear();

                // Başlık
                detailPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text       = $"{emoji}  {title}",
                    FontSize   = 17,
                    FontWeight = FontWeights.Bold,
                    Foreground = primary,
                    Margin     = new Thickness(0, 0, 0, 4)
                });

                // Açıklama
                detailPanel.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text         = desc,
                    FontSize     = 12,
                    Foreground   = secondary,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight   = 18,
                    Margin       = new Thickness(0, 0, 0, 16)
                });

                // Her alt bölüm
                foreach (var (subRegion, best, alts, tips) in regions)
                {
                    // Alt bölüm başlığı
                    var subHeader = new System.Windows.Controls.Border
                    {
                        Background  = accentLt,
                        CornerRadius = new CornerRadius(8),
                        Padding     = new Thickness(12, 8, 12, 8),
                        Margin      = new Thickness(0, 0, 0, 6)
                    };
                    subHeader.Child = new System.Windows.Controls.TextBlock
                    {
                        Text       = subRegion,
                        FontSize   = 13,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = accent
                    };
                    detailPanel.Children.Add(subHeader);

                    // En iyi hareket
                    var bestPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(8, 0, 0, 4) };
                    bestPanel.Children.Add(new System.Windows.Controls.TextBlock
                    {
                        Text       = isEn ? "⭐ Best Exercise:" : "⭐ En İyi Hareket:",
                        FontSize   = 11,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = secondary,
                        Margin     = new Thickness(0, 0, 0, 2)
                    });
                    bestPanel.Children.Add(new System.Windows.Controls.TextBlock
                    {
                        Text         = $"  {best}",
                        FontSize     = 13,
                        FontWeight   = FontWeights.Bold,
                        Foreground   = primary,
                        TextWrapping = TextWrapping.Wrap
                    });
                    detailPanel.Children.Add(bestPanel);

                    // Alternatifler
                    var altPanel = new System.Windows.Controls.StackPanel { Margin = new Thickness(8, 4, 0, 4) };
                    altPanel.Children.Add(new System.Windows.Controls.TextBlock
                    {
                        Text       = isEn ? "🔄 Alternatives:" : "🔄 Alternatifler:",
                        FontSize   = 11,
                        FontWeight = FontWeights.SemiBold,
                        Foreground = secondary,
                        Margin     = new Thickness(0, 0, 0, 2)
                    });
                    foreach (var alt in alts.Split(','))
                    {
                        altPanel.Children.Add(new System.Windows.Controls.TextBlock
                        {
                            Text         = $"  • {alt.Trim()}",
                            FontSize     = 12,
                            Foreground   = primary,
                            TextWrapping = TextWrapping.Wrap,
                            Margin       = new Thickness(0, 1, 0, 1)
                        });
                    }
                    detailPanel.Children.Add(altPanel);

                    // İpucu
                    var tipBorder = new System.Windows.Controls.Border
                    {
                        Background   = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 250, 200, 50)),
                        CornerRadius = new CornerRadius(6),
                        Padding      = new Thickness(10, 6, 10, 6),
                        Margin       = new Thickness(8, 2, 0, 14)
                    };
                    tipBorder.Child = new System.Windows.Controls.TextBlock
                    {
                        Text         = $"💡 {tips}",
                        FontSize     = 11,
                        Foreground   = primary,
                        TextWrapping = TextWrapping.Wrap,
                        FontStyle    = System.Windows.FontStyles.Italic
                    };
                    detailPanel.Children.Add(tipBorder);
                }

                // ── Isınma Hareketleri bölümü ──
                if (warmupData.ContainsKey(key))
                {
                    var warmups = warmupData[key];

                    // Ayırıcı çizgi
                    detailPanel.Children.Add(new System.Windows.Controls.Border
                    {
                        Height = 1,
                        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 128, 128, 128)),
                        Margin = new Thickness(0, 8, 0, 8)
                    });

                    // Isınma başlığı
                    var warmupHeader = new System.Windows.Controls.Border
                    {
                        Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(25, 255, 140, 0)),
                        CornerRadius = new CornerRadius(8),
                        Padding = new Thickness(12, 8, 12, 8),
                        Margin = new Thickness(0, 0, 0, 8)
                    };
                    warmupHeader.Child = new System.Windows.Controls.TextBlock
                    {
                        Text = isEn ? "🔥 Warm-Up Exercises" : "🔥 Isınma Hareketleri",
                        FontSize = 14,
                        FontWeight = FontWeights.Bold,
                        Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230, 126, 34))
                    };
                    detailPanel.Children.Add(warmupHeader);

                    foreach (var (exercise, wDesc) in warmups)
                    {
                        var wCard = new System.Windows.Controls.Border
                        {
                            Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(12, 255, 140, 0)),
                            CornerRadius = new CornerRadius(8),
                            Padding = new Thickness(12, 8, 12, 8),
                            Margin = new Thickness(0, 0, 0, 6),
                            BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 255, 140, 0)),
                            BorderThickness = new Thickness(0, 0, 2, 0)
                        };
                        var wPanel = new System.Windows.Controls.StackPanel();
                        wPanel.Children.Add(new System.Windows.Controls.TextBlock
                        {
                            Text = $"🏃 {exercise}",
                            FontSize = 12,
                            FontWeight = FontWeights.SemiBold,
                            Foreground = primary,
                            Margin = new Thickness(0, 0, 0, 3)
                        });
                        wPanel.Children.Add(new System.Windows.Controls.TextBlock
                        {
                            Text = wDesc,
                            FontSize = 11,
                            Foreground = secondary,
                            TextWrapping = TextWrapping.Wrap,
                            LineHeight = 16
                        });
                        wCard.Child = wPanel;
                        detailPanel.Children.Add(wCard);
                    }
                }

                rightScroll.ScrollToTop();
            }

            // ── Tıklanabilir bölge oluşturma fonksiyonu ──
            System.Windows.Controls.Border CreateHitZone(string label, string muscleKey, double x, double y, double w, double h, System.Windows.Media.Color color)
            {
                var zone = new System.Windows.Controls.Border
                {
                    Width           = w,
                    Height          = h,
                    Background      = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, color.R, color.G, color.B)),
                    BorderBrush     = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(120, color.R, color.G, color.B)),
                    BorderThickness = new Thickness(1.5),
                    CornerRadius    = new CornerRadius(6),
                    Cursor          = System.Windows.Input.Cursors.Hand,
                    ToolTip         = $"🔍 {label} — tıklayın",
                    Tag             = muscleKey,
                    Child           = new System.Windows.Controls.TextBlock
                    {
                        Text                = label,
                        FontSize            = 9,
                        FontWeight          = FontWeights.SemiBold,
                        Foreground          = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(220, color.R, color.G, color.B)),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center,
                        TextAlignment       = System.Windows.TextAlignment.Center,
                        TextWrapping        = TextWrapping.Wrap
                    }
                };

                System.Windows.Controls.Canvas.SetLeft(zone, x);
                System.Windows.Controls.Canvas.SetTop(zone, y);

                zone.MouseEnter += (s, a) =>
                {
                    zone.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(100, color.R, color.G, color.B));
                    zone.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(200, color.R, color.G, color.B));
                };
                zone.MouseLeave += (s, a) =>
                {
                    zone.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(60, color.R, color.G, color.B));
                    zone.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(120, color.R, color.G, color.B));
                };
                zone.MouseLeftButtonUp += (s, a) =>
                {
                    ShowMuscleDetail((string)zone.Tag);
                };

                return zone;
            }

            // Renkler
            var cRed    = System.Windows.Media.Color.FromRgb(220, 60, 60);
            var cBlue   = System.Windows.Media.Color.FromRgb(60, 120, 220);
            var cGreen  = System.Windows.Media.Color.FromRgb(60, 180, 100);
            var cOrange = System.Windows.Media.Color.FromRgb(240, 150, 40);
            var cPurple = System.Windows.Media.Color.FromRgb(160, 80, 200);
            var cTeal   = System.Windows.Media.Color.FromRgb(40, 180, 180);
            var cPink   = System.Windows.Media.Color.FromRgb(220, 100, 150);

            // ── ÖN VÜCUT (Front) ──
            var frontCanvas = new System.Windows.Controls.Canvas { Width = 220, Height = 500 };

            // Anatomik renkler
            var skinFill = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(28, 140, 160, 180));
            var skinStroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 100, 120, 150));
            var muscleLine = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 100, 120, 150));

            // ── Baş (anatomik oval) ──
            var fHead = new System.Windows.Shapes.Ellipse { Width = 36, Height = 42, Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.8 };
            System.Windows.Controls.Canvas.SetLeft(fHead, 92); System.Windows.Controls.Canvas.SetTop(fHead, 2);
            frontCanvas.Children.Add(fHead);

            // ── Boyun (konik) ──
            var fNeck = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse("M 104,42 L 102,58 L 118,58 L 116,42 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            frontCanvas.Children.Add(fNeck);

            // ── Gövde (anatomik Path — omuz-göğüs-bel-kalça) ──
            var fTorso = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 102,58 " +
                    "C 95,58 70,60 56,66 " +    // sol omuz eğimi
                    "C 52,68 50,72 52,78 " +     // sol omuz yuvarlak
                    "L 56,90 " +                  // sol koltuk altı
                    "C 58,94 60,96 62,100 " +     // sol göğüs alt
                    "L 64,130 " +                 // sol bel
                    "C 64,145 66,155 68,165 " +   // sol kalça eğimi
                    "C 70,175 74,185 78,195 " +   // sol kalça 
                    "L 98,200 " +                 // kasık sol
                    "L 110,200 " +                // orta
                    "L 122,200 " +                // kasık sağ
                    "C 146,185 150,175 152,165 " + // sağ kalça
                    "C 154,155 156,145 156,130 " + // sağ bel
                    "L 158,100 " +                 // sağ göğüs alt
                    "C 160,96 162,94 164,90 " +    // sağ koltuk altı
                    "L 168,78 " +                  // sağ omuz
                    "C 170,72 168,68 164,66 " +    // sağ omuz yuvarlak
                    "C 150,60 125,58 118,58 Z"),   // sağ omuz → boyun
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.6
            };
            frontCanvas.Children.Add(fTorso);

            // ── Kas çizgileri (göğüs, karın) ──
            // Göğüs orta çizgi
            var fChestLine = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 110,68 L 110,130"), Stroke = muscleLine, StrokeThickness = 0.8 };
            frontCanvas.Children.Add(fChestLine);
            // Göğüs alt çizgileri (pec)
            var fPecL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 68,100 C 80,106 95,108 110,105"), Stroke = muscleLine, StrokeThickness = 0.7 };
            var fPecR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 152,100 C 140,106 125,108 110,105"), Stroke = muscleLine, StrokeThickness = 0.7 };
            frontCanvas.Children.Add(fPecL); frontCanvas.Children.Add(fPecR);
            // Karın çizgileri (six-pack)
            for (int i = 0; i < 4; i++)
            {
                double y = 120 + i * 16;
                var abLine = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse($"M 90,{y} L 130,{y}"), Stroke = muscleLine, StrokeThickness = 0.5 };
                frontCanvas.Children.Add(abLine);
            }

            // ── Sol kol (anatomik) ──
            var fLeftArm = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 56,68 C 48,70 40,75 36,85 " +
                    "L 30,120 C 28,135 26,150 28,170 " +
                    "L 32,188 C 36,190 42,190 44,188 " +
                    "L 48,170 C 52,150 54,135 56,120 " +
                    "L 58,92 C 58,82 56,72 56,68 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            frontCanvas.Children.Add(fLeftArm);

            // ── Sağ kol (anatomik) ──
            var fRightArm = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 164,68 C 172,70 180,75 184,85 " +
                    "L 190,120 C 192,135 194,150 192,170 " +
                    "L 188,188 C 184,190 178,190 176,188 " +
                    "L 172,170 C 168,150 166,135 164,120 " +
                    "L 162,92 C 162,82 164,72 164,68 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            frontCanvas.Children.Add(fRightArm);

            // Biseps çizgileri
            var fBicL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 40,110 C 44,115 48,115 52,110"), Stroke = muscleLine, StrokeThickness = 0.6 };
            var fBicR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 168,110 C 172,115 176,115 180,110"), Stroke = muscleLine, StrokeThickness = 0.6 };
            frontCanvas.Children.Add(fBicL); frontCanvas.Children.Add(fBicR);

            // ── Sol bacak (anatomik) ──
            var fLeftLeg = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 78,195 C 76,210 72,230 68,260 " +
                    "C 66,280 64,310 62,340 " +
                    "C 60,360 60,375 62,390 " +
                    "L 68,392 C 72,390 74,385 76,375 " +
                    "C 80,355 84,330 88,300 " +
                    "C 92,270 96,240 100,210 " +
                    "L 100,200 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            frontCanvas.Children.Add(fLeftLeg);

            // ── Sağ bacak (anatomik) ──
            var fRightLeg = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 142,195 C 144,210 148,230 152,260 " +
                    "C 154,280 156,310 158,340 " +
                    "C 160,360 160,375 158,390 " +
                    "L 152,392 C 148,390 146,385 144,375 " +
                    "C 140,355 136,330 132,300 " +
                    "C 128,270 124,240 120,210 " +
                    "L 120,200 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            frontCanvas.Children.Add(fRightLeg);

            // Quad çizgileri
            var fQuadL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 82,220 C 80,250 76,280 72,300"), Stroke = muscleLine, StrokeThickness = 0.5 };
            var fQuadR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 138,220 C 140,250 144,280 148,300"), Stroke = muscleLine, StrokeThickness = 0.5 };
            frontCanvas.Children.Add(fQuadL); frontCanvas.Children.Add(fQuadR);

            // ÖN — HitZone'lar
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Neck" : "Boyun", "boyun", 95, 47, 30, 16, cTeal));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Shoulder\n(Front)" : "Omuz\n(Ön)", "omuz", 55, 60, 40, 32, cBlue));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Shoulder\n(Front)" : "Omuz\n(Ön)", "omuz", 125, 60, 40, 32, cBlue));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Chest" : "Göğüs", "göğüs", 64, 95, 92, 42, cRed));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Abs" : "Karın", "karın", 74, 142, 72, 54, cOrange));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Biceps" : "Biseps", "biseps", 26, 100, 28, 42, cGreen));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Biceps" : "Biseps", "biseps", 166, 100, 28, 42, cGreen));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Forearm" : "Ön Kol", "ön kol", 26, 148, 28, 38, cTeal));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Forearm" : "Ön Kol", "ön kol", 166, 148, 28, 38, cTeal));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Quads" : "Ön\nBacak", "ön bacak", 64, 215, 36, 80, cPurple));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Quads" : "Ön\nBacak", "ön bacak", 120, 215, 36, 80, cPurple));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Calves" : "Baldır", "baldır", 66, 340, 32, 38, cPink));
            frontCanvas.Children.Add(CreateHitZone(isEn ? "Calves" : "Baldır", "baldır", 122, 340, 32, 38, cPink));

            // Ön başlık
            var frontTitle = new System.Windows.Controls.TextBlock
            {
                Text       = isEn ? "— FRONT VIEW —" : "— ÖN GÖRÜNÜM —",
                FontSize   = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = accent,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            System.Windows.Controls.Canvas.SetLeft(frontTitle, 56);
            System.Windows.Controls.Canvas.SetTop(frontTitle, 480);
            frontCanvas.Children.Add(frontTitle);

            // ── ARKA VÜCUT (Back) ──
            var backCanvas = new System.Windows.Controls.Canvas { Width = 220, Height = 500 };

            // ── Arka Baş ──
            var bHead = new System.Windows.Shapes.Ellipse { Width = 36, Height = 42, Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.8 };
            System.Windows.Controls.Canvas.SetLeft(bHead, 92); System.Windows.Controls.Canvas.SetTop(bHead, 2);
            backCanvas.Children.Add(bHead);

            // ── Arka Boyun ──
            var bNeck = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse("M 104,42 L 102,58 L 118,58 L 116,42 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            backCanvas.Children.Add(bNeck);

            // ── Arka Gövde ──
            var bTorso = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 102,58 " +
                    "C 95,58 70,60 56,66 " +
                    "C 52,68 50,72 52,78 " +
                    "L 56,90 " +
                    "C 58,94 60,96 62,100 " +
                    "L 64,130 " +
                    "C 64,145 66,155 68,165 " +
                    "C 70,175 74,185 78,195 " +
                    "L 98,200 L 110,200 L 122,200 " +
                    "C 146,185 150,175 152,165 " +
                    "C 154,155 156,145 156,130 " +
                    "L 158,100 " +
                    "C 160,96 162,94 164,90 " +
                    "L 168,78 " +
                    "C 170,72 168,68 164,66 " +
                    "C 150,60 125,58 118,58 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.6
            };
            backCanvas.Children.Add(bTorso);

            // ── Arka kas çizgileri ──
            // Omurga çizgisi
            var bSpine = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 110,58 L 110,195"), Stroke = muscleLine, StrokeThickness = 1.0 };
            backCanvas.Children.Add(bSpine);
            // Trapez çizgileri
            var bTrapL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 110,62 C 100,65 85,68 70,72"), Stroke = muscleLine, StrokeThickness = 0.7 };
            var bTrapR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 110,62 C 120,65 135,68 150,72"), Stroke = muscleLine, StrokeThickness = 0.7 };
            backCanvas.Children.Add(bTrapL); backCanvas.Children.Add(bTrapR);
            // Lat çizgileri
            var bLatL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 68,95 C 75,100 85,110 90,130"), Stroke = muscleLine, StrokeThickness = 0.6 };
            var bLatR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 152,95 C 145,100 135,110 130,130"), Stroke = muscleLine, StrokeThickness = 0.6 };
            backCanvas.Children.Add(bLatL); backCanvas.Children.Add(bLatR);
            // Alt sırt çizgileri
            var bLowL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 100,140 C 95,150 90,160 85,170"), Stroke = muscleLine, StrokeThickness = 0.5 };
            var bLowR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 120,140 C 125,150 130,160 135,170"), Stroke = muscleLine, StrokeThickness = 0.5 };
            backCanvas.Children.Add(bLowL); backCanvas.Children.Add(bLowR);

            // ── Arka kollar ──
            var bLeftArm = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 56,68 C 48,70 40,75 36,85 " +
                    "L 30,120 C 28,135 26,150 28,170 " +
                    "L 32,188 C 36,190 42,190 44,188 " +
                    "L 48,170 C 52,150 54,135 56,120 " +
                    "L 58,92 C 58,82 56,72 56,68 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            backCanvas.Children.Add(bLeftArm);
            var bRightArm = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 164,68 C 172,70 180,75 184,85 " +
                    "L 190,120 C 192,135 194,150 192,170 " +
                    "L 188,188 C 184,190 178,190 176,188 " +
                    "L 172,170 C 168,150 166,135 164,120 " +
                    "L 162,92 C 162,82 164,72 164,68 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            backCanvas.Children.Add(bRightArm);
            // Triseps çizgileri
            var bTriL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 42,100 C 44,110 46,120 44,130"), Stroke = muscleLine, StrokeThickness = 0.6 };
            var bTriR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 178,100 C 176,110 174,120 176,130"), Stroke = muscleLine, StrokeThickness = 0.6 };
            backCanvas.Children.Add(bTriL); backCanvas.Children.Add(bTriR);

            // ── Arka bacaklar ──
            var bLeftLeg = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 78,195 C 76,210 72,230 68,260 " +
                    "C 66,280 64,310 62,340 " +
                    "C 60,360 60,375 62,390 " +
                    "L 68,392 C 72,390 74,385 76,375 " +
                    "C 80,355 84,330 88,300 " +
                    "C 92,270 96,240 100,210 " +
                    "L 100,200 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            backCanvas.Children.Add(bLeftLeg);
            var bRightLeg = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(
                    "M 142,195 C 144,210 148,230 152,260 " +
                    "C 154,280 156,310 158,340 " +
                    "C 160,360 160,375 158,390 " +
                    "L 152,392 C 148,390 146,385 144,375 " +
                    "C 140,355 136,330 132,300 " +
                    "C 128,270 124,240 120,210 " +
                    "L 120,200 Z"),
                Fill = skinFill, Stroke = skinStroke, StrokeThickness = 1.2
            };
            backCanvas.Children.Add(bRightLeg);
            // Hamstring çizgileri
            var bHamL = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 82,220 C 80,250 76,280 72,300"), Stroke = muscleLine, StrokeThickness = 0.5 };
            var bHamR = new System.Windows.Shapes.Path { Data = System.Windows.Media.Geometry.Parse("M 138,220 C 140,250 144,280 148,300"), Stroke = muscleLine, StrokeThickness = 0.5 };
            backCanvas.Children.Add(bHamL); backCanvas.Children.Add(bHamR);

            // ARKA — HitZone'lar
            backCanvas.Children.Add(CreateHitZone(isEn ? "Neck" : "Boyun", "boyun", 95, 47, 30, 16, cTeal));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Traps" : "Trapez", "sırt", 70, 60, 80, 30, cBlue));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Rear\nDelt" : "Arka\nOmuz", "omuz", 55, 60, 40, 32, cBlue));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Rear\nDelt" : "Arka\nOmuz", "omuz", 125, 60, 40, 32, cBlue));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Lats" : "Sırt\n(Lat)", "sırt", 62, 92, 96, 52, cRed));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Lower\nBack" : "Alt\nSırt", "sırt", 74, 148, 72, 40, cOrange));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Triceps" : "Triseps", "triseps", 26, 100, 28, 42, cGreen));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Triceps" : "Triseps", "triseps", 166, 100, 28, 42, cGreen));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Forearm" : "Ön Kol", "ön kol", 26, 148, 28, 38, cTeal));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Forearm" : "Ön Kol", "ön kol", 166, 148, 28, 38, cTeal));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Glutes" : "Kalça", "kalça", 62, 195, 96, 46, cPink));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Ham-\nstrings" : "Arka\nBacak", "arka bacak", 64, 248, 36, 70, cPurple));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Ham-\nstrings" : "Arka\nBacak", "arka bacak", 120, 248, 36, 70, cPurple));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Calves" : "Baldır", "baldır", 66, 340, 32, 38, cPink));
            backCanvas.Children.Add(CreateHitZone(isEn ? "Calves" : "Baldır", "baldır", 122, 340, 32, 38, cPink));

            // Arka başlık
            var backTitle = new System.Windows.Controls.TextBlock
            {
                Text       = isEn ? "— BACK VIEW —" : "— ARKA GÖRÜNÜM —",
                FontSize   = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = accent,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            System.Windows.Controls.Canvas.SetLeft(backTitle, 50);
            System.Windows.Controls.Canvas.SetTop(backTitle, 480);
            backCanvas.Children.Add(backTitle);

            // Canvasları grid'e yerleştir
            System.Windows.Controls.Grid.SetColumn(frontCanvas, 0);
            System.Windows.Controls.Grid.SetColumn(backCanvas, 1);
            bodyGrid.Children.Add(frontCanvas);
            bodyGrid.Children.Add(backCanvas);

            leftPanel.Children.Add(bodyGrid);

            // Sol paneli border içine al
            var leftBorder = new System.Windows.Controls.Border { Child = leftPanel };
            System.Windows.Controls.Grid.SetColumn(leftBorder, 0);
            System.Windows.Controls.Grid.SetColumn(rightBorder, 1);
            mainGrid.Children.Add(leftBorder);
            mainGrid.Children.Add(rightBorder);

            win.Content = mainGrid;
            win.ShowDialog();
        }

        // ─────────────────────────────────────────────────────────────
        // Fizik Çizim Yardımcıları
        // ─────────────────────────────────────────────────────────────

        /// <summary>Kiloya göre ölçeklenmiş vücut silueti çizer (Kilo Hedefi sekmesi).</summary>
        private void DrawWeightFigure(System.Windows.Controls.Canvas canvas, double weight)
        {
            canvas.Children.Clear();
            // BMI-like scaling (175cm referans)
            double bmi = weight / (1.75 * 1.75);
            double s = Math.Max(0.7, Math.Min(1.6, bmi / 22.0));
            double cx = 40;

            var fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(100, 99, 102, 241));
            var stroke = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(180, 99, 102, 241));

            // Baş
            var head = new System.Windows.Shapes.Ellipse { Width = 14, Height = 18, Fill = fill, Stroke = stroke, StrokeThickness = 1.2 };
            System.Windows.Controls.Canvas.SetLeft(head, cx - 7); System.Windows.Controls.Canvas.SetTop(head, 2);
            canvas.Children.Add(head);

            // Boyun
            double nw = 6 * Math.Min(s, 1.1);
            var neck = new System.Windows.Shapes.Rectangle { Width = nw, Height = 6, Fill = fill, RadiusX = 2, RadiusY = 2 };
            System.Windows.Controls.Canvas.SetLeft(neck, cx - nw / 2); System.Windows.Controls.Canvas.SetTop(neck, 18);
            canvas.Children.Add(neck);

            // Gövde (Path ile organik)
            double sw = 28 * s; // omuz
            double ww = 18 * s; // bel
            double hw = 22 * s; // kalça
            
            // Formatlama hatasını önlemek için Invariant Culture
            string torsoData = System.FormattableString.Invariant($"M {cx - sw / 2:F1},26 C {cx - sw / 2:F1},28 {cx - sw / 2 - 2:F1},32 {cx - sw / 2 + 2:F1},38 L {cx - ww / 2:F1},60 C {cx - ww / 2 - 1:F1},65 {cx - hw / 2:F1},70 {cx - hw / 2:F1},74 L {cx + hw / 2:F1},74 C {cx + hw / 2:F1},70 {cx + ww / 2 + 1:F1},65 {cx + ww / 2:F1},60 L {cx + sw / 2 - 2:F1},38 C {cx + sw / 2 + 2:F1},32 {cx + sw / 2:F1},28 {cx + sw / 2:F1},26 Z");
            
            var torso = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(torsoData),
                Fill = fill, Stroke = stroke, StrokeThickness = 1.2
            };
            canvas.Children.Add(torso);

            // Sol kol
            double aw = 5 * Math.Max(0.9, s);
            var lArm = new System.Windows.Shapes.Rectangle { Width = aw, Height = 40, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 2.5, RadiusY = 2.5 };
            System.Windows.Controls.Canvas.SetLeft(lArm, cx - sw / 2 - aw - 1); System.Windows.Controls.Canvas.SetTop(lArm, 28);
            canvas.Children.Add(lArm);
            // Sağ kol
            var rArm = new System.Windows.Shapes.Rectangle { Width = aw, Height = 40, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 2.5, RadiusY = 2.5 };
            System.Windows.Controls.Canvas.SetLeft(rArm, cx + sw / 2 + 1); System.Windows.Controls.Canvas.SetTop(rArm, 28);
            canvas.Children.Add(rArm);

            // Sol bacak
            double lw = 7 * s;
            var lLeg = new System.Windows.Shapes.Rectangle { Width = lw, Height = 55, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 3.5, RadiusY = 3.5 };
            System.Windows.Controls.Canvas.SetLeft(lLeg, cx - hw / 2 + 1); System.Windows.Controls.Canvas.SetTop(lLeg, 74);
            canvas.Children.Add(lLeg);
            // Sağ bacak
            var rLeg = new System.Windows.Shapes.Rectangle { Width = lw, Height = 55, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 3.5, RadiusY = 3.5 };
            System.Windows.Controls.Canvas.SetLeft(rLeg, cx + hw / 2 - lw - 1); System.Windows.Controls.Canvas.SetTop(rLeg, 74);
            canvas.Children.Add(rLeg);

            // Kilo etiketi
            var label = new System.Windows.Controls.TextBlock
            {
                Text = $"{weight:F0} kg",
                FontSize = 10, FontWeight = FontWeights.SemiBold,
                Foreground = stroke, HorizontalAlignment = HorizontalAlignment.Center
            };
            System.Windows.Controls.Canvas.SetLeft(label, cx - 18);
            System.Windows.Controls.Canvas.SetTop(label, 138);
            canvas.Children.Add(label);
        }

        /// <summary>Omuz/bel oranına göre V-vücut silueti çizer (V-Taper sekmesi).</summary>
        private void DrawVTaperFigure(System.Windows.Controls.Canvas canvas, double shoulder, double waist)
        {
            canvas.Children.Clear();
            double cx = 40;
            // Normalize: omuz ve beli canvas genişliğine ölçekle
            double maxDim = Math.Max(shoulder, waist);
            double sw = (shoulder / maxDim) * 34; // omuz genişliği (yarım)
            double ww = (waist / maxDim) * 34;     // bel
            double hw = ww * 1.1;                 // kalça belden biraz geniş

            var fill = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(100, 99, 102, 241));
            var stroke = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(180, 99, 102, 241));

            // Baş
            var head = new System.Windows.Shapes.Ellipse { Width = 14, Height = 18, Fill = fill, Stroke = stroke, StrokeThickness = 1.2 };
            System.Windows.Controls.Canvas.SetLeft(head, cx - 7); System.Windows.Controls.Canvas.SetTop(head, 2);
            canvas.Children.Add(head);

            // Boyun
            var neck = new System.Windows.Shapes.Rectangle { Width = 6, Height = 6, Fill = fill, RadiusX = 2, RadiusY = 2 };
            System.Windows.Controls.Canvas.SetLeft(neck, cx - 3); System.Windows.Controls.Canvas.SetTop(neck, 18);
            canvas.Children.Add(neck);

            // Gövde (V-taper path, invariant format)
            string torsoData = System.FormattableString.Invariant($"M {cx - sw / 2:F1},26 C {cx - sw / 2:F1},28 {cx - sw / 2 - 1:F1},34 {cx - sw / 2 + 1:F1},40 L {cx - ww / 2:F1},60 C {cx - ww / 2:F1},65 {cx - hw / 2:F1},70 {cx - hw / 2:F1},74 L {cx + hw / 2:F1},74 C {cx + hw / 2:F1},70 {cx + ww / 2:F1},65 {cx + ww / 2:F1},60 L {cx + sw / 2 - 1:F1},40 C {cx + sw / 2 + 1:F1},34 {cx + sw / 2:F1},28 {cx + sw / 2:F1},26 Z");
            
            var torso = new System.Windows.Shapes.Path
            {
                Data = System.Windows.Media.Geometry.Parse(torsoData),
                Fill = fill, Stroke = stroke, StrokeThickness = 1.2
            };
            canvas.Children.Add(torso);

            // Kollar
            double aw = 5.5;
            var lArm = new System.Windows.Shapes.Rectangle { Width = aw, Height = 40, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 2.5, RadiusY = 2.5 };
            System.Windows.Controls.Canvas.SetLeft(lArm, cx - sw / 2 - aw - 1); System.Windows.Controls.Canvas.SetTop(lArm, 28);
            canvas.Children.Add(lArm);
            var rArm = new System.Windows.Shapes.Rectangle { Width = aw, Height = 40, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 2.5, RadiusY = 2.5 };
            System.Windows.Controls.Canvas.SetLeft(rArm, cx + sw / 2 + 1); System.Windows.Controls.Canvas.SetTop(rArm, 28);
            canvas.Children.Add(rArm);

            // Bacaklar
            double legW = 7.5;
            var lLeg = new System.Windows.Shapes.Rectangle { Width = legW, Height = 55, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 3.5, RadiusY = 3.5 };
            System.Windows.Controls.Canvas.SetLeft(lLeg, cx - hw / 2 + 1); System.Windows.Controls.Canvas.SetTop(lLeg, 74);
            canvas.Children.Add(lLeg);
            var rLeg = new System.Windows.Shapes.Rectangle { Width = legW, Height = 55, Fill = fill, Stroke = stroke, StrokeThickness = 0.8, RadiusX = 3.5, RadiusY = 3.5 };
            System.Windows.Controls.Canvas.SetLeft(rLeg, cx + hw / 2 - legW - 1); System.Windows.Controls.Canvas.SetTop(rLeg, 74);
            canvas.Children.Add(rLeg);

            // SHR etiketi
            double shr = shoulder / waist;
            var label = new System.Windows.Controls.TextBlock
            {
                Text = $"SHR: {shr:F2}",
                FontSize = 10, FontWeight = FontWeights.SemiBold,
                Foreground = stroke, HorizontalAlignment = HorizontalAlignment.Center
            };
            System.Windows.Controls.Canvas.SetLeft(label, cx - 22);
            System.Windows.Controls.Canvas.SetTop(label, 138);
            canvas.Children.Add(label);
        }
    }
}
