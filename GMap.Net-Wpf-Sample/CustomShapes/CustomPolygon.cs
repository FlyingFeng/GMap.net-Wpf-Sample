using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GMap.Net_Wpf_Sample.CustomShapes {
    internal class CustomPolygon : GMapPolygon {

        private Path? _path;
        private double _strokeThickness = 3;
        private Brush _strokeBrush = Brushes.MidnightBlue;
        private Brush _fill = Brushes.AliceBlue;
        private bool _isSelected;

        public void SetPolygonHitTest(bool flag) {
            if (_path is null) {
                return;
            }
            _path.IsHitTestVisible = flag;
        }

        public double StrokeThickness {
            get => _strokeThickness;
            set {
                _strokeThickness = value;
            }
        }

        public Brush StrokeBrush {
            get => _strokeBrush;
            set {
                _strokeBrush = value;
            }
        }

        public Brush Fill {
            get => _fill;
            set {
                _fill = value;
            }
        }

        public bool IsSelected => _isSelected;


        public CustomPolygon(IEnumerable<PointLatLng> points) : base(points) {
        }


        public override Path CreatePath(List<Point> localPath, bool addBlurEffect) {

            var p = base.CreatePath(localPath, addBlurEffect);
            if (_path == null) {
                _path = p;
                _path.MouseLeftButtonDown += _path_MouseLeftButtonDown;
            }
            else {
                _path.Data = p.Data;
            }
            _path.Stroke = StrokeBrush;
            _path.StrokeThickness = StrokeThickness;
            _path.Fill = Fill;



            return _path;
        }

        private void _path_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed &&
                sender is Path) {
                if (_path is null) {
                    return;
                }
                _isSelected = !_isSelected;

                if (_isSelected) {
                    _path.Stroke = Brushes.Orange;
                    _path.StrokeThickness = 6;
                }
                else {
                    _path.Stroke = StrokeBrush;
                    _path.StrokeThickness = StrokeThickness;
                }
            }
        }
    }
}
