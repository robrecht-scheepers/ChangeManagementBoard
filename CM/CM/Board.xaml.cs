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
        // ***** Configuration *********************************************************
        private const string ProjectNamePlaceholder = "_";
        private const int NumberOfPhases = 24;
        private const double MarkerSize = 22;
        private readonly Color _markerColor = Colors.Blue;
        private readonly Dictionary<int, double> _radii = new Dictionary<int, double>
        {
            {0, 0.5},
            {1, 0.7},
            {2, 0.8},
            {3, 0.9},
            {4, 1.0} // outer radius
        };
        private readonly Dictionary<int, Color> _colors = new Dictionary<int, Color>
        {
            {0, Color.FromRgb(223,239,255) },
            {1, Color.FromRgb(255,201,14) },
            {2, Color.FromRgb(255,127,39) },
            {3, Color.FromRgb(237,28,36) },
        };
        private const double DefaultOpacity = 0.6;
        private const double HoverOpacity = 1;

        private readonly Color _defaultLineColor = Color.FromRgb(50, 50, 50);
        private readonly double _defaultLineThickness = 1;
        private readonly double _hoverLineThickness = 3;
        // *******************************************************************************

        private struct PolarPoint
        {
            public double Radius { get; }
            public double Angle { get; }

            public PolarPoint(double radius, double angle)
            {
                Radius = radius;
                Angle = angle;
            }
        }

        private double _canvasWidth;
        private double _canvasHeight;
        private double _actualRadius;
        private UserControl _movingMarker = null;
        private Point _movingMarkerOffset;
        private readonly Dictionary<Position,Shape> _positionShapes = new Dictionary<Position, Shape>();

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
            _positionShapes.Clear();

            _canvasWidth = Canvas.ActualWidth;
            _canvasHeight = Canvas.ActualHeight;
            _actualRadius = Math.Min(_canvasWidth, _canvasHeight) / 2;

            for(var phase = 1; phase <= NumberOfPhases; phase++)
            {
                var lowAngle = (phase - 1) * 360 / NumberOfPhases;
                var highAngle = phase * 360 / NumberOfPhases;

                for (var res = 0; res <= 3; res++)
                {
                    var lowRadius = _radii[res];
                    var highRadius = _radii[res + 1];
                    
                    var shape = new Path
                    {
                        Stroke = new SolidColorBrush(_defaultLineColor),
                        StrokeThickness = _defaultLineThickness,
                        Fill = new SolidColorBrush(_colors[res]) {Opacity = DefaultOpacity},
                        Data = new PathGeometry
                        {
                            Figures = new PathFigureCollection
                            {
                                new PathFigure
                                {
                                    IsClosed = true,
                                    StartPoint = PolarToPoint(lowRadius, lowAngle),
                                    Segments = new PathSegmentCollection
                                    {
                                        new LineSegment(PolarToPoint(highRadius, lowAngle), true),
                                        new ArcSegment(PolarToPoint(highRadius, highAngle),
                                            new Size(highRadius * _actualRadius, highRadius * _actualRadius),
                                            0, false, SweepDirection.Clockwise, true),
                                        new LineSegment(PolarToPoint(lowRadius, highAngle), true),
                                        new ArcSegment(PolarToPoint(lowRadius, lowAngle),
                                            new Size(lowRadius * _actualRadius, lowRadius * _actualRadius),
                                            0, false, SweepDirection.Counterclockwise, true),
                                    }
                                }
                            }
                        }
                    };
                    Canvas.Children.Add(shape);
                    _positionShapes[new Position(phase, res)] = shape;
                }

                var label = new TextBlock { Text = phase.ToString(), FontSize = 14 };
                Canvas.Children.Add(label);
                var labelPosition = PolarToPoint(_radii[0] - 0.05, (phase - 0.5) * 360 / NumberOfPhases);
                labelPosition.Offset(-10, -10);
                Canvas.SetLeft(label, labelPosition.X);
                Canvas.SetTop(label, labelPosition.Y);
            }

            DrawPeople();
        }

        private void DrawPeople()
        {
            var markers = Canvas.Children.OfType<UserControl>().ToList();
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
                var radius = ResistanceToRadius(position.Resistance);
                var angleStart = (position.Phase - 1) * 360 / (double)NumberOfPhases;
                var angleEnd = position.Phase * 360 / (double)NumberOfPhases;

                if (positions[position].Contains(ProjectNamePlaceholder))
                {
                    var marker = new ProjectMarker
                    {
                        Name = ProjectNamePlaceholder,
                        Fill = new SolidColorBrush(_markerColor),
                        Occupants = positions[position].Count - 1
                    };

                    if (marker.Occupants > 0)
                    {
                        marker.ToolTip = string.Join(",", positions[position].Where(x => x != ProjectNamePlaceholder));
                    }

                    var markerPosition = PolarToPoint(radius, (angleStart + angleEnd)/2);
                    markerPosition.Offset(-1 * MarkerSize / 2, -1 * MarkerSize / 2);
                    Canvas.Children.Add(marker);
                    Canvas.SetLeft(marker, markerPosition.X);
                    Canvas.SetTop(marker, markerPosition.Y);
                    marker.LayoutTransform = new RotateTransform((angleStart + angleEnd) / 2);
                    marker.MouseLeftButtonDown += BeginMarkerMove;
                    continue;
                }
                
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
                            Fill = new SolidColorBrush(_markerColor),
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

                var angleStep = (angleEnd - angleStart) / (positions[position].Count + 1);
                for (int i = 0; i < positions[position].Count; i++)
                {
                    var name = positions[position][i];
                    var marker = new PersonMarker
                    {
                        Name = name,
                        Fill = new SolidColorBrush(_markerColor),
                        Width = MarkerSize,
                        Height = MarkerSize
                    };

                    var markerPosition = PolarToPoint(radius, angleStart + (i + 1) * angleStep);
                    markerPosition.Offset(-1 * MarkerSize / 2, -1 * MarkerSize / 2);
                    Canvas.Children.Add(marker);
                    Canvas.SetLeft(marker, markerPosition.X);
                    Canvas.SetTop(marker, markerPosition.Y);
                    marker.MouseLeftButtonDown += BeginMarkerMove;
                }
            }
        }

        private void BeginMarkerMove(object sender, MouseButtonEventArgs e)
        {
            _movingMarker = sender as UserControl;
            _movingMarkerOffset = e.GetPosition(_movingMarker);
            Canvas.CaptureMouse();
        }

        private void MoveMarker(object sender, MouseEventArgs e)
        {
            if (_movingMarker == null)
                return;

            var newPoint = e.GetPosition(Canvas);
            Canvas.SetLeft(_movingMarker, newPoint.X - _movingMarkerOffset.X);
            Canvas.SetTop(_movingMarker, newPoint.Y - _movingMarkerOffset.Y);

            var hoverPosition = PointToPosition(newPoint);
            foreach (var position in _positionShapes.Keys)
            {
                if (position.Equals(hoverPosition))
                {
                    _positionShapes[position].Fill.Opacity = HoverOpacity;
                    _positionShapes[position].StrokeThickness = _hoverLineThickness;
                }
                else
                {
                    _positionShapes[position].Fill.Opacity = DefaultOpacity;
                    _positionShapes[position].StrokeThickness = _defaultLineThickness;
                }
            }

            if (_movingMarker.Name == ProjectNamePlaceholder)
            {
                _movingMarker.LayoutTransform = new RotateTransform(PointToPolar(newPoint).Angle);
            }
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
            foreach (var position in _positionShapes.Keys)
            {
                _positionShapes[position].Fill.Opacity = DefaultOpacity;
                _positionShapes[position].StrokeThickness = _defaultLineThickness;
            }

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
            return (_radii[resistance] + _radii[resistance + 1]) / 2;
        }

        private int RadiusToResistance(double radius)
        {
            return radius < _radii[0] ? -1 : radius < _radii[1] ? 0 : radius < _radii[2] ? 1 : radius < _radii[3] ? 2 : 3;
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
