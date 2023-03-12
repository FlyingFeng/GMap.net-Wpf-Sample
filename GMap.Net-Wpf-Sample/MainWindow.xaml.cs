using GMap.NET;
using GMap.NET.WindowsPresentation;
using GMap.Net_Wpf_Sample.CustomMapProviders;
using GMap.Net_Wpf_Sample.CustomShapes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
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

namespace GMap.Net_Wpf_Sample {


    public enum MapOptMode {
        DragMap,
        SelectElement,
        DrawLine,
        DrawPolygon
    }

    public struct PointD {
        public double X { get; set; }
        public double Y { get; set; }
        public PointD(double x, double y) {
            X = x;
            Y = y;
        }
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        double EARTH_RADIUS = 6378137;
        private CustomPolygon? _currentPolygon;
        private CustomLine? _currentLine;
        private MapOptMode _mode = MapOptMode.DragMap;
        private int _lineIndex = 0;
        private int _polygonIndex = 0;

        private bool PingBaidu() {
            var flag = false;
            using (var ping = new Ping()) {
                try {
                    var reply = ping.Send("www.baidu.com");

                    flag = reply.Status == IPStatus.Success;
                }
                catch {
                    flag = false;
                }
            }

            return flag;
        }

        public MainWindow() {
            InitializeComponent();

            var path = System.IO.Path.Combine(AppContext.BaseDirectory, "Cache");
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            myMap.CacheLocation = path;

            if (!PingBaidu()) {
                myMap.Manager.Mode = NET.AccessMode.CacheOnly;
            }
            else {
                myMap.Manager.Mode = NET.AccessMode.ServerAndCache;
            }

            myMap.MapProvider = AMapProvider.Instance;
            myMap.Position = new NET.PointLatLng(22.54, 114.06);
            myMap.MinZoom = 3;
            myMap.MaxZoom = 18;
            myMap.Zoom = 6;
            myMap.ShowCenter = false;
            myMap.DragButton = MouseButton.Left;

            myMap.MouseLeftButtonDown += MyMap_MouseLeftButtonDown;
            myMap.MouseMove += MyMap_MouseMove;
            myMap.MouseRightButtonDown += MyMap_MouseRightButtonDown;

        }



        private static double Rad(double d) {
            return d * Math.PI / 180.0;
        }

        private double GetDistance(double lon1, double lat1, double lon2, double lat2) {
            double radLat1 = Rad(lat1);
            double radLat2 = Rad(lat2);
            double a = radLat1 - radLat2;
            double b = Rad(lon1) - Rad(lon2);
            double s = 2 * Math.Asin(Math.Sqrt(Math.Pow(Math.Sin(a / 2), 2) + Math.Cos(radLat1) * Math.Cos(radLat2) * Math.Pow(Math.Sin(b / 2), 2)));
            s = s * EARTH_RADIUS;
            return s;
        }

        private double GetArea(List<PointLatLng> points) {
            double area = 0;
            if (points.Count < 3) {
                return area;
            }

            var minLat = points.Min(s => s.Lat);
            var minLng = points.Min(s => s.Lng);
            var origin = new PointLatLng(minLat, minLng);
            var finalPos = new List<PointD>();
            foreach (var item in points) {
                var toXDistance = GetDistance(item.Lng, item.Lat, minLng, item.Lat);
                var toYDistance = GetDistance(item.Lng, item.Lat, item.Lng, minLat);
                var point = new PointD(toYDistance, toXDistance);
                finalPos.Add(point);
            }


            for (int i = 0; i < finalPos.Count; i++) {
                if (i != finalPos.Count - 1) {
                    area += finalPos[i].X * finalPos[i + 1].Y - finalPos[i].Y * finalPos[i + 1].X;
                }
                else {
                    area += finalPos[i].X * finalPos[0].Y - finalPos[i].Y * finalPos[0].X;
                }
            }

            return Math.Abs(area) * 0.5;
        }


        private void MyMap_MouseRightButtonDown(object sender, MouseButtonEventArgs e) {
            var position = e.GetPosition(myMap);
            var latLng = myMap.FromLocalToLatLng((int)position.X, (int)position.Y);

            if (_mode == MapOptMode.DrawLine) {
                if (_currentLine != null) {
                    var count = _currentLine.Points.Count;
                    _currentLine.Points[count - 1] = latLng;
                    myMap.RegenerateShape(_currentLine);
                    _currentLine = null;
                    _lineIndex = 0;
                }
            }
            else if (_mode == MapOptMode.DrawPolygon) {
                if (_currentPolygon != null) {
                    var count = _currentPolygon.Points.Count;
                    _currentPolygon.Points[count - 1] = latLng;
                    myMap.RegenerateShape(_currentPolygon);
                    var area = GetArea(_currentPolygon.Points);
                    _currentPolygon = null;
                    _polygonIndex = 0;
                }
            }

        }

        private void MyMap_MouseMove(object sender, MouseEventArgs e) {
            var position = e.GetPosition(myMap);
            var latLng = myMap.FromLocalToLatLng((int)position.X, (int)position.Y);
            if (_mode == MapOptMode.DrawLine) {
                if (_currentLine != null) {
                    _currentLine.Points[_lineIndex] = latLng;
                    myMap.RegenerateShape(_currentLine);
                }
            }
            else if (_mode == MapOptMode.DrawPolygon) {
                if (_currentPolygon != null) {
                    _currentPolygon.Points[_polygonIndex] = latLng;
                    myMap.RegenerateShape(_currentPolygon);
                }
            }
        }

        private void MyMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {

            var position = e.GetPosition(myMap);
            var latLng = myMap.FromLocalToLatLng((int)position.X, (int)position.Y);
            txtPos.Text = $"{latLng.Lng},{latLng.Lat}";
            if (_mode == MapOptMode.DrawLine) {
                if (_currentLine == null) {
                    var list = new List<PointLatLng>() { latLng, latLng };
                    _currentLine = new CustomLine(list);
                    myMap.Markers.Add(_currentLine);
                }
                else {
                    _currentLine.Points[_lineIndex] = latLng;
                    _currentLine.Points.Add(latLng);
                    myMap.RegenerateShape(_currentLine);
                }

                _lineIndex++;
            }
            else if (_mode == MapOptMode.DrawPolygon) {
                if (_currentPolygon == null) {
                    var list = new List<PointLatLng>() { latLng, latLng };
                    _currentPolygon = new CustomPolygon(list);
                    myMap.Markers.Add(_currentPolygon);
                }
                else {
                    _currentPolygon.Points[_polygonIndex] = latLng;
                    _currentPolygon.Points.Add(latLng);
                    myMap.RegenerateShape(_currentPolygon);
                }

                _polygonIndex++;
            }


        }

        private void btnDragMap_Click(object sender, RoutedEventArgs e) {
            _mode = MapOptMode.DragMap;
            myMap.CanDragMap = true;
            foreach (var item in myMap.Markers) {
                if (item is CustomLine line) {
                    line.SetPolygonHitTest(false);
                }
                else if (item is CustomPolygon polygon) {
                    polygon.SetPolygonHitTest(false);
                }
            }
        }

        private void btnSelectMap_Click(object sender, RoutedEventArgs e) {
            _mode = MapOptMode.SelectElement;
            myMap.CanDragMap = true;
            foreach (var item in myMap.Markers) {
                if (item is CustomLine line) {
                    line.SetPolygonHitTest(true);
                }
                else if (item is CustomPolygon polygon) {
                    polygon.SetPolygonHitTest(true);
                }
            }
        }

        private void btnDrawLine_Click(object sender, RoutedEventArgs e) {
            _mode = MapOptMode.DrawLine;
            myMap.CanDragMap = false;
            foreach (var item in myMap.Markers) {
                if (item is CustomLine line) {
                    line.SetPolygonHitTest(false);
                }
                else if (item is CustomPolygon polygon) {
                    polygon.SetPolygonHitTest(false);
                }
            }
        }

        private void btnDrawPolygon_Click(object sender, RoutedEventArgs e) {
            _mode = MapOptMode.DrawPolygon;
            myMap.CanDragMap = false;
            foreach (var item in myMap.Markers) {
                if (item is CustomLine line) {
                    line.SetPolygonHitTest(false);
                }
                else if (item is CustomPolygon polygon) {
                    polygon.SetPolygonHitTest(false);
                }
            }
        }

        private void btnRemoveSelected_Click(object sender, RoutedEventArgs e) {
            if (_mode == MapOptMode.SelectElement) {
                myMap.CanDragMap = true;
                var list = new List<GMapMarker>();
                foreach (var item in myMap.Markers) {
                    if (item is CustomLine line) {
                        if (line.IsSelected) {
                            list.Add(item);
                        }
                    }
                    else if (item is CustomPolygon polygon) {
                        if (polygon.IsSelected)
                            list.Add(item);
                    }
                }
                list.ForEach(s => myMap.Markers.Remove(s));
            }
        }
    }
}
