using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using YMInc.StoreDeploymentApp.Models;

namespace YMInc.StoreDeploymentApp.Controls
{
    public class DonutChartControl : Control
    {
        public static readonly DependencyProperty ReportProperty =
            DependencyProperty.Register(nameof(Report), typeof(WeeklyProgressReport), typeof(DonutChartControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnReportChanged));

        public static readonly DependencyProperty SelectedSliceProperty =
            DependencyProperty.Register(nameof(SelectedSlice), typeof(string), typeof(DonutChartControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        private static void OnReportChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DonutChartControl chart)
            {
                chart.InvalidateVisual();
            }
        }

        public WeeklyProgressReport? Report
        {
            get => (WeeklyProgressReport?)GetValue(ReportProperty);
            set => SetValue(ReportProperty, value);
        }

        public string? SelectedSlice
        {
            get => (string?)GetValue(SelectedSliceProperty);
            set => SetValue(SelectedSliceProperty, value);
        }

        public event EventHandler<string?>? SliceClicked;

        private readonly List<(string Name, Geometry Geometry)> _sliceHitAreas = new();

        public DonutChartControl()
        {
            ClipToBounds = true;
            Cursor = Cursors.Hand;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);
            var pt = e.GetPosition(this);

            foreach (var (name, geom) in _sliceHitAreas)
            {
                if (geom.FillContains(pt))
                {
                    SelectedSlice = SelectedSlice == name ? null : name;
                    SliceClicked?.Invoke(this, SelectedSlice);
                    InvalidateVisual();
                    return;
                }
            }

            // Clicked outside slice (or in center): clear filter
            SelectedSlice = null;
            SliceClicked?.Invoke(this, null);
            InvalidateVisual();
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            _sliceHitAreas.Clear();

            double width = ActualWidth;
            double height = ActualHeight;
            if (width <= 0 || height <= 0) return;

            Point center = new(width / 2, height / 2);
            double outerRadius = Math.Min(width, height) / 2 - 12;
            double innerRadius = outerRadius * 0.72;

            if (Report == null || Report.Slices.Count == 0 || !Report.Slices.Any(s => s.Percentage > 0))
            {
                var outer = new EllipseGeometry(center, outerRadius, outerRadius);
                var inner = new EllipseGeometry(center, innerRadius, innerRadius);
                var emptyRing = new CombinedGeometry(GeometryCombineMode.Exclude, outer, inner);
                emptyRing.Freeze();
                var placeholderBrush = new SolidColorBrush(Color.FromArgb(40, 148, 163, 184));
                placeholderBrush.Freeze();
                dc.DrawGeometry(placeholderBrush, null, emptyRing);
                return;
            }

            double startAngle = -90; // Start at top

            foreach (var slice in Report.Slices)
            {
                double sweepAngle = (slice.Percentage / 100.0) * 360.0;
                if (sweepAngle <= 0) continue;

                var geom = CreateDonutSliceGeometry(center, innerRadius, outerRadius, startAngle, sweepAngle);
                _sliceHitAreas.Add((slice.Name, geom));

                var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(slice.ColorHex));
                brush.Freeze();

                bool isSelected = SelectedSlice == slice.Name;
                var pen = new Pen(isSelected ? Brushes.White : new SolidColorBrush(Color.FromArgb(180, 15, 23, 42)), isSelected ? 3.5 : 2);
                pen.Freeze();

                dc.DrawGeometry(brush, pen, geom);
                startAngle += sweepAngle;
            }

            // Draw center text
            var typeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

            var pctText = new FormattedText(
                $"{Report.OverallProgressPct}%",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                28,
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(pctText, new Point(center.X - pctText.Width / 2, center.Y - pctText.Height / 2 - 8));

            var subTypeface = new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal);
            var subText = new FormattedText(
                "TARGET",
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                subTypeface,
                10,
                new SolidColorBrush(Color.FromRgb(148, 163, 184)),
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            dc.DrawText(subText, new Point(center.X - subText.Width / 2, center.Y + 12));
        }

        private static Geometry CreateDonutSliceGeometry(Point center, double innerRadius, double outerRadius, double startAngleDeg, double sweepAngleDeg)
        {
            if (sweepAngleDeg >= 359.5)
            {
                var outer = new EllipseGeometry(center, outerRadius, outerRadius);
                var inner = new EllipseGeometry(center, innerRadius, innerRadius);
                var ring = new CombinedGeometry(GeometryCombineMode.Exclude, outer, inner);
                ring.Freeze();
                return ring;
            }

            if (sweepAngleDeg > 359.0) sweepAngleDeg = 359.0;

            double endAngleDeg = startAngleDeg + sweepAngleDeg;
            double startRad = startAngleDeg * Math.PI / 180.0;
            double endRad = endAngleDeg * Math.PI / 180.0;

            Point p0 = new(center.X + outerRadius * Math.Cos(startRad), center.Y + outerRadius * Math.Sin(startRad));
            Point p1 = new(center.X + outerRadius * Math.Cos(endRad), center.Y + outerRadius * Math.Sin(endRad));
            Point p2 = new(center.X + innerRadius * Math.Cos(endRad), center.Y + innerRadius * Math.Sin(endRad));
            Point p3 = new(center.X + innerRadius * Math.Cos(startRad), center.Y + innerRadius * Math.Sin(startRad));

            bool isLargeArc = sweepAngleDeg > 180;

            var figure = new PathFigure { StartPoint = p0, IsClosed = true, IsFilled = true };
            figure.Segments.Add(new ArcSegment(p1, new Size(outerRadius, outerRadius), 0, isLargeArc, SweepDirection.Clockwise, true));
            figure.Segments.Add(new LineSegment(p2, true));
            figure.Segments.Add(new ArcSegment(p3, new Size(innerRadius, innerRadius), 0, isLargeArc, SweepDirection.Counterclockwise, true));

            var geom = new PathGeometry();
            geom.Figures.Add(figure);
            geom.Freeze();
            return geom;
        }
    }
}
