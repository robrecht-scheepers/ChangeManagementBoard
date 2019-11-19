﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using System.Xaml;

namespace CM
{
    /// <summary>
    /// Interaction logic for Board.xaml
    /// </summary>
    public partial class Board : UserControl
    {
        private struct PolarPoint
        {
            public double Radius { get; set; }
            public double Angle { get; set; }

            public PolarPoint(double radius, double angle)
            {
                Radius = radius;
                Angle = angle;
            }
        }

        private const int NumberOfPhases = 24;
        private const double MarkerSize = 20;

        private double _canvasWidth;
        private double _canvasHeight;
        private double _actualRadius;

        private PersonMarker _movingMarker = null;
        private Point _movingMarkerOffset;

        public static readonly DependencyProperty PersonsProperty = DependencyProperty.Register(
            "Persons", typeof(ObservableCollection<Person>), typeof(Board), new PropertyMetadata(default(ObservableCollection<Person>), PersonsChanged));

        private static void PersonsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var me = (Board) d;
            me.Persons.CollectionChanged += (s, a) => me.DrawPeople();

            me.DrawPeople();
        }

        public ObservableCollection<Person> Persons
        {
            get => (ObservableCollection<Person>) GetValue(PersonsProperty);
            set => SetValue(PersonsProperty, value);
        }
        
        public Board()
        {
            InitializeComponent();
            InitializeElements();
        }

        private void InitializeElements()
        {
            Canvas.Loaded += (s, a) => DrawBoard();
            Canvas.SizeChanged += (s, a) => DrawBoard();

            Canvas.MouseMove += MoveMarker;
            Canvas.MouseLeftButtonUp += EndMarkerMove;
        }

       

        private void DrawBoard()
        {
            Canvas.Children.Clear();

            _canvasWidth = Canvas.ActualWidth;
            _canvasHeight = Canvas.ActualHeight;
            _actualRadius = Math.Min(_canvasWidth, _canvasHeight) / 2;

            DrawCircle(1, Brushes.DarkRed);
            DrawCircle(0.9, Brushes.SandyBrown);
            DrawCircle(0.8, Brushes.LightYellow);
            DrawCircle(0.7, Brushes.White);
            DrawCircle(0.5, Brushes.White);

            for (int phase = 0; phase < NumberOfPhases; phase++)
            {
                var angle = phase * 360 / (double)NumberOfPhases;
                Canvas.Children.Add(LineFromPolar(0.5, angle, 1, angle));

                var label = new TextBlock {Text = (phase + 1).ToString(), FontSize = 12};
                Canvas.Children.Add(label);
                var labelPosition = PolarToPoint(0.45, (phase + 0.5) * 360 / NumberOfPhases);
                labelPosition.Offset(-8, -8);
                Canvas.SetLeft(label, labelPosition.X);
                Canvas.SetTop(label, labelPosition.Y);
            }

            DrawPeople();
        }

        private void DrawCircle(double radius, Brush background = null, double opacity = 1)
        {
            var circle = new Ellipse
            {
                Width = 2 * _actualRadius * radius,
                Height = 2 * _actualRadius * radius,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
                Fill = background ?? Brushes.Transparent,
                
            };
            Canvas.Children.Add(circle);
            Canvas.SetLeft(circle, _actualRadius * (1 - radius));
            Canvas.SetTop(circle, _actualRadius * (1 - radius));
        }

        private void DrawPeople()
        {
            var markers = Canvas.Children.OfType<PersonMarker>().ToList();
            foreach (var marker in markers)
            {
                Canvas.Children.Remove(marker);
            }

            if(Persons == null || !Persons.Any())
                return;

            var positions = new Dictionary<Position, List<string>>();
            foreach (var person in Persons)
            {
                if(positions.ContainsKey(person.Position))
                    positions[person.Position].Add(person.Name);
                else
                    positions[person.Position] = new List<string>{person.Name};
            }

            foreach (var position in positions.Keys)
            {
                if (position.Phase == 0)
                {
                    var xMin = -0.3;
                    var xMax = 0.3;
                    var xStep = (xMax - xMin) / (double)(positions[position].Count + 1);
                    for (int i = 0; i < positions[position].Count; i++)
                    {
                        var marker = new PersonMarker
                        {
                            Name = positions[position][i],
                            Stroke = Brushes.Blue,
                            Width = MarkerSize,
                            Height = MarkerSize
                        };
                        Canvas.Children.Add(marker);
                        var markerPosition = CarthesianToPoint(xMin + (i + 1) * xStep, 0);
                        Canvas.SetLeft(marker, markerPosition.X);
                        Canvas.SetTop(marker, markerPosition.Y);
                        marker.MouseLeftButtonDown += BeginMarkerMove;
                    }
                    continue;
                }

                var radius = ResistanceToRadius(position.Resistance);
                var angleStart = (position.Phase - 1) * 360 / (double)NumberOfPhases;
                var angleEnd = position.Phase * 360 / (double)NumberOfPhases;

                

                var angleStep = (angleEnd - angleStart) / (positions[position].Count + 1);

                for (int i = 0; i < positions[position].Count; i++)
                {
                    var marker = new PersonMarker
                    {
                        Name = positions[position][i],
                        Stroke = Brushes.Blue,
                        Width = MarkerSize,
                        Height = MarkerSize
                    };
                    var markerPosition = PolarToPoint(radius, angleStart + (i + 1) * angleStep);
                    markerPosition.Offset(-1*MarkerSize/2, -1*MarkerSize/2);
                    Canvas.Children.Add(marker);
                    Canvas.SetLeft(marker, markerPosition.X);
                    Canvas.SetTop(marker, markerPosition.Y);

                    marker.MouseLeftButtonDown += BeginMarkerMove;
                }
            }
        }

        private void BeginMarkerMove(object sender, MouseButtonEventArgs e)
        {
            _movingMarker = sender as PersonMarker;
            _movingMarkerOffset = e.GetPosition(_movingMarker);
            Canvas.CaptureMouse();
        }

        private void MoveMarker(object sender, MouseEventArgs e)
        {
            if (_movingMarker == null)
                return;

            var newPosition = e.GetPosition(Canvas);

            Canvas.SetLeft(_movingMarker, newPosition.X - _movingMarkerOffset.X);
            Canvas.SetTop(_movingMarker, newPosition.Y - _movingMarkerOffset.Y);
        }

        private void EndMarkerMove(object sender, MouseButtonEventArgs e)
        {
            Canvas.ReleaseMouseCapture();
            if(_movingMarker == null)
                return;
            
            var markerCenterPosition = e.GetPosition(Canvas);
            markerCenterPosition.Offset(MarkerSize/2 - _movingMarkerOffset.X, MarkerSize/2 - _movingMarkerOffset.Y);
            var name = _movingMarker.Name;

            _movingMarker = null;

            Persons.First(x => x.Name == name).Position = PointToPosition(markerCenterPosition);
            DrawPeople();
        }

        private Position PointToPosition(Point point)
        {
            var polar = PointToPolar(point);
            var phase = (int)Math.Ceiling(polar.Angle / ((double) 360 / NumberOfPhases));
            var resistance = RadiusToResistance(polar.Radius);

            return resistance < 0 ? new Position(0,0) : new Position(phase, resistance);
        }

        private double ResistanceToRadius(int resistance)
        {
            switch (resistance)
            {
                case 0:
                    return 0.6;
                case 1:
                    return 0.75;
                case 2:
                    return 0.85;
                case 3:
                    return 0.95;
                default:
                    throw new ArgumentException("Invalid resistance level");
            }
        }

        private int RadiusToResistance(double radius)
        {
            return radius < 0.5 ? -1 : radius < 0.7 ? 0 : radius < 0.8 ? 1 : radius < 0.9 ? 2 : 3;
        }
        

        private Point PolarToPoint(double r, double a)
        {
            var x = _actualRadius * (1 + r * Math.Sin(a * Math.PI / 180));
            var y = _actualRadius * (1 - r * Math.Cos(a * Math.PI / 180));

            return new Point(x, y);
        }

        private Point CarthesianToPoint(double x, double y)
        {
            return new Point(_actualRadius + x*_actualRadius, _actualRadius - y*_actualRadius);
        }

        private PolarPoint PointToPolar(Point point)
        {
            var x = point.X / _actualRadius - 1;
            var y = 1 - point.Y / _actualRadius;

            var radius = Math.Sqrt(x * x + y * y);
            var angle = Math.Atan(y / x) * 180 / Math.PI;
            if (x < 0)
                angle += 180;
            else if (y < 0)
                angle += 360;

            angle = (90 - angle + 360) % 360;

            return new PolarPoint(radius,angle);
        }

        private Line LineFromPolar(double r1, double a1, double r2, double a2)
        {
            var point1 = PolarToPoint(r1, a1);
            var point2 = PolarToPoint(r2, a2);

            return new Line
            {
                X1 = point1.X,
                Y1 = point1.Y,
                X2 = point2.X,
                Y2 = point2.Y,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
        }
        
    }
}
